using Allrpcs.Domain.Discovery;
using Allrpcs.Domain.Identifiers;

namespace Allrpcs.Domain.Tests.Discovery;

public sealed class PassiveNoticingCandidateConsumptionPolicyTests
{
    [Fact]
    public void RejectsCandidateWhenSnapshotIsEmptyOrCandidateIsAbsent()
    {
        PassiveNoticingCandidate candidate = Candidate("seeker", "weathered-inscription");
        PassiveNoticingCandidate unrelated = Candidate("travelling-healer", "healing-herb");

        bool emptyResult = PassiveNoticingCandidateConsumptionPolicy.CanConsume(
            candidate,
            Candidates());
        bool absentResult = PassiveNoticingCandidateConsumptionPolicy.CanConsume(
            candidate,
            Candidates(unrelated));

        Assert.False(emptyResult);
        Assert.False(absentResult);
    }

    [Fact]
    public void AllowsTheExactRetainedCandidateInstance()
    {
        PassiveNoticingCandidate candidate = Candidate("seeker", "weathered-inscription");

        bool result = PassiveNoticingCandidateConsumptionPolicy.CanConsume(
            candidate,
            Candidates(candidate));

        Assert.True(result);
    }

    [Fact]
    public void AllowsAValueEqualDistinctCandidate()
    {
        PassiveNoticingCandidate candidate = Candidate("seeker", "weathered-inscription");
        PassiveNoticingCandidate retained = Candidate("seeker", "weathered-inscription");

        bool result = PassiveNoticingCandidateConsumptionPolicy.CanConsume(
            candidate,
            Candidates(retained));

        Assert.NotSame(candidate, retained);
        Assert.NotSame(candidate.ObserverCharacterId, retained.ObserverCharacterId);
        Assert.NotSame(candidate.TargetObjectId, retained.TargetObjectId);
        Assert.Equal(candidate, retained);
        Assert.True(result);
    }

    [Fact]
    public void RejectsBothPartialCandidateMatches()
    {
        PassiveNoticingCandidate candidate = Candidate("seeker", "weathered-inscription");
        PassiveNoticingCandidate sameObserverDifferentTarget =
            Candidate("seeker", "healing-herb");
        PassiveNoticingCandidate differentObserverSameTarget =
            Candidate("travelling-healer", "weathered-inscription");

        bool result = PassiveNoticingCandidateConsumptionPolicy.CanConsume(
            candidate,
            Candidates(sameObserverDifferentTarget, differentObserverSameTarget));

        Assert.False(result);
    }

    [Fact]
    public void SelectsTheMatchingCandidateAmongUnrelatedEntries()
    {
        PassiveNoticingCandidate candidate = Candidate("seeker", "weathered-inscription");
        PassiveNoticingCandidate before = Candidate("sage", "wagon-residue");
        PassiveNoticingCandidate retained = Candidate("seeker", "weathered-inscription");
        PassiveNoticingCandidate after = Candidate("travelling-healer", "healing-herb");

        bool result = PassiveNoticingCandidateConsumptionPolicy.CanConsume(
            candidate,
            Candidates(before, retained, after));

        Assert.True(result);
    }

    [Fact]
    public void LeavesEveryInputAndRetainedInstanceUnchanged()
    {
        CharacterId observerCharacterId = new("seeker");
        ObjectId targetObjectId = new("weathered-inscription");
        PassiveNoticingCandidate candidate = new(observerCharacterId, targetObjectId);
        PassiveNoticingCandidate unrelated = Candidate("sage", "wagon-residue");
        PassiveNoticingCandidate retained = Candidate("seeker", "weathered-inscription");
        PassiveNoticingCandidateSnapshot candidates = Candidates(unrelated, retained);

        _ = PassiveNoticingCandidateConsumptionPolicy.CanConsume(candidate, candidates);

        Assert.Same(observerCharacterId, candidate.ObserverCharacterId);
        Assert.Same(targetObjectId, candidate.TargetObjectId);
        Assert.Collection(
            candidates.Entries,
            entry => Assert.Same(unrelated, entry),
            entry => Assert.Same(retained, entry));
        Assert.Same(retained.ObserverCharacterId, candidates.Entries[1].ObserverCharacterId);
        Assert.Same(retained.TargetObjectId, candidates.Entries[1].TargetObjectId);
    }

    [Fact]
    public void RejectsNullInputs()
    {
        PassiveNoticingCandidate candidate = Candidate("seeker", "weathered-inscription");
        PassiveNoticingCandidateSnapshot candidates = Candidates();

        Assert.Throws<ArgumentNullException>(
            () => PassiveNoticingCandidateConsumptionPolicy.CanConsume(null!, candidates));
        Assert.Throws<ArgumentNullException>(
            () => PassiveNoticingCandidateConsumptionPolicy.CanConsume(candidate, null!));
    }

    [Fact]
    public void ExposesOnlyTheSinglePureConsumptionEligibilityOperation()
    {
        Type policyType = typeof(PassiveNoticingCandidateConsumptionPolicy);
        System.Reflection.MethodInfo method = Assert.Single(
            policyType
                .GetMethods()
                .Where(candidate => candidate.DeclaringType == policyType));

        Assert.Equal("CanConsume", method.Name);
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
