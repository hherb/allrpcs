namespace Allrpcs.Domain.WorldEvents;

/// <summary>Derives qualitative world time from ordered authoritative world events.</summary>
public static class WorldTimeProjection
{
    /// <summary>Returns the exact latest advancing event identifier, or the initial state.</summary>
    public static WorldTimeSnapshot FromEvents(WorldEventSnapshot events)
    {
        ArgumentNullException.ThrowIfNull(events);

        WorldEvent? currentEvent = null;
        foreach (WorldEvent worldEvent in events.Entries)
        {
            if (worldEvent.TimeEffect == WorldEventTimeEffect.Advance)
            {
                currentEvent = worldEvent;
            }
        }

        return new WorldTimeSnapshot(currentEvent?.EventId);
    }
}
