using Allrpcs.Domain.Identifiers;

namespace Allrpcs.Domain.WorldEvents;

/// <summary>Resolves an identifier log against authoritative world-event definitions.</summary>
public static class WorldEventLogProjection
{
    /// <summary>
    /// Returns exact authoritative events in log order and rejects unresolved identifiers.
    /// </summary>
    public static WorldEventSnapshot Resolve(
        WorldEventLogSnapshot eventLog,
        WorldEventSnapshot events)
    {
        ArgumentNullException.ThrowIfNull(eventLog);
        ArgumentNullException.ThrowIfNull(events);

        List<WorldEvent> resolvedEvents = new();
        foreach (WorldEventId eventId in eventLog.Entries)
        {
            WorldEvent worldEvent = WorldEventLookup.FindById(eventId, events)
                ?? throw new InvalidOperationException(
                    "The world-event log contains an identifier without an authoritative event.");
            resolvedEvents.Add(worldEvent);
        }

        return new WorldEventSnapshot(resolvedEvents);
    }
}
