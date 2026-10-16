using Allrpcs.Application.Dialogue;
using Allrpcs.Domain.Dialogue;
using Allrpcs.Domain.Identifiers;

namespace Allrpcs.Application.Tests.Dialogue;

public sealed class CharacterModelContextEntryTests
{
    public static TheoryData<CharacterModelContextEntryKind> NonDialogueKinds => new()
    {
        CharacterModelContextEntryKind.FilteredPerception,
        CharacterModelContextEntryKind.RelevantKnowledge,
        CharacterModelContextEntryKind.Goal,
        CharacterModelContextEntryKind.Memory,
        CharacterModelContextEntryKind.ValidAffordance,
        CharacterModelContextEntryKind.ActionOutcome,
    };

    public static TheoryData<CharacterModelContextEntryKind> DialogueKinds => new()
    {
        CharacterModelContextEntryKind.RecentSpeech,
        CharacterModelContextEntryKind.UntrustedPlayerDialogue,
    };

    [Theory]
    [MemberData(nameof(NonDialogueKinds))]
    public void RetainsDefinedNonDialogueKindAndExactText(CharacterModelContextEntryKind kind)
    {
        string text = "  exact context  ";

        CharacterModelContextEntry entry = new(kind, text);

        Assert.Equal(kind, entry.Kind);
        Assert.Same(text, entry.Text);
        Assert.Null(entry.DeliveryMode);
    }

    [Theory]
    [MemberData(nameof(DialogueKinds))]
    public void RetainsDefinedDialogueKindTextAndExactDeliveryMode(
        CharacterModelContextEntryKind kind)
    {
        string text = "  exact dialogue  ";

        CharacterModelContextEntry entry = new(
            kind,
            text,
            DialogueDeliveryMode.Whispering);

        Assert.Equal(kind, entry.Kind);
        Assert.Same(text, entry.Text);
        Assert.Same(DialogueDeliveryMode.Whispering, entry.DeliveryMode);
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

    [Theory]
    [MemberData(nameof(DialogueKinds))]
    public void RejectsDialogueWithoutADeliveryMode(CharacterModelContextEntryKind kind)
    {
        Assert.Throws<ArgumentException>(() => new CharacterModelContextEntry(kind, "dialogue"));
        Assert.Throws<ArgumentNullException>(() => new CharacterModelContextEntry(
            kind,
            "dialogue",
            null!));
    }

    [Theory]
    [MemberData(nameof(NonDialogueKinds))]
    public void RejectsDeliveryModeForNonDialogueContext(CharacterModelContextEntryKind kind)
    {
        Assert.Throws<ArgumentException>(() => new CharacterModelContextEntry(
            kind,
            "context",
            DialogueDeliveryMode.OrdinarySpeech));
    }

    [Fact]
    public void ExposesOnlyTheImmutableContextSurface()
    {
        Type entryType = typeof(CharacterModelContextEntry);

        Assert.True(entryType.IsSealed);
        Assert.Collection(
            entryType.GetProperties().OrderBy(property => property.Name, StringComparer.Ordinal),
            property => AssertProperty(property, "DeliveryMode", typeof(DialogueDeliveryMode)),
            property => AssertProperty(property, "Kind", typeof(CharacterModelContextEntryKind)),
            property => AssertProperty(property, "SpeakerCharacterId", typeof(CharacterId)),
            property => AssertProperty(property, "Text", typeof(string)));
        Assert.Collection(
            entryType.GetConstructors().OrderBy(constructor => constructor.GetParameters().Length),
            constructor => Assert.Equal(
                [typeof(CharacterModelContextEntryKind), typeof(string)],
                constructor.GetParameters().Select(parameter => parameter.ParameterType)),
            constructor => Assert.Equal(
                [
                    typeof(CharacterModelContextEntryKind),
                    typeof(string),
                    typeof(DialogueDeliveryMode),
                    typeof(CharacterId),
                ],
                constructor.GetParameters().Select(parameter => parameter.ParameterType)));
    }

    private static void AssertProperty(
        System.Reflection.PropertyInfo property,
        string name,
        Type propertyType)
    {
        Assert.Equal(name, property.Name);
        Assert.Equal(propertyType, property.PropertyType);
        Assert.NotNull(property.GetMethod);
        Assert.Null(property.SetMethod);
    }
}
