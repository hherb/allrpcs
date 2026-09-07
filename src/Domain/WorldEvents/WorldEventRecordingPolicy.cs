using Allrpcs.Domain.Identifiers;

namespace Allrpcs.Domain.WorldEvents;

/// <summary>Decides whether one defined world event can be recorded in an identifier log.</summary>
public static class WorldEventRecordingPolicy
{
    /// <summary>Returns true only when the event is authoritative and not already recorded.</summary>
    public static bool CanRecord(
        WorldEventId eventId,
        WorldEventSnapshot worldEvents,
        WorldEventLogSnapshot eventLog)
    {
        ArgumentNullException.ThrowIfNull(eventId);
        ArgumentNullException.ThrowIfNull(worldEvents);
        ArgumentNullException.ThrowIfNull(eventLog);

        WorldEvent? worldEvent = WorldEventLookup.FindById(eventId, worldEvents);
        return worldEvent is not null
            && WorldEventLogAppendPolicy.CanAppend(worldEvent.EventId, eventLog);
    }
}
