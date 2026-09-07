using Allrpcs.Domain.Discovery;
using Allrpcs.Domain.Identifiers;
using Allrpcs.Domain.Perception;

namespace Allrpcs.Domain.Tests.Discovery;

public sealed class PassiveNoticingCandidateResolutionTransitionTests
{
    [Fact]
    public void RejectsEveryDelegatedPerceptionIneligibilityCategory()
    {
        PassiveNoticingCandidate candidate = Candidate("seeker", "weathered-inscription");
        CharacterPerceptionSnapshot[] ineligiblePerceptions =
        [
            Perception(
                "travelling-healer",
                Visibility("weathered-inscription", VisibilityState.Unnoticed)),
            Perception(
                "seeker",
                Visibility("wagon-residue", VisibilityState.Unnoticed)),
            Perception(
                "seeker",
                Visibility("weathered-inscription", VisibilityState.Noticed)),
        ];

        foreach (CharacterPerceptionSnapshot perception in ineligiblePerceptions)
        {
            Assert.Throws<InvalidOperationException>(
                () => PassiveNoticingCandidateResolutionTransition.Resolve(
                    candidate,
                    perception,
                    Candidates(candidate)));
        }
    }

    [Fact]
    public void RejectsEveryDelegatedConsumptionIneligibilityCategory()
    {
        PassiveNoticingCandidate candidate = Candidate("seeker", "weathered-inscription");
        CharacterPerceptionSnapshot perception = Perception(
            "seeker",
            Visibility("weathered-inscription", VisibilityState.Unnoticed));
        PassiveNoticingCandidateSnapshot[] ineligibleCandidates =
        [
            Candidates(),
            Candidates(Candidate("seeker", "healing-herb")),
            Candidates(Candidate("travelling-healer", "weathered-inscription")),
        ];

        foreach (PassiveNoticingCandidateSnapshot candidates in ineligibleCandidates)
        {
            Assert.Throws<InvalidOperationException>(
                () => PassiveNoticingCandidateResolutionTransition.Resolve(
                    candidate,
                    perception,
                    candidates));
        }
    }

    [Fact]
    public void ResolvesPerceptionAndConsumesCandidateWithStableOrderAndExactInstances()
    {
        CharacterId candidateCharacterId = new("seeker");
        ObjectId candidateObjectId = new("weathered-inscription");
        PassiveNoticingCandidate candidate =
            new(candidateCharacterId, candidateObjectId);
        CharacterId perceptionCharacterId = new("seeker");
        WorldObjectVisibility firstVisibility =
            Visibility("wagon-residue", VisibilityState.Identified);
        ObjectId perceptionObjectId = new("weathered-inscription");
        VisibilityState currentVisibilityState = VisibilityState.Unnoticed;
        WorldObjectVisibility targetVisibility =
            new(perceptionObjectId, currentVisibilityState);
        WorldObjectVisibility thirdVisibility =
            Visibility("healing-herb", VisibilityState.Noticed);
        CharacterPerceptionSnapshot perception = new(
            perceptionCharacterId,
            new[] { firstVisibility, targetVisibility, thirdVisibility });
        PassiveNoticingCandidate firstCandidate =
            Candidate("sage", "wagon-residue");
        PassiveNoticingCandidate retainedCandidate =
            Candidate("seeker", "weathered-inscription");
        PassiveNoticingCandidate thirdCandidate =
            Candidate("travelling-healer", "healing-herb");
        PassiveNoticingCandidateSnapshot candidates =
            Candidates(firstCandidate, retainedCandidate, thirdCandidate);

        PassiveNoticingCandidateResolutionResult result =
            PassiveNoticingCandidateResolutionTransition.Resolve(
                candidate,
                perception,
                candidates);

        Assert.NotSame(perception, result.Perception);
        Assert.Same(perceptionCharacterId, result.Perception.CharacterId);
        Assert.Collection(
            result.Perception.Entries,
            entry => Assert.Same(firstVisibility, entry),
            entry =>
            {
                Assert.NotSame(targetVisibility, entry);
                Assert.Same(perceptionObjectId, entry.ObjectId);
                Assert.Same(VisibilityState.Noticed, entry.VisibilityState);
            },
            entry => Assert.Same(thirdVisibility, entry));
        Assert.NotSame(candidates, result.Candidates);
        Assert.Collection(
            result.Candidates.Entries,
            entry => Assert.Same(firstCandidate, entry),
            entry => Assert.Same(thirdCandidate, entry));
    }

    [Fact]
    public void LeavesEverySuccessfulInputAndRetainedInstanceUnchanged()
    {
        CharacterId observerCharacterId = new("seeker");
        ObjectId candidateObjectId = new("weathered-inscription");
        PassiveNoticingCandidate candidate =
            new(observerCharacterId, candidateObjectId);
        CharacterId perceptionCharacterId = new("seeker");
        ObjectId perceptionObjectId = new("weathered-inscription");
        WorldObjectVisibility targetVisibility =
            new(perceptionObjectId, VisibilityState.Unnoticed);
        CharacterPerceptionSnapshot perception =
            new(perceptionCharacterId, new[] { targetVisibility });
        PassiveNoticingCandidate retainedCandidate =
            Candidate("seeker", "weathered-inscription");
        PassiveNoticingCandidateSnapshot candidates = Candidates(retainedCandidate);

        _ = PassiveNoticingCandidateResolutionTransition.Resolve(
            candidate,
            perception,
            candidates);

        Assert.Same(observerCharacterId, candidate.ObserverCharacterId);
        Assert.Same(candidateObjectId, candidate.TargetObjectId);
        Assert.Same(perceptionCharacterId, perception.CharacterId);
        Assert.Collection(perception.Entries, entry => Assert.Same(targetVisibility, entry));
        Assert.Same(perceptionObjectId, targetVisibility.ObjectId);
        Assert.Same(VisibilityState.Unnoticed, targetVisibility.VisibilityState);
        Assert.Collection(
            candidates.Entries,
            entry => Assert.Same(retainedCandidate, entry));
    }

    [Fact]
    public void LeavesRejectedInputsAndRetainedInstancesUnchanged()
    {
        PassiveNoticingCandidate candidate = Candidate("seeker", "weathered-inscription");
        WorldObjectVisibility targetVisibility =
            Visibility("weathered-inscription", VisibilityState.Noticed);
        CharacterPerceptionSnapshot perception =
            Perception("seeker", targetVisibility);
        PassiveNoticingCandidate retainedCandidate =
            Candidate("seeker", "weathered-inscription");
        PassiveNoticingCandidateSnapshot candidates = Candidates(retainedCandidate);

        Assert.Throws<InvalidOperationException>(
            () => PassiveNoticingCandidateResolutionTransition.Resolve(
                candidate,
                perception,
                candidates));

        Assert.Collection(perception.Entries, entry => Assert.Same(targetVisibility, entry));
        Assert.Collection(
            candidates.Entries,
            entry => Assert.Same(retainedCandidate, entry));
    }

    [Fact]
    public void RejectsNullInputsThroughResolutionPolicy()
    {
        PassiveNoticingCandidate candidate = Candidate("seeker", "weathered-inscription");
        CharacterPerceptionSnapshot perception = Perception(
            "seeker",
            Visibility("weathered-inscription", VisibilityState.Unnoticed));
        PassiveNoticingCandidateSnapshot candidates = Candidates(candidate);

        Assert.Throws<ArgumentNullException>(
            () => PassiveNoticingCandidateResolutionTransition.Resolve(
                null!,
                perception,
                candidates));
        Assert.Throws<ArgumentNullException>(
            () => PassiveNoticingCandidateResolutionTransition.Resolve(
                candidate,
                null!,
                candidates));
        Assert.Throws<ArgumentNullException>(
            () => PassiveNoticingCandidateResolutionTransition.Resolve(
                candidate,
                perception,
                null!));
    }

    [Fact]
    public void ExposesOnlyTheSinglePureResolutionOperation()
    {
        Type transitionType = typeof(PassiveNoticingCandidateResolutionTransition);
        System.Reflection.MethodInfo method = Assert.Single(
            transitionType
                .GetMethods()
                .Where(candidate => candidate.DeclaringType == transitionType));

        Assert.Equal("Resolve", method.Name);
        Assert.Equal(typeof(PassiveNoticingCandidateResolutionResult), method.ReturnType);
        Assert.True(method.IsStatic);
        Assert.Collection(
            method.GetParameters(),
            parameter => Assert.Equal(
                typeof(PassiveNoticingCandidate),
                parameter.ParameterType),
            parameter => Assert.Equal(
                typeof(CharacterPerceptionSnapshot),
                parameter.ParameterType),
            parameter => Assert.Equal(
                typeof(PassiveNoticingCandidateSnapshot),
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
