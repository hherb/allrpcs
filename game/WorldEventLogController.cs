using Allrpcs.Domain.Identifiers;
using Allrpcs.Domain.WorldEvents;
using Godot;

namespace Allrpcs.Game;

/// <summary>Owns the scene's live immutable append-only world-event log.</summary>
public partial class WorldEventLogController : Node
{
    /// <summary>Gets the current immutable world-event log snapshot.</summary>
    public WorldEventLogSnapshot EventLog { get; private set; } = new([]);

    /// <summary>Records one defined, previously unrecorded world-event identifier.</summary>
    public bool TryRecord(WorldEventId eventId, WorldEventSnapshot worldEvents)
    {
        ArgumentNullException.ThrowIfNull(eventId);
        ArgumentNullException.ThrowIfNull(worldEvents);

        if (!WorldEventRecordingPolicy.CanRecord(eventId, worldEvents, EventLog))
        {
            return false;
        }

        EventLog = WorldEventRecordingTransition.Record(eventId, worldEvents, EventLog);
        return true;
    }
}
