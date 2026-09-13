using Allrpcs.Domain.Identifiers;
using Allrpcs.Domain.WorldObjects;
using Godot;

namespace Allrpcs.Game;

/// <summary>
/// Authors the temporary scene world objects in one retained snapshot.
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
    private const string WaterCaskObjectIdText = "village-water-cask";
    private const string WaterCaskDisplayName = "Village water cask";
    private const string WaterCaskDescription = "A covered cask beside the village well.";

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
                CreateWorldObject(
                    WaterCaskObjectIdText,
                    WaterCaskDisplayName,
                    WaterCaskDescription),
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
