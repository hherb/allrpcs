using Allrpcs.Application.Audio;

namespace Allrpcs.Application.Tests.Audio;

public sealed class FoliageCueHistoryTests
{
    private static readonly string[] FirstEntries = ["1. Leaves rustling", "2. Leaves rustling above"];
    [Fact]
    public void ImmediateFirstRecordHasTwelveSecondDiscardingLimiterAndNoQueue()
    {
        FoliageCueHistory history = new();
        Assert.True(history.Record("Leaves rustling"));
        Assert.False(history.Record("Leaves rustling right"));
        for (int tick = 0; tick < 119; tick++) { Assert.True(history.Advance(0.1)); }
        Assert.False(history.Record("Leaves rustling left"));
        history.Advance(0.1);
        Assert.True(history.Record("Leaves rustling above"));
        Assert.Equal(FirstEntries, history.Entries);
    }

    [Fact]
    public void BoundedReadOnlySnapshotsAndResetPreservePriorOwners()
    {
        FoliageCueHistory foliage = new();
        HealerFootstepCueHistory steps = new(); PositionalCueHistory replies = new();
        steps.Record("Footsteps on earth"); replies.Record("Conversation nearby");
        for (int index = 0; index < 20; index++)
        {
            for (int tick = 0; tick < 120; tick++) { foliage.Advance(0.1); }
            Assert.True(foliage.Record("Leaves rustling"));
        }
        IReadOnlyList<string> detached = foliage.Entries;
        Assert.Equal(16, detached.Count);
        Assert.Equal("5. Leaves rustling", detached[0]);
        Assert.Throws<NotSupportedException>(() => ((IList<string>)detached)[0] = "modified");
        foliage.Clear();
        Assert.Equal(16, detached.Count);
        Assert.Empty(foliage.Entries); Assert.Single(steps.Entries); Assert.Single(replies.Entries);
        Assert.True(foliage.Record("Leaves rustling"));
        Assert.Equal("1. Leaves rustling", foliage.Entries[0]);
    }

    [Theory]
    [InlineData("Healer")]
    [InlineData("Leaves rustling at 2232,1328")]
    [InlineData("Leaves rustling in the garden")]
    [InlineData("Leaves rustling tomorrow")]
    [InlineData("Leaves rustling above north")]
    public void NonGenericTextCannotEnterHistory(string caption) =>
        Assert.False(new FoliageCueHistory().Record(caption));

    [Theory]
    [InlineData(double.NaN)]
    [InlineData(double.PositiveInfinity)]
    [InlineData(0)]
    [InlineData(-0.1)]
    [InlineData(12)]
    public void InvalidTimeCannotUnfreezeLimiter(double seconds)
    {
        FoliageCueHistory history = new(); history.Record("Leaves rustling");
        Assert.False(history.Advance(seconds));
        Assert.False(history.Record("Leaves rustling"));
        Assert.Single(history.Entries);
    }
}
