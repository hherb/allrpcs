using Allrpcs.Application.Dialogue;
using Allrpcs.Domain.Dialogue;

namespace Allrpcs.Application.Tests.Dialogue;

public sealed class CharacterModelContextAssemblerTests
{
    [Fact]
    public void RetainsSuppliedFilteredEntriesThenAppendsExactUntrustedDialogue()
    {
        CharacterTurnRequest request = CharacterModelPlayerDialogueContextProjectionTests.CreateRequest();
        CharacterModelContextEntry perception = new(
            CharacterModelContextEntryKind.FilteredPerception,
            "Visible herb patch.");
        CharacterModelContextEntry goal = new(
            CharacterModelContextEntryKind.Goal,
            "Help when it is safe.");
        CharacterModelContextEntry[] source = [perception, goal];

        CharacterModelGenerationRequest result = CharacterModelContextAssembler.Assemble(
            request,
            source);
        source[0] = goal;

        Assert.Same(request, result.TurnRequest);
        Assert.Collection(
            result.Context.Entries,
            entry => Assert.Same(perception, entry),
            entry => Assert.Same(goal, entry),
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

        CharacterModelGenerationRequest result = CharacterModelContextAssembler.Assemble(request, []);

        Assert.Single(result.Context.Entries);
    }

    [Fact]
    public void RejectsNullInputsAndCallerSuppliedPlayerDialogue()
    {
        CharacterTurnRequest request = CharacterModelPlayerDialogueContextProjectionTests.CreateRequest();
        CharacterModelContextEntry untrusted = new(
            CharacterModelContextEntryKind.UntrustedPlayerDialogue,
            request.DialogueEvent.Utterance.Text);

        Assert.Throws<ArgumentNullException>(() => CharacterModelContextAssembler.Assemble(
            null!,
            []));
        Assert.Throws<ArgumentNullException>(() => CharacterModelContextAssembler.Assemble(
            request,
            null!));
        Assert.Throws<ArgumentException>(() => CharacterModelContextAssembler.Assemble(
            request,
            [untrusted]));
    }

    [Fact]
    public void RejectsExcessiveContextWithoutChangingInputs()
    {
        CharacterTurnRequest request = CharacterModelPlayerDialogueContextProjectionTests.CreateRequest();
        CharacterModelContextEntry entry = new(CharacterModelContextEntryKind.Goal, "goal");
        CharacterModelContextEntry[] entries = Enumerable
            .Repeat(entry, CharacterModelContextSnapshot.MaximumEntryCount)
            .ToArray();

        Assert.Throws<ArgumentException>(() => CharacterModelContextAssembler.Assemble(
            request,
            entries));
        Assert.All(entries, retained => Assert.Same(entry, retained));
    }
}
