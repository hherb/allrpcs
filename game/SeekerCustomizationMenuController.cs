using Allrpcs.Domain.Characters;
using Godot;

namespace Allrpcs.Game;

/// <summary>Collects one explicit player customization and installs its exact selection state.</summary>
public partial class SeekerCustomizationMenuController : Control
{
    /// <summary>Displayed while the menu is ready for a complete selection.</summary>
    public const string ReadyStatusText = "Complete every field and choose both options.";

    /// <summary>Displayed after invalid, incomplete, stale, or repeated submission.</summary>
    public const string RejectedStatusText = "Customization was not accepted.";

    /// <summary>Displayed after the exact selected profile is installed.</summary>
    public const string SelectedStatusText = "Customization selected.";

    private static readonly StringName OpenAction = new("open_seeker_customization");

    private SeekerCustomizationSelectionSceneSource source = null!;
    private TemporarySeekerCharacterSource characterSource = null!;
    private Seeker seeker = null!;
    private LineEdit displayName = null!;
    private LineEdit subject = null!;
    private LineEdit objective = null!;
    private LineEdit possessiveDeterminer = null!;
    private LineEdit possessivePronoun = null!;
    private LineEdit reflexive = null!;
    private OptionButton appearance = null!;
    private OptionButton background = null!;
    private Label status = null!;
    private bool seekerWasPhysicsProcessing;

    /// <inheritdoc />
    public override void _Ready()
    {
        source = GetNode<SeekerCustomizationSelectionSceneSource>(
            "../../SeekerCustomizationSelectionSceneSource");
        characterSource = GetNode<TemporarySeekerCharacterSource>(
            "../../TemporarySeekerCharacterSource");
        seeker = GetNode<Seeker>("../../Seeker");
        displayName = GetNode<LineEdit>("DisplayName");
        subject = GetNode<LineEdit>("Subject");
        objective = GetNode<LineEdit>("Objective");
        possessiveDeterminer = GetNode<LineEdit>("PossessiveDeterminer");
        possessivePronoun = GetNode<LineEdit>("PossessivePronoun");
        reflexive = GetNode<LineEdit>("Reflexive");
        appearance = GetNode<OptionButton>("Appearance");
        background = GetNode<OptionButton>("Background");
        status = GetNode<Label>("Status");

        PopulateOptions();
        status.Text = ReadyStatusText;
        GetNode<Button>("Confirm").Pressed += OnConfirmPressed;
        Hide();
    }

    /// <inheritdoc />
    public override void _UnhandledInput(InputEvent @event)
    {
        if (!@event.IsActionPressed(OpenAction)
            || Visible
            || source.CurrentState.SelectedProfile is not null)
        {
            return;
        }

        seekerWasPhysicsProcessing = seeker.IsPhysicsProcessing();
        Show();
        seeker.Velocity = Vector2.Zero;
        seeker.SetPhysicsProcess(false);
        displayName.GrabFocus();
        GetViewport().SetInputAsHandled();
    }

    private void PopulateOptions()
    {
        appearance.AddItem("Choose appearance");
        appearance.SetItemDisabled(0, true);
        foreach (SeekerAppearanceChoice choice in source.Options.AppearanceChoices)
        {
            appearance.AddItem(choice.DisplayName);
        }

        background.AddItem("Choose background");
        background.SetItemDisabled(0, true);
        foreach (SeekerBackgroundChoice choice in source.Options.BackgroundChoices)
        {
            background.AddItem(choice.DisplayName);
        }

        appearance.Select(-1);
        background.Select(-1);
    }

    private void OnConfirmPressed()
    {
        SeekerCustomizationSelectionState current = source.CurrentState;
        int appearanceIndex = appearance.Selected - 1;
        int backgroundIndex = background.Selected - 1;
        if (!Visible
            || current.SelectedProfile is not null
            || appearanceIndex < 0
            || appearanceIndex >= source.Options.AppearanceChoices.Count
            || backgroundIndex < 0
            || backgroundIndex >= source.Options.BackgroundChoices.Count)
        {
            status.Text = RejectedStatusText;
            return;
        }

        try
        {
            CharacterIdentity identity = new(
                characterSource.Character.Identity.Id,
                displayName.Text);
            SeekerPronouns pronouns = new(
                subject.Text,
                objective.Text,
                possessiveDeterminer.Text,
                possessivePronoun.Text,
                reflexive.Text);
            SeekerCustomizationSelectionState selected =
                SeekerCustomizationSelectionStateTransition.Select(
                    current,
                    identity,
                    pronouns,
                    source.Options.AppearanceChoices[appearanceIndex],
                    source.Options.BackgroundChoices[backgroundIndex]);
            if (!source.TryInstallSelection(current, selected))
            {
                status.Text = RejectedStatusText;
                return;
            }

            status.Text = SelectedStatusText;
            Hide();
            seeker.SetPhysicsProcess(seekerWasPhysicsProcessing);
        }
        catch (ArgumentException)
        {
            status.Text = RejectedStatusText;
        }
    }
}
