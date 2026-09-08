using Allrpcs.Domain.WorldEvents;

namespace Allrpcs.Domain.Dialogue;

/// <summary>
/// Decides whether one speaking response's exact immediate physical intention can occur.
/// </summary>
public static class CharacterTurnImmediateIntentionOccurrencePolicy
{
    /// <summary>
    /// Returns true only after the exact response speech was observed and the exact responder,
    /// proposed action, and authoritative world event are retained by the occurrence.
    /// </summary>
    public static bool CanOccur(
        CharacterTurnActivity activity,
        CharacterTurnWorldEventOccurrence occurrence,
        WorldEventSnapshot worldEvents,
        WorldEventLogSnapshot eventLog,
        WorldTimeSnapshot worldTime,
        CharacterTurnTranscriptSnapshot transcript)
    {
        ArgumentNullException.ThrowIfNull(activity);
        ArgumentNullException.ThrowIfNull(occurrence);
        ArgumentNullException.ThrowIfNull(worldEvents);
        ArgumentNullException.ThrowIfNull(eventLog);
        ArgumentNullException.ThrowIfNull(worldTime);
        ArgumentNullException.ThrowIfNull(transcript);

        CharacterTurnResponse? response = activity.Response;
        DialogueUtterance? speech = response?.Speech;
        CharacterTurnTranscriptEntry action = occurrence.TranscriptEntry;

        return activity.State == CharacterTurnActivityState.Speaking
            && response is not null
            && speech is not null
            && transcript.Entries.Count > 0
            && ReferenceEquals(transcript.Entries[^1].SpokenStatement, speech)
            && ReferenceEquals(action.CharacterId, activity.Request.RespondingCharacterId)
            && ReferenceEquals(action.PhysicalAction, response.ImmediateIntention)
            && CharacterTurnWorldEventOccurrencePolicy.CanOccur(
                occurrence,
                worldEvents,
                eventLog,
                worldTime);
    }
}
