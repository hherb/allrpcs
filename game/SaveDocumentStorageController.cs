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
        if (serializedDocument is null)
        {
            // Not a storage fault: some live state (e.g. a half-initialized map) cannot be captured.
            GD.PushWarning("Save refused: the current game state could not be captured completely.");
            return SaveDocumentWriteResult.Failed;
        }

        return documentStore.Save(serializedDocument);
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

    /// <summary>Returns whether a save document exists to offer Continue.</summary>
    internal static bool HasSavedDocument() => File.Exists(CurrentFilePath());

    internal static string CurrentFilePath() =>
        IsIsolatedSmokeCheck()
            ? Path.Combine(
                Path.GetTempPath(),
                $"allrpcs-save-v1-smoke-{OS.GetProcessId()}.json")
            : ProjectSettings.GlobalizePath(SaveDocumentVirtualPath);

    internal static bool IsIsolatedSmokeCheck() =>
        DebugTestEnvironment.GetEnvironment("ALLRPCS_DIALOGUE_OWNERSHIP_CHECK").Length > 0
        || DebugTestEnvironment.GetEnvironment(SmokeCheckEnvironmentVariable) == EnabledEnvironmentValue
        || DebugTestEnvironment.GetEnvironment(SaveLoadInputController.SmokeCheckEnvironmentVariable)
            == EnabledEnvironmentValue
        || DebugTestEnvironment.GetEnvironment("ALLRPCS_NEW_GAME_CUSTOMIZATION_CHECK")
            == EnabledEnvironmentValue
        || DebugTestEnvironment.GetEnvironment("ALLRPCS_SAVE_CONTINUE_CHECK")
            == EnabledEnvironmentValue
        || DebugTestEnvironment.GetEnvironment("ALLRPCS_INGREDIENT_REMEDY_CHECK")
            == EnabledEnvironmentValue
        || DebugTestEnvironment.GetEnvironment("ALLRPCS_HYPOTHESIS_TEST_CHECK")
            == EnabledEnvironmentValue
        || DebugTestEnvironment.GetEnvironment("ALLRPCS_SLICE_STAKES_CHECK")
            == EnabledEnvironmentValue
        // Its save binding now really saves, so it must never touch the player's save.
        || DebugTestEnvironment.GetEnvironment("ALLRPCS_CONTROLS_MAIN_CHECK")
            == EnabledEnvironmentValue;
}
