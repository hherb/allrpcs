using Allrpcs.Domain.Discovery;
using Allrpcs.Domain.Identifiers;
using Allrpcs.Domain.Perception;

namespace Allrpcs.Domain.Tests.Discovery;

public sealed class FocusedSearchOutcomeRecordingPolicyTests
{
    [Fact]
    public void AllowsOutcomeWhenSnapshotIsEmptyOrRequestIsAbsent()
    {
        FocusedSearchOutcome outcome = Outcome(
            "seeker",
            "weathered-inscription",
            VisibilityState.Understood);
        FocusedSearchOutcome unrelated = Outcome(
            "travelling-healer",
            "concealed-healing-herb",
            VisibilityState.Identified);

        bool emptyResult = FocusedSearchOutcomeRecordingPolicy.CanRecord(
            outcome,
            Outcomes());
        bool absentResult = FocusedSearchOutcomeRecordingPolicy.CanRecord(
            outcome,
            Outcomes(unrelated));

        Assert.True(emptyResult);
        Assert.True(absentResult);
    }

    [Fact]
    public void AllowsBothNearMatchRequests()
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

        bool result = FocusedSearchOutcomeRecordingPolicy.CanRecord(
            outcome,
            Outcomes(sameSearcherDifferentTarget, differentSearcherSameTarget));

        Assert.True(result);
    }

    [Fact]
    public void RejectsTheExactRetainedRequestInstance()
    {
        FocusedSearchOutcome outcome = Outcome(
            "seeker",
            "weathered-inscription",
            VisibilityState.Understood);
        FocusedSearchOutcome retained = new(outcome.Request, VisibilityState.Identified);

        bool result = FocusedSearchOutcomeRecordingPolicy.CanRecord(
            outcome,
            Outcomes(retained));

        Assert.False(result);
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

        bool result = FocusedSearchOutcomeRecordingPolicy.CanRecord(
            outcome,
            Outcomes(retained));

        Assert.NotSame(outcome.Request, retained.Request);
        Assert.Equal(outcome.Request, retained.Request);
        Assert.False(result);
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

        _ = FocusedSearchOutcomeRecordingPolicy.CanRecord(outcome, outcomes);

        Assert.Same(request, outcome.Request);
        Assert.Same(searcherCharacterId, outcome.Request.SearcherCharacterId);
        Assert.Same(targetObjectId, outcome.Request.TargetObjectId);
        Assert.Same(resultingVisibilityState, outcome.ResultingVisibilityState);
        Assert.Collection(
            outcomes.Entries,
            entry => Assert.Same(before, entry),
            entry => Assert.Same(after, entry));
        Assert.Same(before.Request, outcomes.Entries[0].Request);
        Assert.Same(before.ResultingVisibilityState, outcomes.Entries[0].ResultingVisibilityState);
        Assert.Same(after.Request, outcomes.Entries[1].Request);
        Assert.Same(after.ResultingVisibilityState, outcomes.Entries[1].ResultingVisibilityState);
    }

    [Fact]
    public void RejectsNullInputsThroughDelegatedLookup()
    {
        FocusedSearchOutcome outcome = Outcome(
            "seeker",
            "weathered-inscription",
            VisibilityState.Understood);
        FocusedSearchOutcomeSnapshot outcomes = Outcomes();

        Assert.Throws<ArgumentNullException>(
            () => FocusedSearchOutcomeRecordingPolicy.CanRecord(null!, outcomes));
        Assert.Throws<ArgumentNullException>(
            () => FocusedSearchOutcomeRecordingPolicy.CanRecord(outcome, null!));
    }

    [Fact]
    public void ExposesOnlyTheSinglePureRecordingEligibilityOperation()
    {
        Type policyType = typeof(FocusedSearchOutcomeRecordingPolicy);
        System.Reflection.MethodInfo method = Assert.Single(
            policyType
                .GetMethods()
                .Where(candidate => candidate.DeclaringType == policyType));

        Assert.Equal("CanRecord", method.Name);
        Assert.Equal(typeof(bool), method.ReturnType);
        Assert.True(method.IsStatic);
        Assert.Collection(
            method.GetParameters(),
            parameter => Assert.Equal(typeof(FocusedSearchOutcome), parameter.ParameterType),
            parameter => Assert.Equal(
                typeof(FocusedSearchOutcomeSnapshot),
                parameter.ParameterType));
        Assert.Empty(policyType.GetConstructors());
        Assert.Empty(policyType.GetProperties());
        Assert.Empty(policyType.GetEvents());
        Assert.True(policyType.IsAbstract);
        Assert.True(policyType.IsSealed);
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
