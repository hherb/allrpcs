using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Controllers;
using Allrpcs.Domain.Discovery;
using Allrpcs.Domain.Identifiers;
using Allrpcs.Domain.Perception;

namespace Allrpcs.Domain.Tests.Discovery;

public sealed class ObservationReinterpretationCandidateResolutionSequenceTests
{
    [Fact]
    public void ResolvesFirstEligibleCandidateForObserver()
    {
        Character character = CreateCharacter("seeker", "ancient-script", "herbal-lore");
        ObservationReinterpretationCandidate selected = Candidate(
            "seeker", "weathered-inscription", "ancient-script");
        ObservationReinterpretationCandidate remaining = Candidate(
            "seeker", "healing-herb", "herbal-lore");
        WorldObjectVisibility selectedVisibility =
            Visibility("weathered-inscription", VisibilityState.Noticed);
        WorldObjectVisibility remainingVisibility =
            Visibility("healing-herb", VisibilityState.Noticed);

        ObservationReinterpretationCandidateResolutionResult result =
            ObservationReinterpretationCandidateResolutionSequence.ResolveNext(
                character,
                Perception("seeker", selectedVisibility, remainingVisibility),
                Candidates(selected, remaining));

        Assert.Collection(
            result.Perception.Entries,
            entry =>
            {
                Assert.Same(selectedVisibility.ObjectId, entry.ObjectId);
                Assert.Same(VisibilityState.Identified, entry.VisibilityState);
            },
            entry => Assert.Same(remainingVisibility, entry));
        Assert.Same(remaining, Assert.Single(result.Candidates.Entries));
    }

    [Fact]
    public void SkipsIneligibleCandidatesAndResolvesNextEligibleCandidate()
    {
        Character character = CreateCharacter("seeker", "ancient-script");
        ObservationReinterpretationCandidate otherObserver = Candidate(
            "travelling-healer", "healing-herb", "ancient-script");
        ObservationReinterpretationCandidate unmetTrigger = Candidate(
            "seeker", "wagon-residue", "road-history");
        ObservationReinterpretationCandidate alreadyIdentified = Candidate(
            "seeker", "healing-herb", "ancient-script");
        ObservationReinterpretationCandidate selected = Candidate(
            "seeker", "weathered-inscription", "ancient-script");
        CharacterPerceptionSnapshot perception = Perception(
            "seeker",
            Visibility("healing-herb", VisibilityState.Identified),
            Visibility("wagon-residue", VisibilityState.Noticed),
            Visibility("weathered-inscription", VisibilityState.Noticed));

        ObservationReinterpretationCandidateResolutionResult result =
            ObservationReinterpretationCandidateResolutionSequence.ResolveNext(
                character,
                perception,
                Candidates(otherObserver, unmetTrigger, alreadyIdentified, selected));

        Assert.Collection(
            result.Perception.Entries,
            entry => Assert.Same(perception.Entries[0], entry),
            entry => Assert.Same(perception.Entries[1], entry),
            entry =>
            {
                Assert.Same(perception.Entries[2].ObjectId, entry.ObjectId);
                Assert.Same(VisibilityState.Identified, entry.VisibilityState);
            });
        Assert.Collection(
            result.Candidates.Entries,
            entry => Assert.Same(otherObserver, entry),
            entry => Assert.Same(unmetTrigger, entry),
            entry => Assert.Same(alreadyIdentified, entry));
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

        ObservationReinterpretationCandidateResolutionResult result =
            ObservationReinterpretationCandidateResolutionSequence.ResolveNext(
                character,
                perception,
                Candidates(candidate));

        Assert.Empty(result.Candidates.Entries);
        Assert.Same(VisibilityState.Identified, result.Perception.Entries[0].VisibilityState);
    }

    [Fact]
    public void RejectsWhenNoCandidateCanResolveWithoutChangingInputs()
    {
        Character character = CreateCharacter("seeker", "ancient-script");
        ObservationReinterpretationCandidate otherObserver = Candidate(
            "travelling-healer", "weathered-inscription", "ancient-script");
        ObservationReinterpretationCandidate unmetTrigger = Candidate(
            "seeker", "weathered-inscription", "road-history");
        CharacterPerceptionSnapshot perception = Perception(
            "seeker",
            Visibility("weathered-inscription", VisibilityState.Noticed));
        ObservationReinterpretationCandidateSnapshot candidates =
            Candidates(otherObserver, unmetTrigger);

        Assert.Throws<InvalidOperationException>(() =>
            ObservationReinterpretationCandidateResolutionSequence.ResolveNext(
                character,
                perception,
                Candidates()));
        Assert.Throws<InvalidOperationException>(() =>
            ObservationReinterpretationCandidateResolutionSequence.ResolveNext(
                character,
                perception,
                candidates));
        Assert.Collection(
            candidates.Entries,
            entry => Assert.Same(otherObserver, entry),
            entry => Assert.Same(unmetTrigger, entry));
        Assert.Same(VisibilityState.Noticed, Assert.Single(perception.Entries).VisibilityState);
    }

    [Fact]
    public void LeavesEverySuccessfulInputAndNestedInstanceUnchanged()
    {
        CharacterId characterId = new("seeker");
        KnowledgeId knowledgeId = new("ancient-script");
        Character character = CreateCharacter(characterId, knowledgeId);
        CharacterId perceptionCharacterId = new("seeker");
        WorldObjectVisibility unrelated =
            Visibility("healing-herb", VisibilityState.Identified);
        ObjectId targetObjectId = new("weathered-inscription");
        WorldObjectVisibility target = new(targetObjectId, VisibilityState.Noticed);
        CharacterPerceptionSnapshot perception = new(
            perceptionCharacterId,
            [unrelated, target]);
        ObservationReinterpretationCandidate remaining = Candidate(
            "travelling-healer", "healing-herb", "herbal-lore");
        ObservationReinterpretationCandidate selected = Candidate(
            "seeker", "weathered-inscription", "ancient-script");
        ObservationReinterpretationCandidateSnapshot candidates =
            Candidates(remaining, selected);

        ObservationReinterpretationCandidateResolutionResult result =
            ObservationReinterpretationCandidateResolutionSequence.ResolveNext(
                character,
                perception,
                candidates);

        Assert.NotSame(perception, result.Perception);
        Assert.NotSame(candidates, result.Candidates);
        Assert.Same(characterId, character.Identity.Id);
        Assert.Same(knowledgeId, Assert.Single(character.Knowledge.KnowledgeReferences));
        Assert.Same(perceptionCharacterId, perception.CharacterId);
        Assert.Collection(
            perception.Entries,
            entry => Assert.Same(unrelated, entry),
            entry => Assert.Same(target, entry));
        Assert.Same(targetObjectId, target.ObjectId);
        Assert.Same(VisibilityState.Noticed, target.VisibilityState);
        Assert.Collection(
            candidates.Entries,
            entry => Assert.Same(remaining, entry),
            entry => Assert.Same(selected, entry));
        Assert.Same(remaining, Assert.Single(result.Candidates.Entries));
    }

    [Fact]
    public void RejectsEveryNullInput()
    {
        Character character = CreateCharacter("seeker", "ancient-script");
        CharacterPerceptionSnapshot perception = Perception("seeker");
        ObservationReinterpretationCandidateSnapshot candidates = Candidates();

        Assert.Throws<ArgumentNullException>(() =>
            ObservationReinterpretationCandidateResolutionSequence.ResolveNext(
                null!, perception, candidates));
        Assert.Throws<ArgumentNullException>(() =>
            ObservationReinterpretationCandidateResolutionSequence.ResolveNext(
                character, null!, candidates));
        Assert.Throws<ArgumentNullException>(() =>
            ObservationReinterpretationCandidateResolutionSequence.ResolveNext(
                character, perception, null!));
    }

    [Fact]
    public void ExposesOnlyTheSinglePureResolutionSequenceOperation()
    {
        Type sequenceType = typeof(ObservationReinterpretationCandidateResolutionSequence);
        System.Reflection.MethodInfo method = Assert.Single(
            sequenceType.GetMethods().Where(candidate => candidate.DeclaringType == sequenceType));

        Assert.Equal("ResolveNext", method.Name);
        Assert.Equal(typeof(ObservationReinterpretationCandidateResolutionResult), method.ReturnType);
        Assert.True(method.IsStatic);
        Assert.Collection(
            method.GetParameters(),
            parameter => Assert.Equal(typeof(Character), parameter.ParameterType),
            parameter => Assert.Equal(
                typeof(CharacterPerceptionSnapshot), parameter.ParameterType),
            parameter => Assert.Equal(
                typeof(ObservationReinterpretationCandidateSnapshot), parameter.ParameterType));
        Assert.Empty(sequenceType.GetConstructors());
        Assert.Empty(sequenceType.GetProperties());
        Assert.Empty(sequenceType.GetEvents());
        Assert.True(sequenceType.IsAbstract);
        Assert.True(sequenceType.IsSealed);
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
