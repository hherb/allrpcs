using Allrpcs.Domain.WorldEvents;

namespace Allrpcs.Domain.Characters;

/// <summary>Decides whether exact authoritative world state is at one sabotage cadence boundary.</summary>
public static class CharacterSabotageTurnOpportunityCadenceSignalPolicy
{
    /// <summary>Returns true only for the exact current due meaningful world event.</summary>
    public static bool CanCreate(
        CharacterSabotageTurnOpportunityCadence cadence,
        WorldEvent currentEvent,
        WorldEventSnapshot worldEvents,
        WorldEventLogSnapshot eventLog,
        WorldTimeSnapshot worldTime)
    {
        ArgumentNullException.ThrowIfNull(cadence);
        ArgumentNullException.ThrowIfNull(currentEvent);
        ArgumentNullException.ThrowIfNull(worldEvents);
        ArgumentNullException.ThrowIfNull(eventLog);
        ArgumentNullException.ThrowIfNull(worldTime);

        if (!worldEvents.Entries.Any(candidate => ReferenceEquals(candidate, currentEvent))
            || !ReferenceEquals(currentEvent.TimeEffect, WorldEventTimeEffect.Advance)
            || !eventLog.Entries.Any(eventId => eventId == currentEvent.EventId)
            || worldTime.CurrentEventId != currentEvent.EventId)
        {
            return false;
        }

        int occurredMeaningfulEventCount = CountMeaningfulEvents(worldEvents, eventLog);
        return occurredMeaningfulEventCount > 0
            && occurredMeaningfulEventCount % cadence.MeaningfulEventInterval == 0;
    }

    internal static int CountMeaningfulEvents(
        WorldEventSnapshot worldEvents,
        WorldEventLogSnapshot eventLog) =>
        eventLog.Entries.Count(
            eventId => worldEvents.Entries.Any(
                worldEvent => worldEvent.EventId == eventId
                    && ReferenceEquals(worldEvent.TimeEffect, WorldEventTimeEffect.Advance)));
}
