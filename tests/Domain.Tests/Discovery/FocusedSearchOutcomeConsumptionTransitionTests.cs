using Allrpcs.Domain.Discovery;
using Allrpcs.Domain.Identifiers;
using Allrpcs.Domain.Perception;

namespace Allrpcs.Domain.Tests.Discovery;

public sealed class FocusedSearchOutcomeConsumptionTransitionTests
{
    [Fact]
    public void RejectsOutcomeWhenSnapshotIsEmptyOrRequestIsAbsent()
    {
        FocusedSearchOutcome outcome = Outcome(
            "seeker",
            "weathered-inscription",
            VisibilityState.Understood);
        FocusedSearchOutcome unrelated = Outcome(
            "travelling-healer",
            "concealed-healing-herb",
            VisibilityState.Identified);

        Assert.Throws<InvalidOperationException>(
            () => FocusedSearchOutcomeConsumptionTransition.Consume(outcome, Outcomes()));
        Assert.Throws<InvalidOperationException>(
            () => FocusedSearchOutcomeConsumptionTransition.Consume(
                outcome,
                Outcomes(unrelated)));
    }

    [Fact]
    public void RejectsSameRequestWithDifferentResultingVisibilityState()
    {
        FocusedSearchRequest request = Request("seeker", "weathered-inscription");
        FocusedSearchOutcome outcome = new(request, VisibilityState.Understood);
        FocusedSearchOutcome retained = new(request, VisibilityState.Identified);
        FocusedSearchOutcomeSnapshot outcomes = Outcomes(retained);

        Assert.Throws<InvalidOperationException>(
            () => FocusedSearchOutcomeConsumptionTransition.Consume(outcome, outcomes));
        Assert.Collection(outcomes.Entries, entry => Assert.Same(retained, entry));
    }

    [Fact]
    public void RejectsBothNearMatchRequests()
    {
        FocusedSearchOutcome outcome = Outcome(
            "seeker",
            "weathered-inscription",
            VisibilityState.Understood);
        FocusedSearchOutcome sameSearcherDifferentTarget = Outcome(
            "seeker",
            "concealed-healing-herb",
            VisibilityState.Understood);
        FocusedSearchOutcome differentSearcherSameTarget = Outcome(
            "travelling-healer",
            "weathered-inscription",
            VisibilityState.Understood);
        FocusedSearchOutcomeSnapshot outcomes = Outcomes(
            sameSearcherDifferentTarget,
            differentSearcherSameTarget);

        Assert.Throws<InvalidOperationException>(
            () => FocusedSearchOutcomeConsumptionTransition.Consume(outcome, outcomes));
        Assert.Collection(
            outcomes.Entries,
            entry => Assert.Same(sameSearcherDifferentTarget, entry),
            entry => Assert.Same(differentSearcherSameTarget, entry));
    }

    [Fact]
    public void ConsumesTheExactRetainedOutcomeInstance()
    {
        FocusedSearchOutcome outcome = Outcome(
            "seeker",
            "weathered-inscription",
            VisibilityState.Understood);

        FocusedSearchOutcomeSnapshot result =
            FocusedSearchOutcomeConsumptionTransition.Consume(outcome, Outcomes(outcome));

        Assert.Empty(result.Entries);
    }

    [Fact]
    public void ConsumesAValueEqualDistinctOutcome()
    {
        FocusedSearchOutcome outcome = Outcome(
            "seeker",
            "weathered-inscription",
            VisibilityState.Understood);
        FocusedSearchOutcome retained = Outcome(
            "seeker",
            "weathered-inscription",
            VisibilityState.Understood);

        FocusedSearchOutcomeSnapshot result =
            FocusedSearchOutcomeConsumptionTransition.Consume(outcome, Outcomes(retained));

        Assert.NotSame(outcome, retained);
        Assert.NotSame(outcome.Request, retained.Request);
        Assert.Equal(outcome, retained);
        Assert.Empty(result.Entries);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    public void RemovesOnlyTheEligibleOutcomeAndRetainsStableOrder(int targetIndex)
    {
        FocusedSearchOutcome outcome = Outcome(
            "seeker",
            "weathered-inscription",
            VisibilityState.Understood);
        FocusedSearchOutcome first = Outcome(
            "sage",
            "wagon-residue",
            VisibilityState.Noticed);
        FocusedSearchOutcome target = Outcome(
            "seeker",
            "weathered-inscription",
            VisibilityState.Understood);
        FocusedSearchOutcome third = Outcome(
            "travelling-healer",
            "concealed-healing-herb",
            VisibilityState.Identified);
        FocusedSearchOutcome[] entries = [first, target, third];
        (entries[targetIndex], entries[1]) = (entries[1], entries[targetIndex]);
        FocusedSearchOutcomeSnapshot outcomes = Outcomes(entries);

        FocusedSearchOutcomeSnapshot result =
            FocusedSearchOutcomeConsumptionTransition.Consume(outcome, outcomes);

        Assert.Equal(2, result.Entries.Count);
        Assert.Collection(
            result.Entries,
            entry => Assert.Same(entries.Where(entry => entry != target).ElementAt(0), entry),
            entry => Assert.Same(entries.Where(entry => entry != target).ElementAt(1), entry));
    }

    [Fact]
    public void LeavesEveryInputAndRetainedInstanceUnchanged()
    {
        CharacterId searcherCharacterId = new("seeker");
        ObjectId targetObjectId = new("weathered-inscription");
        FocusedSearchRequest request = new(searcherCharacterId, targetObjectId);
        VisibilityState resultingVisibilityState = VisibilityState.Understood;
        FocusedSearchOutcome outcome = new(request, resultingVisibilityState);
        FocusedSearchOutcome before = Outcome(
            "sage",
            "wagon-residue",
            VisibilityState.Noticed);
        FocusedSearchOutcome retained = Outcome(
            "seeker",
            "weathered-inscription",
            resultingVisibilityState);
        FocusedSearchOutcome after = Outcome(
            "travelling-healer",
            "concealed-healing-herb",
            VisibilityState.Identified);
        FocusedSearchOutcomeSnapshot outcomes = Outcomes(before, retained, after);

        FocusedSearchOutcomeSnapshot result =
            FocusedSearchOutcomeConsumptionTransition.Consume(outcome, outcomes);

        Assert.NotSame(outcomes, result);
        Assert.Same(request, outcome.Request);
        Assert.Same(searcherCharacterId, outcome.Request.SearcherCharacterId);
        Assert.Same(targetObjectId, outcome.Request.TargetObjectId);
        Assert.Same(resultingVisibilityState, outcome.ResultingVisibilityState);
        Assert.Collection(
            outcomes.Entries,
            entry => Assert.Same(before, entry),
            entry => Assert.Same(retained, entry),
            entry => Assert.Same(after, entry));
        Assert.Collection(
            result.Entries,
            entry => Assert.Same(before, entry),
            entry => Assert.Same(after, entry));
    }

    [Fact]
    public void RejectsNullInputsThroughConsumptionPolicy()
    {
        FocusedSearchOutcome outcome = Outcome(
            "seeker",
            "weathered-inscription",
            VisibilityState.Understood);
        FocusedSearchOutcomeSnapshot outcomes = Outcomes();

        Assert.Throws<ArgumentNullException>(
            () => FocusedSearchOutcomeConsumptionTransition.Consume(null!, outcomes));
        Assert.Throws<ArgumentNullException>(
            () => FocusedSearchOutcomeConsumptionTransition.Consume(outcome, null!));
    }

    [Fact]
    public void ExposesOnlyTheSinglePureConsumptionOperation()
    {
        Type transitionType = typeof(FocusedSearchOutcomeConsumptionTransition);
        System.Reflection.MethodInfo method = Assert.Single(
            transitionType
                .GetMethods()
                .Where(candidate => candidate.DeclaringType == transitionType));

        Assert.Equal("Consume", method.Name);
        Assert.Equal(typeof(FocusedSearchOutcomeSnapshot), method.ReturnType);
        Assert.True(method.IsStatic);
        Assert.Collection(
            method.GetParameters(),
            parameter => Assert.Equal(typeof(FocusedSearchOutcome), parameter.ParameterType),
            parameter => Assert.Equal(
                typeof(FocusedSearchOutcomeSnapshot),
                parameter.ParameterType));
        Assert.Empty(transitionType.GetConstructors());
        Assert.Empty(transitionType.GetProperties());
        Assert.Empty(transitionType.GetEvents());
        Assert.True(transitionType.IsAbstract);
        Assert.True(transitionType.IsSealed);
    }

    private static FocusedSearchOutcome Outcome(
        string characterId,
        string targetObjectId,
        VisibilityState resultingVisibilityState) =>
        new(Request(characterId, targetObjectId), resultingVisibilityState);

    private static FocusedSearchOutcomeSnapshot Outcomes(
        params FocusedSearchOutcome[] entries) =>
        new(entries);

    private static FocusedSearchRequest Request(
        string characterId,
        string targetObjectId) =>
        new(new CharacterId(characterId), new ObjectId(targetObjectId));
}
