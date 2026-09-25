using Allrpcs.Domain.Characters;

namespace Allrpcs.Domain.Tests.Characters;

public sealed class CharacterSabotageTurnOpportunityCadenceSignalTransitionTests
{
    [Fact]
    public void CreatesOneSignalWithDerivedExactProvenance()
    {
        CharacterSabotageTurnOpportunityCadenceSignalPolicyTests.Input input =
            CharacterSabotageTurnOpportunityCadenceSignalPolicyTests.CreateInput();

        CharacterSabotageTurnOpportunityCadenceSignal signal = Create(input);

        Assert.Same(input.Cadence, signal.Cadence);
        Assert.Same(input.CurrentEvent, signal.WorldEvent);
        Assert.Equal(2, signal.OccurredMeaningfulEventCount);
    }

    [Fact]
    public void RejectsAnEarlyCadenceBoundary()
    {
        CharacterSabotageTurnOpportunityCadenceSignalPolicyTests.Input input =
            CharacterSabotageTurnOpportunityCadenceSignalPolicyTests.CreateInput();

        Assert.Throws<InvalidOperationException>(
            () => Create(input with { Cadence = new CharacterSabotageTurnOpportunityCadence(3) }));
    }

    [Fact]
    public void ExposesOnlyOnePureOperation()
    {
        Type transitionType = typeof(CharacterSabotageTurnOpportunityCadenceSignalTransition);
        System.Reflection.MethodInfo method = Assert.Single(
            transitionType.GetMethods().Where(candidate => candidate.DeclaringType == transitionType));

        Assert.Equal("Create", method.Name);
        Assert.Equal(typeof(CharacterSabotageTurnOpportunityCadenceSignal), method.ReturnType);
        Assert.Empty(transitionType.GetConstructors());
        Assert.Empty(transitionType.GetProperties());
        Assert.Empty(transitionType.GetEvents());
        Assert.True(transitionType.IsAbstract);
        Assert.True(transitionType.IsSealed);
    }

    private static CharacterSabotageTurnOpportunityCadenceSignal Create(
        CharacterSabotageTurnOpportunityCadenceSignalPolicyTests.Input input) =>
        CharacterSabotageTurnOpportunityCadenceSignalTransition.Create(
            input.Cadence,
            input.CurrentEvent,
            input.WorldEvents,
            input.EventLog,
            input.WorldTime);
}
