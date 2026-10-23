using Allrpcs.Application.Persistence;
using Godot;

namespace Allrpcs.Game;

/// <summary>Routes ordinary explicit save and load input to the existing storage boundary.</summary>
public partial class SaveLoadInputController : Node
{
    internal const string SmokeCheckEnvironmentVariable = "ALLRPCS_SAVE_LOAD_INPUT_CHECK";
    internal const string ReadyStatusText = "SAVE / LOAD READY";
    internal const string SavedStatusText = "SAVED";
    internal const string SaveFailedStatusText = "SAVE FAILED";
    internal const string SavedRecoveryText =
        "Saved. Next launch, choose Continue to resume.";
    internal const string SaveFailedRecoveryText =
        "Save failed after initialization. Check local storage and press Q again.";
    internal const string LoadedStatusText = "LOADED";
    internal const string MissingStatusText = "NO SAVE FOUND";
    internal const string LoadFailedStatusText = "LOAD FAILED";

    private static readonly StringName SaveAction = new("save_current");
    private static readonly StringName LoadAction = new("load_current");

    private RepresentativeJournalCompositionCoordinator journalComposition = null!;
    private JournalController journal = null!;
    private SaveDocumentStorageController storage = null!;
    private Label recoveryStatus = null!;
    private Label status = null!;

    /// <inheritdoc />
    public override void _Ready()
    {
        storage = GetNode<SaveDocumentStorageController>("../SaveDocumentStorageController");
        Button save = GetNode<Button>("../CommandOverlay/SaveLoadPanel/Save");
        Button load = GetNode<Button>("../CommandOverlay/SaveLoadPanel/Load");
        journalComposition = GetNode<RepresentativeJournalCompositionCoordinator>(
            "../RepresentativeJournalCompositionCoordinator");
        journal = GetNode<JournalController>("../JournalController");
        recoveryStatus = GetNode<Label>("../CommandOverlay/Panel/RecoveryStatus");
        status = GetNode<Label>("../CommandOverlay/SaveLoadPanel/Status");

        save.Pressed += () => Activate(save, Save);
        load.Pressed += () => Activate(load, Load);
        ControlSettingsController.SetGuidance(status, ReadyStatusText);
    }

    /// <inheritdoc />
    public override void _UnhandledInput(InputEvent @event)
    {
        if (!ControlInputContext.WorldAllowed(this)) { return; }
        ArgumentNullException.ThrowIfNull(@event);

        if (ControlInputMatcher.Pressed(@event, SaveAction))
        {
            Save();
        }
        else if (ControlInputMatcher.Pressed(@event, LoadAction))
        {
            Load();
        }
        else
        {
            return;
        }

        GetViewport().SetInputAsHandled();
    }

    private static void Activate(Button button, Action action)
    {
        try { action(); }
        finally { button.ReleaseFocus(); }
    }

    private void Save()
    {
        // A game saves before the map is first opened too; the save records the map as unopened.
        if (journal.Journal is null && !journalComposition.TryComposeAndInitialize())
        {
            ControlSettingsController.SetGuidance(status, SaveFailedStatusText);
            ControlSettingsController.SetGuidance(recoveryStatus, SaveFailedRecoveryText);
            return;
        }

        bool saved = storage.SaveCurrent() == SaveDocumentWriteResult.Saved;
        ControlSettingsController.SetGuidance(status, saved ? SavedStatusText : SaveFailedStatusText);
        ControlSettingsController.SetGuidance(recoveryStatus, saved ? SavedRecoveryText : SaveFailedRecoveryText);
    }

    private void Load()
    {
        ControlSettingsController.SetGuidance(status, storage.LoadCurrent() switch
        {
            SaveDocumentReadStatus.Loaded => LoadedStatusText,
            SaveDocumentReadStatus.Missing => MissingStatusText,
            _ => LoadFailedStatusText,
        });
    }
}
