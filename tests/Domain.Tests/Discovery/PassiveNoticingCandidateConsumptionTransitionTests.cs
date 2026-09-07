using Allrpcs.Domain.Discovery;
using Allrpcs.Domain.Identifiers;

namespace Allrpcs.Domain.Tests.Discovery;

public sealed class PassiveNoticingCandidateConsumptionTransitionTests
{
    [Fact]
    public void RejectsCandidateWhenSnapshotIsEmptyOrCandidateIsAbsent()
    {
        PassiveNoticingCandidate candidate = Candidate("seeker", "weathered-inscription");
        PassiveNoticingCandidate unrelated = Candidate("travelling-healer", "healing-herb");

        Assert.Throws<InvalidOperationException>(
            () => PassiveNoticingCandidateConsumptionTransition.Consume(
                candidate,
                Candidates()));
        Assert.Throws<InvalidOperationException>(
            () => PassiveNoticingCandidateConsumptionTransition.Consume(
                candidate,
                Candidates(unrelated)));
    }

    [Fact]
    public void RejectsBothPartialCandidateMatches()
    {
        PassiveNoticingCandidate candidate = Candidate("seeker", "weathered-inscription");
        PassiveNoticingCandidate sameObserverDifferentTarget =
            Candidate("seeker", "healing-herb");
        PassiveNoticingCandidate differentObserverSameTarget =
            Candidate("travelling-healer", "weathered-inscription");
        PassiveNoticingCandidateSnapshot candidates =
            Candidates(sameObserverDifferentTarget, differentObserverSameTarget);

        Assert.Throws<InvalidOperationException>(
            () => PassiveNoticingCandidateConsumptionTransition.Consume(candidate, candidates));
        Assert.Collection(
            candidates.Entries,
            entry => Assert.Same(sameObserverDifferentTarget, entry),
            entry => Assert.Same(differentObserverSameTarget, entry));
    }

    [Fact]
    public void RemovesTheExactRetainedCandidateInstance()
    {
        PassiveNoticingCandidate candidate = Candidate("seeker", "weathered-inscription");

        PassiveNoticingCandidateSnapshot result =
            PassiveNoticingCandidateConsumptionTransition.Consume(
                candidate,
                Candidates(candidate));

        Assert.Empty(result.Entries);
    }

    [Fact]
    public void RemovesAValueEqualDistinctCandidate()
    {
        PassiveNoticingCandidate candidate = Candidate("seeker", "weathered-inscription");
        PassiveNoticingCandidate retained = Candidate("seeker", "weathered-inscription");

        PassiveNoticingCandidateSnapshot result =
            PassiveNoticingCandidateConsumptionTransition.Consume(
                candidate,
                Candidates(retained));

        Assert.NotSame(candidate, retained);
        Assert.NotSame(candidate.ObserverCharacterId, retained.ObserverCharacterId);
        Assert.NotSame(candidate.TargetObjectId, retained.TargetObjectId);
        Assert.Equal(candidate, retained);
        Assert.Empty(result.Entries);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    public void RemovesOnlyTheCandidateInItsStablePosition(int targetIndex)
    {
        PassiveNoticingCandidate candidate = Candidate("seeker", "weathered-inscription");
        PassiveNoticingCandidate first = Candidate("sage", "wagon-residue");
        PassiveNoticingCandidate retained = Candidate("seeker", "weathered-inscription");
        PassiveNoticingCandidate third = Candidate("travelling-healer", "healing-herb");
        PassiveNoticingCandidate[] entries = [first, retained, third];
        (entries[targetIndex], entries[1]) = (entries[1], entries[targetIndex]);
        PassiveNoticingCandidateSnapshot candidates = Candidates(entries);

        PassiveNoticingCandidateSnapshot result =
            PassiveNoticingCandidateConsumptionTransition.Consume(candidate, candidates);

        PassiveNoticingCandidate[] expected =
            entries.Where(entry => entry != retained).ToArray();
        Assert.Collection(
            result.Entries,
            entry => Assert.Same(expected[0], entry),
            entry => Assert.Same(expected[1], entry));
        Assert.Same(expected[0].ObserverCharacterId, result.Entries[0].ObserverCharacterId);
        Assert.Same(expected[0].TargetObjectId, result.Entries[0].TargetObjectId);
        Assert.Same(expected[1].ObserverCharacterId, result.Entries[1].ObserverCharacterId);
        Assert.Same(expected[1].TargetObjectId, result.Entries[1].TargetObjectId);
    }

    [Fact]
    public void LeavesEveryInputAndRetainedInstanceUnchanged()
    {
        CharacterId observerCharacterId = new("seeker");
        ObjectId targetObjectId = new("weathered-inscription");
        PassiveNoticingCandidate candidate = new(observerCharacterId, targetObjectId);
        PassiveNoticingCandidate before = Candidate("sage", "wagon-residue");
        PassiveNoticingCandidate retained = Candidate("seeker", "weathered-inscription");
        PassiveNoticingCandidate after = Candidate("travelling-healer", "healing-herb");
        PassiveNoticingCandidateSnapshot candidates = Candidates(before, retained, after);

        PassiveNoticingCandidateSnapshot result =
            PassiveNoticingCandidateConsumptionTransition.Consume(candidate, candidates);

        Assert.NotSame(candidates, result);
        Assert.Same(observerCharacterId, candidate.ObserverCharacterId);
        Assert.Same(targetObjectId, candidate.TargetObjectId);
        Assert.Collection(
            candidates.Entries,
            entry => Assert.Same(before, entry),
            entry => Assert.Same(retained, entry),
            entry => Assert.Same(after, entry));
        Assert.Same(retained.ObserverCharacterId, candidates.Entries[1].ObserverCharacterId);
        Assert.Same(retained.TargetObjectId, candidates.Entries[1].TargetObjectId);
        Assert.Collection(
            result.Entries,
            entry => Assert.Same(before, entry),
            entry => Assert.Same(after, entry));
    }

    [Fact]
    public void RejectsNullInputsThroughConsumptionPolicy()
    {
        PassiveNoticingCandidate candidate = Candidate("seeker", "weathered-inscription");
        PassiveNoticingCandidateSnapshot candidates = Candidates();

        Assert.Throws<ArgumentNullException>(
            () => PassiveNoticingCandidateConsumptionTransition.Consume(null!, candidates));
        Assert.Throws<ArgumentNullException>(
            () => PassiveNoticingCandidateConsumptionTransition.Consume(candidate, null!));
    }

    [Fact]
    public void ExposesOnlyTheSinglePureConsumptionOperation()
    {
        Type transitionType = typeof(PassiveNoticingCandidateConsumptionTransition);
        System.Reflection.MethodInfo method = Assert.Single(
            transitionType
                .GetMethods()
                .Where(candidate => candidate.DeclaringType == transitionType));

        Assert.Equal("Consume", method.Name);
        Assert.Equal(typeof(PassiveNoticingCandidateSnapshot), method.ReturnType);
        Assert.True(method.IsStatic);
        Assert.Collection(
            method.GetParameters(),
            parameter => Assert.Equal(
                typeof(PassiveNoticingCandidate),
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
}
