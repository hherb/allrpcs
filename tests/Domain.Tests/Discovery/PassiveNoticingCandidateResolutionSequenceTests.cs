using Allrpcs.Domain.Discovery;
using Allrpcs.Domain.Identifiers;
using Allrpcs.Domain.Perception;

namespace Allrpcs.Domain.Tests.Discovery;

public sealed class PassiveNoticingCandidateResolutionSequenceTests
{
    [Fact]
    public void ResolvesFirstEligibleCandidateForObserver()
    {
        PassiveNoticingCandidate selected = Candidate("seeker", "weathered-inscription");
        PassiveNoticingCandidate remaining = Candidate("seeker", "healing-herb");
        WorldObjectVisibility selectedVisibility =
            Visibility("weathered-inscription", VisibilityState.Unnoticed);
        WorldObjectVisibility remainingVisibility =
            Visibility("healing-herb", VisibilityState.Unnoticed);

        PassiveNoticingCandidateResolutionResult result =
            PassiveNoticingCandidateResolutionSequence.ResolveNext(
                new CharacterId("seeker"),
                Perception("seeker", selectedVisibility, remainingVisibility),
                Candidates(selected, remaining));

        Assert.Collection(
            result.Perception.Entries,
            entry =>
            {
                Assert.Same(selectedVisibility.ObjectId, entry.ObjectId);
                Assert.Same(VisibilityState.Noticed, entry.VisibilityState);
            },
            entry => Assert.Same(remainingVisibility, entry));
        Assert.Collection(
            result.Candidates.Entries,
            entry => Assert.Same(remaining, entry));
    }

    [Fact]
    public void SkipsIneligibleCandidatesAndResolvesNextEligibleCandidate()
    {
        PassiveNoticingCandidate otherObserver =
            Candidate("travelling-healer", "healing-herb");
        PassiveNoticingCandidate alreadyNoticed =
            Candidate("seeker", "wagon-residue");
        PassiveNoticingCandidate selected =
            Candidate("seeker", "weathered-inscription");
        CharacterPerceptionSnapshot perception = Perception(
            "seeker",
            Visibility("healing-herb", VisibilityState.Unnoticed),
            Visibility("wagon-residue", VisibilityState.Noticed),
            Visibility("weathered-inscription", VisibilityState.Unnoticed));

        PassiveNoticingCandidateResolutionResult result =
            PassiveNoticingCandidateResolutionSequence.ResolveNext(
                new CharacterId("seeker"),
                perception,
                Candidates(otherObserver, alreadyNoticed, selected));

        Assert.Collection(
            result.Perception.Entries,
            entry => Assert.Same(perception.Entries[0], entry),
            entry => Assert.Same(perception.Entries[1], entry),
            entry =>
            {
                Assert.Same(perception.Entries[2].ObjectId, entry.ObjectId);
                Assert.Same(VisibilityState.Noticed, entry.VisibilityState);
            });
        Assert.Collection(
            result.Candidates.Entries,
            entry => Assert.Same(otherObserver, entry),
            entry => Assert.Same(alreadyNoticed, entry));
    }

    [Fact]
    public void AcceptsDistinctValueEqualObserverIdentifier()
    {
        CharacterId observerCharacterId = new("seeker");
        CharacterId candidateObserverCharacterId = new(observerCharacterId.Value);
        PassiveNoticingCandidate candidate = new(
            candidateObserverCharacterId,
            new ObjectId("weathered-inscription"));
        CharacterPerceptionSnapshot perception = Perception(
            "seeker",
            Visibility("weathered-inscription", VisibilityState.Unnoticed));

        PassiveNoticingCandidateResolutionResult result =
            PassiveNoticingCandidateResolutionSequence.ResolveNext(
                observerCharacterId,
                perception,
                Candidates(candidate));

        Assert.NotSame(observerCharacterId, candidateObserverCharacterId);
        Assert.Empty(result.Candidates.Entries);
        Assert.Same(VisibilityState.Noticed, result.Perception.Entries[0].VisibilityState);
    }

    [Fact]
    public void RejectsWhenNoCandidateCanResolve()
    {
        CharacterId observerCharacterId = new("seeker");
        PassiveNoticingCandidate otherObserver =
            Candidate("travelling-healer", "healing-herb");
        PassiveNoticingCandidate alreadyNoticed =
            Candidate("seeker", "weathered-inscription");
        CharacterPerceptionSnapshot perception = Perception(
            "seeker",
            Visibility("weathered-inscription", VisibilityState.Noticed));
        PassiveNoticingCandidateSnapshot candidates =
            Candidates(otherObserver, alreadyNoticed);

        Assert.Throws<InvalidOperationException>(
            () => PassiveNoticingCandidateResolutionSequence.ResolveNext(
                observerCharacterId,
                perception,
                Candidates()));
        Assert.Throws<InvalidOperationException>(
            () => PassiveNoticingCandidateResolutionSequence.ResolveNext(
                observerCharacterId,
                perception,
                candidates));
        Assert.Collection(
            candidates.Entries,
            entry => Assert.Same(otherObserver, entry),
            entry => Assert.Same(alreadyNoticed, entry));
    }

    [Fact]
    public void LeavesEveryInputAndNestedInstanceUnchanged()
    {
        CharacterId observerCharacterId = new("seeker");
        CharacterId perceptionCharacterId = new(observerCharacterId.Value);
        ObjectId targetObjectId = new("weathered-inscription");
        VisibilityState targetState = VisibilityState.Unnoticed;
        WorldObjectVisibility targetVisibility = new(targetObjectId, targetState);
        WorldObjectVisibility unrelatedVisibility =
            Visibility("wagon-residue", VisibilityState.Identified);
        CharacterPerceptionSnapshot perception = new(
            perceptionCharacterId,
            [unrelatedVisibility, targetVisibility]);
        PassiveNoticingCandidate unrelatedCandidate =
            Candidate("travelling-healer", "healing-herb");
        CharacterId candidateObserverCharacterId = new(observerCharacterId.Value);
        ObjectId candidateTargetObjectId = new(targetObjectId.Value);
        PassiveNoticingCandidate selected =
            new(candidateObserverCharacterId, candidateTargetObjectId);
        PassiveNoticingCandidateSnapshot candidates =
            Candidates(unrelatedCandidate, selected);

        PassiveNoticingCandidateResolutionResult result =
            PassiveNoticingCandidateResolutionSequence.ResolveNext(
                observerCharacterId,
                perception,
                candidates);

        Assert.NotSame(perception, result.Perception);
        Assert.NotSame(candidates, result.Candidates);
        Assert.Same(perceptionCharacterId, perception.CharacterId);
        Assert.Collection(
            perception.Entries,
            entry => Assert.Same(unrelatedVisibility, entry),
            entry => Assert.Same(targetVisibility, entry));
        Assert.Same(targetObjectId, targetVisibility.ObjectId);
        Assert.Same(targetState, targetVisibility.VisibilityState);
        Assert.Same(candidateObserverCharacterId, selected.ObserverCharacterId);
        Assert.Same(candidateTargetObjectId, selected.TargetObjectId);
        Assert.Collection(
            candidates.Entries,
            entry => Assert.Same(unrelatedCandidate, entry),
            entry => Assert.Same(selected, entry));
        Assert.Collection(
            result.Candidates.Entries,
            entry => Assert.Same(unrelatedCandidate, entry));
    }

    [Fact]
    public void RejectsEveryNullInput()
    {
        CharacterId observerCharacterId = new("seeker");
        CharacterPerceptionSnapshot perception = Perception("seeker");
        PassiveNoticingCandidateSnapshot candidates = Candidates();

        Assert.Throws<ArgumentNullException>(
            () => PassiveNoticingCandidateResolutionSequence.ResolveNext(
                null!, perception, candidates));
        Assert.Throws<ArgumentNullException>(
            () => PassiveNoticingCandidateResolutionSequence.ResolveNext(
                observerCharacterId, null!, candidates));
        Assert.Throws<ArgumentNullException>(
            () => PassiveNoticingCandidateResolutionSequence.ResolveNext(
                observerCharacterId, perception, null!));
    }

    [Fact]
    public void ExposesOnlyTheSinglePureResolutionSequenceOperation()
    {
        Type sequenceType = typeof(PassiveNoticingCandidateResolutionSequence);
        System.Reflection.MethodInfo method = Assert.Single(
            sequenceType
                .GetMethods()
                .Where(candidate => candidate.DeclaringType == sequenceType));

        Assert.Equal("ResolveNext", method.Name);
        Assert.Equal(typeof(PassiveNoticingCandidateResolutionResult), method.ReturnType);
        Assert.True(method.IsStatic);
        Assert.Collection(
            method.GetParameters(),
            parameter => Assert.Equal(typeof(CharacterId), parameter.ParameterType),
            parameter => Assert.Equal(
                typeof(CharacterPerceptionSnapshot),
                parameter.ParameterType),
            parameter => Assert.Equal(
                typeof(PassiveNoticingCandidateSnapshot),
                parameter.ParameterType));
        Assert.Empty(sequenceType.GetConstructors());
        Assert.Empty(sequenceType.GetProperties());
        Assert.Empty(sequenceType.GetEvents());
        Assert.True(sequenceType.IsAbstract);
        Assert.True(sequenceType.IsSealed);
    }

    private static PassiveNoticingCandidate Candidate(
        string observerCharacterId,
        string targetObjectId) =>
        new(new CharacterId(observerCharacterId), new ObjectId(targetObjectId));

    private static PassiveNoticingCandidateSnapshot Candidates(
        params PassiveNoticingCandidate[] entries) =>
        new(entries);

    private static CharacterPerceptionSnapshot Perception(
        string characterId,
        params WorldObjectVisibility[] entries) =>
        new(new CharacterId(characterId), entries);

    private static WorldObjectVisibility Visibility(
        string objectId,
        VisibilityState visibilityState) =>
        new(new ObjectId(objectId), visibilityState);
}
