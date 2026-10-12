using Allrpcs.Application.Audio;

namespace Allrpcs.Application.Tests.Audio;

public sealed class RemedyCoolingCueHistoryTests
{
    private static readonly string[] FirstEntries = ["1. Cooling step committed", "2. Cooling step committed"];
    [Fact]
    public void UniqueAdmittedCommitsHaveNoTimeLimiterAndOnlyAnonymousText()
    {
        var history = new RemedyCoolingCueHistory();
        Assert.True(history.Record()); Assert.True(history.Record());
        Assert.Equal(FirstEntries, history.Entries);
    }
    [Fact]
    public void CapacityEvictsOldestAndDetachedSnapshotsCannotMutateFutureHistory()
    {
        var history = new RemedyCoolingCueHistory();
        for (int i = 0; i < 20; i++) { Assert.True(history.Record()); }
        var snapshot = history.Entries;
        Assert.Equal(16, snapshot.Count); Assert.Equal("5. Cooling step committed", snapshot[0]);
        Assert.Throws<NotSupportedException>(() => ((IList<string>)snapshot)[0] = "modified");
        history.Clear(); Assert.Empty(history.Entries); Assert.Equal(16, snapshot.Count);
        Assert.True(history.Record()); Assert.Equal("1. Cooling step committed", history.Entries[0]);
    }
    [Fact]
    public void ResetCannotClearOtherCueOwners()
    {
        var cooling = new RemedyCoolingCueHistory(); var steeping = new RemedySteepingCueHistory(); var heat = new RemedyHeatingCueHistory(); var rain = new RainCueHistory(); var wind = new WindCueHistory();
        rain.Record("Rain falling"); wind.Record("Wind blowing"); heat.Record(); steeping.Record(); cooling.Record(); cooling.Clear();
        Assert.Single(rain.Entries); Assert.Single(wind.Entries); Assert.Single(heat.Entries); Assert.Single(steeping.Entries); Assert.Empty(cooling.Entries);
    }
}
