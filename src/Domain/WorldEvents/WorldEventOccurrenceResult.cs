namespace Allrpcs.Domain.WorldEvents;

/// <summary>Contains the immutable event-log and qualitative-time results of one occurrence.</summary>
public sealed class WorldEventOccurrenceResult
{
    /// <summary>Creates a result retaining the exact supplied snapshots.</summary>
    public WorldEventOccurrenceResult(
        WorldEventLogSnapshot eventLog,
        WorldTimeSnapshot worldTime)
    {
        ArgumentNullException.ThrowIfNull(eventLog);
        ArgumentNullException.ThrowIfNull(worldTime);

        EventLog = eventLog;
        WorldTime = worldTime;
    }

    /// <summary>Gets the resulting immutable world-event identifier log.</summary>
    public WorldEventLogSnapshot EventLog { get; }

    /// <summary>Gets the resulting immutable qualitative world-time state.</summary>
    public WorldTimeSnapshot WorldTime { get; }
}
