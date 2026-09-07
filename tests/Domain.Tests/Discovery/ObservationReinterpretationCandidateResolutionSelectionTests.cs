using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Controllers;
using Allrpcs.Domain.Discovery;
using Allrpcs.Domain.Identifiers;
using Allrpcs.Domain.Perception;

namespace Allrpcs.Domain.Tests.Discovery;

public sealed class ObservationReinterpretationCandidateResolutionSelectionTests
{
    [Fact]
    public void SelectsExactFirstResolvableCandidateForObserverInStableOrder()
    {
        Character character = CreateCharacter("seeker", "ancient-script", "herbal-lore");
        ObservationReinterpretationCandidate first = Candidate(
            "seeker", "weathered-inscription", "ancient-script");
        ObservationReinterpretationCandidate second = Candidate(
            "seeker", "healing-herb", "herbal-lore");
        CharacterPerceptionSnapshot perception = Perception(
            "seeker",
            Visibility("weathered-inscription", VisibilityState.Noticed),
            Visibility("healing-herb", VisibilityState.Noticed));

        ObservationReinterpretationCandidate? result =
            ObservationReinterpretationCandidateResolutionSelection.SelectNext(
                character,
                perception,
                Candidates(first, second));

        Assert.Same(first, result);
    }

    [Fact]
    public void AcceptsDistinctValueEqualCharacterAndKnowledgeIdentifiers()
    {
        CharacterId characterId = new("seeker");
        KnowledgeId knowledgeId = new("ancient-script");
        Character character = CreateCharacter(characterId, knowledgeId);
        ObservationReinterpretationCandidate candidate = new(
            new CharacterId(characterId.Value),
            new ObjectId("weathered-inscription"),
            new KnowledgeId(knowledgeId.Value));
        CharacterPerceptionSnapshot perception = Perception(
            "seeker",
            Visibility("weathered-inscription", VisibilityState.Noticed));

        ObservationReinterpretationCandidate? result =
            ObservationReinterpretationCandidateResolutionSelection.SelectNext(
                character,
                perception,
                Candidates(candidate));

        Assert.NotSame(characterId, candidate.ObserverCharacterId);
        Assert.NotSame(knowledgeId, candidate.TriggerKnowledgeId);
        Assert.Same(candidate, result);
    }

    [Fact]
    public void SkipsEveryIneligibleCategoryAndSelectsNextEligibleCandidate()
    {
        Character character = CreateCharacter("seeker", "ancient-script");
        ObservationReinterpretationCandidate otherObserver = Candidate(
            "travelling-healer", "healing-herb", "ancient-script");
        ObservationReinterpretationCandidate unmetTrigger = Candidate(
            "seeker", "wagon-residue", "road-history");
        ObservationReinterpretationCandidate alreadyIdentified = Candidate(
            "seeker", "healing-herb", "ancient-script");
        ObservationReinterpretationCandidate missingTarget = Candidate(
            "seeker", "missing-object", "ancient-script");
        ObservationReinterpretationCandidate selected = Candidate(
            "seeker", "weathered-inscription", "ancient-script");
        CharacterPerceptionSnapshot perception = Perception(
            "seeker",
            Visibility("healing-herb", VisibilityState.Identified),
            Visibility("wagon-residue", VisibilityState.Noticed),
            Visibility("weathered-inscription", VisibilityState.Noticed));

        ObservationReinterpretationCandidate? result =
            ObservationReinterpretationCandidateResolutionSelection.SelectNext(
                character,
                perception,
                Candidates(
                    otherObserver,
                    unmetTrigger,
                    alreadyIdentified,
                    missingTarget,
                    selected));

        Assert.Same(selected, result);
    }

    [Fact]
    public void ReturnsNullWhenNoCandidateCanResolve()
    {
        Character character = CreateCharacter("seeker", "ancient-script");
        ObservationReinterpretationCandidate otherObserver = Candidate(
            "travelling-healer", "weathered-inscription", "ancient-script");
        ObservationReinterpretationCandidate unmetTrigger = Candidate(
            "seeker", "weathered-inscription", "road-history");
        CharacterPerceptionSnapshot perception = Perception(
            "seeker",
            Visibility("weathered-inscription", VisibilityState.Noticed));

        Assert.Null(ObservationReinterpretationCandidateResolutionSelection.SelectNext(
            character,
            perception,
            Candidates()));
        Assert.Null(ObservationReinterpretationCandidateResolutionSelection.SelectNext(
            character,
            perception,
            Candidates(otherObserver, unmetTrigger)));
        Assert.Null(ObservationReinterpretationCandidateResolutionSelection.SelectNext(
            character,
            Perception(
                "travelling-healer",
                Visibility("weathered-inscription", VisibilityState.Noticed)),
            Candidates(Candidate(
                "seeker", "weathered-inscription", "ancient-script"))));
    }

    [Fact]
    public void LeavesEveryInputAndNestedInstanceUnchanged()
    {
        CharacterId characterId = new("seeker");
        KnowledgeId knowledgeId = new("ancient-script");
        Character character = CreateCharacter(characterId, knowledgeId);
        CharacterId perceptionCharacterId = new("seeker");
        ObjectId skippedObjectId = new("healing-herb");
        WorldObjectVisibility skippedVisibility =
            new(skippedObjectId, VisibilityState.Identified);
        ObjectId selectedObjectId = new("weathered-inscription");
        WorldObjectVisibility selectedVisibility =
            new(selectedObjectId, VisibilityState.Noticed);
        CharacterPerceptionSnapshot perception = new(
            perceptionCharacterId,
            [skippedVisibility, selectedVisibility]);
        ObservationReinterpretationCandidate skipped = Candidate(
            "seeker", "healing-herb", "ancient-script");
        ObservationReinterpretationCandidate selected = Candidate(
            "seeker", "weathered-inscription", "ancient-script");
        ObservationReinterpretationCandidateSnapshot candidates =
            Candidates(skipped, selected);

        ObservationReinterpretationCandidate? result =
            ObservationReinterpretationCandidateResolutionSelection.SelectNext(
                character,
                perception,
                candidates);

        Assert.Same(selected, result);
        Assert.Same(characterId, character.Identity.Id);
        Assert.Same(knowledgeId, Assert.Single(character.Knowledge.KnowledgeReferences));
        Assert.Same(perceptionCharacterId, perception.CharacterId);
        Assert.Collection(
            perception.Entries,
            entry => Assert.Same(skippedVisibility, entry),
            entry => Assert.Same(selectedVisibility, entry));
        Assert.Same(skippedObjectId, skippedVisibility.ObjectId);
        Assert.Same(selectedObjectId, selectedVisibility.ObjectId);
        Assert.Collection(
            candidates.Entries,
            entry => Assert.Same(skipped, entry),
            entry => Assert.Same(selected, entry));
    }

    [Fact]
    public void RejectsEveryNullInput()
    {
        Character character = CreateCharacter("seeker", "ancient-script");
        CharacterPerceptionSnapshot perception = Perception("seeker");
        ObservationReinterpretationCandidateSnapshot candidates = Candidates();

        Assert.Throws<ArgumentNullException>(() =>
            ObservationReinterpretationCandidateResolutionSelection.SelectNext(
                null!, perception, candidates));
        Assert.Throws<ArgumentNullException>(() =>
            ObservationReinterpretationCandidateResolutionSelection.SelectNext(
                character, null!, candidates));
        Assert.Throws<ArgumentNullException>(() =>
            ObservationReinterpretationCandidateResolutionSelection.SelectNext(
                character, perception, null!));
    }

    [Fact]
    public void ExposesOnlyTheSinglePureSelectionOperation()
    {
        Type selectionType = typeof(ObservationReinterpretationCandidateResolutionSelection);
        System.Reflection.MethodInfo method = Assert.Single(
            selectionType.GetMethods().Where(candidate => candidate.DeclaringType == selectionType));

        Assert.Equal("SelectNext", method.Name);
        Assert.Equal(typeof(ObservationReinterpretationCandidate), method.ReturnType);
        Assert.True(method.IsStatic);
        Assert.Collection(
            method.GetParameters(),
            parameter => Assert.Equal(typeof(Character), parameter.ParameterType),
            parameter => Assert.Equal(
                typeof(CharacterPerceptionSnapshot), parameter.ParameterType),
            parameter => Assert.Equal(
                typeof(ObservationReinterpretationCandidateSnapshot), parameter.ParameterType));
        Assert.Empty(selectionType.GetConstructors());
        Assert.Empty(selectionType.GetProperties());
        Assert.Empty(selectionType.GetEvents());
        Assert.True(selectionType.IsAbstract);
        Assert.True(selectionType.IsSealed);
    }

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
