using Allrpcs.Application.Audio;

namespace Allrpcs.Application.Tests.Audio;

public sealed class RemedySteepingCueHistoryTests
{
    private static readonly string[] FirstEntries = ["1. Steeping preparation", "2. Steeping preparation"];
    [Fact]
    public void UniqueAdmittedCommitsHaveNoTimeLimiterAndOnlyAnonymousText()
    {
        var history = new RemedySteepingCueHistory();
        Assert.True(history.Record()); Assert.True(history.Record());
        Assert.Equal(FirstEntries, history.Entries);
    }
    [Fact]
    public void CapacityEvictsOldestAndDetachedSnapshotsCannotMutateFutureHistory()
    {
        var history = new RemedySteepingCueHistory();
        for (int i = 0; i < 20; i++) { Assert.True(history.Record()); }
        var snapshot = history.Entries;
        Assert.Equal(16, snapshot.Count); Assert.Equal("5. Steeping preparation", snapshot[0]);
        Assert.Throws<NotSupportedException>(() => ((IList<string>)snapshot)[0] = "modified");
        history.Clear(); Assert.Empty(history.Entries); Assert.Equal(16, snapshot.Count);
        Assert.True(history.Record()); Assert.Equal("1. Steeping preparation", history.Entries[0]);
    }
    [Fact]
    public void ResetCannotClearOtherCueOwners()
    {
        var steep = new RemedySteepingCueHistory(); var heat = new RemedyHeatingCueHistory(); var rain = new RainCueHistory(); var wind = new WindCueHistory();
        rain.Record("Rain falling"); wind.Record("Wind blowing"); heat.Record(); steep.Record(); steep.Clear();
        Assert.Single(rain.Entries); Assert.Single(wind.Entries); Assert.Single(heat.Entries); Assert.Empty(steep.Entries);
    }
}
