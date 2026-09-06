using Allrpcs.Domain.Identifiers;
using Allrpcs.Domain.WorldObjects;
using Godot;

namespace Allrpcs.Game;

/// <summary>
/// Authors the two temporary visible world objects in one retained snapshot.
/// </summary>
public partial class TemporaryWorldObjectSource : Node
{
    private const string HerbPatchObjectIdText = "herb-patch";
    private const string HerbPatchDisplayName = "HERB PATCH";
    private const string StoneMarkerObjectIdText = "stone-marker";
    private const string StoneMarkerDisplayName = "STONE MARKER";

    /// <summary>Gets the single retained temporary world-object snapshot.</summary>
    public WorldObjectSnapshot WorldObjects { get; private set; } = null!;

    /// <inheritdoc />
    public override void _Ready()
    {
        WorldObjects = new WorldObjectSnapshot(
            [
                CreateWorldObject(HerbPatchObjectIdText, HerbPatchDisplayName),
                CreateWorldObject(StoneMarkerObjectIdText, StoneMarkerDisplayName),
            ]);
    }

    private static WorldObject CreateWorldObject(string objectIdText, string displayName)
    {
        WorldObjectIdentity identity = new(ObjectId.Parse(objectIdText), displayName);
        WorldObjectDescription description = new(string.Empty);

        return new WorldObject(identity, description);
    }
}
