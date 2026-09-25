using Allrpcs.Domain.Characters;

namespace Allrpcs.Domain.Tests.Characters;

public sealed class CharacterSabotageTurnOpportunityTransitionTests
{
    [Fact]
    public void CreatesOneExactOpportunityForAnEligibleSignal()
    {
        CharacterSabotageTurnOpportunityPolicyTests.Input input =
            CharacterSabotageTurnOpportunityPolicyTests.CreateInput();

        CharacterSabotageTurnOpportunity opportunity = Create(input);

        Assert.Same(input.Actor, opportunity.Actor);
        Assert.Same(input.Signal, opportunity.Signal);
    }

    [Fact]
    public void RejectsIneligibleAndRepeatedSignals()
    {
        CharacterSabotageTurnOpportunityPolicyTests.Input input =
            CharacterSabotageTurnOpportunityPolicyTests.CreateInput();
        CharacterSabotageTurnOpportunity first = Create(input);

        Assert.Throws<InvalidOperationException>(
            () => Create(input with { CurrentActor = RepresentativeEnemyAgentCharacterSource.Create() }));
        Assert.Throws<InvalidOperationException>(
            () => Create(input with { CurrentOpportunity = first }));
    }

    [Fact]
    public void ExposesOnlyOnePureOperation()
    {
        Type transitionType = typeof(CharacterSabotageTurnOpportunityTransition);
        System.Reflection.MethodInfo method = Assert.Single(
            transitionType.GetMethods().Where(candidate => candidate.DeclaringType == transitionType));

        Assert.Equal("Create", method.Name);
        Assert.Equal(typeof(CharacterSabotageTurnOpportunity), method.ReturnType);
        Assert.Empty(transitionType.GetConstructors());
        Assert.Empty(transitionType.GetProperties());
        Assert.Empty(transitionType.GetEvents());
        Assert.True(transitionType.IsAbstract);
        Assert.True(transitionType.IsSealed);
    }

    private static CharacterSabotageTurnOpportunity Create(
        CharacterSabotageTurnOpportunityPolicyTests.Input input) =>
        CharacterSabotageTurnOpportunityTransition.Create(
            input.Actor,
            input.CurrentActor,
            input.Signal,
            input.Cadence,
            input.WorldEvents,
            input.EventLog,
            input.WorldTime,
            input.CurrentOpportunity);
}
