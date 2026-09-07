using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Controllers;
using Allrpcs.Domain.Discovery;
using Allrpcs.Domain.Identifiers;
using Allrpcs.Domain.Perception;

namespace Allrpcs.Domain.Tests.Discovery;

public sealed class ObservationReinterpretationCandidateApplicabilityPolicyTests
{
    [Fact]
    public void AllowsTriggeredCandidateForMatchingPerceptionTarget()
    {
        ObservationReinterpretationCandidate candidate = Candidate();
        Character character = CreateCharacter("seeker", "ancient-script");
        CharacterPerceptionSnapshot perception = Snapshot(
            "seeker",
            Visibility("weathered-inscription", VisibilityState.Noticed));

        bool result = ObservationReinterpretationCandidateApplicabilityPolicy.CanApply(
            candidate,
            character,
            perception);

        Assert.True(result);
    }

    [Fact]
    public void RejectsUnsatisfiedTriggerConditions()
    {
        ObservationReinterpretationCandidate candidate = Candidate();
        CharacterPerceptionSnapshot perception = Snapshot(
            "seeker",
            Visibility("weathered-inscription", VisibilityState.Noticed));

        Assert.False(ObservationReinterpretationCandidateApplicabilityPolicy.CanApply(
            candidate,
            CreateCharacter("travelling-healer", "ancient-script"),
            perception));
        Assert.False(ObservationReinterpretationCandidateApplicabilityPolicy.CanApply(
            candidate,
            CreateCharacter("seeker"),
            perception));
        Assert.False(ObservationReinterpretationCandidateApplicabilityPolicy.CanApply(
            candidate,
            CreateCharacter("seeker", "herbal-lore"),
            perception));
    }

    [Fact]
    public void RejectsWrongPerceptionOwnerOrAbsentTarget()
    {
        ObservationReinterpretationCandidate candidate = Candidate();
        Character character = CreateCharacter("seeker", "ancient-script");

        Assert.False(ObservationReinterpretationCandidateApplicabilityPolicy.CanApply(
            candidate,
            character,
            Snapshot(
                "travelling-healer",
                Visibility("weathered-inscription", VisibilityState.Noticed))));
        Assert.False(ObservationReinterpretationCandidateApplicabilityPolicy.CanApply(
            candidate,
            character,
            Snapshot("seeker", Visibility("wagon-residue", VisibilityState.Noticed))));
    }

    [Fact]
    public void DoesNotCompareCurrentVisibilityState()
    {
        ObservationReinterpretationCandidate candidate = Candidate();
        Character character = CreateCharacter("seeker", "ancient-script");

        foreach (VisibilityState state in VisibilityState.All)
        {
            Assert.True(ObservationReinterpretationCandidateApplicabilityPolicy.CanApply(
                candidate,
                character,
                Snapshot("seeker", Visibility("weathered-inscription", state))));
        }
    }

    [Fact]
    public void AcceptsDistinctValueEqualIdentifiersAmongUnrelatedEntries()
    {
        CharacterId observer = new("seeker");
        ObjectId target = new("weathered-inscription");
        KnowledgeId trigger = new("ancient-script");
        ObservationReinterpretationCandidate candidate = new(observer, target, trigger);
        CharacterId characterId = new("seeker");
        KnowledgeId retainedTrigger = new("ancient-script");
        Character character = CreateCharacter(characterId, retainedTrigger);
        CharacterId perceptionId = new("seeker");
        ObjectId perceptionTarget = new("weathered-inscription");
        CharacterPerceptionSnapshot perception = new(
            perceptionId,
            new[]
            {
                Visibility("wagon-residue", VisibilityState.Identified),
                new WorldObjectVisibility(perceptionTarget, VisibilityState.Noticed),
            });

        bool result = ObservationReinterpretationCandidateApplicabilityPolicy.CanApply(
            candidate,
            character,
            perception);

        Assert.True(result);
        Assert.NotSame(observer, characterId);
        Assert.NotSame(observer, perceptionId);
        Assert.NotSame(target, perceptionTarget);
        Assert.NotSame(trigger, retainedTrigger);
    }

    [Fact]
    public void LeavesAllInputsAndRetainedInstancesUnchanged()
    {
        CharacterId observer = new("seeker");
        ObjectId targetId = new("weathered-inscription");
        KnowledgeId trigger = new("ancient-script");
        ObservationReinterpretationCandidate candidate = new(observer, targetId, trigger);
        Character character = CreateCharacter(observer, trigger);
        WorldObjectVisibility unrelated = Visibility("wagon-residue", VisibilityState.Identified);
        WorldObjectVisibility target = new(targetId, VisibilityState.Noticed);
        CharacterPerceptionSnapshot perception = new(observer, new[] { unrelated, target });

        _ = ObservationReinterpretationCandidateApplicabilityPolicy.CanApply(
            candidate,
            character,
            perception);

        Assert.Same(observer, candidate.ObserverCharacterId);
        Assert.Same(targetId, candidate.TargetObjectId);
        Assert.Same(trigger, candidate.TriggerKnowledgeId);
        Assert.Same(character.Identity.Id, observer);
        Assert.Same(trigger, Assert.Single(character.Knowledge.KnowledgeReferences));
        Assert.Collection(
            perception.Entries,
            entry => Assert.Same(unrelated, entry),
            entry => Assert.Same(target, entry));
    }

    [Fact]
    public void RejectsNullInputs()
    {
        ObservationReinterpretationCandidate candidate = Candidate();
        Character character = CreateCharacter("seeker", "ancient-script");
        CharacterPerceptionSnapshot perception = Snapshot("seeker");

        Assert.Throws<ArgumentNullException>(() =>
            ObservationReinterpretationCandidateApplicabilityPolicy.CanApply(
                null!, character, perception));
        Assert.Throws<ArgumentNullException>(() =>
            ObservationReinterpretationCandidateApplicabilityPolicy.CanApply(
                candidate, null!, perception));
        Assert.Throws<ArgumentNullException>(() =>
            ObservationReinterpretationCandidateApplicabilityPolicy.CanApply(
                candidate, character, null!));
    }

    [Fact]
    public void ExposesOnlyTheSinglePureApplicabilityOperation()
    {
        Type policyType = typeof(ObservationReinterpretationCandidateApplicabilityPolicy);
        System.Reflection.MethodInfo method = Assert.Single(
            policyType.GetMethods().Where(candidate => candidate.DeclaringType == policyType));

        Assert.Equal("CanApply", method.Name);
        Assert.Equal(typeof(bool), method.ReturnType);
        Assert.True(method.IsStatic);
        Assert.Collection(
            method.GetParameters(),
            parameter => Assert.Equal(
                typeof(ObservationReinterpretationCandidate), parameter.ParameterType),
            parameter => Assert.Equal(typeof(Character), parameter.ParameterType),
            parameter => Assert.Equal(
                typeof(CharacterPerceptionSnapshot), parameter.ParameterType));
        Assert.Empty(policyType.GetConstructors());
        Assert.Empty(policyType.GetProperties());
        Assert.Empty(policyType.GetEvents());
        Assert.True(policyType.IsAbstract);
        Assert.True(policyType.IsSealed);
    }

    private static ObservationReinterpretationCandidate Candidate() =>
        new(
            new CharacterId("seeker"),
            new ObjectId("weathered-inscription"),
            new KnowledgeId("ancient-script"));

    private static Character CreateCharacter(
        string characterId,
        params string[] knowledgeReferences) =>
        CreateCharacter(
            new CharacterId(characterId),
            knowledgeReferences.Select(entry => new KnowledgeId(entry)).ToArray());

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

    private static CharacterPerceptionSnapshot Snapshot(
        string characterId,
        params WorldObjectVisibility[] entries) =>
        new(new CharacterId(characterId), entries);

    private static WorldObjectVisibility Visibility(string objectId, VisibilityState state) =>
        new(new ObjectId(objectId), state);
}
