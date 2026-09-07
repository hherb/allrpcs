namespace Allrpcs.Domain.WorldEvents;

/// <summary>Decides whether one authoritative event can advance qualitative world time.</summary>
public static class WorldTimeAdvancementPolicy
{
    /// <summary>
    /// Returns true only for an advancing event different from the current world-time event.
    /// </summary>
    public static bool CanAdvance(WorldEvent worldEvent, WorldTimeSnapshot worldTime)
    {
        ArgumentNullException.ThrowIfNull(worldEvent);
        ArgumentNullException.ThrowIfNull(worldTime);

        return worldEvent.TimeEffect == WorldEventTimeEffect.Advance &&
            worldEvent.EventId != worldTime.CurrentEventId;
    }
}
