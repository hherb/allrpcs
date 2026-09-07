using Allrpcs.Domain.Identifiers;
using Allrpcs.Domain.WorldEvents;
using Godot;

namespace Allrpcs.Game;

/// <summary>Owns and atomically updates the scene's authoritative world-event state.</summary>
public partial class WorldEventController : Node
{
    /// <summary>Gets the initialized immutable world-event snapshot, if any.</summary>
    public WorldEventSnapshot? WorldEvents { get; private set; }

    /// <summary>Gets the initialized immutable world-event identifier log, if any.</summary>
    public WorldEventLogSnapshot? EventLog { get; private set; }

    /// <summary>Gets the initialized immutable qualitative world-time state, if any.</summary>
    public WorldTimeSnapshot? WorldTime { get; private set; }

    /// <summary>Initializes this controller once from exact existing world-event state.</summary>
    public bool TryInitialize(
        WorldEventSnapshot worldEvents,
        WorldEventLogSnapshot eventLog,
        WorldTimeSnapshot worldTime)
    {
        ArgumentNullException.ThrowIfNull(worldEvents);
        ArgumentNullException.ThrowIfNull(eventLog);
        ArgumentNullException.ThrowIfNull(worldTime);

        if (WorldEvents is not null || EventLog is not null || WorldTime is not null)
        {
            return false;
        }

        WorldEvents = worldEvents;
        EventLog = eventLog;
        WorldTime = worldTime;
        return true;
    }

    /// <summary>Atomically records one eligible event and applies its qualitative-time effect.</summary>
    public bool TryOccur(WorldEventId eventId)
    {
        ArgumentNullException.ThrowIfNull(eventId);

        WorldEventSnapshot? worldEvents = WorldEvents;
        WorldEventLogSnapshot? eventLog = EventLog;
        WorldTimeSnapshot? worldTime = WorldTime;
        if (worldEvents is null
            || eventLog is null
            || worldTime is null
            || !WorldEventOccurrencePolicy.CanOccur(eventId, worldEvents, eventLog, worldTime))
        {
            return false;
        }

        WorldEventOccurrenceResult result = WorldEventOccurrenceTransition.Occur(
            eventId,
            worldEvents,
            eventLog,
            worldTime);
        EventLog = result.EventLog;
        WorldTime = result.WorldTime;
        return true;
    }

    /// <summary>Finds the exact initialized world event with the supplied identifier.</summary>
    public WorldEvent? FindWorldEvent(WorldEventId eventId)
    {
        ArgumentNullException.ThrowIfNull(eventId);

        WorldEventSnapshot? worldEvents = WorldEvents;
        return worldEvents is null
            ? null
            : WorldEventLookup.FindById(eventId, worldEvents);
    }
}
