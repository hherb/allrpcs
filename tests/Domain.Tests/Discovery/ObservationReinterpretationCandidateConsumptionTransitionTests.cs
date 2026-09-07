using Allrpcs.Domain.Discovery;
using Allrpcs.Domain.Identifiers;

namespace Allrpcs.Domain.Tests.Discovery;

public sealed class ObservationReinterpretationCandidateConsumptionTransitionTests
{
    [Fact]
    public void RejectsEmptyAbsentAndPartialMatchesWithoutChangingSnapshot()
    {
        ObservationReinterpretationCandidate candidate = Candidate(
            "seeker",
            "weathered-inscription",
            "ancient-script");
        ObservationReinterpretationCandidate[] partialMatches =
        [
            Candidate("travelling-healer", "weathered-inscription", "ancient-script"),
            Candidate("seeker", "healing-herb", "ancient-script"),
            Candidate("seeker", "weathered-inscription", "herbal-lore"),
        ];
        ObservationReinterpretationCandidateSnapshot partialSnapshot =
            Candidates(partialMatches);

        Assert.Throws<InvalidOperationException>(() =>
            ObservationReinterpretationCandidateConsumptionTransition.Consume(
                candidate,
                Candidates()));
        Assert.Throws<InvalidOperationException>(() =>
            ObservationReinterpretationCandidateConsumptionTransition.Consume(
                candidate,
                Candidates(Candidate("sage", "wagon-residue", "road-history"))));
        Assert.Throws<InvalidOperationException>(() =>
            ObservationReinterpretationCandidateConsumptionTransition.Consume(
                candidate,
                partialSnapshot));
        Assert.Collection(
            partialSnapshot.Entries,
            entry => Assert.Same(partialMatches[0], entry),
            entry => Assert.Same(partialMatches[1], entry),
            entry => Assert.Same(partialMatches[2], entry));
    }

    [Fact]
    public void RemovesExactAndDistinctValueEqualCandidates()
    {
        ObservationReinterpretationCandidate candidate = Candidate(
            "seeker",
            "weathered-inscription",
            "ancient-script");
        ObservationReinterpretationCandidate retained = Candidate(
            "seeker",
            "weathered-inscription",
            "ancient-script");

        Assert.Empty(
            ObservationReinterpretationCandidateConsumptionTransition.Consume(
                candidate,
                Candidates(candidate)).Entries);
        Assert.NotSame(candidate, retained);
        Assert.Equal(candidate, retained);
        Assert.Empty(
            ObservationReinterpretationCandidateConsumptionTransition.Consume(
                candidate,
                Candidates(retained)).Entries);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    public void RemovesOnlyCandidateFromItsStablePosition(int targetIndex)
    {
        ObservationReinterpretationCandidate candidate = Candidate(
            "seeker",
            "weathered-inscription",
            "ancient-script");
        ObservationReinterpretationCandidate first = Candidate(
            "sage",
            "wagon-residue",
            "road-history");
        ObservationReinterpretationCandidate retained = Candidate(
            "seeker",
            "weathered-inscription",
            "ancient-script");
        ObservationReinterpretationCandidate third = Candidate(
            "travelling-healer",
            "healing-herb",
            "herbal-lore");
        ObservationReinterpretationCandidate[] entries = [first, retained, third];
        (entries[targetIndex], entries[1]) = (entries[1], entries[targetIndex]);
        ObservationReinterpretationCandidateSnapshot candidates = Candidates(entries);

        ObservationReinterpretationCandidateSnapshot result =
            ObservationReinterpretationCandidateConsumptionTransition.Consume(
                candidate,
                candidates);

        ObservationReinterpretationCandidate[] expected =
            entries.Where(entry => entry != retained).ToArray();
        Assert.NotSame(candidates, result);
        Assert.Collection(
            result.Entries,
            entry => Assert.Same(expected[0], entry),
            entry => Assert.Same(expected[1], entry));
        Assert.Collection(
            candidates.Entries,
            entry => Assert.Same(entries[0], entry),
            entry => Assert.Same(entries[1], entry),
            entry => Assert.Same(entries[2], entry));
        Assert.Same(expected[0].ObserverCharacterId, result.Entries[0].ObserverCharacterId);
        Assert.Same(expected[0].TargetObjectId, result.Entries[0].TargetObjectId);
        Assert.Same(expected[0].TriggerKnowledgeId, result.Entries[0].TriggerKnowledgeId);
        Assert.Same(expected[1].ObserverCharacterId, result.Entries[1].ObserverCharacterId);
        Assert.Same(expected[1].TargetObjectId, result.Entries[1].TargetObjectId);
        Assert.Same(expected[1].TriggerKnowledgeId, result.Entries[1].TriggerKnowledgeId);
    }

    [Fact]
    public void LeavesCandidateAndNestedIdentifiersUnchanged()
    {
        CharacterId observerCharacterId = new("seeker");
        ObjectId targetObjectId = new("weathered-inscription");
        KnowledgeId triggerKnowledgeId = new("ancient-script");
        ObservationReinterpretationCandidate candidate = new(
            observerCharacterId,
            targetObjectId,
            triggerKnowledgeId);
        ObservationReinterpretationCandidate retained = Candidate(
            "seeker",
            "weathered-inscription",
            "ancient-script");

        _ = ObservationReinterpretationCandidateConsumptionTransition.Consume(
            candidate,
            Candidates(retained));

        Assert.Same(observerCharacterId, candidate.ObserverCharacterId);
        Assert.Same(targetObjectId, candidate.TargetObjectId);
        Assert.Same(triggerKnowledgeId, candidate.TriggerKnowledgeId);
    }

    [Fact]
    public void RejectsNullInputsThroughConsumptionPolicy()
    {
        ObservationReinterpretationCandidate candidate = Candidate(
            "seeker",
            "weathered-inscription",
            "ancient-script");
        ObservationReinterpretationCandidateSnapshot candidates = Candidates();

        Assert.Throws<ArgumentNullException>(() =>
            ObservationReinterpretationCandidateConsumptionTransition.Consume(null!, candidates));
        Assert.Throws<ArgumentNullException>(() =>
            ObservationReinterpretationCandidateConsumptionTransition.Consume(candidate, null!));
    }

    [Fact]
    public void ExposesOnlyTheSinglePureConsumptionOperation()
    {
        Type transitionType = typeof(ObservationReinterpretationCandidateConsumptionTransition);
        System.Reflection.MethodInfo method = Assert.Single(
            transitionType.GetMethods().Where(candidate => candidate.DeclaringType == transitionType));

        Assert.Equal("Consume", method.Name);
        Assert.Equal(typeof(ObservationReinterpretationCandidateSnapshot), method.ReturnType);
        Assert.True(method.IsStatic);
        Assert.Collection(
            method.GetParameters(),
            parameter => Assert.Equal(
                typeof(ObservationReinterpretationCandidate),
                parameter.ParameterType),
            parameter => Assert.Equal(
                typeof(ObservationReinterpretationCandidateSnapshot),
                parameter.ParameterType));
        Assert.Empty(transitionType.GetConstructors());
        Assert.Empty(transitionType.GetProperties());
        Assert.Empty(transitionType.GetEvents());
        Assert.True(transitionType.IsAbstract);
        Assert.True(transitionType.IsSealed);
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
