using Allrpcs.Domain.Identifiers;

namespace Allrpcs.Domain.WorldEvents;

/// <summary>Decides whether an authoritative event can occur against log and world-time state.</summary>
public static class WorldEventOccurrencePolicy
{
    /// <summary>Returns true when recording and any required qualitative-time advance are eligible.</summary>
    public static bool CanOccur(
        WorldEventId eventId,
        WorldEventSnapshot worldEvents,
        WorldEventLogSnapshot eventLog,
        WorldTimeSnapshot worldTime)
    {
        ArgumentNullException.ThrowIfNull(eventId);
        ArgumentNullException.ThrowIfNull(worldEvents);
        ArgumentNullException.ThrowIfNull(eventLog);
        ArgumentNullException.ThrowIfNull(worldTime);

        if (!WorldEventRecordingPolicy.CanRecord(eventId, worldEvents, eventLog))
        {
            return false;
        }

        WorldEvent worldEvent = WorldEventLookup.FindById(eventId, worldEvents)
            ?? throw new InvalidOperationException("The recordable world event was not found.");
        return ReferenceEquals(worldEvent.TimeEffect, WorldEventTimeEffect.NoAdvance)
            || WorldTimeAdvancementPolicy.CanAdvance(worldEvent, worldTime);
    }
}
