using Allrpcs.Domain.Identifiers;

namespace Allrpcs.Domain.WorldEvents;

/// <summary>
/// Decides whether one world-event identifier can be appended without changing the event log.
/// </summary>
public static class WorldEventLogAppendPolicy
{
    /// <summary>
    /// Returns true when no value-equal event identifier is already retained.
    /// </summary>
    public static bool CanAppend(
        WorldEventId eventId,
        WorldEventLogSnapshot eventLog) =>
        WorldEventLogLookup.Find(eventId, eventLog) is null;
}
