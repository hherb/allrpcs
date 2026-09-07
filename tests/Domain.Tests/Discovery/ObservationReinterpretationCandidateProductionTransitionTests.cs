using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Controllers;
using Allrpcs.Domain.Discovery;
using Allrpcs.Domain.Identifiers;
using Allrpcs.Domain.WorldObjects;

namespace Allrpcs.Domain.Tests.Discovery;

public sealed class ObservationReinterpretationCandidateProductionTransitionTests
{
    [Fact]
    public void ProducesCandidateFromExactAuthoritativeInstances()
    {
        Character character = CreateCharacter(new CharacterId("seeker"));
        WorldObject worldObject = CreateWorldObject(new ObjectId("weathered-inscription"));
        ObservationReinterpretationCandidate candidate = new(
            character.Identity.Id,
            worldObject.Identity.Id,
            new KnowledgeId("ancient-script"));

        ObservationReinterpretationCandidateSnapshot result =
            ObservationReinterpretationCandidateProductionTransition.Produce(
                candidate,
                character,
                worldObject,
                Candidates());

        Assert.Same(candidate, Assert.Single(result.Entries));
    }

    [Fact]
    public void ProducesValueEqualCandidateAlongsideEveryPartialMatch()
    {
        ObservationReinterpretationCandidate candidate = Candidate();
        Character character = CreateCharacter(new CharacterId("seeker"));
        WorldObject worldObject = CreateWorldObject(new ObjectId("weathered-inscription"));
        ObservationReinterpretationCandidate first = Candidate(
            "seeker", "healing-herb", "ancient-script");
        ObservationReinterpretationCandidate second = Candidate(
            "travelling-healer", "weathered-inscription", "ancient-script");
        ObservationReinterpretationCandidate third = Candidate(
            "seeker", "weathered-inscription", "herbal-lore");

        ObservationReinterpretationCandidateSnapshot result =
            ObservationReinterpretationCandidateProductionTransition.Produce(
                candidate,
                character,
                worldObject,
                Candidates(first, second, third));

        Assert.NotSame(candidate.ObserverCharacterId, character.Identity.Id);
        Assert.NotSame(candidate.TargetObjectId, worldObject.Identity.Id);
        Assert.Collection(
            result.Entries,
            entry => Assert.Same(first, entry),
            entry => Assert.Same(second, entry),
            entry => Assert.Same(third, entry),
            entry => Assert.Same(candidate, entry));
    }

    [Fact]
    public void RejectsEitherAuthorityMismatchAndCompleteDuplicate()
    {
        ObservationReinterpretationCandidate candidate = Candidate();
        Character character = CreateCharacter(new CharacterId("seeker"));
        WorldObject worldObject = CreateWorldObject(new ObjectId("weathered-inscription"));
        ObservationReinterpretationCandidate retained = Candidate();
        ObservationReinterpretationCandidateSnapshot candidates = Candidates(retained);

        Assert.Throws<InvalidOperationException>(() =>
            ObservationReinterpretationCandidateProductionTransition.Produce(
                candidate,
                CreateCharacter(new CharacterId("travelling-healer")),
                worldObject,
                Candidates()));
        Assert.Throws<InvalidOperationException>(() =>
            ObservationReinterpretationCandidateProductionTransition.Produce(
                candidate,
                character,
                CreateWorldObject(new ObjectId("healing-herb")),
                Candidates()));
        Assert.Throws<InvalidOperationException>(() =>
            ObservationReinterpretationCandidateProductionTransition.Produce(
                candidate,
                character,
                worldObject,
                candidates));
        Assert.Same(retained, Assert.Single(candidates.Entries));
    }

    [Fact]
    public void RetainsStableOrderAndEveryExactSuppliedInstance()
    {
        CharacterId observerCharacterId = new("seeker");
        ObjectId targetObjectId = new("weathered-inscription");
        KnowledgeId triggerKnowledgeId = new("ancient-script");
        ObservationReinterpretationCandidate candidate = new(
            observerCharacterId,
            targetObjectId,
            triggerKnowledgeId);
        Character character = CreateCharacter(new CharacterId("seeker"));
        WorldObject worldObject = CreateWorldObject(new ObjectId("weathered-inscription"));
        ObservationReinterpretationCandidate first = Candidate(
            "sage", "wagon-residue", "road-history");
        ObservationReinterpretationCandidate second = Candidate(
            "travelling-healer", "healing-herb", "herbal-lore");
        ObservationReinterpretationCandidateSnapshot candidates = Candidates(first, second);

        ObservationReinterpretationCandidateSnapshot result =
            ObservationReinterpretationCandidateProductionTransition.Produce(
                candidate,
                character,
                worldObject,
                candidates);

        Assert.NotSame(candidates, result);
        Assert.Collection(
            candidates.Entries,
            entry => Assert.Same(first, entry),
            entry => Assert.Same(second, entry));
        Assert.Collection(
            result.Entries,
            entry => Assert.Same(first, entry),
            entry => Assert.Same(second, entry),
            entry => Assert.Same(candidate, entry));
        Assert.Same(observerCharacterId, result.Entries[2].ObserverCharacterId);
        Assert.Same(targetObjectId, result.Entries[2].TargetObjectId);
        Assert.Same(triggerKnowledgeId, result.Entries[2].TriggerKnowledgeId);
    }

    [Fact]
    public void RejectsEveryNullInputWithoutChangingSnapshot()
    {
        ObservationReinterpretationCandidate candidate = Candidate();
        Character character = CreateCharacter(new CharacterId("seeker"));
        WorldObject worldObject = CreateWorldObject(new ObjectId("weathered-inscription"));
        ObservationReinterpretationCandidate retained = Candidate(
            "sage", "wagon-residue", "road-history");
        ObservationReinterpretationCandidateSnapshot candidates = Candidates(retained);

        Assert.Throws<ArgumentNullException>(() =>
            ObservationReinterpretationCandidateProductionTransition.Produce(
                null!, character, worldObject, candidates));
        Assert.Throws<ArgumentNullException>(() =>
            ObservationReinterpretationCandidateProductionTransition.Produce(
                candidate, null!, worldObject, candidates));
        Assert.Throws<ArgumentNullException>(() =>
            ObservationReinterpretationCandidateProductionTransition.Produce(
                candidate, character, null!, candidates));
        Assert.Throws<ArgumentNullException>(() =>
            ObservationReinterpretationCandidateProductionTransition.Produce(
                candidate, character, worldObject, null!));
        Assert.Same(retained, Assert.Single(candidates.Entries));
    }

    [Fact]
    public void ExposesOnlyTheSinglePureProductionOperation()
    {
        Type transitionType = typeof(ObservationReinterpretationCandidateProductionTransition);
        System.Reflection.MethodInfo method = Assert.Single(
            transitionType.GetMethods().Where(candidate => candidate.DeclaringType == transitionType));

        Assert.Equal("Produce", method.Name);
        Assert.Equal(typeof(ObservationReinterpretationCandidateSnapshot), method.ReturnType);
        Assert.True(method.IsStatic);
        Assert.Collection(
            method.GetParameters(),
            parameter => Assert.Equal(
                typeof(ObservationReinterpretationCandidate), parameter.ParameterType),
            parameter => Assert.Equal(typeof(Character), parameter.ParameterType),
            parameter => Assert.Equal(typeof(WorldObject), parameter.ParameterType),
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

    private static Character CreateCharacter(CharacterId characterId) =>
        new(
            new CharacterIdentity(characterId, "The observer"),
            new PublicCharacterBackground(string.Empty),
            new PrivateCharacterBackground(string.Empty),
            new CharacterCapabilitySet(Array.Empty<CharacterCapability>()),
            new CharacterLimitationSet(Array.Empty<CharacterLimitation>()),
            new CharacterControllerAssignment(characterId, ControllerType.Human),
            new CharacterConditionSet(Array.Empty<CharacterCondition>()),
            new CharacterInventory(Array.Empty<ObjectId>()),
            new CharacterKnowledge(Array.Empty<KnowledgeId>()),
            new CharacterBeliefSet(Array.Empty<CharacterBelief>()),
            new CharacterRelationshipSet(Array.Empty<CharacterRelationship>()),
            new CharacterGoalSet(Array.Empty<CharacterGoal>()));

    private static WorldObject CreateWorldObject(ObjectId objectId) =>
        new(
            new WorldObjectIdentity(objectId, "Weathered inscription"),
            new WorldObjectDescription("Carved stone text."));
}
