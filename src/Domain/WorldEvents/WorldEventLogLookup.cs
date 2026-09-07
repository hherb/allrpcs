using Allrpcs.Domain.Identifiers;

namespace Allrpcs.Domain.WorldEvents;

/// <summary>
/// Selects a retained world-event identifier without changing the event log.
/// </summary>
public static class WorldEventLogLookup
{
    /// <summary>
    /// Returns the exact retained identifier for a value-equal event identifier, or null when no
    /// event matches.
    /// </summary>
    public static WorldEventId? Find(
        WorldEventId eventId,
        WorldEventLogSnapshot eventLog)
    {
        ArgumentNullException.ThrowIfNull(eventId);
        ArgumentNullException.ThrowIfNull(eventLog);

        return eventLog.Entries.FirstOrDefault(entry => entry == eventId);
    }
}
