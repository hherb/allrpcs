using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Controllers;
using Allrpcs.Domain.Discovery;
using Allrpcs.Domain.Identifiers;
using Allrpcs.Domain.Perception;

namespace Allrpcs.Domain.Tests.Discovery;

public sealed class ObservationReinterpretationCandidateResolutionTransitionTests
{
    [Fact]
    public void RejectsEveryDelegatedApplicationIneligibilityCategory()
    {
        ObservationReinterpretationCandidate candidate = Candidate();
        Character matchingCharacter = CreateCharacter("seeker", "ancient-script");
        CharacterPerceptionSnapshot matchingPerception = Perception(
            "seeker",
            Visibility("weathered-inscription", VisibilityState.Noticed));

        Assert.Throws<InvalidOperationException>(() =>
            ObservationReinterpretationCandidateResolutionTransition.Resolve(
                candidate,
                CreateCharacter("seeker"),
                matchingPerception,
                Candidates(candidate)));
        Assert.Throws<InvalidOperationException>(() =>
            ObservationReinterpretationCandidateResolutionTransition.Resolve(
                candidate,
                matchingCharacter,
                Perception(
                    "travelling-healer",
                    Visibility("weathered-inscription", VisibilityState.Noticed)),
                Candidates(candidate)));
        Assert.Throws<InvalidOperationException>(() =>
            ObservationReinterpretationCandidateResolutionTransition.Resolve(
                candidate,
                matchingCharacter,
                Perception("seeker", Visibility("wagon-residue", VisibilityState.Noticed)),
                Candidates(candidate)));
        Assert.Throws<InvalidOperationException>(() =>
            ObservationReinterpretationCandidateResolutionTransition.Resolve(
                candidate,
                matchingCharacter,
                Perception(
                    "seeker",
                    Visibility("weathered-inscription", VisibilityState.Identified)),
                Candidates(candidate)));
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
            Assert.Throws<InvalidOperationException>(() =>
                ObservationReinterpretationCandidateResolutionTransition.Resolve(
                    candidate,
                    character,
                    perception,
                    candidates));
        }
    }

    [Fact]
    public void ResolvesPerceptionAndConsumesCandidateWithStableOrderAndExactInstances()
    {
        CharacterId candidateCharacterId = new("seeker");
        ObjectId candidateObjectId = new("weathered-inscription");
        KnowledgeId candidateKnowledgeId = new("ancient-script");
        ObservationReinterpretationCandidate candidate = new(
            candidateCharacterId,
            candidateObjectId,
            candidateKnowledgeId);
        CharacterId characterId = new("seeker");
        KnowledgeId knowledgeId = new("ancient-script");
        Character character = CreateCharacter(characterId, knowledgeId);
        CharacterId perceptionCharacterId = new("seeker");
        WorldObjectVisibility firstVisibility =
            Visibility("wagon-residue", VisibilityState.Identified);
        ObjectId perceptionObjectId = new("weathered-inscription");
        WorldObjectVisibility targetVisibility =
            new(perceptionObjectId, VisibilityState.Noticed);
        WorldObjectVisibility thirdVisibility =
            Visibility("healing-herb", VisibilityState.Understood);
        CharacterPerceptionSnapshot perception = new(
            perceptionCharacterId,
            new[] { firstVisibility, targetVisibility, thirdVisibility });
        ObservationReinterpretationCandidate firstCandidate = Candidate(
            "sage",
            "wagon-residue",
            "road-history");
        ObservationReinterpretationCandidate retainedCandidate = Candidate();
        ObservationReinterpretationCandidate thirdCandidate = Candidate(
            "travelling-healer",
            "healing-herb",
            "herbal-lore");
        ObservationReinterpretationCandidateSnapshot candidates =
            Candidates(firstCandidate, retainedCandidate, thirdCandidate);

        ObservationReinterpretationCandidateResolutionResult result =
            ObservationReinterpretationCandidateResolutionTransition.Resolve(
                candidate,
                character,
                perception,
                candidates);

        Assert.NotSame(perception, result.Perception);
        Assert.Same(perceptionCharacterId, result.Perception.CharacterId);
        Assert.Collection(
            result.Perception.Entries,
            entry => Assert.Same(firstVisibility, entry),
            entry =>
            {
                Assert.NotSame(targetVisibility, entry);
                Assert.Same(perceptionObjectId, entry.ObjectId);
                Assert.Same(VisibilityState.Identified, entry.VisibilityState);
            },
            entry => Assert.Same(thirdVisibility, entry));
        Assert.NotSame(candidates, result.Candidates);
        Assert.Collection(
            result.Candidates.Entries,
            entry => Assert.Same(firstCandidate, entry),
            entry => Assert.Same(thirdCandidate, entry));
        Assert.Same(characterId, character.Identity.Id);
        Assert.Same(knowledgeId, Assert.Single(character.Knowledge.KnowledgeReferences));
    }

    [Fact]
    public void LeavesEverySuccessfulInputAndRetainedInstanceUnchanged()
    {
        CharacterId observerCharacterId = new("seeker");
        ObjectId candidateObjectId = new("weathered-inscription");
        KnowledgeId triggerKnowledgeId = new("ancient-script");
        ObservationReinterpretationCandidate candidate = new(
            observerCharacterId,
            candidateObjectId,
            triggerKnowledgeId);
        Character character = CreateCharacter("seeker", "ancient-script");
        CharacterId perceptionCharacterId = new("seeker");
        ObjectId perceptionObjectId = new("weathered-inscription");
        WorldObjectVisibility targetVisibility =
            new(perceptionObjectId, VisibilityState.Noticed);
        CharacterPerceptionSnapshot perception =
            new(perceptionCharacterId, new[] { targetVisibility });
        ObservationReinterpretationCandidate retainedCandidate = Candidate();
        ObservationReinterpretationCandidateSnapshot candidates = Candidates(retainedCandidate);

        _ = ObservationReinterpretationCandidateResolutionTransition.Resolve(
            candidate,
            character,
            perception,
            candidates);

        Assert.Same(observerCharacterId, candidate.ObserverCharacterId);
        Assert.Same(candidateObjectId, candidate.TargetObjectId);
        Assert.Same(triggerKnowledgeId, candidate.TriggerKnowledgeId);
        Assert.Same(perceptionCharacterId, perception.CharacterId);
        Assert.Same(targetVisibility, Assert.Single(perception.Entries));
        Assert.Same(perceptionObjectId, targetVisibility.ObjectId);
        Assert.Same(VisibilityState.Noticed, targetVisibility.VisibilityState);
        Assert.Same(retainedCandidate, Assert.Single(candidates.Entries));
    }

    [Fact]
    public void LeavesRejectedInputsAndRetainedInstancesUnchanged()
    {
        ObservationReinterpretationCandidate candidate = Candidate();
        Character character = CreateCharacter("seeker", "ancient-script");
        WorldObjectVisibility targetVisibility =
            Visibility("weathered-inscription", VisibilityState.Identified);
        CharacterPerceptionSnapshot perception = Perception("seeker", targetVisibility);
        ObservationReinterpretationCandidate retainedCandidate = Candidate();
        ObservationReinterpretationCandidateSnapshot candidates = Candidates(retainedCandidate);

        Assert.Throws<InvalidOperationException>(() =>
            ObservationReinterpretationCandidateResolutionTransition.Resolve(
                candidate,
                character,
                perception,
                candidates));

        Assert.Same(targetVisibility, Assert.Single(perception.Entries));
        Assert.Same(retainedCandidate, Assert.Single(candidates.Entries));
    }

    [Fact]
    public void RejectsNullInputsThroughResolutionPolicy()
    {
        ObservationReinterpretationCandidate candidate = Candidate();
        Character character = CreateCharacter("seeker", "ancient-script");
        CharacterPerceptionSnapshot perception = Perception(
            "seeker",
            Visibility("weathered-inscription", VisibilityState.Noticed));
        ObservationReinterpretationCandidateSnapshot candidates = Candidates(candidate);

        Assert.Throws<ArgumentNullException>(() =>
            ObservationReinterpretationCandidateResolutionTransition.Resolve(
                null!, character, perception, candidates));
        Assert.Throws<ArgumentNullException>(() =>
            ObservationReinterpretationCandidateResolutionTransition.Resolve(
                candidate, null!, perception, candidates));
        Assert.Throws<ArgumentNullException>(() =>
            ObservationReinterpretationCandidateResolutionTransition.Resolve(
                candidate, character, null!, candidates));
        Assert.Throws<ArgumentNullException>(() =>
            ObservationReinterpretationCandidateResolutionTransition.Resolve(
                candidate, character, perception, null!));
    }

    [Fact]
    public void ExposesOnlyTheSinglePureResolutionOperation()
    {
        Type transitionType = typeof(ObservationReinterpretationCandidateResolutionTransition);
        System.Reflection.MethodInfo method = Assert.Single(
            transitionType.GetMethods().Where(candidate => candidate.DeclaringType == transitionType));

        Assert.Equal("Resolve", method.Name);
        Assert.Equal(typeof(ObservationReinterpretationCandidateResolutionResult), method.ReturnType);
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
