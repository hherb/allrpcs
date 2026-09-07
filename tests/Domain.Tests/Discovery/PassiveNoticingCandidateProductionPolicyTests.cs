using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Controllers;
using Allrpcs.Domain.Discovery;
using Allrpcs.Domain.Identifiers;
using Allrpcs.Domain.WorldObjects;

namespace Allrpcs.Domain.Tests.Discovery;

public sealed class PassiveNoticingCandidateProductionPolicyTests
{
    [Fact]
    public void AllowsExactAuthoritativeCharacterAndWorldObjectInstances()
    {
        Character character = CreateCharacter(new CharacterId("seeker"));
        WorldObject worldObject = CreateWorldObject(new ObjectId("weathered-inscription"));
        PassiveNoticingCandidate candidate = new(character.Identity.Id, worldObject.Identity.Id);

        bool result = PassiveNoticingCandidateProductionPolicy.CanProduce(
            candidate,
            character,
            worldObject,
            Candidates());

        Assert.True(result);
    }

    [Fact]
    public void AllowsDistinctValueEqualAuthorityAndBothPartialCandidateMatches()
    {
        CharacterId observerCharacterId = new("seeker");
        ObjectId targetObjectId = new("weathered-inscription");
        PassiveNoticingCandidate candidate = new(observerCharacterId, targetObjectId);
        Character character = CreateCharacter(new CharacterId(observerCharacterId.Value));
        WorldObject worldObject = CreateWorldObject(new ObjectId(targetObjectId.Value));
        PassiveNoticingCandidate sameObserverDifferentTarget =
            Candidate("seeker", "healing-herb");
        PassiveNoticingCandidate differentObserverSameTarget =
            Candidate("travelling-healer", "weathered-inscription");

        bool result = PassiveNoticingCandidateProductionPolicy.CanProduce(
            candidate,
            character,
            worldObject,
            Candidates(sameObserverDifferentTarget, differentObserverSameTarget));

        Assert.NotSame(observerCharacterId, character.Identity.Id);
        Assert.NotSame(targetObjectId, worldObject.Identity.Id);
        Assert.Equal(observerCharacterId, character.Identity.Id);
        Assert.Equal(targetObjectId, worldObject.Identity.Id);
        Assert.True(result);
    }

    [Fact]
    public void RejectsCharacterIdentityMismatch()
    {
        PassiveNoticingCandidate candidate = Candidate("seeker", "weathered-inscription");

        bool result = PassiveNoticingCandidateProductionPolicy.CanProduce(
            candidate,
            CreateCharacter(new CharacterId("travelling-healer")),
            CreateWorldObject(new ObjectId("weathered-inscription")),
            Candidates());

        Assert.False(result);
    }

    [Fact]
    public void RejectsWorldObjectIdentityMismatch()
    {
        PassiveNoticingCandidate candidate = Candidate("seeker", "weathered-inscription");

        bool result = PassiveNoticingCandidateProductionPolicy.CanProduce(
            candidate,
            CreateCharacter(new CharacterId("seeker")),
            CreateWorldObject(new ObjectId("wagon-residue")),
            Candidates());

        Assert.False(result);
    }

    [Fact]
    public void RejectsCompleteDuplicateThroughRecordingEligibility()
    {
        PassiveNoticingCandidate candidate = Candidate("seeker", "weathered-inscription");
        PassiveNoticingCandidate retained = Candidate("seeker", "weathered-inscription");

        bool recordingResult = PassiveNoticingCandidateRecordingPolicy.CanRecord(
            candidate,
            Candidates(retained));
        bool result = PassiveNoticingCandidateProductionPolicy.CanProduce(
            candidate,
            CreateCharacter(new CharacterId("seeker")),
            CreateWorldObject(new ObjectId("weathered-inscription")),
            Candidates(retained));

        Assert.False(recordingResult);
        Assert.Equal(recordingResult, result);
    }

    [Fact]
    public void LeavesEveryInputAndNestedInstanceUnchanged()
    {
        CharacterId observerCharacterId = new("seeker");
        ObjectId targetObjectId = new("weathered-inscription");
        PassiveNoticingCandidate candidate = new(observerCharacterId, targetObjectId);
        Character character = CreateCharacter(new CharacterId(observerCharacterId.Value));
        WorldObject worldObject = CreateWorldObject(new ObjectId(targetObjectId.Value));
        CharacterIdentity characterIdentity = character.Identity;
        WorldObjectIdentity worldObjectIdentity = worldObject.Identity;
        WorldObjectDescription worldObjectDescription = worldObject.Description;
        PassiveNoticingCandidate retained = Candidate("sage", "wagon-residue");
        PassiveNoticingCandidateSnapshot candidates = Candidates(retained);

        _ = PassiveNoticingCandidateProductionPolicy.CanProduce(
            candidate,
            character,
            worldObject,
            candidates);

        Assert.Same(observerCharacterId, candidate.ObserverCharacterId);
        Assert.Same(targetObjectId, candidate.TargetObjectId);
        Assert.Same(characterIdentity, character.Identity);
        Assert.Same(character.Identity.Id, character.ControllerAssignment.CharacterId);
        Assert.Same(worldObjectIdentity, worldObject.Identity);
        Assert.Same(worldObjectDescription, worldObject.Description);
        Assert.Collection(candidates.Entries, entry => Assert.Same(retained, entry));
        Assert.Same(retained.ObserverCharacterId, candidates.Entries[0].ObserverCharacterId);
        Assert.Same(retained.TargetObjectId, candidates.Entries[0].TargetObjectId);
    }

    [Fact]
    public void RejectsEveryNullInput()
    {
        PassiveNoticingCandidate candidate = Candidate("seeker", "weathered-inscription");
        Character character = CreateCharacter(new CharacterId("seeker"));
        WorldObject worldObject = CreateWorldObject(new ObjectId("weathered-inscription"));
        PassiveNoticingCandidateSnapshot candidates = Candidates();

        Assert.Throws<ArgumentNullException>(
            () => PassiveNoticingCandidateProductionPolicy.CanProduce(
                null!, character, worldObject, candidates));
        Assert.Throws<ArgumentNullException>(
            () => PassiveNoticingCandidateProductionPolicy.CanProduce(
                candidate, null!, worldObject, candidates));
        Assert.Throws<ArgumentNullException>(
            () => PassiveNoticingCandidateProductionPolicy.CanProduce(
                candidate, character, null!, candidates));
        Assert.Throws<ArgumentNullException>(
            () => PassiveNoticingCandidateProductionPolicy.CanProduce(
                candidate, character, worldObject, null!));
    }

    [Fact]
    public void ExposesOnlyTheSinglePureProductionEligibilityOperation()
    {
        Type policyType = typeof(PassiveNoticingCandidateProductionPolicy);
        System.Reflection.MethodInfo method = Assert.Single(
            policyType
                .GetMethods()
                .Where(candidate => candidate.DeclaringType == policyType));

        Assert.Equal("CanProduce", method.Name);
        Assert.Equal(typeof(bool), method.ReturnType);
        Assert.True(method.IsStatic);
        Assert.Collection(
            method.GetParameters(),
            parameter => Assert.Equal(
                typeof(PassiveNoticingCandidate),
                parameter.ParameterType),
            parameter => Assert.Equal(typeof(Character), parameter.ParameterType),
            parameter => Assert.Equal(typeof(WorldObject), parameter.ParameterType),
            parameter => Assert.Equal(
                typeof(PassiveNoticingCandidateSnapshot),
                parameter.ParameterType));
        Assert.Empty(policyType.GetConstructors());
        Assert.Empty(policyType.GetProperties());
        Assert.Empty(policyType.GetEvents());
        Assert.True(policyType.IsAbstract);
        Assert.True(policyType.IsSealed);
    }

    private static PassiveNoticingCandidate Candidate(
        string observerCharacterId,
        string targetObjectId) =>
        new(new CharacterId(observerCharacterId), new ObjectId(targetObjectId));

    private static PassiveNoticingCandidateSnapshot Candidates(
        params PassiveNoticingCandidate[] entries) =>
        new(entries);

    private static Character CreateCharacter(CharacterId characterId) =>
        new(
            new CharacterIdentity(characterId, "The seeker"),
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
