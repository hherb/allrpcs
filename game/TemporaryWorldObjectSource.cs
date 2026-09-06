using Allrpcs.Domain.WorldObjects;
using Godot;

namespace Allrpcs.Game;

/// <summary>
/// Adapts the two existing visible greybox targets to one temporary world-object snapshot.
/// </summary>
public partial class TemporaryWorldObjectSource : Node
{
    /// <summary>Gets the single retained temporary world-object snapshot.</summary>
    public WorldObjectSnapshot WorldObjects { get; private set; } = null!;

    /// <inheritdoc />
    public override void _Ready()
    {
        GreyboxInteractionTarget herbPatch = GetNode<GreyboxInteractionTarget>(
            "../InteractionCandidates/HerbPatch");
        GreyboxInteractionTarget stoneMarker = GetNode<GreyboxInteractionTarget>(
            "../InteractionCandidates/StoneMarker");

        WorldObjects = new WorldObjectSnapshot(
            [CreateWorldObject(herbPatch), CreateWorldObject(stoneMarker)]);
    }

    private static WorldObject CreateWorldObject(GreyboxInteractionTarget target)
    {
        WorldObjectIdentity identity = new(target.ObjectId, target.DisplayName);
        WorldObjectDescription description = new(string.Empty);

        return new WorldObject(identity, description);
    }
}
