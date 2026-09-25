using Allrpcs.Domain.Characters;

namespace Allrpcs.Domain.Tests.Characters;

public sealed class CharacterSabotageTurnReservationTests
{
    [Fact]
    public void RetainsTheExactOpportunity()
    {
        CharacterSabotageTurnOpportunity opportunity = CreateOpportunity();

        CharacterSabotageTurnReservation reservation = new(opportunity);

        Assert.Same(opportunity, reservation.Opportunity);
    }

    [Fact]
    public void RejectsNullOpportunity()
    {
        Assert.Throws<ArgumentNullException>(() => new CharacterSabotageTurnReservation(null!));
    }

    internal static CharacterSabotageTurnOpportunity CreateOpportunity()
    {
        Character actor = RepresentativeEnemyAgentCharacterSource.Create();
        CharacterSabotageTurnOpportunityCadence cadence = new(2);
        return new CharacterSabotageTurnOpportunity(
            actor,
            new CharacterSabotageTurnOpportunityCadenceSignal(
                cadence,
                new(
                    new Allrpcs.Domain.Identifiers.WorldEventId("second-event"),
                    Allrpcs.Domain.WorldEvents.WorldEventTimeEffect.Advance),
                2));
    }
}
