using Allrpcs.Domain.WorldEvents;

namespace Allrpcs.Domain.Persistence;

/// <summary>Composes the immutable world-event snapshots required for later save restoration.</summary>
public sealed record WorldEventSaveState
{
    /// <summary>Creates world-event save state from exact existing snapshots.</summary>
    public WorldEventSaveState(
        WorldEventLogSnapshot eventLog,
        WorldTimeSnapshot worldTime)
    {
        ArgumentNullException.ThrowIfNull(eventLog);
        ArgumentNullException.ThrowIfNull(worldTime);

        EventLog = eventLog;
        WorldTime = worldTime;
    }

    /// <summary>Gets the exact retained authoritative event-log snapshot.</summary>
    public WorldEventLogSnapshot EventLog { get; }

    /// <summary>Gets the exact retained qualitative world-time snapshot.</summary>
    public WorldTimeSnapshot WorldTime { get; }
}
