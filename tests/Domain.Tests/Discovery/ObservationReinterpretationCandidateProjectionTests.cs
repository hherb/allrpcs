using Allrpcs.Domain.Discovery;
using Allrpcs.Domain.Identifiers;

namespace Allrpcs.Domain.Tests.Discovery;

public sealed class ObservationReinterpretationCandidateProjectionTests
{
    [Fact]
    public void ReturnsANewEmptySnapshotWhenThereAreNoMatches()
    {
        CharacterId observerCharacterId = new("seeker");
        ObservationReinterpretationCandidate unrelated = Candidate(
            "travelling-healer",
            "weathered-inscription",
            "ancient-script-key");
        ObservationReinterpretationCandidateSnapshot candidates = new(new[] { unrelated });

        ObservationReinterpretationCandidateSnapshot result =
            ObservationReinterpretationCandidateProjection.ForCharacter(
                observerCharacterId,
                candidates);

        Assert.Empty(result.Entries);
        Assert.NotSame(candidates, result);
    }

    [Fact]
    public void ReturnsExactValueEqualObserverMatchesInStableOrder()
    {
        CharacterId observerCharacterId = new("seeker");
        CharacterId firstCandidateObserverCharacterId = new("seeker");
        ObservationReinterpretationCandidate firstMatch = new(
            firstCandidateObserverCharacterId,
            new ObjectId("weathered-inscription"),
            new KnowledgeId("ancient-script-key"));
        ObservationReinterpretationCandidate unrelated = Candidate(
            "travelling-healer",
            "concealed-healing-herb",
            "herbal-lore");
        ObservationReinterpretationCandidate secondMatch = Candidate(
            "seeker",
            "concealed-healing-herb",
            "herbal-lore");
        ObservationReinterpretationCandidateSnapshot candidates =
            new(new[] { firstMatch, unrelated, secondMatch });

        ObservationReinterpretationCandidateSnapshot result =
            ObservationReinterpretationCandidateProjection.ForCharacter(
                observerCharacterId,
                candidates);

        Assert.NotSame(observerCharacterId, firstCandidateObserverCharacterId);
        Assert.Equal(observerCharacterId, firstCandidateObserverCharacterId);
        Assert.Collection(
            result.Entries,
            entry => Assert.Same(firstMatch, entry),
            entry => Assert.Same(secondMatch, entry));
        Assert.Same(firstCandidateObserverCharacterId, result.Entries[0].ObserverCharacterId);
        Assert.Same(firstMatch.TargetObjectId, result.Entries[0].TargetObjectId);
        Assert.Same(firstMatch.TriggerKnowledgeId, result.Entries[0].TriggerKnowledgeId);
        Assert.Same(secondMatch.ObserverCharacterId, result.Entries[1].ObserverCharacterId);
        Assert.Same(secondMatch.TargetObjectId, result.Entries[1].TargetObjectId);
        Assert.Same(secondMatch.TriggerKnowledgeId, result.Entries[1].TriggerKnowledgeId);
    }

    [Fact]
    public void DoesNotFilterMatchesByTargetOrTriggerKnowledge()
    {
        ObservationReinterpretationCandidate first = Candidate(
            "seeker",
            "weathered-inscription",
            "ancient-script-key");
        ObservationReinterpretationCandidate sameTarget = Candidate(
            "seeker",
            "weathered-inscription",
            "shrine-history");
        ObservationReinterpretationCandidate sameKnowledge = Candidate(
            "seeker",
            "concealed-healing-herb",
            "ancient-script-key");
        ObservationReinterpretationCandidateSnapshot candidates =
            new(new[] { first, sameTarget, sameKnowledge });

        ObservationReinterpretationCandidateSnapshot result =
            ObservationReinterpretationCandidateProjection.ForCharacter(
                new CharacterId("seeker"),
                candidates);

        Assert.Collection(
            result.Entries,
            entry => Assert.Same(first, entry),
            entry => Assert.Same(sameTarget, entry),
            entry => Assert.Same(sameKnowledge, entry));
    }

    [Fact]
    public void RejectsNullInputs()
    {
        CharacterId observerCharacterId = new("seeker");
        ObservationReinterpretationCandidateSnapshot candidates = new(
            Array.Empty<ObservationReinterpretationCandidate>());

        Assert.Throws<ArgumentNullException>(
            () => ObservationReinterpretationCandidateProjection.ForCharacter(null!, candidates));
        Assert.Throws<ArgumentNullException>(
            () => ObservationReinterpretationCandidateProjection.ForCharacter(
                observerCharacterId,
                null!));
    }

    [Fact]
    public void LeavesInputsUnchanged()
    {
        CharacterId observerCharacterId = new("seeker");
        ObservationReinterpretationCandidate matching = Candidate(
            "seeker",
            "weathered-inscription",
            "ancient-script-key");
        ObservationReinterpretationCandidate unrelated = Candidate(
            "travelling-healer",
            "concealed-healing-herb",
            "herbal-lore");
        ObservationReinterpretationCandidateSnapshot candidates =
            new(new[] { matching, unrelated });

        _ = ObservationReinterpretationCandidateProjection.ForCharacter(
            observerCharacterId,
            candidates);

        Assert.Equal("seeker", observerCharacterId.Value);
        Assert.Collection(
            candidates.Entries,
            entry => Assert.Same(matching, entry),
            entry => Assert.Same(unrelated, entry));
    }

    [Fact]
    public void ExposesOnlyTheSinglePureProjectionOperation()
    {
        Type projectionType = typeof(ObservationReinterpretationCandidateProjection);
        System.Reflection.MethodInfo method = Assert.Single(
            projectionType
                .GetMethods()
                .Where(candidate => candidate.DeclaringType == projectionType));

        Assert.Equal("ForCharacter", method.Name);
        Assert.Equal(typeof(ObservationReinterpretationCandidateSnapshot), method.ReturnType);
        Assert.True(method.IsStatic);
        Assert.Collection(
            method.GetParameters(),
            parameter => Assert.Equal(typeof(CharacterId), parameter.ParameterType),
            parameter => Assert.Equal(
                typeof(ObservationReinterpretationCandidateSnapshot),
                parameter.ParameterType));
        Assert.Empty(projectionType.GetConstructors());
        Assert.Empty(projectionType.GetProperties());
        Assert.Empty(projectionType.GetEvents());
        Assert.True(projectionType.IsAbstract);
        Assert.True(projectionType.IsSealed);
    }

    private static ObservationReinterpretationCandidate Candidate(
        string observerCharacterId,
        string targetObjectId,
        string triggerKnowledgeId) =>
        new(
            new CharacterId(observerCharacterId),
            new ObjectId(targetObjectId),
            new KnowledgeId(triggerKnowledgeId));
}
