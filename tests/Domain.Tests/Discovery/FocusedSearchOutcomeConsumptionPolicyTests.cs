using Allrpcs.Domain.Discovery;
using Allrpcs.Domain.Identifiers;
using Allrpcs.Domain.Perception;

namespace Allrpcs.Domain.Tests.Discovery;

public sealed class FocusedSearchOutcomeConsumptionPolicyTests
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

        bool emptyResult = FocusedSearchOutcomeConsumptionPolicy.CanConsume(
            outcome,
            Outcomes());
        bool absentResult = FocusedSearchOutcomeConsumptionPolicy.CanConsume(
            outcome,
            Outcomes(unrelated));

        Assert.False(emptyResult);
        Assert.False(absentResult);
    }

    [Fact]
    public void RejectsSameRequestWithDifferentResultingVisibilityState()
    {
        FocusedSearchRequest request = Request("seeker", "weathered-inscription");
        FocusedSearchOutcome outcome = new(request, VisibilityState.Understood);
        FocusedSearchOutcome retained = new(request, VisibilityState.Identified);

        bool result = FocusedSearchOutcomeConsumptionPolicy.CanConsume(
            outcome,
            Outcomes(retained));

        Assert.False(result);
    }

    [Fact]
    public void AllowsTheExactRetainedOutcomeInstance()
    {
        FocusedSearchOutcome outcome = Outcome(
            "seeker",
            "weathered-inscription",
            VisibilityState.Understood);

        bool result = FocusedSearchOutcomeConsumptionPolicy.CanConsume(
            outcome,
            Outcomes(outcome));

        Assert.True(result);
    }

    [Fact]
    public void AllowsAValueEqualDistinctOutcome()
    {
        FocusedSearchOutcome outcome = Outcome(
            "seeker",
            "weathered-inscription",
            VisibilityState.Understood);
        FocusedSearchOutcome retained = Outcome(
            "seeker",
            "weathered-inscription",
            VisibilityState.Understood);

        bool result = FocusedSearchOutcomeConsumptionPolicy.CanConsume(
            outcome,
            Outcomes(retained));

        Assert.NotSame(outcome, retained);
        Assert.NotSame(outcome.Request, retained.Request);
        Assert.Equal(outcome, retained);
        Assert.True(result);
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

        bool result = FocusedSearchOutcomeConsumptionPolicy.CanConsume(
            outcome,
            Outcomes(sameSearcherDifferentTarget, differentSearcherSameTarget));

        Assert.False(result);
    }

    [Fact]
    public void SelectsTheMatchingOutcomeAmongUnrelatedEntries()
    {
        FocusedSearchOutcome outcome = Outcome(
            "seeker",
            "weathered-inscription",
            VisibilityState.Understood);
        FocusedSearchOutcome before = Outcome(
            "sage",
            "wagon-residue",
            VisibilityState.Noticed);
        FocusedSearchOutcome retained = Outcome(
            "seeker",
            "weathered-inscription",
            VisibilityState.Understood);
        FocusedSearchOutcome after = Outcome(
            "travelling-healer",
            "concealed-healing-herb",
            VisibilityState.Identified);

        bool result = FocusedSearchOutcomeConsumptionPolicy.CanConsume(
            outcome,
            Outcomes(before, retained, after));

        Assert.True(result);
    }

    [Fact]
    public void LeavesEveryInputAndRetainedInstanceUnchanged()
    {
        CharacterId searcherCharacterId = new("seeker");
        ObjectId targetObjectId = new("weathered-inscription");
        FocusedSearchRequest request = new(searcherCharacterId, targetObjectId);
        VisibilityState resultingVisibilityState = VisibilityState.Understood;
        FocusedSearchOutcome outcome = new(request, resultingVisibilityState);
        FocusedSearchOutcome unrelated = Outcome(
            "sage",
            "wagon-residue",
            VisibilityState.Noticed);
        FocusedSearchOutcome retained = Outcome(
            "seeker",
            "weathered-inscription",
            resultingVisibilityState);
        FocusedSearchOutcomeSnapshot outcomes = Outcomes(unrelated, retained);

        _ = FocusedSearchOutcomeConsumptionPolicy.CanConsume(outcome, outcomes);

        Assert.Same(request, outcome.Request);
        Assert.Same(searcherCharacterId, outcome.Request.SearcherCharacterId);
        Assert.Same(targetObjectId, outcome.Request.TargetObjectId);
        Assert.Same(resultingVisibilityState, outcome.ResultingVisibilityState);
        Assert.Collection(
            outcomes.Entries,
            entry => Assert.Same(unrelated, entry),
            entry => Assert.Same(retained, entry));
        Assert.Same(retained.Request, outcomes.Entries[1].Request);
        Assert.Same(retained.ResultingVisibilityState, outcomes.Entries[1].ResultingVisibilityState);
    }

    [Fact]
    public void RejectsNullInputs()
    {
        FocusedSearchOutcome outcome = Outcome(
            "seeker",
            "weathered-inscription",
            VisibilityState.Understood);
        FocusedSearchOutcomeSnapshot outcomes = Outcomes();

        Assert.Throws<ArgumentNullException>(
            () => FocusedSearchOutcomeConsumptionPolicy.CanConsume(null!, outcomes));
        Assert.Throws<ArgumentNullException>(
            () => FocusedSearchOutcomeConsumptionPolicy.CanConsume(outcome, null!));
    }

    [Fact]
    public void ExposesOnlyTheSinglePureConsumptionEligibilityOperation()
    {
        Type policyType = typeof(FocusedSearchOutcomeConsumptionPolicy);
        System.Reflection.MethodInfo method = Assert.Single(
            policyType
                .GetMethods()
                .Where(candidate => candidate.DeclaringType == policyType));

        Assert.Equal("CanConsume", method.Name);
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
