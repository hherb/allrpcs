using Allrpcs.Domain.Identifiers;

namespace Allrpcs.Domain.WorldEvents;

/// <summary>Selects an authoritative world event by identifier without changing the snapshot.</summary>
public static class WorldEventLookup
{
    /// <summary>
    /// Returns the exact world event for a value-equal identifier, or null when no event matches.
    /// </summary>
    public static WorldEvent? FindById(WorldEventId eventId, WorldEventSnapshot snapshot)
    {
        ArgumentNullException.ThrowIfNull(eventId);
        ArgumentNullException.ThrowIfNull(snapshot);

        return snapshot.Entries.FirstOrDefault(worldEvent => worldEvent.EventId == eventId);
    }
}
