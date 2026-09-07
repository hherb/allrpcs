using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Controllers;
using Allrpcs.Domain.Discovery;
using Allrpcs.Domain.Identifiers;
using Allrpcs.Domain.Perception;

namespace Allrpcs.Domain.Tests.Discovery;

public sealed class ObservationReinterpretationCandidateApplicationTransitionTests
{
    [Fact]
    public void ChangesOnlyApplicableNoticedTargetToIdentified()
    {
        ObservationReinterpretationCandidate candidate = Candidate();
        Character character = CreateCharacter("seeker", "ancient-script");
        WorldObjectVisibility first = Visibility("wagon-residue", VisibilityState.Identified);
        ObjectId targetId = new("weathered-inscription");
        WorldObjectVisibility target = new(targetId, VisibilityState.Noticed);
        WorldObjectVisibility third = Visibility("healing-herb", VisibilityState.Understood);
        CharacterPerceptionSnapshot perception = Snapshot("seeker", first, target, third);

        CharacterPerceptionSnapshot result =
            ObservationReinterpretationCandidateApplicationTransition.Apply(
                candidate,
                character,
                perception);

        Assert.NotSame(perception, result);
        Assert.Same(perception.CharacterId, result.CharacterId);
        Assert.Collection(
            result.Entries,
            entry => Assert.Same(first, entry),
            entry =>
            {
                Assert.NotSame(target, entry);
                Assert.Same(targetId, entry.ObjectId);
                Assert.Same(VisibilityState.Identified, entry.VisibilityState);
            },
            entry => Assert.Same(third, entry));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    public void ReplacesTargetInItsStablePosition(int targetIndex)
    {
        ObservationReinterpretationCandidate candidate = Candidate();
        Character character = CreateCharacter("seeker", "ancient-script");
        WorldObjectVisibility[] entries =
        [
            Visibility("wagon-residue", VisibilityState.Identified),
            Visibility("weathered-inscription", VisibilityState.Noticed),
            Visibility("healing-herb", VisibilityState.Understood),
        ];
        (entries[targetIndex], entries[1]) = (entries[1], entries[targetIndex]);
        CharacterPerceptionSnapshot perception = Snapshot("seeker", entries);

        CharacterPerceptionSnapshot result =
            ObservationReinterpretationCandidateApplicationTransition.Apply(
                candidate,
                character,
                perception);

        for (int index = 0; index < entries.Length; index++)
        {
            if (index == targetIndex)
            {
                Assert.NotSame(entries[index], result.Entries[index]);
                Assert.Same(entries[index].ObjectId, result.Entries[index].ObjectId);
                Assert.Same(VisibilityState.Identified, result.Entries[index].VisibilityState);
            }
            else
            {
                Assert.Same(entries[index], result.Entries[index]);
            }
        }
    }

    [Fact]
    public void AcceptsDistinctValueEqualIdentifiersAndRetainsPerceptionIdentifiers()
    {
        ObservationReinterpretationCandidate candidate = Candidate();
        CharacterId characterId = new("seeker");
        KnowledgeId knowledgeId = new("ancient-script");
        Character character = CreateCharacter(characterId, knowledgeId);
        CharacterId perceptionCharacterId = new("seeker");
        ObjectId perceptionObjectId = new("weathered-inscription");
        WorldObjectVisibility target = new(perceptionObjectId, VisibilityState.Noticed);
        CharacterPerceptionSnapshot perception = new(perceptionCharacterId, new[] { target });

        CharacterPerceptionSnapshot result =
            ObservationReinterpretationCandidateApplicationTransition.Apply(
                candidate,
                character,
                perception);

        WorldObjectVisibility replacement = Assert.Single(result.Entries);
        Assert.Same(perceptionCharacterId, result.CharacterId);
        Assert.Same(perceptionObjectId, replacement.ObjectId);
        Assert.NotSame(candidate.ObserverCharacterId, perceptionCharacterId);
        Assert.NotSame(candidate.TargetObjectId, perceptionObjectId);
        Assert.NotSame(candidate.TriggerKnowledgeId, knowledgeId);
    }

    [Fact]
    public void RejectsEveryNonChangingCombinationWithoutMutatingInputs()
    {
        ObservationReinterpretationCandidate candidate = Candidate();
        Character matchingCharacter = CreateCharacter("seeker", "ancient-script");
        WorldObjectVisibility noticedTarget =
            Visibility("weathered-inscription", VisibilityState.Noticed);
        CharacterPerceptionSnapshot matchingPerception = Snapshot("seeker", noticedTarget);

        Assert.Throws<InvalidOperationException>(() =>
            ObservationReinterpretationCandidateApplicationTransition.Apply(
                candidate,
                CreateCharacter("seeker"),
                matchingPerception));
        Assert.Throws<InvalidOperationException>(() =>
            ObservationReinterpretationCandidateApplicationTransition.Apply(
                candidate,
                matchingCharacter,
                Snapshot("travelling-healer", noticedTarget)));
        Assert.Throws<InvalidOperationException>(() =>
            ObservationReinterpretationCandidateApplicationTransition.Apply(
                candidate,
                matchingCharacter,
                Snapshot("seeker", Visibility("wagon-residue", VisibilityState.Noticed))));

        foreach (VisibilityState state in VisibilityState.All.Where(
                     state => state != VisibilityState.Noticed))
        {
            WorldObjectVisibility target = Visibility("weathered-inscription", state);
            CharacterPerceptionSnapshot perception = Snapshot("seeker", target);

            Assert.Throws<InvalidOperationException>(() =>
                ObservationReinterpretationCandidateApplicationTransition.Apply(
                    candidate,
                    matchingCharacter,
                    perception));
            Assert.Same(target, Assert.Single(perception.Entries));
            Assert.Same(state, target.VisibilityState);
        }

        Assert.Same(noticedTarget, Assert.Single(matchingPerception.Entries));
    }

    [Fact]
    public void RejectsNullInputsThroughChangePolicy()
    {
        ObservationReinterpretationCandidate candidate = Candidate();
        Character character = CreateCharacter("seeker", "ancient-script");
        CharacterPerceptionSnapshot perception = Snapshot("seeker");

        Assert.Throws<ArgumentNullException>(() =>
            ObservationReinterpretationCandidateApplicationTransition.Apply(
                null!, character, perception));
        Assert.Throws<ArgumentNullException>(() =>
            ObservationReinterpretationCandidateApplicationTransition.Apply(
                candidate, null!, perception));
        Assert.Throws<ArgumentNullException>(() =>
            ObservationReinterpretationCandidateApplicationTransition.Apply(
                candidate, character, null!));
    }

    [Fact]
    public void ExposesOnlyTheSinglePureApplicationOperation()
    {
        Type transitionType = typeof(ObservationReinterpretationCandidateApplicationTransition);
        System.Reflection.MethodInfo method = Assert.Single(
            transitionType.GetMethods().Where(candidate => candidate.DeclaringType == transitionType));

        Assert.Equal("Apply", method.Name);
        Assert.Equal(typeof(CharacterPerceptionSnapshot), method.ReturnType);
        Assert.True(method.IsStatic);
        Assert.Collection(
            method.GetParameters(),
            parameter => Assert.Equal(
                typeof(ObservationReinterpretationCandidate), parameter.ParameterType),
            parameter => Assert.Equal(typeof(Character), parameter.ParameterType),
            parameter => Assert.Equal(
                typeof(CharacterPerceptionSnapshot), parameter.ParameterType));
        Assert.Empty(transitionType.GetConstructors());
        Assert.Empty(transitionType.GetProperties());
        Assert.Empty(transitionType.GetEvents());
        Assert.True(transitionType.IsAbstract);
        Assert.True(transitionType.IsSealed);
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
