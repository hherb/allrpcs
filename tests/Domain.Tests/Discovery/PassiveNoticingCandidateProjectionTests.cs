using Allrpcs.Domain.Discovery;
using Allrpcs.Domain.Identifiers;

namespace Allrpcs.Domain.Tests.Discovery;

public sealed class PassiveNoticingCandidateProjectionTests
{
    [Fact]
    public void ReturnsEmptySnapshotWhenThereAreNoCandidates()
    {
        CharacterId observerCharacterId = new("seeker");
        PassiveNoticingCandidateSnapshot candidates = Candidates();

        PassiveNoticingCandidateSnapshot result =
            PassiveNoticingCandidateProjection.ForCharacter(observerCharacterId, candidates);

        Assert.Empty(result.Entries);
        Assert.NotSame(candidates, result);
    }

    [Fact]
    public void ReturnsEmptySnapshotWhenAllCandidatesAreUnrelated()
    {
        CharacterId observerCharacterId = new("seeker");
        PassiveNoticingCandidate first = Candidate("travelling-healer", "healing-herb");
        PassiveNoticingCandidate second = Candidate("sage", "weathered-inscription");
        PassiveNoticingCandidateSnapshot candidates = new(new[] { first, second });

        PassiveNoticingCandidateSnapshot result =
            PassiveNoticingCandidateProjection.ForCharacter(observerCharacterId, candidates);

        Assert.Empty(result.Entries);
    }

    [Fact]
    public void ReturnsTheExactCandidateForAValueEqualObserverIdentifier()
    {
        CharacterId observerCharacterId = new("seeker");
        CharacterId candidateObserverCharacterId = new("seeker");
        PassiveNoticingCandidate matching = new(
            candidateObserverCharacterId,
            new ObjectId("weathered-inscription"));
        PassiveNoticingCandidateSnapshot candidates = new(new[] { matching });

        PassiveNoticingCandidateSnapshot result =
            PassiveNoticingCandidateProjection.ForCharacter(observerCharacterId, candidates);

        Assert.NotSame(observerCharacterId, candidateObserverCharacterId);
        Assert.Equal(observerCharacterId, candidateObserverCharacterId);
        Assert.Collection(result.Entries, entry => Assert.Same(matching, entry));
        Assert.Same(candidateObserverCharacterId, result.Entries[0].ObserverCharacterId);
    }

    [Fact]
    public void ReturnsAllMatchesInStableOrderAndOmitsUnrelatedCandidates()
    {
        CharacterId observerCharacterId = new("seeker");
        PassiveNoticingCandidate firstMatch = new(
            observerCharacterId,
            new ObjectId("weathered-inscription"));
        PassiveNoticingCandidate unrelated = Candidate("sage", "wagon-residue");
        PassiveNoticingCandidate secondMatch = Candidate("seeker", "healing-herb");
        PassiveNoticingCandidateSnapshot candidates =
            new(new[] { firstMatch, unrelated, secondMatch });

        PassiveNoticingCandidateSnapshot result =
            PassiveNoticingCandidateProjection.ForCharacter(observerCharacterId, candidates);

        Assert.Collection(
            result.Entries,
            entry => Assert.Same(firstMatch, entry),
            entry => Assert.Same(secondMatch, entry));
        Assert.Same(observerCharacterId, result.Entries[0].ObserverCharacterId);
        Assert.NotSame(observerCharacterId, result.Entries[1].ObserverCharacterId);
        Assert.Equal(observerCharacterId, result.Entries[1].ObserverCharacterId);
    }

    [Fact]
    public void RejectsNullInputs()
    {
        CharacterId observerCharacterId = new("seeker");
        PassiveNoticingCandidateSnapshot candidates = Candidates();

        Assert.Throws<ArgumentNullException>(
            () => PassiveNoticingCandidateProjection.ForCharacter(null!, candidates));
        Assert.Throws<ArgumentNullException>(
            () => PassiveNoticingCandidateProjection.ForCharacter(observerCharacterId, null!));
    }

    [Fact]
    public void DoesNotMutateInputs()
    {
        CharacterId observerCharacterId = new("seeker");
        PassiveNoticingCandidate firstMatch = Candidate("seeker", "weathered-inscription");
        PassiveNoticingCandidate unrelated = Candidate("sage", "wagon-residue");
        PassiveNoticingCandidate secondMatch = Candidate("seeker", "healing-herb");
        PassiveNoticingCandidateSnapshot candidates =
            new(new[] { firstMatch, unrelated, secondMatch });

        PassiveNoticingCandidateSnapshot result =
            PassiveNoticingCandidateProjection.ForCharacter(observerCharacterId, candidates);

        Assert.Equal("seeker", observerCharacterId.Value);
        Assert.Collection(
            candidates.Entries,
            entry => Assert.Same(firstMatch, entry),
            entry => Assert.Same(unrelated, entry),
            entry => Assert.Same(secondMatch, entry));
        Assert.NotSame(candidates, result);
    }

    [Fact]
    public void ExposesOnlyTheSinglePureProjectionOperation()
    {
        Type projectionType = typeof(PassiveNoticingCandidateProjection);
        System.Reflection.MethodInfo method = Assert.Single(
            projectionType
                .GetMethods()
                .Where(candidate => candidate.DeclaringType == projectionType));

        Assert.Equal("ForCharacter", method.Name);
        Assert.Equal(typeof(PassiveNoticingCandidateSnapshot), method.ReturnType);
        Assert.True(method.IsStatic);
        Assert.Collection(
            method.GetParameters(),
            parameter => Assert.Equal(typeof(CharacterId), parameter.ParameterType),
            parameter => Assert.Equal(
                typeof(PassiveNoticingCandidateSnapshot),
                parameter.ParameterType));
        Assert.Empty(projectionType.GetConstructors());
        Assert.Empty(projectionType.GetProperties());
        Assert.Empty(projectionType.GetEvents());
        Assert.True(projectionType.IsAbstract);
        Assert.True(projectionType.IsSealed);
    }

    private static PassiveNoticingCandidate Candidate(
        string observerCharacterId,
        string targetObjectId) =>
        new(new CharacterId(observerCharacterId), new ObjectId(targetObjectId));

    private static PassiveNoticingCandidateSnapshot Candidates(
        params (string ObserverCharacterId, string TargetObjectId)[] entries) =>
        new(entries.Select(entry => Candidate(entry.ObserverCharacterId, entry.TargetObjectId)));
}
