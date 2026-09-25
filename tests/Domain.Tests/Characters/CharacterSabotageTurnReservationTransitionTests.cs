using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Dialogue;

namespace Allrpcs.Domain.Tests.Characters;

public sealed class CharacterSabotageTurnReservationTransitionTests
{
    [Fact]
    public void ReservesTheExactCurrentOpportunity()
    {
        CharacterSabotageTurnOpportunity opportunity =
            CharacterSabotageTurnReservationTests.CreateOpportunity();

        CharacterSabotageTurnReservation reservation =
            CharacterSabotageTurnReservationTransition.Reserve(
                opportunity,
                opportunity,
                null,
                null);

        Assert.Same(opportunity, reservation.Opportunity);
    }

    [Fact]
    public void RejectsActiveAndRepeatedReservation()
    {
        CharacterSabotageTurnOpportunity opportunity =
            CharacterSabotageTurnReservationTests.CreateOpportunity();
        CharacterTurnActivity active = CharacterSabotageTurnReservationPolicyTests.CreateTurn(
            CharacterTurnActivityState.AwaitingResponse);
        CharacterSabotageTurnReservation existing = new(opportunity);

        Assert.Throws<InvalidOperationException>(() =>
            CharacterSabotageTurnReservationTransition.Reserve(
                opportunity,
                opportunity,
                active,
                null));
        Assert.Throws<InvalidOperationException>(() =>
            CharacterSabotageTurnReservationTransition.Reserve(
                opportunity,
                opportunity,
                null,
                existing));
    }

    [Fact]
    public void ExposesOnlyOnePureOperation()
    {
        Type transitionType = typeof(CharacterSabotageTurnReservationTransition);
        System.Reflection.MethodInfo method = Assert.Single(
            transitionType.GetMethods().Where(candidate => candidate.DeclaringType == transitionType));

        Assert.Equal("Reserve", method.Name);
        Assert.Equal(typeof(CharacterSabotageTurnReservation), method.ReturnType);
        Assert.Empty(transitionType.GetConstructors());
        Assert.Empty(transitionType.GetProperties());
        Assert.Empty(transitionType.GetEvents());
        Assert.True(transitionType.IsAbstract);
        Assert.True(transitionType.IsSealed);
    }
}
