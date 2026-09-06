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
    private const string HerbPatchDescription =
        "A cluster of narrow leaves gives off a sharp scent.";
    private const string StoneMarkerObjectIdText = "stone-marker";
    private const string StoneMarkerDisplayName = "STONE MARKER";
    private const string StoneMarkerDescription =
        "Weathered carvings mark one face of the standing stone.";

    /// <summary>Gets the single retained temporary world-object snapshot.</summary>
    public WorldObjectSnapshot WorldObjects { get; private set; } = null!;

    /// <inheritdoc />
    public override void _Ready()
    {
        WorldObjects = new WorldObjectSnapshot(
            [
                CreateWorldObject(
                    HerbPatchObjectIdText,
                    HerbPatchDisplayName,
                    HerbPatchDescription),
                CreateWorldObject(
                    StoneMarkerObjectIdText,
                    StoneMarkerDisplayName,
                    StoneMarkerDescription),
            ]);
    }

    private static WorldObject CreateWorldObject(
        string objectIdText,
        string displayName,
        string descriptionText)
    {
        WorldObjectIdentity identity = new(ObjectId.Parse(objectIdText), displayName);
        WorldObjectDescription description = new(descriptionText);

        return new WorldObject(identity, description);
    }
}
