using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Controllers;
using Allrpcs.Domain.Discovery;
using Allrpcs.Domain.Identifiers;
using Allrpcs.Domain.WorldObjects;

namespace Allrpcs.Domain.Tests.Discovery;

public sealed class PassiveNoticingCandidateProductionTransitionTests
{
    [Fact]
    public void ProducesCandidateFromExactAuthoritativeInstances()
    {
        Character character = CreateCharacter(new CharacterId("seeker"));
        WorldObject worldObject = CreateWorldObject(new ObjectId("weathered-inscription"));
        PassiveNoticingCandidate candidate = new(character.Identity.Id, worldObject.Identity.Id);

        PassiveNoticingCandidateSnapshot result =
            PassiveNoticingCandidateProductionTransition.Produce(
                candidate,
                character,
                worldObject,
                Candidates());

        Assert.Collection(result.Entries, entry => Assert.Same(candidate, entry));
    }

    [Fact]
    public void ProducesDistinctValueEqualCandidateAlongsideBothPartialMatches()
    {
        PassiveNoticingCandidate candidate = Candidate("seeker", "weathered-inscription");
        Character character = CreateCharacter(new CharacterId("seeker"));
        WorldObject worldObject = CreateWorldObject(new ObjectId("weathered-inscription"));
        PassiveNoticingCandidate sameObserverDifferentTarget =
            Candidate("seeker", "healing-herb");
        PassiveNoticingCandidate differentObserverSameTarget =
            Candidate("travelling-healer", "weathered-inscription");

        PassiveNoticingCandidateSnapshot result =
            PassiveNoticingCandidateProductionTransition.Produce(
                candidate,
                character,
                worldObject,
                Candidates(sameObserverDifferentTarget, differentObserverSameTarget));

        Assert.NotSame(candidate.ObserverCharacterId, character.Identity.Id);
        Assert.NotSame(candidate.TargetObjectId, worldObject.Identity.Id);
        Assert.Collection(
            result.Entries,
            entry => Assert.Same(sameObserverDifferentTarget, entry),
            entry => Assert.Same(differentObserverSameTarget, entry),
            entry => Assert.Same(candidate, entry));
    }

    [Fact]
    public void RejectsCharacterIdentityMismatch()
    {
        PassiveNoticingCandidate candidate = Candidate("seeker", "weathered-inscription");
        PassiveNoticingCandidateSnapshot candidates = Candidates();

        Assert.Throws<InvalidOperationException>(
            () => PassiveNoticingCandidateProductionTransition.Produce(
                candidate,
                CreateCharacter(new CharacterId("travelling-healer")),
                CreateWorldObject(new ObjectId("weathered-inscription")),
                candidates));
        Assert.Empty(candidates.Entries);
    }

    [Fact]
    public void RejectsWorldObjectIdentityMismatch()
    {
        PassiveNoticingCandidate candidate = Candidate("seeker", "weathered-inscription");
        PassiveNoticingCandidateSnapshot candidates = Candidates();

        Assert.Throws<InvalidOperationException>(
            () => PassiveNoticingCandidateProductionTransition.Produce(
                candidate,
                CreateCharacter(new CharacterId("seeker")),
                CreateWorldObject(new ObjectId("wagon-residue")),
                candidates));
        Assert.Empty(candidates.Entries);
    }

    [Fact]
    public void RejectsCompleteDuplicateThroughProductionEligibility()
    {
        PassiveNoticingCandidate candidate = Candidate("seeker", "weathered-inscription");
        PassiveNoticingCandidate retained = Candidate("seeker", "weathered-inscription");
        PassiveNoticingCandidateSnapshot candidates = Candidates(retained);

        Assert.Throws<InvalidOperationException>(
            () => PassiveNoticingCandidateProductionTransition.Produce(
                candidate,
                CreateCharacter(new CharacterId("seeker")),
                CreateWorldObject(new ObjectId("weathered-inscription")),
                candidates));
        Assert.Collection(candidates.Entries, entry => Assert.Same(retained, entry));
    }

    [Fact]
    public void RetainsStableOrderAndExactSuppliedInstances()
    {
        CharacterId observerCharacterId = new("seeker");
        ObjectId targetObjectId = new("weathered-inscription");
        PassiveNoticingCandidate candidate = new(observerCharacterId, targetObjectId);
        Character character = CreateCharacter(new CharacterId(observerCharacterId.Value));
        WorldObject worldObject = CreateWorldObject(new ObjectId(targetObjectId.Value));
        PassiveNoticingCandidate first = Candidate("sage", "wagon-residue");
        PassiveNoticingCandidate second = Candidate("travelling-healer", "healing-herb");
        PassiveNoticingCandidateSnapshot candidates = Candidates(first, second);

        PassiveNoticingCandidateSnapshot result =
            PassiveNoticingCandidateProductionTransition.Produce(
                candidate,
                character,
                worldObject,
                candidates);

        Assert.NotSame(candidates, result);
        Assert.Same(observerCharacterId, candidate.ObserverCharacterId);
        Assert.Same(targetObjectId, candidate.TargetObjectId);
        Assert.Collection(
            candidates.Entries,
            entry => Assert.Same(first, entry),
            entry => Assert.Same(second, entry));
        Assert.Collection(
            result.Entries,
            entry => Assert.Same(first, entry),
            entry => Assert.Same(second, entry),
            entry => Assert.Same(candidate, entry));
    }

    [Fact]
    public void RejectsEveryNullInputWithoutChangingTheSnapshot()
    {
        PassiveNoticingCandidate candidate = Candidate("seeker", "weathered-inscription");
        Character character = CreateCharacter(new CharacterId("seeker"));
        WorldObject worldObject = CreateWorldObject(new ObjectId("weathered-inscription"));
        PassiveNoticingCandidate retained = Candidate("sage", "wagon-residue");
        PassiveNoticingCandidateSnapshot candidates = Candidates(retained);

        Assert.Throws<ArgumentNullException>(
            () => PassiveNoticingCandidateProductionTransition.Produce(
                null!, character, worldObject, candidates));
        Assert.Throws<ArgumentNullException>(
            () => PassiveNoticingCandidateProductionTransition.Produce(
                candidate, null!, worldObject, candidates));
        Assert.Throws<ArgumentNullException>(
            () => PassiveNoticingCandidateProductionTransition.Produce(
                candidate, character, null!, candidates));
        Assert.Throws<ArgumentNullException>(
            () => PassiveNoticingCandidateProductionTransition.Produce(
                candidate, character, worldObject, null!));
        Assert.Collection(candidates.Entries, entry => Assert.Same(retained, entry));
    }

    [Fact]
    public void ExposesOnlyTheSinglePureProductionOperation()
    {
        Type transitionType = typeof(PassiveNoticingCandidateProductionTransition);
        System.Reflection.MethodInfo method = Assert.Single(
            transitionType
                .GetMethods()
                .Where(candidate => candidate.DeclaringType == transitionType));

        Assert.Equal("Produce", method.Name);
        Assert.Equal(typeof(PassiveNoticingCandidateSnapshot), method.ReturnType);
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
