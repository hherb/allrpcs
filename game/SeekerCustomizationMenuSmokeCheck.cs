using Allrpcs.Domain.Characters;
using Godot;

namespace Allrpcs.Game;

/// <summary>Proves explicit modal customization submission into the exact scene state.</summary>
public partial class SeekerCustomizationMenuSmokeCheck : Node
{
    private const string CheckEnvironmentVariable = "ALLRPCS_SEEKER_CUSTOMIZATION_MENU_CHECK";
    private const string EnabledEnvironmentValue = "1";
    private const string OpenAction = "open_seeker_customization";
    private const string UnrelatedAction = "seeker_customization_unrelated";

    /// <inheritdoc />
    public override void _Ready()
    {
        SetProcess(OS.GetEnvironment(CheckEnvironmentVariable) == EnabledEnvironmentValue);
    }

    /// <inheritdoc />
    public override void _Process(double delta)
    {
        _ = delta;
        SetProcess(false);
        RunChecks();
    }

    private void RunChecks()
    {
        SeekerCustomizationSelectionSceneSource source =
            GetNode<SeekerCustomizationSelectionSceneSource>(
                "../SeekerCustomizationSelectionSceneSource");
        SeekerCustomizationMenuController menu =
            GetNode<SeekerCustomizationMenuController>(
                "../SeekerCustomizationOverlay/Panel");
        Seeker seeker = GetNode<Seeker>("../Seeker");
        TemporarySeekerCharacterSource characterSource =
            GetNode<TemporarySeekerCharacterSource>("../TemporarySeekerCharacterSource");
        SeekerCustomizationSelectionState initial = source.CurrentState;
        LineEdit displayName = menu.GetNode<LineEdit>("DisplayName");
        LineEdit subject = menu.GetNode<LineEdit>("Subject");
        LineEdit objective = menu.GetNode<LineEdit>("Objective");
        LineEdit possessiveDeterminer = menu.GetNode<LineEdit>("PossessiveDeterminer");
        LineEdit possessivePronoun = menu.GetNode<LineEdit>("PossessivePronoun");
        LineEdit reflexive = menu.GetNode<LineEdit>("Reflexive");
        OptionButton appearance = menu.GetNode<OptionButton>("Appearance");
        OptionButton background = menu.GetNode<OptionButton>("Background");
        Button confirm = menu.GetNode<Button>("Confirm");
        Label status = menu.GetNode<Label>("Status");

        menu._UnhandledInput(Action(UnrelatedAction, pressed: true));
        menu._UnhandledInput(Action(OpenAction, pressed: false));
        if (menu.Visible
            || initial.SelectedProfile is not null
            || !ReferenceEquals(source.CurrentState, initial)
            || !seeker.IsPhysicsProcessing()
            || appearance.ItemCount != source.Options.AppearanceChoices.Count + 1
            || background.ItemCount != source.Options.BackgroundChoices.Count + 1
            || appearance.Selected != -1
            || background.Selected != -1)
        {
            Fail("Ordinary startup or released/unrelated input changed customization state.");
            return;
        }

        menu._UnhandledInput(Action(OpenAction, pressed: true));
        menu._UnhandledInput(Action(OpenAction, pressed: true));
        confirm.EmitSignal(BaseButton.SignalName.Pressed);
        if (!menu.Visible
            || seeker.IsPhysicsProcessing()
            || status.Text != SeekerCustomizationMenuController.RejectedStatusText
            || !ReferenceEquals(source.CurrentState, initial))
        {
            Fail("The modal menu did not reject an incomplete selection inertly.");
            return;
        }

        displayName.Text = "Mara";
        subject.Text = "they";
        objective.Text = "them";
        possessiveDeterminer.Text = "their";
        possessivePronoun.Text = "theirs";
        reflexive.Text = "themself";
        appearance.Select(2);
        background.Select(3);
        confirm.EmitSignal(BaseButton.SignalName.Pressed);

        SeekerCustomizationProfile? selected = source.CurrentState.SelectedProfile;
        if (menu.Visible
            || !seeker.IsPhysicsProcessing()
            || status.Text != SeekerCustomizationMenuController.SelectedStatusText
            || selected is null
            || selected.Identity.DisplayName != "Mara"
            || !ReferenceEquals(
                selected.Identity.Id,
                characterSource.Character.Identity.Id)
            || selected.Pronouns.Subject != "they"
            || !ReferenceEquals(selected.Appearance, source.Options.AppearanceChoices[1])
            || !ReferenceEquals(selected.Background, source.Options.BackgroundChoices[2]))
        {
            Fail("The complete modal selection did not retain and install exact input.");
            return;
        }

        SeekerCustomizationSelectionState installed = source.CurrentState;
        menu._UnhandledInput(Action(OpenAction, pressed: true));
        confirm.EmitSignal(BaseButton.SignalName.Pressed);
        if (menu.Visible
            || !ReferenceEquals(source.CurrentState, installed)
            || !seeker.IsPhysicsProcessing())
        {
            Fail("Repeated menu input changed the installed selection or movement gate.");
            return;
        }

        GD.Print(
            "Seeker customization menu smoke passed: inert hidden startup, released/unrelated "
            + "input rejection, explicit modal opening with movement suspension, complete-field "
            + "validation, exact appearance/background submission, one-shot closure, movement "
            + "resumption, and repeated-input preservation.");
        GetTree().Quit();
    }

    private static InputEventAction Action(string action, bool pressed) => new()
    {
        Action = action,
        Pressed = pressed,
    };

    private void Fail(string message)
    {
        GD.PushError(message);
        GetTree().Quit(exitCode: 1);
    }
}
