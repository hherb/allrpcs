using Allrpcs.Application.Audio;

namespace Allrpcs.Application.Tests.Audio;

public sealed class RainCueHistoryTests
{
    private static readonly string[] FirstEntries = ["1. Rain falling", "2. Rain falling above"];
    [Fact]
    public void ImmediateFirstRecordHasTwentySecondDiscardingLimiterAndNoQueue()
    {
        RainCueHistory history = new();
        Assert.True(history.Record("Rain falling"));
        Assert.False(history.Record("Rain falling right"));
        for (int tick = 0; tick < 199; tick++) { Assert.True(history.Advance(0.1)); }
        Assert.False(history.Record("Rain falling left"));
        history.Advance(0.1);
        Assert.True(history.Record("Rain falling above"));
        Assert.Equal(FirstEntries, history.Entries);
    }

    [Fact]
    public void BoundedReadOnlySnapshotsAndResetPreservePriorOwners()
    {
        RainCueHistory rain = new();
        HealerFootstepCueHistory steps = new(); PositionalCueHistory replies = new(); FoliageCueHistory leaves = new(); WindCueHistory wind = new();
        steps.Record("Footsteps on earth"); replies.Record("Conversation nearby"); leaves.Record("Leaves rustling"); wind.Record("Wind blowing");
        for (int index = 0; index < 20; index++)
        {
            for (int tick = 0; tick < 200; tick++) { rain.Advance(0.1); }
            Assert.True(rain.Record("Rain falling"));
        }
        IReadOnlyList<string> detached = rain.Entries;
        Assert.Equal(16, detached.Count);
        Assert.Equal("5. Rain falling", detached[0]);
        Assert.Throws<NotSupportedException>(() => ((IList<string>)detached)[0] = "modified");
        rain.Clear();
        Assert.Equal(16, detached.Count);
        Assert.Empty(rain.Entries); Assert.Single(steps.Entries); Assert.Single(replies.Entries); Assert.Single(leaves.Entries); Assert.Single(wind.Entries);
        Assert.True(rain.Record("Rain falling"));
        Assert.Equal("1. Rain falling", rain.Entries[0]);
    }

    [Theory]
    [InlineData("Healer")]
    [InlineData("Rain falling at 1000,350")]
    [InlineData("Rain falling on the woodland clearing")]
    [InlineData("Rain falling tomorrow")]
    [InlineData("Rain falling above north")]
    public void NonGenericTextCannotEnterHistory(string caption) =>
        Assert.False(new RainCueHistory().Record(caption));

    [Theory]
    [InlineData(double.NaN)]
    [InlineData(double.PositiveInfinity)]
    [InlineData(0)]
    [InlineData(-0.1)]
    [InlineData(12)]
    public void InvalidTimeCannotUnfreezeLimiter(double seconds)
    {
        RainCueHistory history = new(); history.Record("Rain falling");
        Assert.False(history.Advance(seconds));
        Assert.False(history.Record("Rain falling"));
        Assert.Single(history.Entries);
    }
}
