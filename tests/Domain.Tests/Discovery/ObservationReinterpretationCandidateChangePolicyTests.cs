using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Controllers;
using Allrpcs.Domain.Discovery;
using Allrpcs.Domain.Identifiers;
using Allrpcs.Domain.Perception;

namespace Allrpcs.Domain.Tests.Discovery;

public sealed class ObservationReinterpretationCandidateChangePolicyTests
{
    [Fact]
    public void AllowsApplicableNoticedTargetToChange()
    {
        ObservationReinterpretationCandidate candidate = Candidate();
        Character character = CreateCharacter("seeker", "ancient-script");
        CharacterPerceptionSnapshot perception = Snapshot(
            "seeker",
            Visibility("weathered-inscription", VisibilityState.Noticed));

        bool result = ObservationReinterpretationCandidateChangePolicy.WouldChange(
            candidate,
            character,
            perception);

        Assert.True(result);
    }

    [Fact]
    public void RejectsEveryStateOtherThanNoticed()
    {
        ObservationReinterpretationCandidate candidate = Candidate();
        Character character = CreateCharacter("seeker", "ancient-script");

        foreach (VisibilityState state in VisibilityState.All)
        {
            bool result = ObservationReinterpretationCandidateChangePolicy.WouldChange(
                candidate,
                character,
                Snapshot("seeker", Visibility("weathered-inscription", state)));

            Assert.Equal(state == VisibilityState.Noticed, result);
        }
    }

    [Fact]
    public void RejectsEachInapplicableCombination()
    {
        ObservationReinterpretationCandidate candidate = Candidate();
        Character character = CreateCharacter("seeker", "ancient-script");
        CharacterPerceptionSnapshot perception = Snapshot(
            "seeker",
            Visibility("weathered-inscription", VisibilityState.Noticed));

        Assert.False(ObservationReinterpretationCandidateChangePolicy.WouldChange(
            candidate,
            CreateCharacter("travelling-healer", "ancient-script"),
            perception));
        Assert.False(ObservationReinterpretationCandidateChangePolicy.WouldChange(
            candidate,
            CreateCharacter("seeker"),
            perception));
        Assert.False(ObservationReinterpretationCandidateChangePolicy.WouldChange(
            candidate,
            character,
            Snapshot(
                "travelling-healer",
                Visibility("weathered-inscription", VisibilityState.Noticed))));
        Assert.False(ObservationReinterpretationCandidateChangePolicy.WouldChange(
            candidate,
            character,
            Snapshot("seeker", Visibility("wagon-residue", VisibilityState.Noticed))));
    }

    [Fact]
    public void AcceptsDistinctValueEqualIdentifiersAndFindsTargetAmongUnrelatedEntries()
    {
        CharacterId observer = new("seeker");
        ObjectId target = new("weathered-inscription");
        KnowledgeId trigger = new("ancient-script");
        ObservationReinterpretationCandidate candidate = new(observer, target, trigger);
        Character character = CreateCharacter(
            new CharacterId("seeker"),
            new KnowledgeId("ancient-script"));
        ObjectId perceptionTarget = new("weathered-inscription");
        WorldObjectVisibility retainedTarget = new(perceptionTarget, VisibilityState.Noticed);
        CharacterPerceptionSnapshot perception = new(
            new CharacterId("seeker"),
            new[]
            {
                Visibility("wagon-residue", VisibilityState.Noticed),
                retainedTarget,
                Visibility("healing-herb", VisibilityState.Identified),
            });

        bool result = ObservationReinterpretationCandidateChangePolicy.WouldChange(
            candidate,
            character,
            perception);

        Assert.True(result);
        Assert.NotSame(target, perceptionTarget);
        Assert.Same(perceptionTarget, retainedTarget.ObjectId);
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
        VisibilityState state = VisibilityState.Noticed;
        WorldObjectVisibility target = new(targetId, state);
        CharacterPerceptionSnapshot perception = new(observer, new[] { unrelated, target });

        _ = ObservationReinterpretationCandidateChangePolicy.WouldChange(
            candidate,
            character,
            perception);

        Assert.Same(observer, candidate.ObserverCharacterId);
        Assert.Same(targetId, candidate.TargetObjectId);
        Assert.Same(trigger, candidate.TriggerKnowledgeId);
        Assert.Same(trigger, Assert.Single(character.Knowledge.KnowledgeReferences));
        Assert.Collection(
            perception.Entries,
            entry => Assert.Same(unrelated, entry),
            entry =>
            {
                Assert.Same(target, entry);
                Assert.Same(state, entry.VisibilityState);
            });
    }

    [Fact]
    public void RejectsNullInputs()
    {
        ObservationReinterpretationCandidate candidate = Candidate();
        Character character = CreateCharacter("seeker", "ancient-script");
        CharacterPerceptionSnapshot perception = Snapshot("seeker");

        Assert.Throws<ArgumentNullException>(() =>
            ObservationReinterpretationCandidateChangePolicy.WouldChange(
                null!, character, perception));
        Assert.Throws<ArgumentNullException>(() =>
            ObservationReinterpretationCandidateChangePolicy.WouldChange(
                candidate, null!, perception));
        Assert.Throws<ArgumentNullException>(() =>
            ObservationReinterpretationCandidateChangePolicy.WouldChange(
                candidate, character, null!));
    }

    [Fact]
    public void ExposesOnlyTheSinglePureChangeOperation()
    {
        Type policyType = typeof(ObservationReinterpretationCandidateChangePolicy);
        System.Reflection.MethodInfo method = Assert.Single(
            policyType.GetMethods().Where(candidate => candidate.DeclaringType == policyType));

        Assert.Equal("WouldChange", method.Name);
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
