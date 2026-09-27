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
    internal const string MapRequiredStatusText = "PRESS M FIRST";
    internal const string MapRequiredRecoveryText =
        "Save: press M to initialize the map, close it with M, then press Q again.";
    internal const string SavedRecoveryText =
        "Saved. Fully close the game, reopen it, then press L.";
    internal const string SaveFailedRecoveryText =
        "Save failed after initialization. Check local storage and press Q again.";
    internal const string LoadedStatusText = "LOADED";
    internal const string MissingStatusText = "NO SAVE FOUND";
    internal const string LoadFailedStatusText = "LOAD FAILED";

    private static readonly StringName SaveAction = new("save_current");
    private static readonly StringName LoadAction = new("load_current");

    private RepresentativeJournalCompositionCoordinator journalComposition = null!;
    private JournalController journal = null!;
    private SeekerMapController map = null!;
    private OpeningRegionInvestigationRouteProgressController routes = null!;
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
        map = GetNode<SeekerMapController>("../SeekerMapController");
        routes = GetNode<OpeningRegionInvestigationRouteProgressController>(
            "../OpeningRegionInvestigationRouteProgressController");
        recoveryStatus = GetNode<Label>("../CommandOverlay/Panel/RecoveryStatus");
        status = GetNode<Label>("../CommandOverlay/SaveLoadPanel/Status");

        save.Pressed += Save;
        load.Pressed += Load;
        status.Text = ReadyStatusText;
    }

    /// <inheritdoc />
    public override void _UnhandledInput(InputEvent @event)
    {
        ArgumentNullException.ThrowIfNull(@event);

        if (@event.IsActionPressed(SaveAction))
        {
            Save();
        }
        else if (@event.IsActionPressed(LoadAction))
        {
            Load();
        }
        else
        {
            return;
        }

        GetViewport().SetInputAsHandled();
    }

    private void Save()
    {
        if (map.Catalogue is null
            || map.Map is null
            || map.Notes is null
            || routes.Progress is null)
        {
            status.Text = MapRequiredStatusText;
            recoveryStatus.Text = MapRequiredRecoveryText;
            return;
        }

        if (journal.Journal is null && !journalComposition.TryComposeAndInitialize())
        {
            status.Text = SaveFailedStatusText;
            recoveryStatus.Text = SaveFailedRecoveryText;
            return;
        }

        bool saved = storage.SaveCurrent() == SaveDocumentWriteResult.Saved;
        status.Text = saved ? SavedStatusText : SaveFailedStatusText;
        recoveryStatus.Text = saved ? SavedRecoveryText : SaveFailedRecoveryText;
    }

    private void Load()
    {
        status.Text = storage.LoadCurrent() switch
        {
            SaveDocumentReadStatus.Loaded => LoadedStatusText,
            SaveDocumentReadStatus.Missing => MissingStatusText,
            _ => LoadFailedStatusText,
        };
    }
}
