using Allrpcs.Domain.Discovery;
using Allrpcs.Domain.Identifiers;
using Allrpcs.Domain.Perception;

namespace Allrpcs.Domain.Tests.Discovery;

public sealed class FocusedSearchOutcomeApplicationTransitionTests
{
    [Fact]
    public void RejectsOutcomeForWrongCharacter()
    {
        FocusedSearchOutcome outcome = Outcome(
            "seeker",
            "weathered-inscription",
            VisibilityState.Understood);
        WorldObjectVisibility target = Visibility(
            "weathered-inscription",
            VisibilityState.Noticed);
        CharacterPerceptionSnapshot perception = Snapshot("travelling-healer", target);

        Assert.Throws<InvalidOperationException>(
            () => FocusedSearchOutcomeApplicationTransition.Apply(outcome, perception));
        Assert.Collection(perception.Entries, entry => Assert.Same(target, entry));
    }

    [Fact]
    public void RejectsOutcomeWhenTargetIsAbsent()
    {
        FocusedSearchOutcome outcome = Outcome(
            "seeker",
            "weathered-inscription",
            VisibilityState.Understood);
        WorldObjectVisibility unrelated = Visibility(
            "wagon-residue",
            VisibilityState.Noticed);
        CharacterPerceptionSnapshot perception = Snapshot("seeker", unrelated);

        Assert.Throws<InvalidOperationException>(
            () => FocusedSearchOutcomeApplicationTransition.Apply(outcome, perception));
        Assert.Collection(perception.Entries, entry => Assert.Same(unrelated, entry));
    }

    [Fact]
    public void RejectsEveryEqualCurrentAndResultingVisibilityState()
    {
        foreach (VisibilityState state in VisibilityState.All)
        {
            FocusedSearchOutcome outcome = Outcome(
                "seeker",
                "weathered-inscription",
                state);
            WorldObjectVisibility target = Visibility("weathered-inscription", state);
            CharacterPerceptionSnapshot perception = Snapshot("seeker", target);

            Assert.Throws<InvalidOperationException>(
                () => FocusedSearchOutcomeApplicationTransition.Apply(outcome, perception));
            Assert.Collection(perception.Entries, entry => Assert.Same(target, entry));
        }
    }

    [Fact]
    public void AppliesEveryDistinctCurrentAndResultingVisibilityStatePair()
    {
        foreach (VisibilityState currentState in VisibilityState.All)
        {
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
                WorldObjectVisibility target = Visibility(
                    "weathered-inscription",
                    currentState);
                CharacterPerceptionSnapshot perception = Snapshot("seeker", target);

                CharacterPerceptionSnapshot result =
                    FocusedSearchOutcomeApplicationTransition.Apply(outcome, perception);

                WorldObjectVisibility replacement = Assert.Single(result.Entries);
                Assert.Same(target.ObjectId, replacement.ObjectId);
                Assert.Same(resultingState, replacement.VisibilityState);
                Assert.NotSame(target, replacement);
            }
        }
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    public void ReplacesOnlyTheTargetInItsStablePosition(int targetIndex)
    {
        FocusedSearchOutcome outcome = Outcome(
            "seeker",
            "weathered-inscription",
            VisibilityState.Understood);
        WorldObjectVisibility first = Visibility("wagon-residue", VisibilityState.Identified);
        WorldObjectVisibility target = Visibility(
            "weathered-inscription",
            VisibilityState.Noticed);
        WorldObjectVisibility third = Visibility(
            "concealed-healing-herb",
            VisibilityState.Unnoticed);
        WorldObjectVisibility[] entries = [first, target, third];
        (entries[targetIndex], entries[1]) = (entries[1], entries[targetIndex]);
        CharacterPerceptionSnapshot perception = Snapshot("seeker", entries);

        CharacterPerceptionSnapshot result =
            FocusedSearchOutcomeApplicationTransition.Apply(outcome, perception);

        Assert.Equal(entries.Length, result.Entries.Count);
        for (int index = 0; index < entries.Length; index++)
        {
            if (index == targetIndex)
            {
                Assert.NotSame(entries[index], result.Entries[index]);
                Assert.Same(entries[index].ObjectId, result.Entries[index].ObjectId);
                Assert.Same(
                    outcome.ResultingVisibilityState,
                    result.Entries[index].VisibilityState);
                continue;
            }

            Assert.Same(entries[index], result.Entries[index]);
        }
    }

    [Fact]
    public void MatchesValueEqualIdentifiersAndRetainsPerceptionInstances()
    {
        CharacterId outcomeCharacterId = new("seeker");
        ObjectId outcomeObjectId = new("weathered-inscription");
        VisibilityState resultingVisibilityState = VisibilityState.Understood;
        FocusedSearchOutcome outcome = new(
            new FocusedSearchRequest(outcomeCharacterId, outcomeObjectId),
            resultingVisibilityState);
        CharacterId perceptionCharacterId = new("seeker");
        ObjectId perceptionObjectId = new("weathered-inscription");
        WorldObjectVisibility target = new(perceptionObjectId, VisibilityState.Noticed);
        CharacterPerceptionSnapshot perception = new(perceptionCharacterId, new[] { target });

        CharacterPerceptionSnapshot result =
            FocusedSearchOutcomeApplicationTransition.Apply(outcome, perception);

        WorldObjectVisibility replacement = Assert.Single(result.Entries);
        Assert.NotSame(outcomeCharacterId, perceptionCharacterId);
        Assert.NotSame(outcomeObjectId, perceptionObjectId);
        Assert.Same(perceptionCharacterId, result.CharacterId);
        Assert.Same(perceptionObjectId, replacement.ObjectId);
        Assert.NotSame(outcomeObjectId, replacement.ObjectId);
        Assert.Same(resultingVisibilityState, replacement.VisibilityState);
    }

    [Fact]
    public void DoesNotMutateSuccessfulInputsOrRetainedInstances()
    {
        CharacterId searcherCharacterId = new("seeker");
        ObjectId outcomeObjectId = new("weathered-inscription");
        FocusedSearchRequest request = new(searcherCharacterId, outcomeObjectId);
        VisibilityState resultingVisibilityState = VisibilityState.Understood;
        FocusedSearchOutcome outcome = new(request, resultingVisibilityState);
        CharacterId perceptionCharacterId = new("seeker");
        WorldObjectVisibility unrelated = Visibility(
            "wagon-residue",
            VisibilityState.Identified);
        ObjectId perceptionObjectId = new("weathered-inscription");
        VisibilityState currentVisibilityState = VisibilityState.Noticed;
        WorldObjectVisibility target = new(perceptionObjectId, currentVisibilityState);
        CharacterPerceptionSnapshot perception = new(
            perceptionCharacterId,
            new[] { unrelated, target });

        CharacterPerceptionSnapshot result =
            FocusedSearchOutcomeApplicationTransition.Apply(outcome, perception);

        Assert.NotSame(perception, result);
        Assert.Same(request, outcome.Request);
        Assert.Same(searcherCharacterId, request.SearcherCharacterId);
        Assert.Same(outcomeObjectId, request.TargetObjectId);
        Assert.Same(resultingVisibilityState, outcome.ResultingVisibilityState);
        Assert.Same(perceptionCharacterId, perception.CharacterId);
        Assert.Collection(
            perception.Entries,
            entry => Assert.Same(unrelated, entry),
            entry => Assert.Same(target, entry));
        Assert.Same(perceptionObjectId, target.ObjectId);
        Assert.Same(currentVisibilityState, target.VisibilityState);
        Assert.Collection(
            result.Entries,
            entry => Assert.Same(unrelated, entry),
            entry =>
            {
                Assert.NotSame(target, entry);
                Assert.Same(perceptionObjectId, entry.ObjectId);
                Assert.Same(resultingVisibilityState, entry.VisibilityState);
            });
    }

    [Fact]
    public void RejectsNullInputsThroughChangePolicy()
    {
        FocusedSearchOutcome outcome = Outcome(
            "seeker",
            "weathered-inscription",
            VisibilityState.Understood);
        CharacterPerceptionSnapshot perception = Snapshot("seeker");

        Assert.Throws<ArgumentNullException>(
            () => FocusedSearchOutcomeApplicationTransition.Apply(null!, perception));
        Assert.Throws<ArgumentNullException>(
            () => FocusedSearchOutcomeApplicationTransition.Apply(outcome, null!));
    }

    [Fact]
    public void ExposesOnlyTheSinglePureApplicationOperation()
    {
        Type transitionType = typeof(FocusedSearchOutcomeApplicationTransition);
        System.Reflection.MethodInfo method = Assert.Single(
            transitionType
                .GetMethods()
                .Where(candidate => candidate.DeclaringType == transitionType));

        Assert.Equal("Apply", method.Name);
        Assert.Equal(typeof(CharacterPerceptionSnapshot), method.ReturnType);
        Assert.True(method.IsStatic);
        Assert.Collection(
            method.GetParameters(),
            parameter => Assert.Equal(typeof(FocusedSearchOutcome), parameter.ParameterType),
            parameter => Assert.Equal(
                typeof(CharacterPerceptionSnapshot),
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
