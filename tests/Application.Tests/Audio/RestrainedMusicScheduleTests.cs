using System.Numerics;
using Allrpcs.Application.Audio;

namespace Allrpcs.Application.Tests.Audio;

public sealed class RestrainedMusicScheduleTests
{
    private static void Advance(RestrainedMusicSchedule schedule, double seconds, bool blocked = false, bool audible = true)
    {
        for (int tick = 0; tick < (int)Math.Round(seconds * 4); tick++) { schedule.Advance(0.25, blocked, audible); }
    }

    private static RestrainedMusicSchedule Exploration(double distance = 24)
    {
        RestrainedMusicSchedule schedule = new();
        schedule.ObserveRegion("B01", true);
        schedule.ObserveRegion("B03", true);
        schedule.ObserveMotion(new(1, Vector2.Zero, new((float)distance, 0), 0.1, 240));
        return schedule;
    }

    [Fact]
    public void StartupStandingAndSameBedCannotCreateMusic()
    {
        RestrainedMusicSchedule schedule = new();
        schedule.ObserveRegion("B03", true);
        Advance(schedule, 300);
        schedule.ObserveRegion("B03", true);
        Assert.Null(schedule.Pending);
        Assert.Null(schedule.Active);
    }

    [Fact]
    public void ExplorationNeedsStableDwellQuietAndActualTravel()
    {
        RestrainedMusicSchedule schedule = Exploration(23);
        Advance(schedule, 12);
        Assert.Null(schedule.Active);
        schedule.ObserveMotion(new(2, new(23, 0), new(24, 0), 0.1, 240));
        Assert.Equal(MusicPassage.Exploration, schedule.Advance(0, false, true));
        Assert.Null(schedule.Pending);
        Assert.Equal(1, schedule.Gain);
    }

    [Fact]
    public void QuietMustBeConsecutiveAndExpiryWinsAdmissionTie()
    {
        RestrainedMusicSchedule schedule = Exploration();
        Advance(schedule, 11.75);
        Assert.Null(schedule.Active);
        Advance(schedule, 21.25, true);
        Advance(schedule, 11.75);
        Assert.Null(schedule.Active);
        Assert.Null(schedule.Advance(0.25, false, true));
        Assert.Null(schedule.Pending);
    }

    [Fact]
    public void RegionChangesReplaceOnlyExplorationAndNullRebasesWithoutEntry()
    {
        RestrainedMusicSchedule schedule = Exploration();
        Advance(schedule, 11);
        schedule.ObserveRegion("B04", true);
        Advance(schedule, 12);
        Assert.Null(schedule.Active);
        schedule.ObserveRegion(null, true);
        schedule.ObserveRegion("B01", true);
        Assert.Null(schedule.Pending);
        object binding = new();
        schedule.ObserveTreatment(binding, binding, true);
        schedule.ObserveRegion("B02", true);
        Assert.Equal(MusicPassage.Treatment, schedule.Pending);
    }

    [Fact]
    public void LiveTreatmentWaitsEightThenTwelveSecondsAndDuplicatesStayInert()
    {
        RestrainedMusicSchedule schedule = new();
        object binding = new();
        schedule.ObserveTreatment(new object(), binding, true);
        Assert.Null(schedule.Pending);
        schedule.ObserveTreatment(binding, binding, true);
        Advance(schedule, 19.75);
        Assert.Null(schedule.Active);
        Assert.Equal(MusicPassage.Treatment, schedule.Advance(0.25, false, true));
        schedule.Finished();
        Advance(schedule, 180);
        schedule.ObserveTreatment(binding, binding, true);
        Assert.Null(schedule.Pending);
    }

    [Fact]
    public void TreatmentOverridesPendingExplorationButCannotSwapActiveMusic()
    {
        RestrainedMusicSchedule schedule = Exploration();
        object binding = new();
        schedule.ObserveTreatment(binding, binding, true);
        Advance(schedule, 20);
        Assert.Equal(MusicPassage.Treatment, schedule.Active);
        object next = new();
        schedule.ObserveTreatment(next, next, true);
        Assert.Null(schedule.Pending);
        Assert.True(schedule.Fading);
        Advance(schedule, 0.5);
        Assert.InRange(schedule.Gain, 0.16, 0.17);
        Advance(schedule, 0.25);
        Assert.Null(schedule.Active);
        Assert.Equal(180, schedule.CooldownRemaining);
    }

    [Fact]
    public void FadeCannotReverseAndCooldownNeedsFreshEvent()
    {
        RestrainedMusicSchedule schedule = Exploration();
        Advance(schedule, 12);
        schedule.Foreground(3);
        Advance(schedule, 0.25);
        double gain = schedule.Gain;
        Advance(schedule, 0.25);
        Assert.True(schedule.Gain < gain);
        Advance(schedule, 0.25);
        schedule.ObserveRegion("B02", true);
        Advance(schedule, 180);
        Assert.Null(schedule.Pending);
        Assert.Null(schedule.Active);
        schedule.ObserveRegion("B01", true);
        schedule.ObserveMotion(new(2, new(24, 0), new(48, 0), 0.1, 240));
        Advance(schedule, 12);
        Assert.Equal(MusicPassage.Exploration, schedule.Active);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void MuteCancelsPendingOrActiveAndUnmuteCannotReplay(bool playing)
    {
        RestrainedMusicSchedule schedule = Exploration();
        if (playing) { Advance(schedule, 12); }
        schedule.Advance(0, false, false);
        Assert.Null(schedule.Active);
        Assert.Null(schedule.Pending);
        Advance(schedule, 300);
        Assert.Null(schedule.Active);
    }

    [Theory]
    [InlineData(double.NaN)]
    [InlineData(double.PositiveInfinity)]
    [InlineData(-1)]
    public void InvalidDeltaCannotAdvanceAdmission(double delta)
    {
        RestrainedMusicSchedule schedule = Exploration();
        Assert.Null(schedule.Advance(delta, false, true));
        Advance(schedule, 11.75);
        Assert.Null(schedule.Active);
        Assert.Equal(MusicPassage.Exploration, schedule.Advance(0.25, false, true));
    }

    [Theory]
    [InlineData(0.251, true)]
    [InlineData(0.25, false)]
    public void SuspendedTimeAndFocusLossResetWithoutCatchUp(double delta, bool focused)
    {
        RestrainedMusicSchedule schedule = Exploration();
        Advance(schedule, 12);
        schedule.Advance(delta, false, true, focused);
        Assert.Null(schedule.Active);
        Assert.Null(schedule.Pending);
        Assert.Equal(180, schedule.CooldownRemaining);
        Advance(schedule, 180);
        schedule.ObserveRegion("B04", true);
        Assert.Null(schedule.Pending);
    }

    [Fact]
    public void SuccessfulResetForgetsTreatmentButNeverReplaysIt()
    {
        RestrainedMusicSchedule schedule = new();
        object binding = new();
        schedule.ObserveTreatment(binding, binding, true);
        schedule.Reset();
        Advance(schedule, 180);
        Assert.Null(schedule.Pending);
        schedule.ObserveTreatment(binding, binding, true);
        Advance(schedule, 20);
        Assert.Equal(MusicPassage.Treatment, schedule.Active);
    }

    [Theory]
    [InlineData(96, false)]
    [InlineData(96.01, true)]
    public void TeleportBoundaryUsesMeasuredDiscontinuity(double distance, bool resets)
    {
        RestrainedMusicSchedule schedule = Exploration();
        schedule.ObserveMotion(new(2, new(24, 0), new((float)(24 + distance), 0), 0.1, 240));
        Assert.Equal(resets, schedule.Pending is null);
    }

    [Fact]
    public void FailedStartCreatesCooldownAndForegroundHoldIsIndependentOfAudibility()
    {
        RestrainedMusicSchedule schedule = Exploration();
        Advance(schedule, 12);
        schedule.Finished();
        Assert.Equal(180, schedule.CooldownRemaining);
        Advance(schedule, 180);
        object binding = new();
        schedule.ObserveTreatment(binding, binding, false);
        Advance(schedule, 20);
        Assert.Null(schedule.Active);
        schedule.ObserveTreatment(binding, binding, true);
        Assert.Null(schedule.Pending);
    }

    [Fact]
    public void DuplicateOrImpossibleMotionDoesNotSupplyTravel()
    {
        RestrainedMusicSchedule schedule = Exploration(0);
        schedule.ObserveMotion(new(1, Vector2.Zero, new(24, 0), 0.1, 240));
        schedule.ObserveMotion(new(2, Vector2.Zero, new(50, 0), 0.1, 240));
        Advance(schedule, 12);
        Assert.Null(schedule.Active);
    }
    [Fact]
    public void ReplyHoldRequiresThreeFullSecondsThenTwelveQuietSeconds()
    {
        RestrainedMusicSchedule schedule = Exploration();
        schedule.Foreground(RestrainedMusicSchedule.ReplyHoldSeconds);
        Advance(schedule, 14.75);
        Assert.Null(schedule.Active);
        Assert.Equal(MusicPassage.Exploration, schedule.Advance(0.25, false, true));
    }

    [Fact]
    public void StopAndCooldownUseExactBoundariesWithoutNewOpportunity()
    {
        RestrainedMusicSchedule schedule = Exploration();
        Advance(schedule, 12);
        schedule.Foreground(0);
        schedule.Advance(0.25, false, true);
        schedule.Advance(0.25, false, true);
        schedule.Advance(0.099, false, true);
        Assert.NotNull(schedule.Active);
        schedule.Advance(0.001, false, true);
        Assert.Null(schedule.Active);
        Assert.Equal(180, schedule.CooldownRemaining);
        Advance(schedule, 179.75);
        schedule.Advance(0.249, false, true);
        Assert.True(schedule.CooldownRemaining > 0);
        schedule.Advance(0.001, false, true);
        Assert.InRange(schedule.CooldownRemaining, 0, 1e-9);
        Assert.Null(schedule.Pending);
        Assert.Null(schedule.Active);
    }

}
