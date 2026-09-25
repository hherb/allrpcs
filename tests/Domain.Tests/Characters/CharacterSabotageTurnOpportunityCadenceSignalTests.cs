using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Identifiers;
using Allrpcs.Domain.WorldEvents;

namespace Allrpcs.Domain.Tests.Characters;

public sealed class CharacterSabotageTurnOpportunityCadenceSignalTests
{
    [Fact]
    public void RetainsExactCadenceEventAndMeaningfulEventCount()
    {
        CharacterSabotageTurnOpportunityCadence cadence = new(2);
        WorldEvent worldEvent = new(new WorldEventId("second-event"), WorldEventTimeEffect.Advance);

        CharacterSabotageTurnOpportunityCadenceSignal signal = new(cadence, worldEvent, 2);

        Assert.Same(cadence, signal.Cadence);
        Assert.Same(worldEvent, signal.WorldEvent);
        Assert.Equal(2, signal.OccurredMeaningfulEventCount);
    }

    [Fact]
    public void RejectsNullsAndNonPositiveCounts()
    {
        CharacterSabotageTurnOpportunityCadence cadence = new(2);
        WorldEvent worldEvent = new(new WorldEventId("second-event"), WorldEventTimeEffect.Advance);

        Assert.Throws<ArgumentNullException>(
            () => new CharacterSabotageTurnOpportunityCadenceSignal(null!, worldEvent, 2));
        Assert.Throws<ArgumentNullException>(
            () => new CharacterSabotageTurnOpportunityCadenceSignal(cadence, null!, 2));
        Assert.Throws<ArgumentOutOfRangeException>(
            () => new CharacterSabotageTurnOpportunityCadenceSignal(cadence, worldEvent, 0));
    }
}
