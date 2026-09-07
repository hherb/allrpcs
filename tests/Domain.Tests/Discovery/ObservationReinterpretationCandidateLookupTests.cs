using Allrpcs.Domain.Discovery;
using Allrpcs.Domain.Identifiers;

namespace Allrpcs.Domain.Tests.Discovery;

public sealed class ObservationReinterpretationCandidateLookupTests
{
    [Fact]
    public void ReturnsNullForEmptyOrUnrelatedSnapshots()
    {
        ObservationReinterpretationCandidate candidate = Candidate(
            "seeker",
            "weathered-inscription",
            "ancient-script-key");
        ObservationReinterpretationCandidate unrelated = Candidate(
            "travelling-healer",
            "concealed-healing-herb",
            "herbal-lore");

        Assert.Null(ObservationReinterpretationCandidateLookup.Find(
            candidate,
            Candidates()));
        Assert.Null(ObservationReinterpretationCandidateLookup.Find(
            candidate,
            Candidates(unrelated)));
    }

    [Fact]
    public void ReturnsTheExactRetainedCandidateForDistinctValueEqualInput()
    {
        ObservationReinterpretationCandidate candidate = Candidate(
            "seeker",
            "weathered-inscription",
            "ancient-script-key");
        CharacterId retainedObserverCharacterId = new("seeker");
        ObjectId retainedTargetObjectId = new("weathered-inscription");
        KnowledgeId retainedTriggerKnowledgeId = new("ancient-script-key");
        ObservationReinterpretationCandidate retained = new(
            retainedObserverCharacterId,
            retainedTargetObjectId,
            retainedTriggerKnowledgeId);

        ObservationReinterpretationCandidate? result =
            ObservationReinterpretationCandidateLookup.Find(
                candidate,
                Candidates(retained));

        Assert.NotSame(candidate, retained);
        Assert.Equal(candidate, retained);
        Assert.Same(retained, result);
        Assert.Same(retainedObserverCharacterId, result!.ObserverCharacterId);
        Assert.Same(retainedTargetObjectId, result.TargetObjectId);
        Assert.Same(retainedTriggerKnowledgeId, result.TriggerKnowledgeId);
    }

    [Fact]
    public void RejectsEveryPartialMatch()
    {
        ObservationReinterpretationCandidate candidate = Candidate(
            "seeker",
            "weathered-inscription",
            "ancient-script-key");
        ObservationReinterpretationCandidateSnapshot candidates = Candidates(
            Candidate("travelling-healer", "weathered-inscription", "ancient-script-key"),
            Candidate("seeker", "concealed-healing-herb", "ancient-script-key"),
            Candidate("seeker", "weathered-inscription", "shrine-history"));

        ObservationReinterpretationCandidate? result =
            ObservationReinterpretationCandidateLookup.Find(candidate, candidates);

        Assert.Null(result);
    }

    [Fact]
    public void ReturnsTheMatchAmongUnrelatedCandidatesWithoutChangingOrder()
    {
        ObservationReinterpretationCandidate before = Candidate(
            "sage",
            "wagon-residue",
            "road-history");
        ObservationReinterpretationCandidate matching = Candidate(
            "seeker",
            "weathered-inscription",
            "ancient-script-key");
        ObservationReinterpretationCandidate after = Candidate(
            "seeker",
            "concealed-healing-herb",
            "herbal-lore");
        ObservationReinterpretationCandidateSnapshot candidates =
            Candidates(before, matching, after);

        ObservationReinterpretationCandidate? result =
            ObservationReinterpretationCandidateLookup.Find(
                Candidate("seeker", "weathered-inscription", "ancient-script-key"),
                candidates);

        Assert.Same(matching, result);
        Assert.Collection(
            candidates.Entries,
            entry => Assert.Same(before, entry),
            entry => Assert.Same(matching, entry),
            entry => Assert.Same(after, entry));
    }

    [Fact]
    public void RejectsNullInputs()
    {
        ObservationReinterpretationCandidate candidate = Candidate(
            "seeker",
            "weathered-inscription",
            "ancient-script-key");
        ObservationReinterpretationCandidateSnapshot candidates = Candidates();

        Assert.Throws<ArgumentNullException>(
            () => ObservationReinterpretationCandidateLookup.Find(null!, candidates));
        Assert.Throws<ArgumentNullException>(
            () => ObservationReinterpretationCandidateLookup.Find(candidate, null!));
    }

    [Fact]
    public void LeavesCandidateAndSnapshotInputsUnchanged()
    {
        ObservationReinterpretationCandidate candidate = Candidate(
            "seeker",
            "weathered-inscription",
            "ancient-script-key");
        ObservationReinterpretationCandidate retained = Candidate(
            "seeker",
            "weathered-inscription",
            "ancient-script-key");
        ObservationReinterpretationCandidateSnapshot candidates = Candidates(retained);

        _ = ObservationReinterpretationCandidateLookup.Find(candidate, candidates);

        Assert.Equal("seeker", candidate.ObserverCharacterId.Value);
        Assert.Equal("weathered-inscription", candidate.TargetObjectId.Value);
        Assert.Equal("ancient-script-key", candidate.TriggerKnowledgeId.Value);
        Assert.Collection(candidates.Entries, entry => Assert.Same(retained, entry));
    }

    [Fact]
    public void ExposesOnlyTheSinglePureLookupOperation()
    {
        Type lookupType = typeof(ObservationReinterpretationCandidateLookup);
        System.Reflection.MethodInfo method = Assert.Single(
            lookupType.GetMethods().Where(candidate => candidate.DeclaringType == lookupType));

        Assert.Equal("Find", method.Name);
        Assert.Equal(typeof(ObservationReinterpretationCandidate), method.ReturnType);
        Assert.True(method.IsStatic);
        Assert.Collection(
            method.GetParameters(),
            parameter => Assert.Equal(
                typeof(ObservationReinterpretationCandidate),
                parameter.ParameterType),
            parameter => Assert.Equal(
                typeof(ObservationReinterpretationCandidateSnapshot),
                parameter.ParameterType));
        Assert.Empty(lookupType.GetConstructors());
        Assert.Empty(lookupType.GetProperties());
        Assert.Empty(lookupType.GetEvents());
        Assert.True(lookupType.IsAbstract);
        Assert.True(lookupType.IsSealed);
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
