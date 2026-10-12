using Allrpcs.Application.Audio;

namespace Allrpcs.Application.Tests.Audio;

public sealed class RemedyHeatingCueHistoryTests
{
    private static readonly string[] FirstEntries = ["1. Heating preparation", "2. Heating preparation"];
    [Fact]
    public void UniqueAdmittedCommitsHaveNoTimeLimiterAndOnlyAnonymousText()
    {
        var history = new RemedyHeatingCueHistory();
        Assert.True(history.Record()); Assert.True(history.Record());
        Assert.Equal(FirstEntries, history.Entries);
    }
    [Fact]
    public void CapacityEvictsOldestAndDetachedSnapshotsCannotMutateFutureHistory()
    {
        var history = new RemedyHeatingCueHistory();
        for (int i = 0; i < 20; i++) { Assert.True(history.Record()); }
        var snapshot = history.Entries;
        Assert.Equal(16, snapshot.Count); Assert.Equal("5. Heating preparation", snapshot[0]);
        Assert.Throws<NotSupportedException>(() => ((IList<string>)snapshot)[0] = "modified");
        history.Clear(); Assert.Empty(history.Entries); Assert.Equal(16, snapshot.Count);
        Assert.True(history.Record()); Assert.Equal("1. Heating preparation", history.Entries[0]);
    }
    [Fact]
    public void ResetCannotClearOtherCueOwners()
    {
        var heat = new RemedyHeatingCueHistory(); var rain = new RainCueHistory(); var wind = new WindCueHistory();
        rain.Record("Rain falling"); wind.Record("Wind blowing"); heat.Record(); heat.Clear();
        Assert.Single(rain.Entries); Assert.Single(wind.Entries); Assert.Empty(heat.Entries);
    }
}
