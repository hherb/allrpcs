using Allrpcs.Domain.Dialogue;
using Allrpcs.Domain.Identifiers;

namespace Allrpcs.Domain.Tests.Dialogue;

public sealed class CharacterTurnTranscriptSnapshotTests
{
    [Fact]
    public void EmptySnapshotIsExplicitlyValid()
    {
        CharacterTurnTranscriptSnapshot snapshot = new([]);

        Assert.Empty(snapshot.Entries);
    }

    [Fact]
    public void PreservesOrderExactInstancesAndRepeatedEvents()
    {
        DialogueUtterance repeatedSpeech = new(new CharacterId("healer"), "Careful.");
        CharacterTurnTranscriptEntry first = new(
            new DialogueDelivery(repeatedSpeech, DialogueDeliveryMode.OrdinarySpeech));
        CharacterTurnTranscriptEntry action = new(
            repeatedSpeech.SpeakerCharacterId,
            new CharacterIntention(
                CharacterIntentionType.ExamineObject,
                new ObjectId("moonwort")));

        CharacterTurnTranscriptSnapshot snapshot = new([first, action, first]);

        Assert.Collection(
            snapshot.Entries,
            entry => Assert.Same(first, entry),
            entry => Assert.Same(action, entry),
            entry => Assert.Same(first, entry));
    }

    [Fact]
    public void CopiesTheCallerCollection()
    {
        CharacterTurnTranscriptEntry original = Speech("First.");
        CharacterTurnTranscriptEntry replacement = Speech("Replacement.");
        CharacterTurnTranscriptEntry[] source = [original];

        CharacterTurnTranscriptSnapshot snapshot = new(source);
        source[0] = replacement;

        Assert.Same(original, Assert.Single(snapshot.Entries));
        Assert.Throws<NotSupportedException>(() =>
            ((IList<CharacterTurnTranscriptEntry>)snapshot.Entries).Add(replacement));
    }

    [Fact]
    public void RejectsNullCollectionAndEntries()
    {
        Assert.Throws<ArgumentNullException>(() => new CharacterTurnTranscriptSnapshot(null!));
        Assert.Throws<ArgumentException>(() =>
            new CharacterTurnTranscriptSnapshot([null!]));
    }

    [Fact]
    public void ExposesOnlyTheReadOnlySnapshotSurface()
    {
        Type snapshotType = typeof(CharacterTurnTranscriptSnapshot);
        System.Reflection.PropertyInfo property = Assert.Single(snapshotType.GetProperties());
        System.Reflection.ConstructorInfo constructor = Assert.Single(
            snapshotType.GetConstructors());
        string[] publicMethods = snapshotType
            .GetMethods()
            .Where(method => method.DeclaringType == snapshotType && !method.IsSpecialName)
            .Select(method => method.Name)
            .ToArray();

        Assert.Equal("Entries", property.Name);
        Assert.Equal(typeof(IReadOnlyList<CharacterTurnTranscriptEntry>), property.PropertyType);
        Assert.False(property.CanWrite);
        Assert.Collection(
            constructor.GetParameters(),
            parameter => Assert.Equal(
                typeof(IEnumerable<CharacterTurnTranscriptEntry>),
                parameter.ParameterType));
        Assert.Empty(snapshotType.GetEvents());
        Assert.Empty(publicMethods);
        Assert.True(snapshotType.IsSealed);
    }

    private static CharacterTurnTranscriptEntry Speech(string text) =>
        new(new DialogueDelivery(
            new DialogueUtterance(new CharacterId("healer"), text),
            DialogueDeliveryMode.OrdinarySpeech));
}
