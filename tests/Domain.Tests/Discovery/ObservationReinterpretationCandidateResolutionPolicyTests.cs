using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Controllers;
using Allrpcs.Domain.Discovery;
using Allrpcs.Domain.Identifiers;
using Allrpcs.Domain.Perception;

namespace Allrpcs.Domain.Tests.Discovery;

public sealed class ObservationReinterpretationCandidateResolutionPolicyTests
{
    [Fact]
    public void RejectsEveryDelegatedApplicationIneligibilityCategory()
    {
        ObservationReinterpretationCandidate candidate = Candidate();
        Character matchingCharacter = CreateCharacter("seeker", "ancient-script");
        CharacterPerceptionSnapshot matchingPerception = Perception(
            "seeker",
            Visibility("weathered-inscription", VisibilityState.Noticed));

        Assert.False(ObservationReinterpretationCandidateResolutionPolicy.CanResolve(
            candidate,
            CreateCharacter("seeker"),
            matchingPerception,
            Candidates(candidate)));
        Assert.False(ObservationReinterpretationCandidateResolutionPolicy.CanResolve(
            candidate,
            CreateCharacter("travelling-healer", "ancient-script"),
            matchingPerception,
            Candidates(candidate)));
        Assert.False(ObservationReinterpretationCandidateResolutionPolicy.CanResolve(
            candidate,
            matchingCharacter,
            Perception(
                "travelling-healer",
                Visibility("weathered-inscription", VisibilityState.Noticed)),
            Candidates(candidate)));
        Assert.False(ObservationReinterpretationCandidateResolutionPolicy.CanResolve(
            candidate,
            matchingCharacter,
            Perception("seeker", Visibility("wagon-residue", VisibilityState.Noticed)),
            Candidates(candidate)));

        foreach (VisibilityState state in VisibilityState.All.Where(
                     state => state != VisibilityState.Noticed))
        {
            Assert.False(ObservationReinterpretationCandidateResolutionPolicy.CanResolve(
                candidate,
                matchingCharacter,
                Perception("seeker", Visibility("weathered-inscription", state)),
                Candidates(candidate)));
        }
    }

    [Fact]
    public void RejectsEveryDelegatedConsumptionIneligibilityCategory()
    {
        ObservationReinterpretationCandidate candidate = Candidate();
        Character character = CreateCharacter("seeker", "ancient-script");
        CharacterPerceptionSnapshot perception = Perception(
            "seeker",
            Visibility("weathered-inscription", VisibilityState.Noticed));
        ObservationReinterpretationCandidateSnapshot[] ineligibleCandidates =
        [
            Candidates(),
            Candidates(Candidate("travelling-healer", "weathered-inscription", "ancient-script")),
            Candidates(Candidate("seeker", "healing-herb", "ancient-script")),
            Candidates(Candidate("seeker", "weathered-inscription", "herbal-lore")),
        ];

        foreach (ObservationReinterpretationCandidateSnapshot candidates in ineligibleCandidates)
        {
            Assert.False(ObservationReinterpretationCandidateResolutionPolicy.CanResolve(
                candidate,
                character,
                perception,
                candidates));
        }
    }

    [Fact]
    public void AllowsExactAndValueEqualCandidatesWhenPerceptionWouldChange()
    {
        ObservationReinterpretationCandidate candidate = Candidate();
        ObservationReinterpretationCandidate retained = Candidate();
        Character character = CreateCharacter("seeker", "ancient-script");
        CharacterPerceptionSnapshot perception = Perception(
            "seeker",
            Visibility("weathered-inscription", VisibilityState.Noticed));

        Assert.True(ObservationReinterpretationCandidateResolutionPolicy.CanResolve(
            candidate,
            character,
            perception,
            Candidates(candidate)));
        Assert.NotSame(candidate, retained);
        Assert.Equal(candidate, retained);
        Assert.True(ObservationReinterpretationCandidateResolutionPolicy.CanResolve(
            candidate,
            character,
            perception,
            Candidates(retained)));
    }

    [Fact]
    public void LeavesEveryInputAndRetainedInstanceUnchanged()
    {
        CharacterId observerCharacterId = new("seeker");
        ObjectId candidateObjectId = new("weathered-inscription");
        KnowledgeId triggerKnowledgeId = new("ancient-script");
        ObservationReinterpretationCandidate candidate = new(
            observerCharacterId,
            candidateObjectId,
            triggerKnowledgeId);
        CharacterId characterId = new("seeker");
        KnowledgeId retainedKnowledgeId = new("ancient-script");
        Character character = CreateCharacter(characterId, retainedKnowledgeId);
        CharacterId perceptionCharacterId = new("seeker");
        WorldObjectVisibility unrelated =
            Visibility("wagon-residue", VisibilityState.Identified);
        ObjectId perceptionObjectId = new("weathered-inscription");
        WorldObjectVisibility target =
            new(perceptionObjectId, VisibilityState.Noticed);
        CharacterPerceptionSnapshot perception = new(
            perceptionCharacterId,
            new[] { unrelated, target });
        ObservationReinterpretationCandidate before = Candidate(
            "sage",
            "healing-herb",
            "herbal-lore");
        ObservationReinterpretationCandidate retained = Candidate();
        ObservationReinterpretationCandidateSnapshot candidates = Candidates(before, retained);

        _ = ObservationReinterpretationCandidateResolutionPolicy.CanResolve(
            candidate,
            character,
            perception,
            candidates);

        Assert.Same(observerCharacterId, candidate.ObserverCharacterId);
        Assert.Same(candidateObjectId, candidate.TargetObjectId);
        Assert.Same(triggerKnowledgeId, candidate.TriggerKnowledgeId);
        Assert.Same(characterId, character.Identity.Id);
        Assert.Same(
            retainedKnowledgeId,
            Assert.Single(character.Knowledge.KnowledgeReferences));
        Assert.Same(perceptionCharacterId, perception.CharacterId);
        Assert.Collection(
            perception.Entries,
            entry => Assert.Same(unrelated, entry),
            entry => Assert.Same(target, entry));
        Assert.Same(perceptionObjectId, target.ObjectId);
        Assert.Same(VisibilityState.Noticed, target.VisibilityState);
        Assert.Collection(
            candidates.Entries,
            entry => Assert.Same(before, entry),
            entry => Assert.Same(retained, entry));
    }

    [Fact]
    public void RejectsNullInputsThroughDelegatedPolicies()
    {
        ObservationReinterpretationCandidate candidate = Candidate();
        Character character = CreateCharacter("seeker", "ancient-script");
        CharacterPerceptionSnapshot perception = Perception(
            "seeker",
            Visibility("weathered-inscription", VisibilityState.Noticed));
        ObservationReinterpretationCandidateSnapshot candidates = Candidates(candidate);

        Assert.Throws<ArgumentNullException>(() =>
            ObservationReinterpretationCandidateResolutionPolicy.CanResolve(
                null!, character, perception, candidates));
        Assert.Throws<ArgumentNullException>(() =>
            ObservationReinterpretationCandidateResolutionPolicy.CanResolve(
                candidate, null!, perception, candidates));
        Assert.Throws<ArgumentNullException>(() =>
            ObservationReinterpretationCandidateResolutionPolicy.CanResolve(
                candidate, character, null!, candidates));
        Assert.Throws<ArgumentNullException>(() =>
            ObservationReinterpretationCandidateResolutionPolicy.CanResolve(
                candidate, character, perception, null!));
    }

    [Fact]
    public void ExposesOnlyTheSinglePureResolutionEligibilityOperation()
    {
        Type policyType = typeof(ObservationReinterpretationCandidateResolutionPolicy);
        System.Reflection.MethodInfo method = Assert.Single(
            policyType.GetMethods().Where(candidate => candidate.DeclaringType == policyType));

        Assert.Equal("CanResolve", method.Name);
        Assert.Equal(typeof(bool), method.ReturnType);
        Assert.True(method.IsStatic);
        Assert.Collection(
            method.GetParameters(),
            parameter => Assert.Equal(
                typeof(ObservationReinterpretationCandidate), parameter.ParameterType),
            parameter => Assert.Equal(typeof(Character), parameter.ParameterType),
            parameter => Assert.Equal(
                typeof(CharacterPerceptionSnapshot), parameter.ParameterType),
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
            new CharacterIdentity(characterId, "The observer"),
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

    private static CharacterPerceptionSnapshot Perception(
        string characterId,
        params WorldObjectVisibility[] entries) =>
        new(new CharacterId(characterId), entries);

    private static WorldObjectVisibility Visibility(
        string objectId,
        VisibilityState state) =>
        new(new ObjectId(objectId), state);
}
