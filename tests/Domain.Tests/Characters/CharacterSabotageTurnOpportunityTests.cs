using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Identifiers;
using Allrpcs.Domain.WorldEvents;

namespace Allrpcs.Domain.Tests.Characters;

public sealed class CharacterSabotageTurnOpportunityTests
{
    [Fact]
    public void RetainsExactActorAndCadenceSignal()
    {
        Character actor = RepresentativeEnemyAgentCharacterSource.Create();
        CharacterSabotageTurnOpportunityCadence cadence = new(2);
        WorldEvent worldEvent = new(new WorldEventId("second-event"), WorldEventTimeEffect.Advance);
        CharacterSabotageTurnOpportunityCadenceSignal signal = new(cadence, worldEvent, 2);

        CharacterSabotageTurnOpportunity opportunity = new(actor, signal);

        Assert.Same(actor, opportunity.Actor);
        Assert.Same(signal, opportunity.Signal);
    }

    [Fact]
    public void RejectsNulls()
    {
        Character actor = RepresentativeEnemyAgentCharacterSource.Create();
        CharacterSabotageTurnOpportunityCadence cadence = new(2);
        WorldEvent worldEvent = new(new WorldEventId("second-event"), WorldEventTimeEffect.Advance);
        CharacterSabotageTurnOpportunityCadenceSignal signal = new(cadence, worldEvent, 2);

        Assert.Throws<ArgumentNullException>(() => new CharacterSabotageTurnOpportunity(null!, signal));
        Assert.Throws<ArgumentNullException>(() => new CharacterSabotageTurnOpportunity(actor, null!));
    }
}
