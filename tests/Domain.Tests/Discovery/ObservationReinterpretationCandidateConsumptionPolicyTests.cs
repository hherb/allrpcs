using Allrpcs.Domain.Discovery;
using Allrpcs.Domain.Identifiers;

namespace Allrpcs.Domain.Tests.Discovery;

public sealed class ObservationReinterpretationCandidateConsumptionPolicyTests
{
    [Fact]
    public void RejectsCandidateWhenSnapshotIsEmptyOrCandidateIsAbsent()
    {
        ObservationReinterpretationCandidate candidate = Candidate(
            "seeker",
            "weathered-inscription",
            "ancient-script");
        ObservationReinterpretationCandidate unrelated = Candidate(
            "travelling-healer",
            "healing-herb",
            "herbal-lore");

        Assert.False(ObservationReinterpretationCandidateConsumptionPolicy.CanConsume(
            candidate,
            Candidates()));
        Assert.False(ObservationReinterpretationCandidateConsumptionPolicy.CanConsume(
            candidate,
            Candidates(unrelated)));
    }

    [Fact]
    public void AllowsExactAndDistinctValueEqualCandidates()
    {
        ObservationReinterpretationCandidate candidate = Candidate(
            "seeker",
            "weathered-inscription",
            "ancient-script");
        ObservationReinterpretationCandidate retained = Candidate(
            "seeker",
            "weathered-inscription",
            "ancient-script");

        Assert.True(ObservationReinterpretationCandidateConsumptionPolicy.CanConsume(
            candidate,
            Candidates(candidate)));
        Assert.NotSame(candidate, retained);
        Assert.Equal(candidate, retained);
        Assert.True(ObservationReinterpretationCandidateConsumptionPolicy.CanConsume(
            candidate,
            Candidates(retained)));
    }

    [Fact]
    public void RejectsEveryPartialCandidateMatch()
    {
        ObservationReinterpretationCandidate candidate = Candidate(
            "seeker",
            "weathered-inscription",
            "ancient-script");
        ObservationReinterpretationCandidateSnapshot candidates = Candidates(
            Candidate("travelling-healer", "weathered-inscription", "ancient-script"),
            Candidate("seeker", "healing-herb", "ancient-script"),
            Candidate("seeker", "weathered-inscription", "herbal-lore"));

        Assert.False(ObservationReinterpretationCandidateConsumptionPolicy.CanConsume(
            candidate,
            candidates));
    }

    [Fact]
    public void FindsMatchingCandidateAmongUnrelatedEntriesWithoutChangingInputs()
    {
        CharacterId observerCharacterId = new("seeker");
        ObjectId targetObjectId = new("weathered-inscription");
        KnowledgeId triggerKnowledgeId = new("ancient-script");
        ObservationReinterpretationCandidate candidate = new(
            observerCharacterId,
            targetObjectId,
            triggerKnowledgeId);
        ObservationReinterpretationCandidate before = Candidate(
            "sage",
            "wagon-residue",
            "road-history");
        ObservationReinterpretationCandidate retained = Candidate(
            "seeker",
            "weathered-inscription",
            "ancient-script");
        ObservationReinterpretationCandidate after = Candidate(
            "travelling-healer",
            "healing-herb",
            "herbal-lore");
        ObservationReinterpretationCandidateSnapshot candidates = Candidates(
            before,
            retained,
            after);

        Assert.True(ObservationReinterpretationCandidateConsumptionPolicy.CanConsume(
            candidate,
            candidates));
        Assert.Same(observerCharacterId, candidate.ObserverCharacterId);
        Assert.Same(targetObjectId, candidate.TargetObjectId);
        Assert.Same(triggerKnowledgeId, candidate.TriggerKnowledgeId);
        Assert.Collection(
            candidates.Entries,
            entry => Assert.Same(before, entry),
            entry => Assert.Same(retained, entry),
            entry => Assert.Same(after, entry));
        Assert.Same(retained.ObserverCharacterId, candidates.Entries[1].ObserverCharacterId);
        Assert.Same(retained.TargetObjectId, candidates.Entries[1].TargetObjectId);
        Assert.Same(retained.TriggerKnowledgeId, candidates.Entries[1].TriggerKnowledgeId);
    }

    [Fact]
    public void RejectsNullInputs()
    {
        ObservationReinterpretationCandidate candidate = Candidate(
            "seeker",
            "weathered-inscription",
            "ancient-script");
        ObservationReinterpretationCandidateSnapshot candidates = Candidates();

        Assert.Throws<ArgumentNullException>(() =>
            ObservationReinterpretationCandidateConsumptionPolicy.CanConsume(null!, candidates));
        Assert.Throws<ArgumentNullException>(() =>
            ObservationReinterpretationCandidateConsumptionPolicy.CanConsume(candidate, null!));
    }

    [Fact]
    public void ExposesOnlyTheSinglePureConsumptionEligibilityOperation()
    {
        Type policyType = typeof(ObservationReinterpretationCandidateConsumptionPolicy);
        System.Reflection.MethodInfo method = Assert.Single(
            policyType.GetMethods().Where(candidate => candidate.DeclaringType == policyType));

        Assert.Equal("CanConsume", method.Name);
        Assert.Equal(typeof(bool), method.ReturnType);
        Assert.True(method.IsStatic);
        Assert.Collection(
            method.GetParameters(),
            parameter => Assert.Equal(
                typeof(ObservationReinterpretationCandidate),
                parameter.ParameterType),
            parameter => Assert.Equal(
                typeof(ObservationReinterpretationCandidateSnapshot),
                parameter.ParameterType));
        Assert.Empty(policyType.GetConstructors());
        Assert.Empty(policyType.GetProperties());
        Assert.Empty(policyType.GetEvents());
        Assert.True(policyType.IsAbstract);
        Assert.True(policyType.IsSealed);
    }

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
