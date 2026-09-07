using Allrpcs.Domain.Discovery;
using Allrpcs.Domain.Identifiers;

namespace Allrpcs.Domain.Tests.Discovery;

public sealed class ObservationReinterpretationCandidateRecordingPolicyTests
{
    [Fact]
    public void AllowsEmptyAbsentAndEveryPartialCandidateMatch()
    {
        ObservationReinterpretationCandidate candidate = Candidate();
        ObservationReinterpretationCandidate[] nonDuplicates =
        [
            Candidate("travelling-healer", "weathered-inscription", "ancient-script"),
            Candidate("seeker", "healing-herb", "ancient-script"),
            Candidate("seeker", "weathered-inscription", "herbal-lore"),
            Candidate("travelling-healer", "healing-herb", "ancient-script"),
            Candidate("travelling-healer", "weathered-inscription", "herbal-lore"),
            Candidate("seeker", "healing-herb", "herbal-lore"),
        ];

        Assert.True(ObservationReinterpretationCandidateRecordingPolicy.CanRecord(
            candidate,
            Candidates()));
        Assert.True(ObservationReinterpretationCandidateRecordingPolicy.CanRecord(
            candidate,
            Candidates(nonDuplicates)));
    }

    [Fact]
    public void RejectsExactAndDistinctValueEqualRetainedCandidates()
    {
        ObservationReinterpretationCandidate candidate = Candidate();
        ObservationReinterpretationCandidate retained = Candidate();

        Assert.False(ObservationReinterpretationCandidateRecordingPolicy.CanRecord(
            candidate,
            Candidates(candidate)));
        Assert.NotSame(candidate, retained);
        Assert.Equal(candidate, retained);
        Assert.False(ObservationReinterpretationCandidateRecordingPolicy.CanRecord(
            candidate,
            Candidates(retained)));
    }

    [Fact]
    public void LeavesEveryInputAndNestedInstanceUnchanged()
    {
        CharacterId observerCharacterId = new("seeker");
        ObjectId targetObjectId = new("weathered-inscription");
        KnowledgeId triggerKnowledgeId = new("ancient-script");
        ObservationReinterpretationCandidate candidate = new(
            observerCharacterId,
            targetObjectId,
            triggerKnowledgeId);
        ObservationReinterpretationCandidate before = Candidate(
            "sage", "wagon-residue", "road-history");
        ObservationReinterpretationCandidate after = Candidate(
            "travelling-healer", "healing-herb", "herbal-lore");
        ObservationReinterpretationCandidateSnapshot candidates = Candidates(before, after);

        _ = ObservationReinterpretationCandidateRecordingPolicy.CanRecord(candidate, candidates);

        Assert.Same(observerCharacterId, candidate.ObserverCharacterId);
        Assert.Same(targetObjectId, candidate.TargetObjectId);
        Assert.Same(triggerKnowledgeId, candidate.TriggerKnowledgeId);
        Assert.Collection(
            candidates.Entries,
            entry => Assert.Same(before, entry),
            entry => Assert.Same(after, entry));
        Assert.Same(before.ObserverCharacterId, candidates.Entries[0].ObserverCharacterId);
        Assert.Same(before.TargetObjectId, candidates.Entries[0].TargetObjectId);
        Assert.Same(before.TriggerKnowledgeId, candidates.Entries[0].TriggerKnowledgeId);
    }

    [Fact]
    public void RejectsNullInputsThroughDelegatedLookup()
    {
        ObservationReinterpretationCandidate candidate = Candidate();
        ObservationReinterpretationCandidateSnapshot candidates = Candidates();

        Assert.Throws<ArgumentNullException>(() =>
            ObservationReinterpretationCandidateRecordingPolicy.CanRecord(null!, candidates));
        Assert.Throws<ArgumentNullException>(() =>
            ObservationReinterpretationCandidateRecordingPolicy.CanRecord(candidate, null!));
    }

    [Fact]
    public void ExposesOnlyTheSinglePureRecordingEligibilityOperation()
    {
        Type policyType = typeof(ObservationReinterpretationCandidateRecordingPolicy);
        System.Reflection.MethodInfo method = Assert.Single(
            policyType.GetMethods().Where(candidate => candidate.DeclaringType == policyType));

        Assert.Equal("CanRecord", method.Name);
        Assert.Equal(typeof(bool), method.ReturnType);
        Assert.True(method.IsStatic);
        Assert.Collection(
            method.GetParameters(),
            parameter => Assert.Equal(
                typeof(ObservationReinterpretationCandidate), parameter.ParameterType),
            parameter => Assert.Equal(
                typeof(ObservationReinterpretationCandidateSnapshot), parameter.ParameterType));
        Assert.Empty(policyType.GetConstructors());
        Assert.Empty(policyType.GetProperties());
        Assert.Empty(policyType.GetEvents());
        Assert.True(policyType.IsAbstract);
        Assert.True(policyType.IsSealed);
    }

    private static ObservationReinterpretationCandidate Candidate() =>
        Candidate("seeker", "weathered-inscription", "ancient-script");

    private static ObservationReinterpretationCandidate Candidate(
        string observerCharacterId,
        string targetObjectId,
        string triggerKnowledgeId) =>
        new(
            new CharacterId(observerCharacterId),
            new ObjectId(targetObjectId),
            new KnowledgeId(triggerKnowledgeId));

    private static ObservationReinterpretationCandidateSnapshot Candidates(
        params ObservationReinterpretationCandidate[] entries) =>
        new(entries);
}
