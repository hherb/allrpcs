using Allrpcs.Domain.Discovery;
using Allrpcs.Domain.Identifiers;
using Allrpcs.Domain.Perception;

namespace Allrpcs.Domain.Tests.Discovery;

public sealed class FocusedSearchOutcomeRecordingTransitionTests
{
    [Fact]
    public void RecordsOutcomeWhenSnapshotIsEmptyOrRequestIsAbsent()
    {
        FocusedSearchOutcome outcome = Outcome(
            "seeker",
            "weathered-inscription",
            VisibilityState.Understood);
        FocusedSearchOutcome unrelated = Outcome(
            "travelling-healer",
            "concealed-healing-herb",
            VisibilityState.Identified);

        FocusedSearchOutcomeSnapshot emptyResult =
            FocusedSearchOutcomeRecordingTransition.Record(outcome, Outcomes());
        FocusedSearchOutcomeSnapshot absentResult =
            FocusedSearchOutcomeRecordingTransition.Record(outcome, Outcomes(unrelated));

        Assert.Collection(emptyResult.Entries, entry => Assert.Same(outcome, entry));
        Assert.Collection(
            absentResult.Entries,
            entry => Assert.Same(unrelated, entry),
            entry => Assert.Same(outcome, entry));
    }

    [Fact]
    public void RecordsOutcomeForBothNearMatchRequests()
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

        FocusedSearchOutcomeSnapshot result = FocusedSearchOutcomeRecordingTransition.Record(
            outcome,
            Outcomes(sameSearcherDifferentTarget, differentSearcherSameTarget));

        Assert.Collection(
            result.Entries,
            entry => Assert.Same(sameSearcherDifferentTarget, entry),
            entry => Assert.Same(differentSearcherSameTarget, entry),
            entry => Assert.Same(outcome, entry));
    }

    [Fact]
    public void RejectsTheExactRetainedRequestInstance()
    {
        FocusedSearchOutcome outcome = Outcome(
            "seeker",
            "weathered-inscription",
            VisibilityState.Understood);
        FocusedSearchOutcome retained = new(outcome.Request, VisibilityState.Identified);
        FocusedSearchOutcomeSnapshot outcomes = Outcomes(retained);

        Assert.Throws<InvalidOperationException>(
            () => FocusedSearchOutcomeRecordingTransition.Record(outcome, outcomes));
        Assert.Collection(outcomes.Entries, entry => Assert.Same(retained, entry));
    }

    [Fact]
    public void RejectsAValueEqualDistinctRetainedRequest()
    {
        FocusedSearchOutcome outcome = Outcome(
            "seeker",
            "weathered-inscription",
            VisibilityState.Understood);
        FocusedSearchOutcome retained = Outcome(
            "seeker",
            "weathered-inscription",
            VisibilityState.Noticed);
        FocusedSearchOutcomeSnapshot outcomes = Outcomes(retained);

        Assert.NotSame(outcome.Request, retained.Request);
        Assert.Equal(outcome.Request, retained.Request);
        Assert.Throws<InvalidOperationException>(
            () => FocusedSearchOutcomeRecordingTransition.Record(outcome, outcomes));
        Assert.Collection(outcomes.Entries, entry => Assert.Same(retained, entry));
    }

    [Fact]
    public void RetainsStableOrderAndAppendsTheExactOutcomeInstance()
    {
        FocusedSearchOutcome outcome = Outcome(
            "seeker",
            "weathered-inscription",
            VisibilityState.Understood);
        FocusedSearchOutcome first = Outcome(
            "sage",
            "wagon-residue",
            VisibilityState.Noticed);
        FocusedSearchOutcome second = Outcome(
            "travelling-healer",
            "concealed-healing-herb",
            VisibilityState.Identified);

        FocusedSearchOutcomeSnapshot result = FocusedSearchOutcomeRecordingTransition.Record(
            outcome,
            Outcomes(first, second));

        Assert.Collection(
            result.Entries,
            entry => Assert.Same(first, entry),
            entry => Assert.Same(second, entry),
            entry => Assert.Same(outcome, entry));
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
        FocusedSearchOutcome after = Outcome(
            "travelling-healer",
            "concealed-healing-herb",
            VisibilityState.Identified);
        FocusedSearchOutcomeSnapshot outcomes = Outcomes(before, after);

        FocusedSearchOutcomeSnapshot result =
            FocusedSearchOutcomeRecordingTransition.Record(outcome, outcomes);

        Assert.NotSame(outcomes, result);
        Assert.Same(request, outcome.Request);
        Assert.Same(searcherCharacterId, outcome.Request.SearcherCharacterId);
        Assert.Same(targetObjectId, outcome.Request.TargetObjectId);
        Assert.Same(resultingVisibilityState, outcome.ResultingVisibilityState);
        Assert.Collection(
            outcomes.Entries,
            entry => Assert.Same(before, entry),
            entry => Assert.Same(after, entry));
        Assert.Collection(
            result.Entries,
            entry => Assert.Same(before, entry),
            entry => Assert.Same(after, entry),
            entry => Assert.Same(outcome, entry));
    }

    [Fact]
    public void RejectsNullInputsThroughRecordingPolicy()
    {
        FocusedSearchOutcome outcome = Outcome(
            "seeker",
            "weathered-inscription",
            VisibilityState.Understood);
        FocusedSearchOutcomeSnapshot outcomes = Outcomes();

        Assert.Throws<ArgumentNullException>(
            () => FocusedSearchOutcomeRecordingTransition.Record(null!, outcomes));
        Assert.Throws<ArgumentNullException>(
            () => FocusedSearchOutcomeRecordingTransition.Record(outcome, null!));
    }

    [Fact]
    public void ExposesOnlyTheSinglePureRecordingOperation()
    {
        Type transitionType = typeof(FocusedSearchOutcomeRecordingTransition);
        System.Reflection.MethodInfo method = Assert.Single(
            transitionType
                .GetMethods()
                .Where(candidate => candidate.DeclaringType == transitionType));

        Assert.Equal("Record", method.Name);
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
