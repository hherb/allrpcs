using Allrpcs.Application.Audio;

namespace Allrpcs.Application.Tests.Audio;

public sealed class FoliagePresentationScheduleTests
{
    [Fact]
    public void FirstAndSubsequentStartsWaitTwelveActiveSecondsWithThreeSecondPassages()
    {
        FoliagePresentationSchedule schedule = new();
        for (int tick = 0; tick < 119; tick++) { Assert.False(schedule.Advance(0.1, false)); }
        Assert.False(schedule.InPassage);
        Assert.True(schedule.Advance(0.1, false));
        for (int tick = 0; tick < 29; tick++)
        { Assert.False(schedule.Advance(0.1, false)); Assert.True(schedule.InPassage); }
        Assert.False(schedule.Advance(0.1, false));
        Assert.False(schedule.InPassage);
        for (int tick = 0; tick < 89; tick++) { Assert.False(schedule.Advance(0.1, false)); }
        Assert.True(schedule.Advance(0.1, false));
    }

    [Fact]
    public void CancelledOrUnadmittedPassageCannotReplayOnApproachOrUnmute()
    {
        FoliagePresentationSchedule schedule = new();
        for (int tick = 0; tick < 120; tick++) { schedule.Advance(0.1, false); }
        schedule.CancelPassage();
        Assert.False(schedule.InPassage);
        for (int tick = 0; tick < 119; tick++)
        { Assert.False(schedule.Advance(0.1, false)); Assert.False(schedule.InPassage); }
        Assert.True(schedule.Advance(0.1, false));
    }

    [Theory]
    [InlineData(double.NaN)]
    [InlineData(double.PositiveInfinity)]
    [InlineData(0)]
    [InlineData(-0.1)]
    [InlineData(0.10001)]
    [InlineData(24)]
    public void StallAndInvalidDeltaRebaseWithoutCatchUp(double delta)
    {
        FoliagePresentationSchedule schedule = new();
        for (int tick = 0; tick < 120; tick++) { schedule.Advance(0.1, false); }
        Assert.False(schedule.Advance(delta, false));
        Assert.False(schedule.InPassage);
        for (int tick = 0; tick < 119; tick++) { Assert.False(schedule.Advance(0.1, false)); }
        Assert.True(schedule.Advance(0.1, false));
    }

    [Fact]
    public void PresentationBlockerFreezesAndResumesWithFreshWait()
    {
        FoliagePresentationSchedule schedule = new();
        for (int tick = 0; tick < 110; tick++) { schedule.Advance(0.1, false); }
        for (int tick = 0; tick < 500; tick++) { Assert.False(schedule.Advance(0.1, true)); }
        for (int tick = 0; tick < 119; tick++) { Assert.False(schedule.Advance(0.1, false)); }
        Assert.True(schedule.Advance(0.1, false));
        schedule.Reset();
        Assert.False(schedule.InPassage);
        for (int tick = 0; tick < 119; tick++) { Assert.False(schedule.Advance(0.1, false)); }
        Assert.True(schedule.Advance(0.1, false));
    }
}
