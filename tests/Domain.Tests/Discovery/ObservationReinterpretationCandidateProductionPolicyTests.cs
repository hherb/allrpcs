using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Controllers;
using Allrpcs.Domain.Discovery;
using Allrpcs.Domain.Identifiers;
using Allrpcs.Domain.WorldObjects;

namespace Allrpcs.Domain.Tests.Discovery;

public sealed class ObservationReinterpretationCandidateProductionPolicyTests
{
    [Fact]
    public void AllowsExactAuthoritativeCharacterAndWorldObjectInstances()
    {
        Character character = CreateCharacter(new CharacterId("seeker"));
        WorldObject worldObject = CreateWorldObject(new ObjectId("weathered-inscription"));
        ObservationReinterpretationCandidate candidate = new(
            character.Identity.Id,
            worldObject.Identity.Id,
            new KnowledgeId("ancient-script"));

        Assert.True(ObservationReinterpretationCandidateProductionPolicy.CanProduce(
            candidate,
            character,
            worldObject,
            Candidates()));
    }

    [Fact]
    public void AllowsDistinctValueEqualAuthorityAndEveryNonDuplicateCandidate()
    {
        CharacterId observerCharacterId = new("seeker");
        ObjectId targetObjectId = new("weathered-inscription");
        ObservationReinterpretationCandidate candidate = new(
            observerCharacterId,
            targetObjectId,
            new KnowledgeId("ancient-script"));
        Character character = CreateCharacter(new CharacterId(observerCharacterId.Value));
        WorldObject worldObject = CreateWorldObject(new ObjectId(targetObjectId.Value));
        ObservationReinterpretationCandidateSnapshot candidates = Candidates(
            Candidate("seeker", "healing-herb", "ancient-script"),
            Candidate("travelling-healer", "weathered-inscription", "ancient-script"),
            Candidate("seeker", "weathered-inscription", "herbal-lore"));

        Assert.True(ObservationReinterpretationCandidateProductionPolicy.CanProduce(
            candidate,
            character,
            worldObject,
            candidates));
        Assert.NotSame(observerCharacterId, character.Identity.Id);
        Assert.NotSame(targetObjectId, worldObject.Identity.Id);
    }

    [Fact]
    public void RejectsEitherAuthoritativeIdentityMismatch()
    {
        ObservationReinterpretationCandidate candidate = Candidate();

        Assert.False(ObservationReinterpretationCandidateProductionPolicy.CanProduce(
            candidate,
            CreateCharacter(new CharacterId("travelling-healer")),
            CreateWorldObject(new ObjectId("weathered-inscription")),
            Candidates()));
        Assert.False(ObservationReinterpretationCandidateProductionPolicy.CanProduce(
            candidate,
            CreateCharacter(new CharacterId("seeker")),
            CreateWorldObject(new ObjectId("healing-herb")),
            Candidates()));
    }

    [Fact]
    public void RejectsCompleteDuplicateThroughRecordingEligibility()
    {
        ObservationReinterpretationCandidate candidate = Candidate();
        ObservationReinterpretationCandidate retained = Candidate();
        ObservationReinterpretationCandidateSnapshot candidates = Candidates(retained);

        bool recordingResult = ObservationReinterpretationCandidateRecordingPolicy.CanRecord(
            candidate,
            candidates);
        bool result = ObservationReinterpretationCandidateProductionPolicy.CanProduce(
            candidate,
            CreateCharacter(new CharacterId("seeker")),
            CreateWorldObject(new ObjectId("weathered-inscription")),
            candidates);

        Assert.False(recordingResult);
        Assert.Equal(recordingResult, result);
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
        Character character = CreateCharacter(new CharacterId("seeker"));
        WorldObject worldObject = CreateWorldObject(new ObjectId("weathered-inscription"));
        CharacterIdentity characterIdentity = character.Identity;
        WorldObjectIdentity worldObjectIdentity = worldObject.Identity;
        WorldObjectDescription worldObjectDescription = worldObject.Description;
        ObservationReinterpretationCandidate retained = Candidate(
            "sage", "wagon-residue", "road-history");
        ObservationReinterpretationCandidateSnapshot candidates = Candidates(retained);

        _ = ObservationReinterpretationCandidateProductionPolicy.CanProduce(
            candidate,
            character,
            worldObject,
            candidates);

        Assert.Same(observerCharacterId, candidate.ObserverCharacterId);
        Assert.Same(targetObjectId, candidate.TargetObjectId);
        Assert.Same(triggerKnowledgeId, candidate.TriggerKnowledgeId);
        Assert.Same(characterIdentity, character.Identity);
        Assert.Same(character.Identity.Id, character.ControllerAssignment.CharacterId);
        Assert.Same(worldObjectIdentity, worldObject.Identity);
        Assert.Same(worldObjectDescription, worldObject.Description);
        Assert.Same(retained, Assert.Single(candidates.Entries));
        Assert.Same(retained.TriggerKnowledgeId, candidates.Entries[0].TriggerKnowledgeId);
    }

    [Fact]
    public void RejectsEveryNullInput()
    {
        ObservationReinterpretationCandidate candidate = Candidate();
        Character character = CreateCharacter(new CharacterId("seeker"));
        WorldObject worldObject = CreateWorldObject(new ObjectId("weathered-inscription"));
        ObservationReinterpretationCandidateSnapshot candidates = Candidates();

        Assert.Throws<ArgumentNullException>(() =>
            ObservationReinterpretationCandidateProductionPolicy.CanProduce(
                null!, character, worldObject, candidates));
        Assert.Throws<ArgumentNullException>(() =>
            ObservationReinterpretationCandidateProductionPolicy.CanProduce(
                candidate, null!, worldObject, candidates));
        Assert.Throws<ArgumentNullException>(() =>
            ObservationReinterpretationCandidateProductionPolicy.CanProduce(
                candidate, character, null!, candidates));
        Assert.Throws<ArgumentNullException>(() =>
            ObservationReinterpretationCandidateProductionPolicy.CanProduce(
                candidate, character, worldObject, null!));
    }

    [Fact]
    public void ExposesOnlyTheSinglePureProductionEligibilityOperation()
    {
        Type policyType = typeof(ObservationReinterpretationCandidateProductionPolicy);
        System.Reflection.MethodInfo method = Assert.Single(
            policyType.GetMethods().Where(candidate => candidate.DeclaringType == policyType));

        Assert.Equal("CanProduce", method.Name);
        Assert.Equal(typeof(bool), method.ReturnType);
        Assert.True(method.IsStatic);
        Assert.Collection(
            method.GetParameters(),
            parameter => Assert.Equal(
                typeof(ObservationReinterpretationCandidate), parameter.ParameterType),
            parameter => Assert.Equal(typeof(Character), parameter.ParameterType),
            parameter => Assert.Equal(typeof(WorldObject), parameter.ParameterType),
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
