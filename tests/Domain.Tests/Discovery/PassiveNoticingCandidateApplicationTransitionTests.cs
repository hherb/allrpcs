using Allrpcs.Domain.Discovery;
using Allrpcs.Domain.Identifiers;
using Allrpcs.Domain.Perception;

namespace Allrpcs.Domain.Tests.Discovery;

public sealed class PassiveNoticingCandidateApplicationTransitionTests
{
    [Fact]
    public void RejectsCandidateForWrongObserver()
    {
        PassiveNoticingCandidate candidate = Candidate("seeker", "weathered-inscription");
        WorldObjectVisibility target = Visibility(
            "weathered-inscription",
            VisibilityState.Unnoticed);
        CharacterPerceptionSnapshot perception = Snapshot("travelling-healer", target);

        Assert.Throws<InvalidOperationException>(
            () => PassiveNoticingCandidateApplicationTransition.Apply(candidate, perception));
        Assert.Collection(perception.Entries, entry => Assert.Same(target, entry));
    }

    [Fact]
    public void RejectsCandidateWhenTargetIsAbsent()
    {
        PassiveNoticingCandidate candidate = Candidate("seeker", "weathered-inscription");
        WorldObjectVisibility unrelated = Visibility(
            "wagon-residue",
            VisibilityState.Unnoticed);
        CharacterPerceptionSnapshot perception = Snapshot("seeker", unrelated);

        Assert.Throws<InvalidOperationException>(
            () => PassiveNoticingCandidateApplicationTransition.Apply(candidate, perception));
        Assert.Collection(perception.Entries, entry => Assert.Same(unrelated, entry));
    }

    [Fact]
    public void RejectsEveryStateOtherThanUnnoticed()
    {
        PassiveNoticingCandidate candidate = Candidate("seeker", "weathered-inscription");

        foreach (VisibilityState currentState in VisibilityState.All)
        {
            if (currentState == VisibilityState.Unnoticed)
            {
                continue;
            }

            WorldObjectVisibility target = Visibility("weathered-inscription", currentState);
            CharacterPerceptionSnapshot perception = Snapshot("seeker", target);

            Assert.Throws<InvalidOperationException>(
                () => PassiveNoticingCandidateApplicationTransition.Apply(candidate, perception));
            Assert.Collection(perception.Entries, entry => Assert.Same(target, entry));
        }
    }

    [Fact]
    public void ChangesUnnoticedTargetToNoticed()
    {
        PassiveNoticingCandidate candidate = Candidate("seeker", "weathered-inscription");
        WorldObjectVisibility target = Visibility(
            "weathered-inscription",
            VisibilityState.Unnoticed);
        CharacterPerceptionSnapshot perception = Snapshot("seeker", target);

        CharacterPerceptionSnapshot result =
            PassiveNoticingCandidateApplicationTransition.Apply(candidate, perception);

        WorldObjectVisibility replacement = Assert.Single(result.Entries);
        Assert.NotSame(target, replacement);
        Assert.Same(target.ObjectId, replacement.ObjectId);
        Assert.Same(VisibilityState.Noticed, replacement.VisibilityState);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    public void ReplacesOnlyTheTargetInItsStablePosition(int targetIndex)
    {
        PassiveNoticingCandidate candidate = Candidate("seeker", "weathered-inscription");
        WorldObjectVisibility first = Visibility("wagon-residue", VisibilityState.Identified);
        WorldObjectVisibility target = Visibility(
            "weathered-inscription",
            VisibilityState.Unnoticed);
        WorldObjectVisibility third = Visibility("healing-herb", VisibilityState.Noticed);
        WorldObjectVisibility[] entries = [first, target, third];
        (entries[targetIndex], entries[1]) = (entries[1], entries[targetIndex]);
        CharacterPerceptionSnapshot perception = Snapshot("seeker", entries);

        CharacterPerceptionSnapshot result =
            PassiveNoticingCandidateApplicationTransition.Apply(candidate, perception);

        Assert.Equal(entries.Length, result.Entries.Count);
        for (int index = 0; index < entries.Length; index++)
        {
            if (index == targetIndex)
            {
                Assert.NotSame(entries[index], result.Entries[index]);
                Assert.Same(entries[index].ObjectId, result.Entries[index].ObjectId);
                Assert.Same(VisibilityState.Noticed, result.Entries[index].VisibilityState);
                continue;
            }

            Assert.Same(entries[index], result.Entries[index]);
        }
    }

    [Fact]
    public void MatchesValueEqualIdentifiersAndRetainsPerceptionInstances()
    {
        CharacterId candidateCharacterId = new("seeker");
        ObjectId candidateObjectId = new("weathered-inscription");
        PassiveNoticingCandidate candidate = new(candidateCharacterId, candidateObjectId);
        CharacterId perceptionCharacterId = new("seeker");
        ObjectId perceptionObjectId = new("weathered-inscription");
        WorldObjectVisibility target = new(perceptionObjectId, VisibilityState.Unnoticed);
        CharacterPerceptionSnapshot perception = new(perceptionCharacterId, new[] { target });

        CharacterPerceptionSnapshot result =
            PassiveNoticingCandidateApplicationTransition.Apply(candidate, perception);

        WorldObjectVisibility replacement = Assert.Single(result.Entries);
        Assert.NotSame(candidateCharacterId, perceptionCharacterId);
        Assert.NotSame(candidateObjectId, perceptionObjectId);
        Assert.Same(perceptionCharacterId, result.CharacterId);
        Assert.Same(perceptionObjectId, replacement.ObjectId);
        Assert.NotSame(candidateObjectId, replacement.ObjectId);
        Assert.Same(VisibilityState.Noticed, replacement.VisibilityState);
    }

    [Fact]
    public void DoesNotMutateSuccessfulInputsOrRetainedInstances()
    {
        CharacterId observerCharacterId = new("seeker");
        ObjectId candidateObjectId = new("weathered-inscription");
        PassiveNoticingCandidate candidate = new(observerCharacterId, candidateObjectId);
        CharacterId perceptionCharacterId = new("seeker");
        WorldObjectVisibility unrelated = Visibility(
            "wagon-residue",
            VisibilityState.Identified);
        ObjectId perceptionObjectId = new("weathered-inscription");
        VisibilityState currentVisibilityState = VisibilityState.Unnoticed;
        WorldObjectVisibility target = new(perceptionObjectId, currentVisibilityState);
        CharacterPerceptionSnapshot perception = new(
            perceptionCharacterId,
            new[] { unrelated, target });

        CharacterPerceptionSnapshot result =
            PassiveNoticingCandidateApplicationTransition.Apply(candidate, perception);

        Assert.NotSame(perception, result);
        Assert.Same(observerCharacterId, candidate.ObserverCharacterId);
        Assert.Same(candidateObjectId, candidate.TargetObjectId);
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
                Assert.Same(VisibilityState.Noticed, entry.VisibilityState);
            });
    }

    [Fact]
    public void RejectsNullInputsThroughChangePolicy()
    {
        PassiveNoticingCandidate candidate = Candidate("seeker", "weathered-inscription");
        CharacterPerceptionSnapshot perception = Snapshot("seeker");

        Assert.Throws<ArgumentNullException>(
            () => PassiveNoticingCandidateApplicationTransition.Apply(null!, perception));
        Assert.Throws<ArgumentNullException>(
            () => PassiveNoticingCandidateApplicationTransition.Apply(candidate, null!));
    }

    [Fact]
    public void ExposesOnlyTheSinglePureApplicationOperation()
    {
        Type transitionType = typeof(PassiveNoticingCandidateApplicationTransition);
        System.Reflection.MethodInfo method = Assert.Single(
            transitionType
                .GetMethods()
                .Where(candidate => candidate.DeclaringType == transitionType));

        Assert.Equal("Apply", method.Name);
        Assert.Equal(typeof(CharacterPerceptionSnapshot), method.ReturnType);
        Assert.True(method.IsStatic);
        Assert.Collection(
            method.GetParameters(),
            parameter => Assert.Equal(
                typeof(PassiveNoticingCandidate),
                parameter.ParameterType),
            parameter => Assert.Equal(
                typeof(CharacterPerceptionSnapshot),
                parameter.ParameterType));
        Assert.Empty(transitionType.GetConstructors());
        Assert.Empty(transitionType.GetProperties());
        Assert.Empty(transitionType.GetEvents());
        Assert.True(transitionType.IsAbstract);
        Assert.True(transitionType.IsSealed);
    }

    private static PassiveNoticingCandidate Candidate(
        string observerCharacterId,
        string targetObjectId) =>
        new(new CharacterId(observerCharacterId), new ObjectId(targetObjectId));

    private static CharacterPerceptionSnapshot Snapshot(
        string characterId,
        params WorldObjectVisibility[] entries) =>
        new(new CharacterId(characterId), entries);

    private static WorldObjectVisibility Visibility(
        string objectId,
        VisibilityState visibilityState) =>
        new(new ObjectId(objectId), visibilityState);
}
