using Allrpcs.Domain.Identifiers;
using Allrpcs.Domain.WorldEvents;
using Godot;

namespace Allrpcs.Game;

/// <summary>Owns the scene's live immutable append-only world-event log.</summary>
public partial class WorldEventLogController : Node
{
    /// <summary>Gets the current immutable world-event log snapshot.</summary>
    public WorldEventLogSnapshot EventLog { get; private set; } = new([]);

    /// <summary>Appends one previously unrecorded world-event identifier.</summary>
    public bool TryAppend(WorldEventId eventId)
    {
        ArgumentNullException.ThrowIfNull(eventId);

        if (!WorldEventLogAppendPolicy.CanAppend(eventId, EventLog))
        {
            return false;
        }

        EventLog = WorldEventLogAppendTransition.Append(eventId, EventLog);
        return true;
    }
}
