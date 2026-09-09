using Allrpcs.Application.Dialogue;

namespace Allrpcs.Application.Tests.Dialogue;

public sealed class CharacterModelContextEntryTests
{
    public static TheoryData<CharacterModelContextEntryKind> DefinedKinds => new()
    {
        CharacterModelContextEntryKind.FilteredPerception,
        CharacterModelContextEntryKind.RelevantKnowledge,
        CharacterModelContextEntryKind.Goal,
        CharacterModelContextEntryKind.Memory,
        CharacterModelContextEntryKind.RecentSpeech,
        CharacterModelContextEntryKind.ValidAffordance,
        CharacterModelContextEntryKind.UntrustedPlayerDialogue,
    };

    [Theory]
    [MemberData(nameof(DefinedKinds))]
    public void RetainsDefinedKindAndExactText(CharacterModelContextEntryKind kind)
    {
        string text = "  exact context  ";

        CharacterModelContextEntry entry = new(kind, text);

        Assert.Equal(kind, entry.Kind);
        Assert.Same(text, entry.Text);
    }

    [Fact]
    public void AcceptsTheMaximumTextLength()
    {
        string text = new('x', CharacterModelContextEntry.MaximumTextLength);

        CharacterModelContextEntry entry = new(CharacterModelContextEntryKind.Goal, text);

        Assert.Same(text, entry.Text);
    }

    [Fact]
    public void RejectsUndefinedKindAndInvalidText()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new CharacterModelContextEntry(
            (CharacterModelContextEntryKind)int.MaxValue,
            "context"));
        Assert.Throws<ArgumentNullException>(() => new CharacterModelContextEntry(
            CharacterModelContextEntryKind.Goal,
            null!));
        Assert.Throws<ArgumentException>(() => new CharacterModelContextEntry(
            CharacterModelContextEntryKind.Goal,
            string.Empty));
        Assert.Throws<ArgumentException>(() => new CharacterModelContextEntry(
            CharacterModelContextEntryKind.Goal,
            " \t\r\n"));
        Assert.Throws<ArgumentException>(() => new CharacterModelContextEntry(
            CharacterModelContextEntryKind.Goal,
            new string('x', CharacterModelContextEntry.MaximumTextLength + 1)));
    }
}
