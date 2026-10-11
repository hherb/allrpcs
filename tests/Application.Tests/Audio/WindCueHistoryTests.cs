using Allrpcs.Application.Audio;

namespace Allrpcs.Application.Tests.Audio;

public sealed class WindCueHistoryTests
{
    private static readonly string[] FirstEntries = ["1. Wind blowing", "2. Wind blowing above"];
    [Fact]
    public void ImmediateFirstRecordHasEighteenSecondDiscardingLimiterAndNoQueue()
    {
        WindCueHistory history = new();
        Assert.True(history.Record("Wind blowing"));
        Assert.False(history.Record("Wind blowing right"));
        for (int tick = 0; tick < 179; tick++) { Assert.True(history.Advance(0.1)); }
        Assert.False(history.Record("Wind blowing left"));
        history.Advance(0.1);
        Assert.True(history.Record("Wind blowing above"));
        Assert.Equal(FirstEntries, history.Entries);
    }

    [Fact]
    public void BoundedReadOnlySnapshotsAndResetPreservePriorOwners()
    {
        WindCueHistory wind = new();
        HealerFootstepCueHistory steps = new(); PositionalCueHistory replies = new(); FoliageCueHistory leaves = new();
        steps.Record("Footsteps on earth"); replies.Record("Conversation nearby"); leaves.Record("Leaves rustling");
        for (int index = 0; index < 20; index++)
        {
            for (int tick = 0; tick < 180; tick++) { wind.Advance(0.1); }
            Assert.True(wind.Record("Wind blowing"));
        }
        IReadOnlyList<string> detached = wind.Entries;
        Assert.Equal(16, detached.Count);
        Assert.Equal("5. Wind blowing", detached[0]);
        Assert.Throws<NotSupportedException>(() => ((IList<string>)detached)[0] = "modified");
        wind.Clear();
        Assert.Equal(16, detached.Count);
        Assert.Empty(wind.Entries); Assert.Single(steps.Entries); Assert.Single(replies.Entries); Assert.Single(leaves.Entries);
        Assert.True(wind.Record("Wind blowing"));
        Assert.Equal("1. Wind blowing", wind.Entries[0]);
    }

    [Theory]
    [InlineData("Healer")]
    [InlineData("Wind blowing at 1500,1400")]
    [InlineData("Wind blowing on the southern road")]
    [InlineData("Wind blowing tomorrow")]
    [InlineData("Wind blowing above north")]
    public void NonGenericTextCannotEnterHistory(string caption) =>
        Assert.False(new WindCueHistory().Record(caption));

    [Theory]
    [InlineData(double.NaN)]
    [InlineData(double.PositiveInfinity)]
    [InlineData(0)]
    [InlineData(-0.1)]
    [InlineData(12)]
    public void InvalidTimeCannotUnfreezeLimiter(double seconds)
    {
        WindCueHistory history = new(); history.Record("Wind blowing");
        Assert.False(history.Advance(seconds));
        Assert.False(history.Record("Wind blowing"));
        Assert.Single(history.Entries);
    }
}
