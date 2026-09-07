using Allrpcs.Domain.Identifiers;

namespace Allrpcs.Domain.WorldEvents;

/// <summary>
/// Appends one eligible world-event identifier to an event-log snapshot.
/// </summary>
public static class WorldEventLogAppendTransition
{
    /// <summary>
    /// Returns a new log with the supplied identifier appended after all retained identifiers.
    /// </summary>
    public static WorldEventLogSnapshot Append(
        WorldEventId eventId,
        WorldEventLogSnapshot eventLog)
    {
        if (!WorldEventLogAppendPolicy.CanAppend(eventId, eventLog))
        {
            throw new InvalidOperationException(
                "The supplied world-event identifier is already recorded.");
        }

        return new WorldEventLogSnapshot(eventLog.Entries.Append(eventId));
    }
}
