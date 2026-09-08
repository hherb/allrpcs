using Allrpcs.Domain.WorldEvents;

namespace Allrpcs.Domain.Dialogue;

/// <summary>
/// Decides whether collection can occur after one exact authoritative examination result.
/// </summary>
public static class CharacterPostExaminationCollectionOccurrencePolicy
{
    /// <summary>
    /// Returns true only for exact-target collection after the latest exact safe examination.
    /// </summary>
    public static bool CanOccur(
        CharacterExaminationResult result,
        CharacterTurnWorldEventOccurrence collection,
        WorldEventSnapshot worldEvents,
        WorldEventLogSnapshot eventLog,
        WorldTimeSnapshot worldTime,
        CharacterTurnTranscriptSnapshot transcript)
    {
        ArgumentNullException.ThrowIfNull(result);
        ArgumentNullException.ThrowIfNull(collection);
        ArgumentNullException.ThrowIfNull(worldEvents);
        ArgumentNullException.ThrowIfNull(eventLog);
        ArgumentNullException.ThrowIfNull(worldTime);
        ArgumentNullException.ThrowIfNull(transcript);

        CharacterTurnTranscriptEntry collectionEntry = collection.TranscriptEntry;
        CharacterIntention? collectionAction = collectionEntry.PhysicalAction;
        WorldEvent? examinationEvent = WorldEventLookup.FindById(
            result.Occurrence.WorldEvent.EventId,
            worldEvents);

        return result.Safety == CharacterExaminationSafety.SafeToCollect
            && ReferenceEquals(examinationEvent, result.Occurrence.WorldEvent)
            && eventLog.Entries.Any(
                eventId => ReferenceEquals(
                    eventId,
                    result.Occurrence.WorldEvent.EventId))
            && transcript.Entries.Count > 0
            && ReferenceEquals(transcript.Entries[^1], result.Occurrence.TranscriptEntry)
            && ReferenceEquals(
                collectionEntry.CharacterId,
                result.Occurrence.TranscriptEntry.CharacterId)
            && collectionAction?.Type == CharacterIntentionType.CollectObject
            && ReferenceEquals(collectionAction.TargetObjectId, result.TargetObjectId)
            && CharacterTurnWorldEventOccurrencePolicy.CanOccur(
                collection,
                worldEvents,
                eventLog,
                worldTime);
    }
}
