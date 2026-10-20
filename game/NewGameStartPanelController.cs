using Allrpcs.Application.Persistence;
using Godot;

namespace Allrpcs.Game;

/// <summary>Offers Continue or New game at launch when a save exists; otherwise opens customization.</summary>
public partial class NewGameStartPanelController : Control
{
    internal const string MissingStatusText = "No save was found. Choose New game.";
    internal const string FailedStatusText = "The save could not be loaded. Choose New game.";

    private SaveDocumentStorageController storage = null!;
    private SeekerCustomizationSelectionSceneSource customization = null!;
    private SeekerCustomizationMenuController menu = null!;
    private Seeker seeker = null!;
    private Button continueButton = null!;
    private Label status = null!;

    /// <inheritdoc />
    public override void _Ready()
    {
        storage = GetNode<SaveDocumentStorageController>("../../SaveDocumentStorageController");
        customization = GetNode<SeekerCustomizationSelectionSceneSource>(
            "../../SeekerCustomizationSelectionSceneSource");
        menu = GetNode<SeekerCustomizationMenuController>("../Panel");
        seeker = GetNode<Seeker>("../../Seeker");
        continueButton = GetNode<Button>("Continue");
        status = GetNode<Label>("Status");
        continueButton.Pressed += OnContinuePressed;
        GetNode<Button>("NewGame").Pressed += OnNewGamePressed;
        Hide();
    }

    /// <summary>Starts a launch: the panel when a save exists, otherwise new-game customization.</summary>
    internal void Begin()
    {
        if (!SaveDocumentStorageController.HasSavedDocument())
        {
            _ = menu.TryOpen();
            return;
        }

        status.Text = string.Empty;
        Show();
        seeker.SuspendForModal(this);
        continueButton.GrabFocus();
    }

    internal void OnContinuePressed()
    {
        SaveDocumentReadStatus result = storage.LoadCurrent();
        if (result != SaveDocumentReadStatus.Loaded)
        {
            status.Text = result == SaveDocumentReadStatus.Missing ? MissingStatusText : FailedStatusText;
            return;
        }

        Close();
        // A save taken before customization still needs a Seeker.
        if (customization.CurrentState.SelectedProfile is null) { _ = menu.TryOpen(); }
    }

    internal void OnNewGamePressed()
    {
        Close();
        _ = menu.TryOpen();
    }

    private void Close()
    {
        Hide();
        Control? focus = GetViewport().GuiGetFocusOwner();
        if (focus is not null && IsAncestorOf(focus)) { focus.ReleaseFocus(); }
        seeker.ResumeAfterModal(this);
    }
}
