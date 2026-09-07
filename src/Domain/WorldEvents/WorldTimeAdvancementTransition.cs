namespace Allrpcs.Domain.WorldEvents;

/// <summary>Advances qualitative world time to one eligible authoritative event.</summary>
public static class WorldTimeAdvancementTransition
{
    /// <summary>Returns a new snapshot retaining the exact advancing event identifier.</summary>
    public static WorldTimeSnapshot Advance(WorldEvent worldEvent, WorldTimeSnapshot worldTime)
    {
        if (!WorldTimeAdvancementPolicy.CanAdvance(worldEvent, worldTime))
        {
            throw new InvalidOperationException(
                "The supplied world event cannot advance the current world time.");
        }

        return new WorldTimeSnapshot(worldEvent.EventId);
    }
}
