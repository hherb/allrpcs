using Allrpcs.Application.Audio;

namespace Allrpcs.Application.Tests.Audio;

public sealed class WindPresentationScheduleTests
{
    [Fact]
    public void FirstAndSubsequentStartsWaitEighteenActiveSecondsWithFourSecondPassages()
    {
        WindPresentationSchedule schedule = new();
        for (int tick = 0; tick < 179; tick++) { Assert.False(schedule.Advance(0.1, false)); }
        Assert.False(schedule.InPassage);
        Assert.True(schedule.Advance(0.1, false));
        for (int tick = 0; tick < 39; tick++)
        { Assert.False(schedule.Advance(0.1, false)); Assert.True(schedule.InPassage); }
        Assert.False(schedule.Advance(0.1, false));
        Assert.False(schedule.InPassage);
        for (int tick = 0; tick < 139; tick++) { Assert.False(schedule.Advance(0.1, false)); }
        Assert.True(schedule.Advance(0.1, false));
    }

    [Fact]
    public void CancelledOrUnadmittedPassageCannotReplayOnApproachOrUnmute()
    {
        WindPresentationSchedule schedule = new();
        for (int tick = 0; tick < 180; tick++) { schedule.Advance(0.1, false); }
        schedule.CancelPassage();
        Assert.False(schedule.InPassage);
        for (int tick = 0; tick < 179; tick++)
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
        WindPresentationSchedule schedule = new();
        for (int tick = 0; tick < 180; tick++) { schedule.Advance(0.1, false); }
        Assert.False(schedule.Advance(delta, false));
        Assert.False(schedule.InPassage);
        for (int tick = 0; tick < 179; tick++) { Assert.False(schedule.Advance(0.1, false)); }
        Assert.True(schedule.Advance(0.1, false));
    }

    [Fact]
    public void PresentationBlockerFreezesAndResumesWithFreshWait()
    {
        WindPresentationSchedule schedule = new();
        for (int tick = 0; tick < 170; tick++) { schedule.Advance(0.1, false); }
        for (int tick = 0; tick < 500; tick++) { Assert.False(schedule.Advance(0.1, true)); }
        for (int tick = 0; tick < 179; tick++) { Assert.False(schedule.Advance(0.1, false)); }
        Assert.True(schedule.Advance(0.1, false));
        schedule.Reset();
        Assert.False(schedule.InPassage);
        for (int tick = 0; tick < 179; tick++) { Assert.False(schedule.Advance(0.1, false)); }
        Assert.True(schedule.Advance(0.1, false));
    }
}
