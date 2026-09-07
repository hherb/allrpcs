using Allrpcs.Domain.Identifiers;
using Allrpcs.Domain.WorldEvents;
using Godot;

namespace Allrpcs.Game;

/// <summary>Owns the scene's initialized immutable authoritative world-event snapshot.</summary>
public partial class WorldEventController : Node
{
    /// <summary>Gets the initialized immutable world-event snapshot, if any.</summary>
    public WorldEventSnapshot? WorldEvents { get; private set; }

    /// <summary>Initializes this controller once from an existing world-event snapshot.</summary>
    public bool TryInitializeWorldEvents(WorldEventSnapshot worldEvents)
    {
        ArgumentNullException.ThrowIfNull(worldEvents);

        if (WorldEvents is not null)
        {
            return false;
        }

        WorldEvents = worldEvents;
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
