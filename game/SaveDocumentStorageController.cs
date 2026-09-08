using Allrpcs.Application.Persistence;
using Allrpcs.Infrastructure.Persistence;
using Godot;

namespace Allrpcs.Game;

/// <summary>Composes complete serialized save capture and restoration with local storage.</summary>
public partial class SaveDocumentStorageController : Node
{
    internal const string SmokeCheckEnvironmentVariable =
        "ALLRPCS_SAVE_DOCUMENT_STORAGE_CONTROLLER_CHECK";

    private const string EnabledEnvironmentValue = "1";
    private const string SaveDocumentVirtualPath = "user://save-v1.json";

    private SaveSnapshotJsonController jsonController = null!;
    private FileSaveDocumentStore documentStore = null!;

    /// <inheritdoc />
    public override void _Ready()
    {
        jsonController = GetNode<SaveSnapshotJsonController>("../SaveSnapshotJsonController");
        documentStore = new FileSaveDocumentStore(CurrentFilePath());
    }

    /// <summary>Captures and stores the complete current state when it is initialized.</summary>
    public SaveDocumentWriteResult SaveCurrent()
    {
        string? serializedDocument = jsonController.CaptureSerialized();
        return serializedDocument is null
            ? SaveDocumentWriteResult.Failed
            : documentStore.Save(serializedDocument);
    }

    /// <summary>
    /// Loads and restores the complete current document, preserving missing and failure outcomes.
    /// </summary>
    public SaveDocumentReadStatus LoadCurrent()
    {
        SaveDocumentReadResult result = documentStore.Load();
        if (result.Status != SaveDocumentReadStatus.Loaded)
        {
            return result.Status;
        }

        return jsonController.TryRestoreSerialized(result.SerializedDocument!)
            ? SaveDocumentReadStatus.Loaded
            : SaveDocumentReadStatus.Failed;
    }

    internal static string CurrentFilePath() =>
        OS.GetEnvironment(SmokeCheckEnvironmentVariable) == EnabledEnvironmentValue
            ? Path.Combine(
                Path.GetTempPath(),
                $"allrpcs-save-v1-smoke-{OS.GetProcessId()}.json")
            : ProjectSettings.GlobalizePath(SaveDocumentVirtualPath);
}
