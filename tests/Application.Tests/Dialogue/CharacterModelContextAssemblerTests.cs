using Allrpcs.Application.Dialogue;
using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Dialogue;

namespace Allrpcs.Application.Tests.Dialogue;

public sealed class CharacterModelContextAssemblerTests
{
    [Fact]
    public void RetainsFilteredEntriesThenAppendsOwnedContextInStableOrder()
    {
        CharacterTurnRequest request = CharacterModelPlayerDialogueContextProjectionTests.CreateRequest();
        CharacterModelContextEntry perception = new(
            CharacterModelContextEntryKind.FilteredPerception,
            "Visible herb patch.");
        CharacterModelContextEntry knowledge = new(
            CharacterModelContextEntryKind.RelevantKnowledge,
            "The herb is safe when prepared correctly.");
        CharacterGoal goal = new("help-safely", "Help when it is safe.");
        Character character = CharacterModelGoalContextProjectionTests.CreateCharacter(
            new CharacterGoalSet([goal]));
        DialogueEvent recentSpeech = CharacterModelRecentSpeechContextProjectionTests.CreateEvent(
            "healer",
            "I will help safely.",
            "seeker");
        CharacterModelContextEntry[] source = [perception, knowledge];

        CharacterModelGenerationRequest result = CharacterModelContextAssembler.Assemble(
            request,
            character,
            [recentSpeech],
            source);
        source[0] = knowledge;

        Assert.Same(request, result.TurnRequest);
        Assert.Collection(
            result.Context.Entries,
            entry => Assert.Same(perception, entry),
            entry => Assert.Same(knowledge, entry),
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

        CharacterModelGenerationRequest result = CharacterModelContextAssembler.Assemble(
            request,
            character,
            [],
            []);

        Assert.Single(result.Context.Entries);
    }

    [Fact]
    public void RejectsNullMismatchedAndCallerSuppliedOwnedContext()
    {
        CharacterTurnRequest request = CharacterModelPlayerDialogueContextProjectionTests.CreateRequest();
        Character character = CharacterModelGoalContextProjectionTests.CreateCharacter(
            new CharacterGoalSet([]));
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

        Assert.Throws<ArgumentNullException>(() => CharacterModelContextAssembler.Assemble(
            null!,
            character,
            [],
            []));
        Assert.Throws<ArgumentNullException>(() => CharacterModelContextAssembler.Assemble(
            request,
            null!,
            [],
            []));
        Assert.Throws<ArgumentNullException>(() => CharacterModelContextAssembler.Assemble(
            request,
            character,
            null!,
            []));
        Assert.Throws<ArgumentNullException>(() => CharacterModelContextAssembler.Assemble(
            request,
            character,
            [],
            null!));
        Assert.Throws<ArgumentException>(() => CharacterModelContextAssembler.Assemble(
            request,
            wrongCharacter,
            [],
            []));
        Assert.Throws<ArgumentException>(() => CharacterModelContextAssembler.Assemble(
            request,
            character,
            [],
            [untrusted]));
        Assert.Throws<ArgumentException>(() => CharacterModelContextAssembler.Assemble(
            request,
            character,
            [],
            [goal]));
        Assert.Throws<ArgumentException>(() => CharacterModelContextAssembler.Assemble(
            request,
            character,
            [],
            [recentSpeech]));
    }

    [Fact]
    public void RejectsExcessiveContextWithoutChangingInputs()
    {
        CharacterTurnRequest request = CharacterModelPlayerDialogueContextProjectionTests.CreateRequest();
        Character character = CharacterModelGoalContextProjectionTests.CreateCharacter(
            new CharacterGoalSet([]));
        CharacterModelContextEntry entry = new(CharacterModelContextEntryKind.Memory, "memory");
        CharacterModelContextEntry[] entries = Enumerable
            .Repeat(entry, CharacterModelContextSnapshot.MaximumEntryCount)
            .ToArray();

        Assert.Throws<ArgumentException>(() => CharacterModelContextAssembler.Assemble(
            request,
            character,
            [],
            entries));
        Assert.All(entries, retained => Assert.Same(entry, retained));
    }
}
