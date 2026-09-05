using Allrpcs.Domain.Discovery;
using Allrpcs.Domain.Identifiers;
using Allrpcs.Domain.Perception;

namespace Allrpcs.Domain.Tests.Discovery;

public sealed class FocusedSearchOutcomeApplicabilityPolicyTests
{
    [Fact]
    public void AllowsOutcomeForMatchingCharacterAndPresentTarget()
    {
        FocusedSearchOutcome outcome = Outcome(
            "seeker",
            "weathered-inscription",
            VisibilityState.Understood);
        CharacterPerceptionSnapshot perception = Snapshot(
            "seeker",
            Visibility("weathered-inscription", VisibilityState.Noticed));

        bool result = FocusedSearchOutcomeApplicabilityPolicy.CanApply(outcome, perception);

        Assert.True(result);
    }

    [Fact]
    public void RejectsOutcomeForDifferentCharacterEvenWhenTargetIsPresent()
    {
        FocusedSearchOutcome outcome = Outcome(
            "seeker",
            "weathered-inscription",
            VisibilityState.Understood);
        CharacterPerceptionSnapshot perception = Snapshot(
            "travelling-healer",
            Visibility("weathered-inscription", VisibilityState.Noticed));

        bool result = FocusedSearchOutcomeApplicabilityPolicy.CanApply(outcome, perception);

        Assert.False(result);
    }

    [Fact]
    public void RejectsOutcomeWhenTargetIsAbsent()
    {
        FocusedSearchOutcome outcome = Outcome(
            "seeker",
            "weathered-inscription",
            VisibilityState.Understood);
        CharacterPerceptionSnapshot perception = Snapshot(
            "seeker",
            Visibility("concealed-healing-herb", VisibilityState.Noticed));

        bool result = FocusedSearchOutcomeApplicabilityPolicy.CanApply(outcome, perception);

        Assert.False(result);
    }

    [Fact]
    public void FindsTheUniqueTargetAmongUnrelatedEntries()
    {
        ObjectId targetObjectId = new("weathered-inscription");
        WorldObjectVisibility target = new(targetObjectId, VisibilityState.Noticed);
        CharacterPerceptionSnapshot perception = Snapshot(
            "seeker",
            Visibility("wagon-residue", VisibilityState.Identified),
            target,
            Visibility("concealed-healing-herb", VisibilityState.Unnoticed));
        FocusedSearchOutcome outcome = new(
            new FocusedSearchRequest(new CharacterId("seeker"), targetObjectId),
            VisibilityState.Understood);

        bool result = FocusedSearchOutcomeApplicabilityPolicy.CanApply(outcome, perception);

        Assert.True(result);
        Assert.Same(targetObjectId, target.ObjectId);
    }

    [Fact]
    public void MatchesValueEqualDistinctCharacterAndObjectIdentifiers()
    {
        CharacterId outcomeCharacterId = new("seeker");
        ObjectId outcomeObjectId = new("weathered-inscription");
        CharacterId perceptionCharacterId = new("seeker");
        ObjectId perceptionObjectId = new("weathered-inscription");
        FocusedSearchOutcome outcome = new(
            new FocusedSearchRequest(outcomeCharacterId, outcomeObjectId),
            VisibilityState.Understood);
        CharacterPerceptionSnapshot perception = new(
            perceptionCharacterId,
            new[]
            {
                new WorldObjectVisibility(perceptionObjectId, VisibilityState.Noticed),
            });

        bool result = FocusedSearchOutcomeApplicabilityPolicy.CanApply(outcome, perception);

        Assert.True(result);
        Assert.NotSame(outcomeCharacterId, perceptionCharacterId);
        Assert.Equal(outcomeCharacterId, perceptionCharacterId);
        Assert.NotSame(outcomeObjectId, perceptionObjectId);
        Assert.Equal(outcomeObjectId, perceptionObjectId);
    }

    [Fact]
    public void DoesNotCompareCurrentAndResultingVisibilityStates()
    {
        foreach (VisibilityState currentState in VisibilityState.All)
        {
            CharacterPerceptionSnapshot perception = Snapshot(
                "seeker",
                Visibility("weathered-inscription", currentState));

            foreach (VisibilityState resultingState in VisibilityState.All)
            {
                FocusedSearchOutcome outcome = Outcome(
                    "seeker",
                    "weathered-inscription",
                    resultingState);

                Assert.True(
                    FocusedSearchOutcomeApplicabilityPolicy.CanApply(outcome, perception));
            }
        }
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
        WorldObjectVisibility unrelated = Visibility(
            "wagon-residue",
            VisibilityState.Identified);
        VisibilityState currentVisibilityState = VisibilityState.Noticed;
        WorldObjectVisibility target = new(targetObjectId, currentVisibilityState);
        CharacterPerceptionSnapshot perception = new(
            perceptionCharacterId,
            new[] { unrelated, target });

        _ = FocusedSearchOutcomeApplicabilityPolicy.CanApply(outcome, perception);

        Assert.Same(request, outcome.Request);
        Assert.Same(searcherCharacterId, outcome.Request.SearcherCharacterId);
        Assert.Same(targetObjectId, outcome.Request.TargetObjectId);
        Assert.Same(resultingVisibilityState, outcome.ResultingVisibilityState);
        Assert.Same(perceptionCharacterId, perception.CharacterId);
        Assert.Collection(
            perception.Entries,
            entry => Assert.Same(unrelated, entry),
            entry => Assert.Same(target, entry));
        Assert.Same(targetObjectId, target.ObjectId);
        Assert.Same(currentVisibilityState, target.VisibilityState);
    }

    [Fact]
    public void RejectsNullInputs()
    {
        FocusedSearchOutcome outcome = Outcome(
            "seeker",
            "weathered-inscription",
            VisibilityState.Understood);
        CharacterPerceptionSnapshot perception = Snapshot("seeker");

        Assert.Throws<ArgumentNullException>(
            () => FocusedSearchOutcomeApplicabilityPolicy.CanApply(null!, perception));
        Assert.Throws<ArgumentNullException>(
            () => FocusedSearchOutcomeApplicabilityPolicy.CanApply(outcome, null!));
    }

    [Fact]
    public void ExposesOnlyTheSinglePureApplicabilityOperation()
    {
        Type policyType = typeof(FocusedSearchOutcomeApplicabilityPolicy);
        System.Reflection.MethodInfo method = Assert.Single(
            policyType
                .GetMethods()
                .Where(candidate => candidate.DeclaringType == policyType));

        Assert.Equal("CanApply", method.Name);
        Assert.Equal(typeof(bool), method.ReturnType);
        Assert.True(method.IsStatic);
        Assert.Collection(
            method.GetParameters(),
            parameter => Assert.Equal(typeof(FocusedSearchOutcome), parameter.ParameterType),
            parameter => Assert.Equal(
                typeof(CharacterPerceptionSnapshot),
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
        new(
            new FocusedSearchRequest(
                new CharacterId(characterId),
                new ObjectId(targetObjectId)),
            resultingVisibilityState);

    private static CharacterPerceptionSnapshot Snapshot(
        string characterId,
        params WorldObjectVisibility[] entries) =>
        new(new CharacterId(characterId), entries);

    private static WorldObjectVisibility Visibility(
        string objectId,
        VisibilityState visibilityState) =>
        new(new ObjectId(objectId), visibilityState);
}
