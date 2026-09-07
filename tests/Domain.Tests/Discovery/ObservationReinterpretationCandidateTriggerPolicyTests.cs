using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Controllers;
using Allrpcs.Domain.Discovery;
using Allrpcs.Domain.Identifiers;

namespace Allrpcs.Domain.Tests.Discovery;

public sealed class ObservationReinterpretationCandidateTriggerPolicyTests
{
    [Fact]
    public void AllowsMatchingObserverWithTriggerKnowledge()
    {
        ObservationReinterpretationCandidate candidate = Candidate(
            "seeker",
            "weathered-inscription",
            "ancient-script");
        Character character = CreateCharacter(
            new CharacterId("seeker"),
            new KnowledgeId("ancient-script"));

        bool result = ObservationReinterpretationCandidateTriggerPolicy.CanTrigger(
            candidate,
            character);

        Assert.True(result);
    }

    [Fact]
    public void RejectsWrongCharacterEvenWithTriggerKnowledge()
    {
        ObservationReinterpretationCandidate candidate = Candidate(
            "seeker",
            "weathered-inscription",
            "ancient-script");
        Character character = CreateCharacter(
            new CharacterId("travelling-healer"),
            new KnowledgeId("ancient-script"));

        bool result = ObservationReinterpretationCandidateTriggerPolicy.CanTrigger(
            candidate,
            character);

        Assert.False(result);
    }

    [Fact]
    public void RejectsEmptyOrAbsentTriggerKnowledge()
    {
        ObservationReinterpretationCandidate candidate = Candidate(
            "seeker",
            "weathered-inscription",
            "ancient-script");

        Assert.False(ObservationReinterpretationCandidateTriggerPolicy.CanTrigger(
            candidate,
            CreateCharacter(new CharacterId("seeker"))));
        Assert.False(ObservationReinterpretationCandidateTriggerPolicy.CanTrigger(
            candidate,
            CreateCharacter(new CharacterId("seeker"), new KnowledgeId("herbal-lore"))));
    }

    [Fact]
    public void AcceptsDistinctValueEqualIdentifiersAmongUnrelatedKnowledge()
    {
        CharacterId candidateObserver = new("seeker");
        KnowledgeId candidateTrigger = new("ancient-script");
        ObservationReinterpretationCandidate candidate = new(
            candidateObserver,
            new ObjectId("weathered-inscription"),
            candidateTrigger);
        CharacterId characterId = new("seeker");
        KnowledgeId retainedTrigger = new("ancient-script");
        Character character = CreateCharacter(
            characterId,
            new KnowledgeId("herbal-lore"),
            retainedTrigger,
            new KnowledgeId("village-history"));

        bool result = ObservationReinterpretationCandidateTriggerPolicy.CanTrigger(
            candidate,
            character);

        Assert.True(result);
        Assert.NotSame(candidateObserver, characterId);
        Assert.NotSame(candidateTrigger, retainedTrigger);
    }

    [Fact]
    public void TargetDoesNotAffectTriggerSatisfaction()
    {
        Character character = CreateCharacter(
            new CharacterId("seeker"),
            new KnowledgeId("ancient-script"));
        ObservationReinterpretationCandidate first = Candidate(
            "seeker",
            "weathered-inscription",
            "ancient-script");
        ObservationReinterpretationCandidate second = Candidate(
            "seeker",
            "wagon-residue",
            "ancient-script");

        Assert.True(ObservationReinterpretationCandidateTriggerPolicy.CanTrigger(first, character));
        Assert.True(ObservationReinterpretationCandidateTriggerPolicy.CanTrigger(second, character));
    }

    [Fact]
    public void LeavesCandidateAndCompleteCharacterUnchanged()
    {
        CharacterId observer = new("seeker");
        ObjectId target = new("weathered-inscription");
        KnowledgeId trigger = new("ancient-script");
        ObservationReinterpretationCandidate candidate = new(observer, target, trigger);
        CharacterId characterId = new("seeker");
        KnowledgeId unrelated = new("herbal-lore");
        KnowledgeId retainedTrigger = new("ancient-script");
        Character character = CreateCharacter(characterId, unrelated, retainedTrigger);

        _ = ObservationReinterpretationCandidateTriggerPolicy.CanTrigger(candidate, character);

        Assert.Same(observer, candidate.ObserverCharacterId);
        Assert.Same(target, candidate.TargetObjectId);
        Assert.Same(trigger, candidate.TriggerKnowledgeId);
        Assert.Same(characterId, character.Identity.Id);
        Assert.Collection(
            character.Knowledge.KnowledgeReferences,
            entry => Assert.Same(unrelated, entry),
            entry => Assert.Same(retainedTrigger, entry));
    }

    [Fact]
    public void RejectsNullInputs()
    {
        ObservationReinterpretationCandidate candidate = Candidate(
            "seeker",
            "weathered-inscription",
            "ancient-script");
        Character character = CreateCharacter(new CharacterId("seeker"));

        Assert.Throws<ArgumentNullException>(
            () => ObservationReinterpretationCandidateTriggerPolicy.CanTrigger(null!, character));
        Assert.Throws<ArgumentNullException>(
            () => ObservationReinterpretationCandidateTriggerPolicy.CanTrigger(candidate, null!));
    }

    [Fact]
    public void ExposesOnlyTheSinglePureTriggerOperation()
    {
        Type policyType = typeof(ObservationReinterpretationCandidateTriggerPolicy);
        System.Reflection.MethodInfo method = Assert.Single(
            policyType.GetMethods().Where(candidate => candidate.DeclaringType == policyType));

        Assert.Equal("CanTrigger", method.Name);
        Assert.Equal(typeof(bool), method.ReturnType);
        Assert.True(method.IsStatic);
        Assert.Collection(
            method.GetParameters(),
            parameter => Assert.Equal(
                typeof(ObservationReinterpretationCandidate),
                parameter.ParameterType),
            parameter => Assert.Equal(typeof(Character), parameter.ParameterType));
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

    private static Character CreateCharacter(
        CharacterId characterId,
        params KnowledgeId[] knowledgeReferences) =>
        new(
            new CharacterIdentity(characterId, "The seeker"),
            new PublicCharacterBackground(string.Empty),
            new PrivateCharacterBackground(string.Empty),
            new CharacterCapabilitySet(Array.Empty<CharacterCapability>()),
            new CharacterLimitationSet(Array.Empty<CharacterLimitation>()),
            new CharacterControllerAssignment(characterId, ControllerType.Human),
            new CharacterConditionSet(Array.Empty<CharacterCondition>()),
            new CharacterInventory(Array.Empty<ObjectId>()),
            new CharacterKnowledge(knowledgeReferences),
            new CharacterBeliefSet(Array.Empty<CharacterBelief>()),
            new CharacterRelationshipSet(Array.Empty<CharacterRelationship>()),
            new CharacterGoalSet(Array.Empty<CharacterGoal>()));
}
