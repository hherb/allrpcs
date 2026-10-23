using Allrpcs.Application.Characters;
using Allrpcs.Application.Persistence;
using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Persistence;
using Allrpcs.Infrastructure.Persistence;
using Godot;

namespace Allrpcs.Game;

/// <summary>
/// Offers Continue or New game at launch when a save exists. Without a save it offers
/// "Start as &lt;name&gt;" for the last confirmed Seeker profile, where New game changes the Seeker,
/// or opens customization when none is remembered.
/// </summary>
public partial class NewGameStartPanelController : Control
{
    internal const string MissingStatusText = "No save was found. Choose New game.";
    internal const string FailedStatusText = "The save could not be loaded. Choose New game.";
    private const string SavedTitleText = "A SAVED JOURNEY WAITS";
    private const string RememberedTitleText = "NO SAVED JOURNEY YET";

    private SaveDocumentStorageController storage = null!;
    private SeekerCustomizationSelectionSceneSource customization = null!;
    private SeekerCustomizationMenuController menu = null!;
    private SeekerCustomizationApplicationCoordinator application = null!;
    private TemporarySeekerCharacterSource seekerCharacter = null!;
    // Set only by Begin, so checks that confirm a profile never touch the player's remembered file.
    private FileSaveDocumentStore? rememberedProfile;
    private Seeker seeker = null!;
    private Button continueButton = null!;
    private Label title = null!;
    private Label status = null!;
    // Set while Continue reads "Start as <name>" because no save exists: the Seeker it starts as.
    private SeekerCustomizationSelectionState? rememberedSelection;

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
        title = GetNode<Label>("Title");
        status = GetNode<Label>("Status");
        continueButton.Pressed += OnContinuePressed;
        GetNode<Button>("NewGame").Pressed += OnNewGamePressed;
        Hide();
    }

    /// <summary>
    /// Starts a launch: the panel when a save or a usable remembered Seeker exists, otherwise
    /// new-game customization.
    /// </summary>
    internal void Begin()
    {
        rememberedProfile = new FileSaveDocumentStore(RememberedProfilePath());
        rememberedSelection = null;
        if (!SaveDocumentStorageController.HasSavedDocument())
        {
            if (LoadRemembered() is not { } profile || DecideRemembered(profile) is not { } selected)
            {
                _ = menu.TryOpen();
                return;
            }

            rememberedSelection = selected;
            title.Text = RememberedTitleText;
            continueButton.Text = $"START AS {profile.Identity.DisplayName.ToUpperInvariant()}";
        }
        else
        {
            title.Text = SavedTitleText;
            continueButton.Text = "CONTINUE";
        }

        status.Text = string.Empty;
        Show();
        seeker.SuspendForModal(this);
        continueButton.GrabFocus();
    }

    internal void OnContinuePressed()
    {
        if (rememberedSelection is { } selected)
        {
            Close();
            if (!application.TryApply(customization.CurrentState, selected)) { _ = menu.TryOpen(); }
            return;
        }

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

    // An option this build no longer offers yields null, so the launch falls back to the form.
    private SeekerCustomizationSelectionState? DecideRemembered(SeekerCustomizationSaveState profile) =>
        SeekerCustomizationRestorationPolicy.TryDecide(
            customization.CurrentState, profile, seekerCharacter.Character.Identity.Id,
            out SeekerCustomizationSelectionState? selected)
            ? selected
            : null;

    private SeekerCustomizationSaveState? LoadRemembered()
    {
        SaveDocumentReadResult? result = rememberedProfile?.Load();
        if (result?.Status == SaveDocumentReadStatus.Failed) { GD.PushWarning($"Could not read {RememberedProfilePath()}."); }
        if (result?.Status != SaveDocumentReadStatus.Loaded) { return null; }
        try { return RememberedSeekerProfileJson.Deserialize(result.SerializedDocument!); }
        catch (ArgumentException error)
        {
            GD.PushWarning($"Ignoring the remembered Seeker profile: {error.Message}");
            return null;
        }
    }

    private void Remember(SeekerCustomizationProfile profile)
    {
        // A failed write only means the next launch asks again, so it is logged, not shown.
        try
        {
            if (rememberedProfile?.Save(RememberedSeekerProfileJson.Serialize(SeekerCustomizationSaveState.From(profile)))
                == SaveDocumentWriteResult.Failed)
            {
                GD.PushWarning($"Could not remember the Seeker profile at {RememberedProfilePath()}.");
            }
        }
        catch (ArgumentException error) { GD.PushWarning($"Could not remember the Seeker profile: {error.Message}"); }
    }

    private void Close()
    {
        Hide();
        Control? focus = GetViewport().GuiGetFocusOwner();
        if (focus is not null && IsAncestorOf(focus)) { focus.ReleaseFocus(); }
        seeker.ResumeAfterModal(this);
    }
}
