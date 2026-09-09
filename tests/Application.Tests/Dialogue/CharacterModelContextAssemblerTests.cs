using Allrpcs.Application.Dialogue;
using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Dialogue;
using Allrpcs.Domain.Identifiers;
using Allrpcs.Domain.Knowledge;
using Allrpcs.Domain.Perception;
using Allrpcs.Domain.WorldObjects;

namespace Allrpcs.Application.Tests.Dialogue;

public sealed class CharacterModelContextAssemblerTests
{
    [Fact]
    public void RetainsFilteredEntriesThenAppendsOwnedContextInStableOrder()
    {
        CharacterTurnRequest request = CharacterModelPlayerDialogueContextProjectionTests.CreateRequest();
        string perceptionText = "Visible herb patch.";
        WorldObject perceivedObject = CharacterModelPerceptionContextProjectionTests.CreateWorldObject(
            "herb-patch",
            "Herb patch",
            perceptionText);
        string knowledgeText = "The herb is safe when prepared correctly.";
        CharacterGoal goal = new("help-safely", "Help when it is safe.");
        Character character = CharacterModelGoalContextProjectionTests.CreateCharacter(
            new CharacterGoalSet([goal]));
        DialogueEvent recentSpeech = CharacterModelRecentSpeechContextProjectionTests.CreateEvent(
            "healer",
            "I will help safely.",
            "seeker");
        CharacterPerceptionSnapshot perception = new(
            character.Identity.Id,
            [new WorldObjectVisibility(new ObjectId("herb-patch"), VisibilityState.Identified)]);
        WorldObjectSnapshot worldObjects = new([perceivedObject]);
        KnowledgeSnapshot knowledge = new(
            [new KnowledgeEntry(new KnowledgeId("herbal-remedies"), knowledgeText)]);
        CharacterModelContextEntry memory = new(CharacterModelContextEntryKind.Memory, "A memory.");
        CharacterModelContextEntry[] source = [memory];

        CharacterModelGenerationRequest result = CharacterModelContextAssembler.Assemble(
            request,
            character,
            perception,
            worldObjects,
            knowledge,
            [recentSpeech],
            source);
        source[0] = memory;

        Assert.Same(request, result.TurnRequest);
        Assert.Collection(
            result.Context.Entries,
            entry =>
            {
                Assert.Equal(CharacterModelContextEntryKind.FilteredPerception, entry.Kind);
                Assert.Same(perceptionText, entry.Text);
            },
            entry =>
            {
                Assert.Equal(CharacterModelContextEntryKind.RelevantKnowledge, entry.Kind);
                Assert.Same(knowledgeText, entry.Text);
            },
            entry => Assert.Same(memory, entry),
            entry =>
            {
                Assert.Equal(CharacterModelContextEntryKind.Goal, entry.Kind);
                Assert.Same(goal.Description, entry.Text);
            },
            entry =>
            {
                Assert.Equal(CharacterModelContextEntryKind.RecentSpeech, entry.Kind);
                Assert.Same(recentSpeech.Utterance.Text, entry.Text);
            },
            entry =>
            {
                Assert.Equal(CharacterModelContextEntryKind.UntrustedPlayerDialogue, entry.Kind);
                Assert.Same(request.DialogueEvent.Utterance.Text, entry.Text);
            });
    }

    [Fact]
    public void AcceptsNoSuppliedFilteredEntries()
    {
        CharacterTurnRequest request = CharacterModelPlayerDialogueContextProjectionTests.CreateRequest();
        Character character = CharacterModelGoalContextProjectionTests.CreateCharacter(
            new CharacterGoalSet([]));
        CharacterPerceptionSnapshot perception = CreateEmptyPerception(character);
        KnowledgeSnapshot knowledge = CreateKnowledge(character);

        CharacterModelGenerationRequest result = CharacterModelContextAssembler.Assemble(
            request,
            character,
            perception,
            new WorldObjectSnapshot([]),
            knowledge,
            [],
            []);

        Assert.Equal(2, result.Context.Entries.Count);
    }

    [Fact]
    public void RejectsNullMismatchedAndCallerSuppliedOwnedContext()
    {
        CharacterTurnRequest request = CharacterModelPlayerDialogueContextProjectionTests.CreateRequest();
        Character character = CharacterModelGoalContextProjectionTests.CreateCharacter(
            new CharacterGoalSet([]));
        CharacterPerceptionSnapshot perception = CreateEmptyPerception(character);
        WorldObjectSnapshot worldObjects = new([]);
        KnowledgeSnapshot knowledge = CreateKnowledge(character);
        CharacterModelContextEntry filteredPerception = new(
            CharacterModelContextEntryKind.FilteredPerception,
            "Caller-supplied perception.");
        CharacterModelContextEntry relevantKnowledge = new(
            CharacterModelContextEntryKind.RelevantKnowledge,
            "Caller-supplied knowledge.");
        CharacterModelContextEntry untrusted = new(
            CharacterModelContextEntryKind.UntrustedPlayerDialogue,
            request.DialogueEvent.Utterance.Text);
        CharacterModelContextEntry goal = new(
            CharacterModelContextEntryKind.Goal,
            "Caller-supplied goal.");
        CharacterModelContextEntry recentSpeech = new(
            CharacterModelContextEntryKind.RecentSpeech,
            "Caller-supplied speech.");
        Character wrongCharacter = CharacterModelGoalContextProjectionTests.CreateCharacter(
            new CharacterGoalSet([]),
            "someone-else");
        CharacterPerceptionSnapshot wrongPerception = new(new CharacterId("someone-else"), []);

        Assert.Throws<ArgumentNullException>(() => CharacterModelContextAssembler.Assemble(
            null!,
            character,
            perception,
            worldObjects,
            knowledge,
            [],
            []));
        Assert.Throws<ArgumentNullException>(() => CharacterModelContextAssembler.Assemble(
            request,
            null!,
            perception,
            worldObjects,
            knowledge,
            [],
            []));
        Assert.Throws<ArgumentNullException>(() => CharacterModelContextAssembler.Assemble(
            request,
            character,
            null!,
            worldObjects,
            knowledge,
            [],
            []));
        Assert.Throws<ArgumentNullException>(() => CharacterModelContextAssembler.Assemble(
            request,
            character,
            perception,
            null!,
            knowledge,
            [],
            []));
        Assert.Throws<ArgumentNullException>(() => CharacterModelContextAssembler.Assemble(
            request,
            character,
            perception,
            worldObjects,
            null!,
            [],
            []));
        Assert.Throws<ArgumentNullException>(() => CharacterModelContextAssembler.Assemble(
            request,
            character,
            perception,
            worldObjects,
            knowledge,
            [],
            null!));
        Assert.Throws<ArgumentException>(() => CharacterModelContextAssembler.Assemble(
            request,
            wrongCharacter,
            perception,
            worldObjects,
            knowledge,
            [],
            []));
        Assert.Throws<ArgumentException>(() => CharacterModelContextAssembler.Assemble(
            request,
            character,
            wrongPerception,
            worldObjects,
            knowledge,
            [],
            []));
        Assert.Throws<ArgumentException>(() => CharacterModelContextAssembler.Assemble(
            request,
            character,
            perception,
            worldObjects,
            knowledge,
            [],
            [filteredPerception]));
        Assert.Throws<ArgumentException>(() => CharacterModelContextAssembler.Assemble(
            request,
            character,
            perception,
            worldObjects,
            knowledge,
            [],
            [relevantKnowledge]));
        Assert.Throws<ArgumentException>(() => CharacterModelContextAssembler.Assemble(
            request,
            character,
            perception,
            worldObjects,
            knowledge,
            [],
            [untrusted]));
        Assert.Throws<ArgumentException>(() => CharacterModelContextAssembler.Assemble(
            request,
            character,
            perception,
            worldObjects,
            knowledge,
            [],
            [goal]));
        Assert.Throws<ArgumentException>(() => CharacterModelContextAssembler.Assemble(
            request,
            character,
            perception,
            worldObjects,
            knowledge,
            [],
            [recentSpeech]));
    }

    [Fact]
    public void RejectsExcessiveContextWithoutChangingInputs()
    {
        CharacterTurnRequest request = CharacterModelPlayerDialogueContextProjectionTests.CreateRequest();
        Character character = CharacterModelGoalContextProjectionTests.CreateCharacter(
            new CharacterGoalSet([]));
        CharacterPerceptionSnapshot perception = CreateEmptyPerception(character);
        CharacterModelContextEntry entry = new(CharacterModelContextEntryKind.Memory, "memory");
        CharacterModelContextEntry[] entries = Enumerable
            .Repeat(entry, CharacterModelContextSnapshot.MaximumEntryCount)
            .ToArray();

        Assert.Throws<ArgumentException>(() => CharacterModelContextAssembler.Assemble(
            request,
            character,
            perception,
            new WorldObjectSnapshot([]),
            CreateKnowledge(character),
            [],
            entries));
        Assert.All(entries, retained => Assert.Same(entry, retained));
    }

    private static CharacterPerceptionSnapshot CreateEmptyPerception(Character character) =>
        new(character.Identity.Id, []);

    private static KnowledgeSnapshot CreateKnowledge(Character character) =>
        new(character.Knowledge.KnowledgeReferences.Select(
            reference => new KnowledgeEntry(reference, $"Knowledge for {reference.Value}.")));
}
