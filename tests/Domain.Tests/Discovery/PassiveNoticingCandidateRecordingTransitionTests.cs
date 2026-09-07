using Allrpcs.Domain.Discovery;
using Allrpcs.Domain.Identifiers;

namespace Allrpcs.Domain.Tests.Discovery;

public sealed class PassiveNoticingCandidateRecordingTransitionTests
{
    [Fact]
    public void RecordsCandidateWhenSnapshotIsEmptyOrCompleteCandidateIsAbsent()
    {
        PassiveNoticingCandidate candidate = Candidate("seeker", "weathered-inscription");
        PassiveNoticingCandidate unrelated = Candidate("travelling-healer", "healing-herb");

        PassiveNoticingCandidateSnapshot emptyResult =
            PassiveNoticingCandidateRecordingTransition.Record(candidate, Candidates());
        PassiveNoticingCandidateSnapshot absentResult =
            PassiveNoticingCandidateRecordingTransition.Record(
                candidate,
                Candidates(unrelated));

        Assert.Collection(emptyResult.Entries, entry => Assert.Same(candidate, entry));
        Assert.Collection(
            absentResult.Entries,
            entry => Assert.Same(unrelated, entry),
            entry => Assert.Same(candidate, entry));
    }

    [Fact]
    public void RecordsCandidateAlongsideBothPartialMatches()
    {
        PassiveNoticingCandidate candidate = Candidate("seeker", "weathered-inscription");
        PassiveNoticingCandidate sameObserverDifferentTarget =
            Candidate("seeker", "healing-herb");
        PassiveNoticingCandidate differentObserverSameTarget =
            Candidate("travelling-healer", "weathered-inscription");

        PassiveNoticingCandidateSnapshot result =
            PassiveNoticingCandidateRecordingTransition.Record(
                candidate,
                Candidates(sameObserverDifferentTarget, differentObserverSameTarget));

        Assert.Collection(
            result.Entries,
            entry => Assert.Same(sameObserverDifferentTarget, entry),
            entry => Assert.Same(differentObserverSameTarget, entry),
            entry => Assert.Same(candidate, entry));
    }

    [Fact]
    public void RejectsTheExactRetainedCandidateInstance()
    {
        PassiveNoticingCandidate candidate = Candidate("seeker", "weathered-inscription");
        PassiveNoticingCandidateSnapshot candidates = Candidates(candidate);

        Assert.Throws<InvalidOperationException>(
            () => PassiveNoticingCandidateRecordingTransition.Record(candidate, candidates));
        Assert.Collection(candidates.Entries, entry => Assert.Same(candidate, entry));
    }

    [Fact]
    public void RejectsAValueEqualDistinctRetainedCandidate()
    {
        PassiveNoticingCandidate candidate = Candidate("seeker", "weathered-inscription");
        PassiveNoticingCandidate retained = Candidate("seeker", "weathered-inscription");
        PassiveNoticingCandidateSnapshot candidates = Candidates(retained);

        Assert.NotSame(candidate, retained);
        Assert.NotSame(candidate.ObserverCharacterId, retained.ObserverCharacterId);
        Assert.NotSame(candidate.TargetObjectId, retained.TargetObjectId);
        Assert.Equal(candidate, retained);
        Assert.Throws<InvalidOperationException>(
            () => PassiveNoticingCandidateRecordingTransition.Record(candidate, candidates));
        Assert.Collection(candidates.Entries, entry => Assert.Same(retained, entry));
    }

    [Fact]
    public void RetainsStableOrderAndAppendsTheExactCandidateInstance()
    {
        PassiveNoticingCandidate candidate = Candidate("seeker", "weathered-inscription");
        PassiveNoticingCandidate first = Candidate("sage", "wagon-residue");
        PassiveNoticingCandidate second = Candidate("travelling-healer", "healing-herb");

        PassiveNoticingCandidateSnapshot result =
            PassiveNoticingCandidateRecordingTransition.Record(
                candidate,
                Candidates(first, second));

        Assert.Collection(
            result.Entries,
            entry => Assert.Same(first, entry),
            entry => Assert.Same(second, entry),
            entry => Assert.Same(candidate, entry));
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

        PassiveNoticingCandidateSnapshot result =
            PassiveNoticingCandidateRecordingTransition.Record(candidate, candidates);

        Assert.NotSame(candidates, result);
        Assert.Same(observerCharacterId, candidate.ObserverCharacterId);
        Assert.Same(targetObjectId, candidate.TargetObjectId);
        Assert.Collection(
            candidates.Entries,
            entry => Assert.Same(before, entry),
            entry => Assert.Same(after, entry));
        Assert.Collection(
            result.Entries,
            entry => Assert.Same(before, entry),
            entry => Assert.Same(after, entry),
            entry => Assert.Same(candidate, entry));
    }

    [Fact]
    public void RejectsNullInputsThroughRecordingPolicy()
    {
        PassiveNoticingCandidate candidate = Candidate("seeker", "weathered-inscription");
        PassiveNoticingCandidateSnapshot candidates = Candidates();

        Assert.Throws<ArgumentNullException>(
            () => PassiveNoticingCandidateRecordingTransition.Record(null!, candidates));
        Assert.Throws<ArgumentNullException>(
            () => PassiveNoticingCandidateRecordingTransition.Record(candidate, null!));
    }

    [Fact]
    public void ExposesOnlyTheSinglePureRecordingOperation()
    {
        Type transitionType = typeof(PassiveNoticingCandidateRecordingTransition);
        System.Reflection.MethodInfo method = Assert.Single(
            transitionType
                .GetMethods()
                .Where(candidate => candidate.DeclaringType == transitionType));

        Assert.Equal("Record", method.Name);
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
