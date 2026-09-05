using Allrpcs.Domain.Discovery;
using Allrpcs.Domain.Identifiers;
using Allrpcs.Domain.Perception;

namespace Allrpcs.Domain.Tests.Discovery;

public sealed class FocusedSearchOutcomeChangePolicyTests
{
    [Fact]
    public void RejectsEveryEqualCurrentAndResultingVisibilityState()
    {
        foreach (VisibilityState state in VisibilityState.All)
        {
            FocusedSearchOutcome outcome = Outcome(
                "seeker",
                "weathered-inscription",
                state);
            CharacterPerceptionSnapshot perception = Snapshot(
                "seeker",
                Visibility("weathered-inscription", state));

            Assert.False(FocusedSearchOutcomeChangePolicy.WouldChange(outcome, perception));
        }
    }

    [Fact]
    public void AllowsEveryDistinctCurrentAndResultingVisibilityStatePair()
    {
        foreach (VisibilityState currentState in VisibilityState.All)
        {
            CharacterPerceptionSnapshot perception = Snapshot(
                "seeker",
                Visibility("weathered-inscription", currentState));

            foreach (VisibilityState resultingState in VisibilityState.All)
            {
                if (currentState == resultingState)
                {
                    continue;
                }

                FocusedSearchOutcome outcome = Outcome(
                    "seeker",
                    "weathered-inscription",
                    resultingState);

                Assert.True(FocusedSearchOutcomeChangePolicy.WouldChange(outcome, perception));
            }
        }
    }

    [Fact]
    public void RejectsWrongCharacterBeforeComparingDifferentVisibilityStates()
    {
        FocusedSearchOutcome outcome = Outcome(
            "seeker",
            "weathered-inscription",
            VisibilityState.Understood);
        CharacterPerceptionSnapshot perception = Snapshot(
            "travelling-healer",
            Visibility("weathered-inscription", VisibilityState.Noticed));

        bool result = FocusedSearchOutcomeChangePolicy.WouldChange(outcome, perception);

        Assert.False(result);
    }

    [Fact]
    public void RejectsAbsentTargetBeforeComparingDifferentVisibilityStates()
    {
        FocusedSearchOutcome outcome = Outcome(
            "seeker",
            "weathered-inscription",
            VisibilityState.Understood);
        CharacterPerceptionSnapshot perception = Snapshot(
            "seeker",
            Visibility("wagon-residue", VisibilityState.Noticed));

        bool result = FocusedSearchOutcomeChangePolicy.WouldChange(outcome, perception);

        Assert.False(result);
    }

    [Fact]
    public void ComparesOnlyTheExactTargetAmongUnrelatedEntries()
    {
        FocusedSearchOutcome outcome = Outcome(
            "seeker",
            "weathered-inscription",
            VisibilityState.Understood);
        CharacterPerceptionSnapshot perception = Snapshot(
            "seeker",
            Visibility("wagon-residue", VisibilityState.Noticed),
            Visibility("weathered-inscription", VisibilityState.Understood),
            Visibility("concealed-healing-herb", VisibilityState.Identified));

        bool result = FocusedSearchOutcomeChangePolicy.WouldChange(outcome, perception);

        Assert.False(result);
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

        bool result = FocusedSearchOutcomeChangePolicy.WouldChange(outcome, perception);

        Assert.True(result);
        Assert.NotSame(outcomeCharacterId, perceptionCharacterId);
        Assert.Equal(outcomeCharacterId, perceptionCharacterId);
        Assert.NotSame(outcomeObjectId, perceptionObjectId);
        Assert.Equal(outcomeObjectId, perceptionObjectId);
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

        _ = FocusedSearchOutcomeChangePolicy.WouldChange(outcome, perception);

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
            () => FocusedSearchOutcomeChangePolicy.WouldChange(null!, perception));
        Assert.Throws<ArgumentNullException>(
            () => FocusedSearchOutcomeChangePolicy.WouldChange(outcome, null!));
    }

    [Fact]
    public void ExposesOnlyTheSinglePureChangeDetectionOperation()
    {
        Type policyType = typeof(FocusedSearchOutcomeChangePolicy);
        System.Reflection.MethodInfo method = Assert.Single(
            policyType
                .GetMethods()
                .Where(candidate => candidate.DeclaringType == policyType));

        Assert.Equal("WouldChange", method.Name);
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
