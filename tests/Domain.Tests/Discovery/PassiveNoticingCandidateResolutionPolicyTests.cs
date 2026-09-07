using Allrpcs.Domain.Discovery;
using Allrpcs.Domain.Identifiers;
using Allrpcs.Domain.Perception;

namespace Allrpcs.Domain.Tests.Discovery;

public sealed class PassiveNoticingCandidateResolutionPolicyTests
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
            Assert.False(
                PassiveNoticingCandidateResolutionPolicy.CanResolve(
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
            Assert.False(
                PassiveNoticingCandidateResolutionPolicy.CanResolve(
                    candidate,
                    perception,
                    candidates));
        }
    }

    [Fact]
    public void AllowsTheExactRetainedCandidateWhenPerceptionWouldChange()
    {
        PassiveNoticingCandidate candidate = Candidate("seeker", "weathered-inscription");
        CharacterPerceptionSnapshot perception = Perception(
            "seeker",
            Visibility("weathered-inscription", VisibilityState.Unnoticed));

        bool result = PassiveNoticingCandidateResolutionPolicy.CanResolve(
            candidate,
            perception,
            Candidates(candidate));

        Assert.True(result);
    }

    [Fact]
    public void AllowsAValueEqualRetainedCandidateWhenPerceptionWouldChange()
    {
        PassiveNoticingCandidate candidate = Candidate("seeker", "weathered-inscription");
        PassiveNoticingCandidate retained = Candidate("seeker", "weathered-inscription");
        CharacterPerceptionSnapshot perception = Perception(
            "seeker",
            Visibility("weathered-inscription", VisibilityState.Unnoticed));

        bool result = PassiveNoticingCandidateResolutionPolicy.CanResolve(
            candidate,
            perception,
            Candidates(retained));

        Assert.NotSame(candidate, retained);
        Assert.NotSame(candidate.ObserverCharacterId, retained.ObserverCharacterId);
        Assert.NotSame(candidate.TargetObjectId, retained.TargetObjectId);
        Assert.Equal(candidate, retained);
        Assert.True(result);
    }

    [Fact]
    public void LeavesEveryInputAndRetainedInstanceUnchanged()
    {
        CharacterId observerCharacterId = new("seeker");
        ObjectId candidateObjectId = new("weathered-inscription");
        PassiveNoticingCandidate candidate = new(observerCharacterId, candidateObjectId);
        CharacterId perceptionCharacterId = new("seeker");
        WorldObjectVisibility unrelatedVisibility =
            Visibility("wagon-residue", VisibilityState.Identified);
        ObjectId perceptionObjectId = new("weathered-inscription");
        VisibilityState currentVisibilityState = VisibilityState.Unnoticed;
        WorldObjectVisibility targetVisibility =
            new(perceptionObjectId, currentVisibilityState);
        CharacterPerceptionSnapshot perception = new(
            perceptionCharacterId,
            new[] { unrelatedVisibility, targetVisibility });
        PassiveNoticingCandidate unrelatedCandidate =
            Candidate("sage", "healing-herb");
        PassiveNoticingCandidate retainedCandidate =
            Candidate("seeker", "weathered-inscription");
        PassiveNoticingCandidateSnapshot candidates =
            Candidates(unrelatedCandidate, retainedCandidate);

        _ = PassiveNoticingCandidateResolutionPolicy.CanResolve(
            candidate,
            perception,
            candidates);

        Assert.Same(observerCharacterId, candidate.ObserverCharacterId);
        Assert.Same(candidateObjectId, candidate.TargetObjectId);
        Assert.Same(perceptionCharacterId, perception.CharacterId);
        Assert.Collection(
            perception.Entries,
            entry => Assert.Same(unrelatedVisibility, entry),
            entry => Assert.Same(targetVisibility, entry));
        Assert.Same(perceptionObjectId, targetVisibility.ObjectId);
        Assert.Same(currentVisibilityState, targetVisibility.VisibilityState);
        Assert.Collection(
            candidates.Entries,
            entry => Assert.Same(unrelatedCandidate, entry),
            entry => Assert.Same(retainedCandidate, entry));
        Assert.Same(
            retainedCandidate.ObserverCharacterId,
            candidates.Entries[1].ObserverCharacterId);
        Assert.Same(
            retainedCandidate.TargetObjectId,
            candidates.Entries[1].TargetObjectId);
    }

    [Fact]
    public void RejectsNullInputsThroughDelegatedPolicies()
    {
        PassiveNoticingCandidate candidate = Candidate("seeker", "weathered-inscription");
        CharacterPerceptionSnapshot perception = Perception(
            "seeker",
            Visibility("weathered-inscription", VisibilityState.Unnoticed));
        PassiveNoticingCandidateSnapshot candidates = Candidates(candidate);

        Assert.Throws<ArgumentNullException>(
            () => PassiveNoticingCandidateResolutionPolicy.CanResolve(
                null!,
                perception,
                candidates));
        Assert.Throws<ArgumentNullException>(
            () => PassiveNoticingCandidateResolutionPolicy.CanResolve(
                candidate,
                null!,
                candidates));
        Assert.Throws<ArgumentNullException>(
            () => PassiveNoticingCandidateResolutionPolicy.CanResolve(
                candidate,
                perception,
                null!));
    }

    [Fact]
    public void ExposesOnlyTheSinglePureResolutionEligibilityOperation()
    {
        Type policyType = typeof(PassiveNoticingCandidateResolutionPolicy);
        System.Reflection.MethodInfo method = Assert.Single(
            policyType
                .GetMethods()
                .Where(candidate => candidate.DeclaringType == policyType));

        Assert.Equal("CanResolve", method.Name);
        Assert.Equal(typeof(bool), method.ReturnType);
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
        Assert.Empty(policyType.GetConstructors());
        Assert.Empty(policyType.GetProperties());
        Assert.Empty(policyType.GetEvents());
        Assert.True(policyType.IsAbstract);
        Assert.True(policyType.IsSealed);
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
