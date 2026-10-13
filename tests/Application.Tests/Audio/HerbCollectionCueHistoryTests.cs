using Allrpcs.Application.Audio;

namespace Allrpcs.Application.Tests.Audio;

public sealed class HerbCollectionCueHistoryTests
{
    private static readonly string[] FirstEntries = ["1. Soft rustle", "2. Soft rustle"];
    [Fact]
    public void UniqueAdmittedCommitsHaveNoTimeLimiterAndOnlyAnonymousText()
    {
        var history = new HerbCollectionCueHistory();
        Assert.True(history.Record()); Assert.True(history.Record());
        Assert.Equal(FirstEntries, history.Entries);
    }
    [Fact]
    public void CapacityEvictsOldestAndDetachedSnapshotsCannotMutateFutureHistory()
    {
        var history = new HerbCollectionCueHistory();
        for (int i = 0; i < 20; i++) { Assert.True(history.Record()); }
        var snapshot = history.Entries;
        Assert.Equal(16, snapshot.Count); Assert.Equal("5. Soft rustle", snapshot[0]);
        Assert.Throws<NotSupportedException>(() => ((IList<string>)snapshot)[0] = "modified");
        history.Clear(); Assert.Empty(history.Entries); Assert.Equal(16, snapshot.Count);
        Assert.True(history.Record()); Assert.Equal("1. Soft rustle", history.Entries[0]);
    }
    [Fact]
    public void ResetCannotClearOtherCueOwners()
    {
        var rustle = new HerbCollectionCueHistory(); var steeping = new RemedySteepingCueHistory(); var heat = new RemedyHeatingCueHistory(); var rain = new RainCueHistory(); var wind = new WindCueHistory();
        rain.Record("Rain falling"); wind.Record("Wind blowing"); heat.Record(); steeping.Record(); rustle.Record(); rustle.Clear();
        Assert.Single(rain.Entries); Assert.Single(wind.Entries); Assert.Single(heat.Entries); Assert.Single(steeping.Entries); Assert.Empty(rustle.Entries);
    }
}
