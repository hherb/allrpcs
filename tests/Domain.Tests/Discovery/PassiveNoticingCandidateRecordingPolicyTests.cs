using Allrpcs.Domain.Discovery;
using Allrpcs.Domain.Identifiers;

namespace Allrpcs.Domain.Tests.Discovery;

public sealed class PassiveNoticingCandidateRecordingPolicyTests
{
    [Fact]
    public void AllowsCandidateWhenSnapshotIsEmptyOrCompleteCandidateIsAbsent()
    {
        PassiveNoticingCandidate candidate = Candidate("seeker", "weathered-inscription");
        PassiveNoticingCandidate unrelated = Candidate("travelling-healer", "healing-herb");

        bool emptyResult = PassiveNoticingCandidateRecordingPolicy.CanRecord(
            candidate,
            Candidates());
        bool absentResult = PassiveNoticingCandidateRecordingPolicy.CanRecord(
            candidate,
            Candidates(unrelated));

        Assert.True(emptyResult);
        Assert.True(absentResult);
    }

    [Fact]
    public void AllowsBothPartialCandidateMatches()
    {
        PassiveNoticingCandidate candidate = Candidate("seeker", "weathered-inscription");
        PassiveNoticingCandidate sameObserverDifferentTarget =
            Candidate("seeker", "healing-herb");
        PassiveNoticingCandidate differentObserverSameTarget =
            Candidate("travelling-healer", "weathered-inscription");

        bool result = PassiveNoticingCandidateRecordingPolicy.CanRecord(
            candidate,
            Candidates(sameObserverDifferentTarget, differentObserverSameTarget));

        Assert.True(result);
    }

    [Fact]
    public void RejectsTheExactRetainedCandidateInstance()
    {
        PassiveNoticingCandidate candidate = Candidate("seeker", "weathered-inscription");

        bool result = PassiveNoticingCandidateRecordingPolicy.CanRecord(
            candidate,
            Candidates(candidate));

        Assert.False(result);
    }

    [Fact]
    public void RejectsAValueEqualDistinctRetainedCandidate()
    {
        PassiveNoticingCandidate candidate = Candidate("seeker", "weathered-inscription");
        PassiveNoticingCandidate retained = Candidate("seeker", "weathered-inscription");

        bool result = PassiveNoticingCandidateRecordingPolicy.CanRecord(
            candidate,
            Candidates(retained));

        Assert.NotSame(candidate, retained);
        Assert.NotSame(candidate.ObserverCharacterId, retained.ObserverCharacterId);
        Assert.NotSame(candidate.TargetObjectId, retained.TargetObjectId);
        Assert.Equal(candidate, retained);
        Assert.False(result);
    }

    [Fact]
    public void LeavesEveryInputAndRetainedInstanceUnchanged()
    {
        CharacterId observerCharacterId = new("seeker");
        ObjectId targetObjectId = new("weathered-inscription");
        PassiveNoticingCandidate candidate = new(observerCharacterId, targetObjectId);
        PassiveNoticingCandidate before = Candidate("sage", "wagon-residue");
        PassiveNoticingCandidate after = Candidate("travelling-healer", "healing-herb");
        PassiveNoticingCandidateSnapshot candidates = Candidates(before, after);

        _ = PassiveNoticingCandidateRecordingPolicy.CanRecord(candidate, candidates);

        Assert.Same(observerCharacterId, candidate.ObserverCharacterId);
        Assert.Same(targetObjectId, candidate.TargetObjectId);
        Assert.Collection(
            candidates.Entries,
            entry => Assert.Same(before, entry),
            entry => Assert.Same(after, entry));
        Assert.Same(before.ObserverCharacterId, candidates.Entries[0].ObserverCharacterId);
        Assert.Same(before.TargetObjectId, candidates.Entries[0].TargetObjectId);
        Assert.Same(after.ObserverCharacterId, candidates.Entries[1].ObserverCharacterId);
        Assert.Same(after.TargetObjectId, candidates.Entries[1].TargetObjectId);
    }

    [Fact]
    public void RejectsNullInputsThroughDelegatedLookup()
    {
        PassiveNoticingCandidate candidate = Candidate("seeker", "weathered-inscription");
        PassiveNoticingCandidateSnapshot candidates = Candidates();

        Assert.Throws<ArgumentNullException>(
            () => PassiveNoticingCandidateRecordingPolicy.CanRecord(null!, candidates));
        Assert.Throws<ArgumentNullException>(
            () => PassiveNoticingCandidateRecordingPolicy.CanRecord(candidate, null!));
    }

    [Fact]
    public void ExposesOnlyTheSinglePureRecordingEligibilityOperation()
    {
        Type policyType = typeof(PassiveNoticingCandidateRecordingPolicy);
        System.Reflection.MethodInfo method = Assert.Single(
            policyType
                .GetMethods()
                .Where(candidate => candidate.DeclaringType == policyType));

        Assert.Equal("CanRecord", method.Name);
        Assert.Equal(typeof(bool), method.ReturnType);
        Assert.True(method.IsStatic);
        Assert.Collection(
            method.GetParameters(),
            parameter => Assert.Equal(
                typeof(PassiveNoticingCandidate),
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
}
