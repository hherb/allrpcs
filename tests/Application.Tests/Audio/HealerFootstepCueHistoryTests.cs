using Allrpcs.Application.Audio;

namespace Allrpcs.Application.Tests.Audio;

public sealed class HealerFootstepCueHistoryTests
{
    private static readonly string[] ExpectedFirst = ["1. Footsteps on earth", "2. Footsteps on timber above/left"];
    [Fact]
    public void FirstContactRecordsThenOneSecondLimitDiscardsChangedWordsWithoutQueue()
    {
        HealerFootstepCueHistory history = new();
        Assert.True(history.Record("Footsteps on earth"));
        Assert.False(history.Record("Footsteps on gravel right"));
        for (int tick = 0; tick < 9; tick++) { Assert.True(history.Advance(0.1)); }
        Assert.False(history.Record("Footsteps in leaves"));
        Assert.True(history.Advance(0.1));
        Assert.True(history.Record("Footsteps on timber above/left"));
        Assert.Equal(ExpectedFirst, history.Entries);
    }

    [Fact]
    public void EvictionAndDetachedCopiesCannotMutatePriorOrReplyHistory()
    {
        HealerFootstepCueHistory history = new();
        PositionalCueHistory reply = new();
        reply.Record("Conversation nearby");
        for (int index = 0; index < 20; index++)
        {
            for (int tick = 0; tick < 10; tick++) { history.Advance(0.1); }
            Assert.True(history.Record("Footsteps on earth"));
        }
        Assert.Equal(16, history.Entries.Count);
        Assert.Equal("5. Footsteps on earth", history.Entries[0]);
        ((string[])history.Entries)[0] = "tampered";
        Assert.Equal("5. Footsteps on earth", history.Entries[0]);
        history.Clear();
        Assert.Empty(history.Entries);
        Assert.Single(reply.Entries);
        Assert.True(history.Record("Footsteps on earth"));
        Assert.Equal("1. Footsteps on earth", history.Entries[0]);
    }

    [Fact]
    public void StalledTimeAndNonGenericTextCannotLeakHiddenInformation()
    {
        HealerFootstepCueHistory history = new();
        Assert.False(history.Record("Healer at 1000,350"));
        Assert.True(history.Record("Footsteps on earth"));
        Assert.False(history.Advance(1));
        Assert.False(history.Advance(double.NaN));
        Assert.False(history.Record("Footsteps on earth right"));
        Assert.Single(history.Entries);
    }
}
