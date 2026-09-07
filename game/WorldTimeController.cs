using Allrpcs.Domain.WorldEvents;
using Godot;

namespace Allrpcs.Game;

/// <summary>Owns the scene's live immutable qualitative world-time state.</summary>
public partial class WorldTimeController : Node
{
    /// <summary>Gets the current immutable world-time snapshot.</summary>
    public WorldTimeSnapshot WorldTime { get; private set; } = new(null);

    /// <summary>Advances world time to one eligible authoritative event.</summary>
    public bool TryAdvance(WorldEvent worldEvent)
    {
        ArgumentNullException.ThrowIfNull(worldEvent);

        if (!WorldTimeAdvancementPolicy.CanAdvance(worldEvent, WorldTime))
        {
            return false;
        }

        WorldTime = WorldTimeAdvancementTransition.Advance(worldEvent, WorldTime);
        return true;
    }
}
