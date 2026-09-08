using Allrpcs.Domain.Persistence;
using Allrpcs.Infrastructure.Persistence;
using Godot;

namespace Allrpcs.Game;

/// <summary>Composes in-memory snapshot capture and restoration with the JSON format boundary.</summary>
public partial class SaveSnapshotJsonController : Node
{
    private SaveSnapshotCaptureController captureController = null!;
    private SaveSnapshotRestorationController restorationController = null!;

    /// <inheritdoc />
    public override void _Ready()
    {
        captureController = GetNode<SaveSnapshotCaptureController>(
            "../SaveSnapshotCaptureController");
        restorationController = GetNode<SaveSnapshotRestorationController>(
            "../SaveSnapshotRestorationController");
    }

    /// <summary>Captures canonical JSON only when the complete live state is initialized.</summary>
    public string? CaptureSerialized()
    {
        SaveSnapshot? snapshot = captureController.Capture();
        return snapshot is null
            ? null
            : SaveSnapshotJsonSerializer.Serialize(snapshot);
    }

    /// <summary>
    /// Restores one strict, supported JSON document after complete scenario preflight.
    /// </summary>
    public bool TryRestoreSerialized(string serialized)
    {
        ArgumentNullException.ThrowIfNull(serialized);

        SaveSnapshot snapshot;
        try
        {
            snapshot = SaveSnapshotJsonDeserializer.Deserialize(serialized);
        }
        catch (ArgumentException)
        {
            return false;
        }

        return restorationController.TryRestore(snapshot);
    }
}
