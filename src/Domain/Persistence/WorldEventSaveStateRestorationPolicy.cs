using Allrpcs.Domain.Identifiers;
using Allrpcs.Domain.WorldEvents;

namespace Allrpcs.Domain.Persistence;

/// <summary>Decides whether saved event state matches authoritative event definitions.</summary>
public static class WorldEventSaveStateRestorationPolicy
{
    /// <summary>
    /// Returns whether every logged event resolves and reproduces the saved qualitative time.
    /// </summary>
    public static bool CanRestore(
        WorldEventSaveState state,
        WorldEventSnapshot worldEvents)
    {
        ArgumentNullException.ThrowIfNull(state);
        ArgumentNullException.ThrowIfNull(worldEvents);

        foreach (WorldEventId eventId in state.EventLog.Entries)
        {
            if (WorldEventLookup.FindById(eventId, worldEvents) is null)
            {
                return false;
            }
        }

        WorldEventSnapshot resolvedEvents = WorldEventLogProjection.Resolve(
            state.EventLog,
            worldEvents);
        WorldTimeSnapshot projectedTime = WorldTimeProjection.FromEvents(resolvedEvents);
        return projectedTime.CurrentEventId == state.WorldTime.CurrentEventId;
    }
}
