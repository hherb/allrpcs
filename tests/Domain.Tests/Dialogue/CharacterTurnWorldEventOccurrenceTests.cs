using Allrpcs.Domain.Dialogue;
using Allrpcs.Domain.Identifiers;
using Allrpcs.Domain.WorldEvents;

namespace Allrpcs.Domain.Tests.Dialogue;

public sealed class CharacterTurnWorldEventOccurrenceTests
{
    [Fact]
    public void RetainsExactWorldEventAndTranscriptEntry()
    {
        WorldEvent worldEvent = new(
            new WorldEventId("healer-examined-moonwort"),
            WorldEventTimeEffect.NoAdvance);
        CharacterTurnTranscriptEntry transcriptEntry = new(
            new CharacterId("healer"),
            new CharacterIntention(
                CharacterIntentionType.ExamineObject,
                new ObjectId("moonwort")));

        CharacterTurnWorldEventOccurrence occurrence = new(worldEvent, transcriptEntry);

        Assert.Same(worldEvent, occurrence.WorldEvent);
        Assert.Same(transcriptEntry, occurrence.TranscriptEntry);
        Assert.Same(CharacterTurnTranscriptEntryKind.PhysicalAction, occurrence.TranscriptEntry.Kind);
    }

    [Fact]
    public void PreservesAnExactSpokenTranscriptEntry()
    {
        WorldEvent worldEvent = new(
            new WorldEventId("healer-spoke"),
            WorldEventTimeEffect.NoAdvance);
        DialogueUtterance speech = new(new CharacterId("healer"), "The flowers appear safe.");
        CharacterTurnTranscriptEntry transcriptEntry = new(speech);

        CharacterTurnWorldEventOccurrence occurrence = new(worldEvent, transcriptEntry);

        Assert.Same(transcriptEntry, occurrence.TranscriptEntry);
        Assert.Same(CharacterTurnTranscriptEntryKind.SpokenStatement, occurrence.TranscriptEntry.Kind);
        Assert.Same(speech, occurrence.TranscriptEntry.SpokenStatement);
    }

    [Fact]
    public void RejectsNullInputs()
    {
        WorldEvent worldEvent = new(
            new WorldEventId("healer-spoke"),
            WorldEventTimeEffect.NoAdvance);
        CharacterTurnTranscriptEntry transcriptEntry = new(
            new DialogueUtterance(new CharacterId("healer"), "I will examine it."));

        Assert.Throws<ArgumentNullException>(() =>
            new CharacterTurnWorldEventOccurrence(null!, transcriptEntry));
        Assert.Throws<ArgumentNullException>(() =>
            new CharacterTurnWorldEventOccurrence(worldEvent, null!));
    }

    [Fact]
    public void ExposesOnlyTheImmutableOccurrenceSurface()
    {
        Type occurrenceType = typeof(CharacterTurnWorldEventOccurrence);
        (string Name, Type Type)[] expectedProperties =
        [
            ("TranscriptEntry", typeof(CharacterTurnTranscriptEntry)),
            ("WorldEvent", typeof(WorldEvent)),
        ];
        (string Name, Type Type, bool CanWrite)[] actualProperties = occurrenceType
            .GetProperties()
            .OrderBy(property => property.Name, StringComparer.Ordinal)
            .Select(property => (property.Name, property.PropertyType, property.CanWrite))
            .ToArray();

        Assert.Equal(
            expectedProperties,
            actualProperties.Select(property => (property.Name, property.Type)).ToArray());
        Assert.All(actualProperties, property => Assert.False(property.CanWrite));
        Assert.Single(occurrenceType.GetConstructors());
        Assert.Empty(occurrenceType.GetEvents());
        Assert.True(occurrenceType.IsSealed);
    }
}
