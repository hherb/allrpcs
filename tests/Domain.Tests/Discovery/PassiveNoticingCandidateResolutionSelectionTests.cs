using Allrpcs.Domain.Discovery;
using Allrpcs.Domain.Identifiers;
using Allrpcs.Domain.Perception;

namespace Allrpcs.Domain.Tests.Discovery;

public sealed class PassiveNoticingCandidateResolutionSelectionTests
{
    [Fact]
    public void SelectsExactFirstResolvableCandidateForObserverInStableOrder()
    {
        CharacterId observerCharacterId = new("seeker");
        PassiveNoticingCandidate first = Candidate("seeker", "weathered-inscription");
        PassiveNoticingCandidate second = Candidate("seeker", "healing-herb");
        PassiveNoticingCandidateSnapshot candidates = Candidates(first, second);
        CharacterPerceptionSnapshot perception = Perception(
            "seeker",
            Visibility("weathered-inscription", VisibilityState.Unnoticed),
            Visibility("healing-herb", VisibilityState.Unnoticed));

        PassiveNoticingCandidate? result =
            PassiveNoticingCandidateResolutionSelection.SelectNext(
                observerCharacterId,
                perception,
                candidates);

        Assert.Same(first, result);
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

        PassiveNoticingCandidate? result =
            PassiveNoticingCandidateResolutionSelection.SelectNext(
                observerCharacterId,
                perception,
                Candidates(candidate));

        Assert.NotSame(observerCharacterId, candidateObserverCharacterId);
        Assert.Equal(observerCharacterId, candidateObserverCharacterId);
        PassiveNoticingCandidate selected = Assert.IsType<PassiveNoticingCandidate>(result);
        Assert.Same(candidate, selected);
        Assert.Same(candidateObserverCharacterId, selected.ObserverCharacterId);
    }

    [Fact]
    public void SkipsOtherObserversAndNonChangingCandidates()
    {
        PassiveNoticingCandidate otherObserver =
            Candidate("travelling-healer", "healing-herb");
        PassiveNoticingCandidate alreadyNoticed =
            Candidate("seeker", "wagon-residue");
        PassiveNoticingCandidate missingTarget =
            Candidate("seeker", "missing-object");
        PassiveNoticingCandidate selected =
            Candidate("seeker", "weathered-inscription");
        CharacterPerceptionSnapshot perception = Perception(
            "seeker",
            Visibility("healing-herb", VisibilityState.Unnoticed),
            Visibility("wagon-residue", VisibilityState.Noticed),
            Visibility("weathered-inscription", VisibilityState.Unnoticed));

        PassiveNoticingCandidate? result =
            PassiveNoticingCandidateResolutionSelection.SelectNext(
                new CharacterId("seeker"),
                perception,
                Candidates(otherObserver, alreadyNoticed, missingTarget, selected));

        Assert.Same(selected, result);
    }

    [Fact]
    public void ReturnsNullWhenNoCandidateCanResolve()
    {
        PassiveNoticingCandidate otherObserver =
            Candidate("travelling-healer", "healing-herb");
        PassiveNoticingCandidate alreadyNoticed =
            Candidate("seeker", "weathered-inscription");
        CharacterPerceptionSnapshot perception = Perception(
            "seeker",
            Visibility("weathered-inscription", VisibilityState.Noticed));

        PassiveNoticingCandidate? emptyResult =
            PassiveNoticingCandidateResolutionSelection.SelectNext(
                new CharacterId("seeker"),
                perception,
                Candidates());
        PassiveNoticingCandidate? ineligibleResult =
            PassiveNoticingCandidateResolutionSelection.SelectNext(
                new CharacterId("seeker"),
                perception,
                Candidates(otherObserver, alreadyNoticed));

        Assert.Null(emptyResult);
        Assert.Null(ineligibleResult);
    }

    [Fact]
    public void ReturnsNullWhenPerceptionBelongsToAnotherCharacter()
    {
        PassiveNoticingCandidate candidate = Candidate("seeker", "weathered-inscription");
        CharacterPerceptionSnapshot perception = Perception(
            "travelling-healer",
            Visibility("weathered-inscription", VisibilityState.Unnoticed));

        PassiveNoticingCandidate? result =
            PassiveNoticingCandidateResolutionSelection.SelectNext(
                new CharacterId("seeker"),
                perception,
                Candidates(candidate));

        Assert.Null(result);
    }

    [Fact]
    public void LeavesEveryInputAndNestedInstanceUnchanged()
    {
        CharacterId observerCharacterId = new("seeker");
        CharacterId perceptionCharacterId = new(observerCharacterId.Value);
        ObjectId firstObjectId = new("weathered-inscription");
        VisibilityState firstState = VisibilityState.Noticed;
        WorldObjectVisibility firstVisibility = new(firstObjectId, firstState);
        ObjectId secondObjectId = new("healing-herb");
        VisibilityState secondState = VisibilityState.Unnoticed;
        WorldObjectVisibility secondVisibility = new(secondObjectId, secondState);
        CharacterPerceptionSnapshot perception = new(
            perceptionCharacterId,
            [firstVisibility, secondVisibility]);
        PassiveNoticingCandidate skipped =
            new(new CharacterId("seeker"), new ObjectId(firstObjectId.Value));
        PassiveNoticingCandidate selected =
            new(new CharacterId("seeker"), new ObjectId(secondObjectId.Value));
        PassiveNoticingCandidateSnapshot candidates = Candidates(skipped, selected);

        PassiveNoticingCandidate? result =
            PassiveNoticingCandidateResolutionSelection.SelectNext(
                observerCharacterId,
                perception,
                candidates);

        Assert.Same(selected, result);
        Assert.Same(perceptionCharacterId, perception.CharacterId);
        Assert.Collection(
            perception.Entries,
            entry => Assert.Same(firstVisibility, entry),
            entry => Assert.Same(secondVisibility, entry));
        Assert.Same(firstObjectId, firstVisibility.ObjectId);
        Assert.Same(firstState, firstVisibility.VisibilityState);
        Assert.Same(secondObjectId, secondVisibility.ObjectId);
        Assert.Same(secondState, secondVisibility.VisibilityState);
        Assert.Collection(
            candidates.Entries,
            entry => Assert.Same(skipped, entry),
            entry => Assert.Same(selected, entry));
    }

    [Fact]
    public void RejectsEveryNullInput()
    {
        CharacterId observerCharacterId = new("seeker");
        CharacterPerceptionSnapshot perception = Perception("seeker");
        PassiveNoticingCandidateSnapshot candidates = Candidates();

        Assert.Throws<ArgumentNullException>(
            () => PassiveNoticingCandidateResolutionSelection.SelectNext(
                null!, perception, candidates));
        Assert.Throws<ArgumentNullException>(
            () => PassiveNoticingCandidateResolutionSelection.SelectNext(
                observerCharacterId, null!, candidates));
        Assert.Throws<ArgumentNullException>(
            () => PassiveNoticingCandidateResolutionSelection.SelectNext(
                observerCharacterId, perception, null!));
    }

    [Fact]
    public void ExposesOnlyTheSinglePureSelectionOperation()
    {
        Type selectionType = typeof(PassiveNoticingCandidateResolutionSelection);
        System.Reflection.MethodInfo method = Assert.Single(
            selectionType
                .GetMethods()
                .Where(candidate => candidate.DeclaringType == selectionType));

        Assert.Equal("SelectNext", method.Name);
        Assert.Equal(typeof(PassiveNoticingCandidate), method.ReturnType);
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
        Assert.Empty(selectionType.GetConstructors());
        Assert.Empty(selectionType.GetProperties());
        Assert.Empty(selectionType.GetEvents());
        Assert.True(selectionType.IsAbstract);
        Assert.True(selectionType.IsSealed);
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
