using Allrpcs.Domain.WorldEvents;

namespace Allrpcs.Domain.Dialogue;

/// <summary>
/// Decides whether one exact authoritative character-turn occurrence can affect world state.
/// </summary>
public static class CharacterTurnWorldEventOccurrencePolicy
{
    /// <summary>
    /// Returns true when the retained event is the authoritative definition and can occur.
    /// </summary>
    public static bool CanOccur(
        CharacterTurnWorldEventOccurrence occurrence,
        WorldEventSnapshot worldEvents,
        WorldEventLogSnapshot eventLog,
        WorldTimeSnapshot worldTime)
    {
        ArgumentNullException.ThrowIfNull(occurrence);
        ArgumentNullException.ThrowIfNull(worldEvents);
        ArgumentNullException.ThrowIfNull(eventLog);
        ArgumentNullException.ThrowIfNull(worldTime);

        WorldEvent? authoritativeEvent = WorldEventLookup.FindById(
            occurrence.WorldEvent.EventId,
            worldEvents);
        return ReferenceEquals(authoritativeEvent, occurrence.WorldEvent)
            && WorldEventOccurrencePolicy.CanOccur(
                occurrence.WorldEvent.EventId,
                worldEvents,
                eventLog,
                worldTime);
    }
}
