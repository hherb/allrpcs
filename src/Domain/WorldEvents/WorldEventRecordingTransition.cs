using Allrpcs.Domain.Identifiers;

namespace Allrpcs.Domain.WorldEvents;

/// <summary>Records one eligible authoritative world event in an identifier-log snapshot.</summary>
public static class WorldEventRecordingTransition
{
    /// <summary>Returns a new log containing the exact authoritative event identifier.</summary>
    public static WorldEventLogSnapshot Record(
        WorldEventId eventId,
        WorldEventSnapshot worldEvents,
        WorldEventLogSnapshot eventLog)
    {
        if (!WorldEventRecordingPolicy.CanRecord(eventId, worldEvents, eventLog))
        {
            throw new InvalidOperationException(
                "The supplied world event is absent or already recorded.");
        }

        WorldEvent worldEvent = WorldEventLookup.FindById(eventId, worldEvents)
            ?? throw new InvalidOperationException("The eligible world event was not found.");
        return WorldEventLogAppendTransition.Append(worldEvent.EventId, eventLog);
    }
}
