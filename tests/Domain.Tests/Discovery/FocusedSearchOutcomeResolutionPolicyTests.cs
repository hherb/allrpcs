using Allrpcs.Domain.Discovery;
using Allrpcs.Domain.Identifiers;
using Allrpcs.Domain.Perception;

namespace Allrpcs.Domain.Tests.Discovery;

public sealed class FocusedSearchOutcomeResolutionPolicyTests
{
    [Fact]
    public void RejectsWrongCharacter()
    {
        FocusedSearchOutcome outcome = Outcome(
            "seeker",
            "weathered-inscription",
            VisibilityState.Understood);
        CharacterPerceptionSnapshot perception = Perception(
            "travelling-healer",
            Visibility("weathered-inscription", VisibilityState.Noticed));

        bool result = FocusedSearchOutcomeResolutionPolicy.CanResolve(
            outcome,
            perception,
            Outcomes(outcome));

        Assert.False(result);
    }

    [Fact]
    public void RejectsAbsentTarget()
    {
        FocusedSearchOutcome outcome = Outcome(
            "seeker",
            "weathered-inscription",
            VisibilityState.Understood);
        CharacterPerceptionSnapshot perception = Perception(
            "seeker",
            Visibility("wagon-residue", VisibilityState.Noticed));

        bool result = FocusedSearchOutcomeResolutionPolicy.CanResolve(
            outcome,
            perception,
            Outcomes(outcome));

        Assert.False(result);
    }

    [Fact]
    public void RejectsUnchangedVisibilityState()
    {
        FocusedSearchOutcome outcome = Outcome(
            "seeker",
            "weathered-inscription",
            VisibilityState.Understood);
        CharacterPerceptionSnapshot perception = Perception(
            "seeker",
            Visibility("weathered-inscription", VisibilityState.Understood));

        bool result = FocusedSearchOutcomeResolutionPolicy.CanResolve(
            outcome,
            perception,
            Outcomes(outcome));

        Assert.False(result);
    }

    [Fact]
    public void RejectsAbsentRecordedOutcome()
    {
        FocusedSearchOutcome outcome = Outcome(
            "seeker",
            "weathered-inscription",
            VisibilityState.Understood);
        CharacterPerceptionSnapshot perception = Perception(
            "seeker",
            Visibility("weathered-inscription", VisibilityState.Noticed));

        bool result = FocusedSearchOutcomeResolutionPolicy.CanResolve(
            outcome,
            perception,
            Outcomes());

        Assert.False(result);
    }

    [Fact]
    public void RejectsSameRecordedRequestWithDifferentResultingVisibilityState()
    {
        FocusedSearchRequest request = Request("seeker", "weathered-inscription");
        FocusedSearchOutcome outcome = new(request, VisibilityState.Understood);
        FocusedSearchOutcome retained = new(request, VisibilityState.Identified);
        CharacterPerceptionSnapshot perception = Perception(
            "seeker",
            Visibility("weathered-inscription", VisibilityState.Noticed));

        bool result = FocusedSearchOutcomeResolutionPolicy.CanResolve(
            outcome,
            perception,
            Outcomes(retained));

        Assert.False(result);
    }

    [Fact]
    public void RejectsNearMatchRecordedRequests()
    {
        FocusedSearchOutcome outcome = Outcome(
            "seeker",
            "weathered-inscription",
            VisibilityState.Understood);
        CharacterPerceptionSnapshot perception = Perception(
            "seeker",
            Visibility("weathered-inscription", VisibilityState.Noticed));
        FocusedSearchOutcome sameSearcherDifferentTarget = Outcome(
            "seeker",
            "concealed-healing-herb",
            VisibilityState.Understood);
        FocusedSearchOutcome differentSearcherSameTarget = Outcome(
            "travelling-healer",
            "weathered-inscription",
            VisibilityState.Understood);

        bool result = FocusedSearchOutcomeResolutionPolicy.CanResolve(
            outcome,
            perception,
            Outcomes(sameSearcherDifferentTarget, differentSearcherSameTarget));

        Assert.False(result);
    }

    [Fact]
    public void AllowsTheExactRecordedOutcomeInstanceWhenPerceptionWouldChange()
    {
        FocusedSearchOutcome outcome = Outcome(
            "seeker",
            "weathered-inscription",
            VisibilityState.Understood);
        CharacterPerceptionSnapshot perception = Perception(
            "seeker",
            Visibility("weathered-inscription", VisibilityState.Noticed));

        bool result = FocusedSearchOutcomeResolutionPolicy.CanResolve(
            outcome,
            perception,
            Outcomes(outcome));

        Assert.True(result);
    }

    [Fact]
    public void AllowsAValueEqualRecordedOutcomeWhenPerceptionWouldChange()
    {
        FocusedSearchOutcome outcome = Outcome(
            "seeker",
            "weathered-inscription",
            VisibilityState.Understood);
        FocusedSearchOutcome retained = Outcome(
            "seeker",
            "weathered-inscription",
            VisibilityState.Understood);
        CharacterPerceptionSnapshot perception = Perception(
            "seeker",
            Visibility("weathered-inscription", VisibilityState.Noticed));

        bool result = FocusedSearchOutcomeResolutionPolicy.CanResolve(
            outcome,
            perception,
            Outcomes(retained));

        Assert.NotSame(outcome, retained);
        Assert.NotSame(outcome.Request, retained.Request);
        Assert.Equal(outcome, retained);
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
        CharacterId perceptionCharacterId = new("seeker");
        WorldObjectVisibility unrelatedVisibility = Visibility(
            "wagon-residue",
            VisibilityState.Identified);
        VisibilityState currentVisibilityState = VisibilityState.Noticed;
        WorldObjectVisibility targetVisibility = new(targetObjectId, currentVisibilityState);
        CharacterPerceptionSnapshot perception = new(
            perceptionCharacterId,
            new[] { unrelatedVisibility, targetVisibility });
        FocusedSearchOutcome unrelatedOutcome = Outcome(
            "sage",
            "concealed-healing-herb",
            VisibilityState.Noticed);
        FocusedSearchOutcome retainedOutcome = Outcome(
            "seeker",
            "weathered-inscription",
            resultingVisibilityState);
        FocusedSearchOutcomeSnapshot outcomes = Outcomes(unrelatedOutcome, retainedOutcome);

        _ = FocusedSearchOutcomeResolutionPolicy.CanResolve(outcome, perception, outcomes);

        Assert.Same(request, outcome.Request);
        Assert.Same(searcherCharacterId, outcome.Request.SearcherCharacterId);
        Assert.Same(targetObjectId, outcome.Request.TargetObjectId);
        Assert.Same(resultingVisibilityState, outcome.ResultingVisibilityState);
        Assert.Same(perceptionCharacterId, perception.CharacterId);
        Assert.Collection(
            perception.Entries,
            entry => Assert.Same(unrelatedVisibility, entry),
            entry => Assert.Same(targetVisibility, entry));
        Assert.Same(targetObjectId, targetVisibility.ObjectId);
        Assert.Same(currentVisibilityState, targetVisibility.VisibilityState);
        Assert.Collection(
            outcomes.Entries,
            entry => Assert.Same(unrelatedOutcome, entry),
            entry => Assert.Same(retainedOutcome, entry));
        Assert.Same(retainedOutcome.Request, outcomes.Entries[1].Request);
        Assert.Same(
            retainedOutcome.ResultingVisibilityState,
            outcomes.Entries[1].ResultingVisibilityState);
    }

    [Fact]
    public void RejectsNullInputsThroughDelegatedPolicies()
    {
        FocusedSearchOutcome outcome = Outcome(
            "seeker",
            "weathered-inscription",
            VisibilityState.Understood);
        CharacterPerceptionSnapshot perception = Perception(
            "seeker",
            Visibility("weathered-inscription", VisibilityState.Noticed));
        FocusedSearchOutcomeSnapshot outcomes = Outcomes(outcome);

        Assert.Throws<ArgumentNullException>(
            () => FocusedSearchOutcomeResolutionPolicy.CanResolve(
                null!,
                perception,
                outcomes));
        Assert.Throws<ArgumentNullException>(
            () => FocusedSearchOutcomeResolutionPolicy.CanResolve(
                outcome,
                null!,
                outcomes));
        Assert.Throws<ArgumentNullException>(
            () => FocusedSearchOutcomeResolutionPolicy.CanResolve(
                outcome,
                perception,
                null!));
    }

    [Fact]
    public void ExposesOnlyTheSinglePureResolutionEligibilityOperation()
    {
        Type policyType = typeof(FocusedSearchOutcomeResolutionPolicy);
        System.Reflection.MethodInfo method = Assert.Single(
            policyType
                .GetMethods()
                .Where(candidate => candidate.DeclaringType == policyType));

        Assert.Equal("CanResolve", method.Name);
        Assert.Equal(typeof(bool), method.ReturnType);
        Assert.True(method.IsStatic);
        Assert.Collection(
            method.GetParameters(),
            parameter => Assert.Equal(typeof(FocusedSearchOutcome), parameter.ParameterType),
            parameter => Assert.Equal(
                typeof(CharacterPerceptionSnapshot),
                parameter.ParameterType),
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

    private static CharacterPerceptionSnapshot Perception(
        string characterId,
        params WorldObjectVisibility[] entries) =>
        new(new CharacterId(characterId), entries);

    private static FocusedSearchOutcomeSnapshot Outcomes(
        params FocusedSearchOutcome[] entries) =>
        new(entries);

    private static FocusedSearchRequest Request(
        string characterId,
        string targetObjectId) =>
        new(new CharacterId(characterId), new ObjectId(targetObjectId));

    private static WorldObjectVisibility Visibility(
        string objectId,
        VisibilityState visibilityState) =>
        new(new ObjectId(objectId), visibilityState);
}
