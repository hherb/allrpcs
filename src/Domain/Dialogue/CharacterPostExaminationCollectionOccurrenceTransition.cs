using Allrpcs.Domain.WorldEvents;

namespace Allrpcs.Domain.Dialogue;

/// <summary>
/// Applies one eligible exact collection occurrence after an authoritative safe examination.
/// </summary>
public static class CharacterPostExaminationCollectionOccurrenceTransition
{
    /// <summary>
    /// Records the exact authoritative event and collection after validating result and ordering.
    /// </summary>
    public static CharacterTurnWorldEventOccurrenceResult Occur(
        CharacterExaminationResult result,
        CharacterTurnWorldEventOccurrence collection,
        WorldEventSnapshot worldEvents,
        WorldEventLogSnapshot eventLog,
        WorldTimeSnapshot worldTime,
        CharacterTurnTranscriptSnapshot transcript)
    {
        if (!CharacterPostExaminationCollectionOccurrencePolicy.CanOccur(
                result,
                collection,
                worldEvents,
                eventLog,
                worldTime,
                transcript))
        {
            throw new InvalidOperationException(
                "Only exact-target collection after an authoritative safe examination can occur.");
        }

        return CharacterTurnWorldEventOccurrenceTransition.Occur(
            collection,
            worldEvents,
            eventLog,
            worldTime,
            transcript);
    }
}
