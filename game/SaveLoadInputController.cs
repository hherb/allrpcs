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
    internal const string LoadedStatusText = "LOADED";
    internal const string MissingStatusText = "NO SAVE FOUND";
    internal const string LoadFailedStatusText = "LOAD FAILED";

    private static readonly StringName SaveAction = new("save_current");
    private static readonly StringName LoadAction = new("load_current");

    private SaveDocumentStorageController storage = null!;
    private Label status = null!;

    /// <inheritdoc />
    public override void _Ready()
    {
        storage = GetNode<SaveDocumentStorageController>("../SaveDocumentStorageController");
        Button save = GetNode<Button>("../CommandOverlay/SaveLoadPanel/Save");
        Button load = GetNode<Button>("../CommandOverlay/SaveLoadPanel/Load");
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
        status.Text = storage.SaveCurrent() == SaveDocumentWriteResult.Saved
            ? SavedStatusText
            : SaveFailedStatusText;
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
