using Allrpcs.Domain.Discovery;
using Allrpcs.Domain.Identifiers;

namespace Allrpcs.Domain.Tests.Discovery;

public sealed class PassiveNoticingCandidateLookupTests
{
    [Fact]
    public void ReturnsNullForEmptyOrUnrelatedSnapshots()
    {
        PassiveNoticingCandidate candidate = Candidate("seeker", "weathered-inscription");
        PassiveNoticingCandidate unrelated = Candidate("travelling-healer", "healing-herb");

        PassiveNoticingCandidate? emptyResult = PassiveNoticingCandidateLookup.Find(
            candidate,
            Candidates());
        PassiveNoticingCandidate? unrelatedResult = PassiveNoticingCandidateLookup.Find(
            candidate,
            Candidates(unrelated));

        Assert.Null(emptyResult);
        Assert.Null(unrelatedResult);
    }

    [Fact]
    public void MatchesADistinctValueEqualCandidate()
    {
        PassiveNoticingCandidate candidate = Candidate("seeker", "weathered-inscription");
        CharacterId retainedObserverCharacterId = new("seeker");
        ObjectId retainedTargetObjectId = new("weathered-inscription");
        PassiveNoticingCandidate retained = new(
            retainedObserverCharacterId,
            retainedTargetObjectId);
        PassiveNoticingCandidateSnapshot candidates = Candidates(retained);

        PassiveNoticingCandidate? result = PassiveNoticingCandidateLookup.Find(
            candidate,
            candidates);

        Assert.NotSame(candidate, retained);
        Assert.Equal(candidate, retained);
        Assert.Same(retained, result);
        Assert.Same(retainedObserverCharacterId, result!.ObserverCharacterId);
        Assert.Same(retainedTargetObjectId, result.TargetObjectId);
    }

    [Fact]
    public void RejectsBothPartialCandidateMatches()
    {
        PassiveNoticingCandidate candidate = Candidate("seeker", "weathered-inscription");
        PassiveNoticingCandidate sameObserverDifferentTarget =
            Candidate("seeker", "healing-herb");
        PassiveNoticingCandidate differentObserverSameTarget =
            Candidate("travelling-healer", "weathered-inscription");
        PassiveNoticingCandidateSnapshot candidates = Candidates(
            sameObserverDifferentTarget,
            differentObserverSameTarget);

        PassiveNoticingCandidate? result = PassiveNoticingCandidateLookup.Find(
            candidate,
            candidates);

        Assert.Null(result);
    }

    [Fact]
    public void ReturnsTheExactMatchAmongSurroundingUnrelatedCandidates()
    {
        PassiveNoticingCandidate candidate = Candidate("seeker", "weathered-inscription");
        PassiveNoticingCandidate before = Candidate("sage", "wagon-residue");
        PassiveNoticingCandidate matching = Candidate("seeker", "weathered-inscription");
        PassiveNoticingCandidate after = Candidate("seeker", "healing-herb");
        PassiveNoticingCandidateSnapshot candidates = Candidates(before, matching, after);

        PassiveNoticingCandidate? result = PassiveNoticingCandidateLookup.Find(
            candidate,
            candidates);

        Assert.Same(matching, result);
    }

    [Fact]
    public void RetainsTheExactCandidateAndNestedIdentifierInstances()
    {
        CharacterId observerCharacterId = new("seeker");
        ObjectId targetObjectId = new("weathered-inscription");
        PassiveNoticingCandidate matching = new(observerCharacterId, targetObjectId);
        PassiveNoticingCandidateSnapshot candidates = Candidates(matching);

        PassiveNoticingCandidate? result = PassiveNoticingCandidateLookup.Find(
            Candidate("seeker", "weathered-inscription"),
            candidates);

        Assert.Same(matching, result);
        Assert.Same(observerCharacterId, result!.ObserverCharacterId);
        Assert.Same(targetObjectId, result.TargetObjectId);
    }

    [Fact]
    public void RejectsNullInputs()
    {
        PassiveNoticingCandidate candidate = Candidate("seeker", "weathered-inscription");
        PassiveNoticingCandidateSnapshot candidates = Candidates();

        Assert.Throws<ArgumentNullException>(
            () => PassiveNoticingCandidateLookup.Find(null!, candidates));
        Assert.Throws<ArgumentNullException>(
            () => PassiveNoticingCandidateLookup.Find(candidate, null!));
    }

    [Fact]
    public void DoesNotMutateAnyInput()
    {
        PassiveNoticingCandidate candidate = Candidate("seeker", "weathered-inscription");
        PassiveNoticingCandidate matching = Candidate("seeker", "weathered-inscription");
        PassiveNoticingCandidate unrelated = Candidate("sage", "wagon-residue");
        PassiveNoticingCandidateSnapshot candidates = Candidates(matching, unrelated);
        CharacterId observerCharacterId = candidate.ObserverCharacterId;
        ObjectId targetObjectId = candidate.TargetObjectId;

        PassiveNoticingCandidate? result = PassiveNoticingCandidateLookup.Find(
            candidate,
            candidates);

        Assert.Same(matching, result);
        Assert.Same(observerCharacterId, candidate.ObserverCharacterId);
        Assert.Same(targetObjectId, candidate.TargetObjectId);
        Assert.Collection(
            candidates.Entries,
            entry => Assert.Same(matching, entry),
            entry => Assert.Same(unrelated, entry));
    }

    [Fact]
    public void ExposesOnlyTheSinglePureLookupOperation()
    {
        Type lookupType = typeof(PassiveNoticingCandidateLookup);
        System.Reflection.MethodInfo method = Assert.Single(
            lookupType
                .GetMethods()
                .Where(candidate => candidate.DeclaringType == lookupType));

        Assert.Equal("Find", method.Name);
        Assert.Equal(typeof(PassiveNoticingCandidate), method.ReturnType);
        Assert.True(method.IsStatic);
        Assert.Collection(
            method.GetParameters(),
            parameter => Assert.Equal(
                typeof(PassiveNoticingCandidate),
                parameter.ParameterType),
            parameter => Assert.Equal(
                typeof(PassiveNoticingCandidateSnapshot),
                parameter.ParameterType));
        Assert.Empty(lookupType.GetConstructors());
        Assert.Empty(lookupType.GetProperties());
        Assert.Empty(lookupType.GetEvents());
        Assert.True(lookupType.IsAbstract);
        Assert.True(lookupType.IsSealed);
    }

    private static PassiveNoticingCandidate Candidate(
        string observerCharacterId,
        string targetObjectId) =>
        new(new CharacterId(observerCharacterId), new ObjectId(targetObjectId));

    private static PassiveNoticingCandidateSnapshot Candidates(
        params PassiveNoticingCandidate[] entries) =>
        new(entries);
}
