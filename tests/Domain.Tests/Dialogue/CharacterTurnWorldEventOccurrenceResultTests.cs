using Allrpcs.Domain.Dialogue;
using Allrpcs.Domain.Identifiers;
using Allrpcs.Domain.WorldEvents;

namespace Allrpcs.Domain.Tests.Dialogue;

public sealed class CharacterTurnWorldEventOccurrenceResultTests
{
    [Fact]
    public void RetainsExactWorldEventResultAndTranscript()
    {
        WorldEvent worldEvent = new(
            new WorldEventId("healer-spoke"),
            WorldEventTimeEffect.NoAdvance);
        CharacterTurnWorldEventOccurrence occurrence = new(
            worldEvent,
            new CharacterTurnTranscriptEntry(
                new DialogueDelivery(
                    new DialogueUtterance(new CharacterId("healer"), "The flowers appear safe."),
                    DialogueDeliveryMode.OrdinarySpeech)));
        WorldEventOccurrenceResult worldEventResult = new(
            new WorldEventLogSnapshot([worldEvent.EventId]),
            new WorldTimeSnapshot(null));
        CharacterTurnTranscriptSnapshot transcript = new(
            [occurrence.TranscriptEntry]);

        CharacterTurnWorldEventOccurrenceResult result = new(
            occurrence,
            worldEventResult,
            transcript);

        Assert.Same(occurrence, result.Occurrence);
        Assert.Same(worldEventResult, result.WorldEventResult);
        Assert.Same(transcript, result.Transcript);
    }

    [Fact]
    public void RejectsNullInputs()
    {
        WorldEvent worldEvent = new(
            new WorldEventId("healer-spoke"),
            WorldEventTimeEffect.NoAdvance);
        CharacterTurnWorldEventOccurrence occurrence = new(
            worldEvent,
            new CharacterTurnTranscriptEntry(
                new DialogueDelivery(
                    new DialogueUtterance(new CharacterId("healer"), "It is safe."),
                    DialogueDeliveryMode.OrdinarySpeech)));
        WorldEventOccurrenceResult worldEventResult = new(
            new WorldEventLogSnapshot([]),
            new WorldTimeSnapshot(null));
        CharacterTurnTranscriptSnapshot transcript = new([]);

        Assert.Throws<ArgumentNullException>(() =>
            new CharacterTurnWorldEventOccurrenceResult(null!, worldEventResult, transcript));
        Assert.Throws<ArgumentNullException>(() =>
            new CharacterTurnWorldEventOccurrenceResult(occurrence, null!, transcript));
        Assert.Throws<ArgumentNullException>(() =>
            new CharacterTurnWorldEventOccurrenceResult(occurrence, worldEventResult, null!));
    }

    [Fact]
    public void ExposesOnlyTheImmutableResultSurface()
    {
        Type resultType = typeof(CharacterTurnWorldEventOccurrenceResult);
        (string Name, Type Type)[] expectedProperties =
        [
            ("Occurrence", typeof(CharacterTurnWorldEventOccurrence)),
            ("Transcript", typeof(CharacterTurnTranscriptSnapshot)),
            ("WorldEventResult", typeof(WorldEventOccurrenceResult)),
        ];
        (string Name, Type Type, bool CanWrite)[] actualProperties = resultType
            .GetProperties()
            .OrderBy(property => property.Name, StringComparer.Ordinal)
            .Select(property => (property.Name, property.PropertyType, property.CanWrite))
            .ToArray();

        Assert.Equal(
            expectedProperties,
            actualProperties.Select(property => (property.Name, property.Type)).ToArray());
        Assert.All(actualProperties, property => Assert.False(property.CanWrite));
        Assert.Single(resultType.GetConstructors());
        Assert.Empty(resultType.GetEvents());
        Assert.True(resultType.IsSealed);
    }
}
