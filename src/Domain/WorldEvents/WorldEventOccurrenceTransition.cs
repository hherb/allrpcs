using Allrpcs.Domain.Identifiers;

namespace Allrpcs.Domain.WorldEvents;

/// <summary>Applies one eligible authoritative event to immutable log and world-time state.</summary>
public static class WorldEventOccurrenceTransition
{
    /// <summary>Records the event and advances qualitative time only when its effect requires it.</summary>
    public static WorldEventOccurrenceResult Occur(
        WorldEventId eventId,
        WorldEventSnapshot worldEvents,
        WorldEventLogSnapshot eventLog,
        WorldTimeSnapshot worldTime)
    {
        if (!WorldEventOccurrencePolicy.CanOccur(eventId, worldEvents, eventLog, worldTime))
        {
            throw new InvalidOperationException("The supplied world event cannot occur.");
        }

        WorldEvent worldEvent = WorldEventLookup.FindById(eventId, worldEvents)
            ?? throw new InvalidOperationException("The eligible world event was not found.");
        WorldEventLogSnapshot resultingLog = WorldEventRecordingTransition.Record(
            eventId,
            worldEvents,
            eventLog);
        WorldTimeSnapshot resultingTime = ReferenceEquals(
            worldEvent.TimeEffect,
            WorldEventTimeEffect.Advance)
            ? WorldTimeAdvancementTransition.Advance(worldEvent, worldTime)
            : worldTime;

        return new WorldEventOccurrenceResult(resultingLog, resultingTime);
    }
}
