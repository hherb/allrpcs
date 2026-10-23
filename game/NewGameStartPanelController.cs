using Allrpcs.Application.Characters;
using Allrpcs.Application.Persistence;
using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Persistence;
using Allrpcs.Infrastructure.Persistence;
using Godot;

namespace Allrpcs.Game;

/// <summary>
/// Offers Continue or New game at launch when a save exists. Without a save, a launch starts with
/// the last confirmed Seeker profile, or opens customization when none is remembered.
/// </summary>
public partial class NewGameStartPanelController : Control
{
    internal const string MissingStatusText = "No save was found. Choose New game.";
    internal const string FailedStatusText = "The save could not be loaded. Choose New game.";

    private SaveDocumentStorageController storage = null!;
    private SeekerCustomizationSelectionSceneSource customization = null!;
    private SeekerCustomizationMenuController menu = null!;
    private SeekerCustomizationApplicationCoordinator application = null!;
    private TemporarySeekerCharacterSource seekerCharacter = null!;
    // Set only by Begin, so checks that confirm a profile never touch the player's remembered file.
    private FileSaveDocumentStore? rememberedProfile;
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
        application = GetNode<SeekerCustomizationApplicationCoordinator>(
            "../../SeekerCustomizationApplicationCoordinator");
        seekerCharacter = GetNode<TemporarySeekerCharacterSource>("../../TemporarySeekerCharacterSource");
        menu.ProfileConfirmed += Remember;
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
        rememberedProfile = new FileSaveDocumentStore(RememberedProfilePath());
        if (!SaveDocumentStorageController.HasSavedDocument())
        {
            if (!TryStartAsRemembered()) { _ = menu.TryOpen(); }
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

    // New game is where a remembered Seeker can be changed, so it asks again, filled in.
    internal void OnNewGamePressed()
    {
        Close();
        if (menu.TryOpen() && LoadRemembered() is { } profile) { menu.Prefill(profile); }
    }

    internal static string RememberedProfilePath() =>
        SaveDocumentStorageController.IsIsolatedSmokeCheck()
            ? Path.Combine(Path.GetTempPath(), $"allrpcs-seeker-profile-v1-smoke-{OS.GetProcessId()}.json")
            : ProjectSettings.GlobalizePath("user://seeker-profile-v1.json");

    private bool TryStartAsRemembered()
    {
        SeekerCustomizationSelectionState current = customization.CurrentState;
        // An unreadable file or an option this build no longer offers falls back to the form.
        return LoadRemembered() is { } profile
            && SeekerCustomizationRestorationPolicy.TryDecide(
                current, profile, seekerCharacter.Character.Identity.Id, out SeekerCustomizationSelectionState? selected)
            && selected is not null
            && application.TryApply(current, selected);
    }

    private SeekerCustomizationSaveState? LoadRemembered()
    {
        SaveDocumentReadResult? result = rememberedProfile?.Load();
        if (result?.Status != SaveDocumentReadStatus.Loaded) { return null; }
        try { return RememberedSeekerProfileJson.Deserialize(result.SerializedDocument!); }
        catch (ArgumentException) { return null; }
    }

    private void Remember(SeekerCustomizationProfile profile)
    {
        // A failed write only means the next launch asks again.
        _ = rememberedProfile?.Save(RememberedSeekerProfileJson.Serialize(SeekerCustomizationSaveState.From(profile)));
    }

    private void Close()
    {
        Hide();
        Control? focus = GetViewport().GuiGetFocusOwner();
        if (focus is not null && IsAncestorOf(focus)) { focus.ReleaseFocus(); }
        seeker.ResumeAfterModal(this);
    }
}
