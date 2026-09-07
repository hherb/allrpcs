using Allrpcs.Domain.Discovery;
using Allrpcs.Domain.Identifiers;

namespace Allrpcs.Domain.Tests.Discovery;

public sealed class ObservationReinterpretationCandidateRecordingTransitionTests
{
    [Fact]
    public void AppendsExactCandidateAfterEveryPartialMatch()
    {
        ObservationReinterpretationCandidate candidate = Candidate();
        ObservationReinterpretationCandidate first = Candidate(
            "travelling-healer", "weathered-inscription", "ancient-script");
        ObservationReinterpretationCandidate second = Candidate(
            "seeker", "healing-herb", "ancient-script");
        ObservationReinterpretationCandidate third = Candidate(
            "seeker", "weathered-inscription", "herbal-lore");
        ObservationReinterpretationCandidateSnapshot candidates =
            Candidates(first, second, third);

        ObservationReinterpretationCandidateSnapshot result =
            ObservationReinterpretationCandidateRecordingTransition.Record(
                candidate,
                candidates);

        Assert.NotSame(candidates, result);
        Assert.Collection(
            result.Entries,
            entry => Assert.Same(first, entry),
            entry => Assert.Same(second, entry),
            entry => Assert.Same(third, entry),
            entry => Assert.Same(candidate, entry));
        Assert.Collection(
            candidates.Entries,
            entry => Assert.Same(first, entry),
            entry => Assert.Same(second, entry),
            entry => Assert.Same(third, entry));
    }

    [Fact]
    public void RecordsCandidateInEmptySnapshot()
    {
        ObservationReinterpretationCandidate candidate = Candidate();

        ObservationReinterpretationCandidateSnapshot result =
            ObservationReinterpretationCandidateRecordingTransition.Record(
                candidate,
                Candidates());

        Assert.Same(candidate, Assert.Single(result.Entries));
    }

    [Fact]
    public void RejectsExactAndDistinctValueEqualDuplicatesWithoutChangingSnapshot()
    {
        ObservationReinterpretationCandidate candidate = Candidate();
        ObservationReinterpretationCandidate retained = Candidate();
        ObservationReinterpretationCandidateSnapshot exact = Candidates(candidate);
        ObservationReinterpretationCandidateSnapshot equal = Candidates(retained);

        Assert.Throws<InvalidOperationException>(() =>
            ObservationReinterpretationCandidateRecordingTransition.Record(candidate, exact));
        Assert.Throws<InvalidOperationException>(() =>
            ObservationReinterpretationCandidateRecordingTransition.Record(candidate, equal));
        Assert.Same(candidate, Assert.Single(exact.Entries));
        Assert.Same(retained, Assert.Single(equal.Entries));
    }

    [Fact]
    public void RetainsCandidateAndEveryNestedIdentifierInstance()
    {
        CharacterId observerCharacterId = new("seeker");
        ObjectId targetObjectId = new("weathered-inscription");
        KnowledgeId triggerKnowledgeId = new("ancient-script");
        ObservationReinterpretationCandidate candidate = new(
            observerCharacterId,
            targetObjectId,
            triggerKnowledgeId);
        ObservationReinterpretationCandidate existing = Candidate(
            "sage", "wagon-residue", "road-history");

        ObservationReinterpretationCandidateSnapshot result =
            ObservationReinterpretationCandidateRecordingTransition.Record(
                candidate,
                Candidates(existing));

        Assert.Same(existing, result.Entries[0]);
        Assert.Same(candidate, result.Entries[1]);
        Assert.Same(observerCharacterId, result.Entries[1].ObserverCharacterId);
        Assert.Same(targetObjectId, result.Entries[1].TargetObjectId);
        Assert.Same(triggerKnowledgeId, result.Entries[1].TriggerKnowledgeId);
    }

    [Fact]
    public void RejectsNullInputsThroughRecordingPolicy()
    {
        ObservationReinterpretationCandidate candidate = Candidate();
        ObservationReinterpretationCandidateSnapshot candidates = Candidates();

        Assert.Throws<ArgumentNullException>(() =>
            ObservationReinterpretationCandidateRecordingTransition.Record(null!, candidates));
        Assert.Throws<ArgumentNullException>(() =>
            ObservationReinterpretationCandidateRecordingTransition.Record(candidate, null!));
    }

    [Fact]
    public void ExposesOnlyTheSinglePureRecordingOperation()
    {
        Type transitionType = typeof(ObservationReinterpretationCandidateRecordingTransition);
        System.Reflection.MethodInfo method = Assert.Single(
            transitionType.GetMethods().Where(candidate => candidate.DeclaringType == transitionType));

        Assert.Equal("Record", method.Name);
        Assert.Equal(typeof(ObservationReinterpretationCandidateSnapshot), method.ReturnType);
        Assert.True(method.IsStatic);
        Assert.Collection(
            method.GetParameters(),
            parameter => Assert.Equal(
                typeof(ObservationReinterpretationCandidate), parameter.ParameterType),
            parameter => Assert.Equal(
                typeof(ObservationReinterpretationCandidateSnapshot), parameter.ParameterType));
        Assert.Empty(transitionType.GetConstructors());
        Assert.Empty(transitionType.GetProperties());
        Assert.Empty(transitionType.GetEvents());
        Assert.True(transitionType.IsAbstract);
        Assert.True(transitionType.IsSealed);
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
