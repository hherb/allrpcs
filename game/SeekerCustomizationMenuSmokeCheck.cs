using Allrpcs.Application.Navigation;
using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Identifiers;
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
        string mode = OS.GetEnvironment("ALLRPCS_MODAL_INPUT_CHECK");
        if (mode.Length > 0) { ModalInputRecoverySmokeCheck.Run(this, mode); return; }
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
        SeekerCustomizationApplicationCoordinator applicationCoordinator =
            GetNode<SeekerCustomizationApplicationCoordinator>(
                "../SeekerCustomizationApplicationCoordinator");
        SeekerBackgroundRouteVariationSceneSource routeSource =
            GetNode<SeekerBackgroundRouteVariationSceneSource>(
                "../SeekerBackgroundRouteVariationSceneSource");
        SeekerBackgroundRouteAccessSceneSource routeAccessSource =
            GetNode<SeekerBackgroundRouteAccessSceneSource>(
                "../SeekerBackgroundRouteAccessSceneSource");
        OpeningRegionClosedForestPathSceneSource closedPathSource =
            GetNode<OpeningRegionClosedForestPathSceneSource>(
                "../OpeningRegionClosedForestPathSceneSource");
        StaticBody2D closedGate = closedPathSource.GetNode<StaticBody2D>("ClosedGate");
        SeekerCustomizationSelectionState initial = source.CurrentState;
        SeekerBackgroundRouteVariationState initialRoutes = routeSource.CurrentState;
        Character initialCharacter = characterSource.Character;
        LineEdit displayName = menu.GetNode<LineEdit>("DisplayName");
        LineEdit subject = menu.GetNode<LineEdit>("Subject");
        LineEdit objective = menu.GetNode<LineEdit>("Objective");
        LineEdit possessiveDeterminer = menu.GetNode<LineEdit>("PossessiveDeterminer");
        LineEdit possessivePronoun = menu.GetNode<LineEdit>("PossessivePronoun");
        LineEdit reflexive = menu.GetNode<LineEdit>("Reflexive");
        OptionButton appearance = menu.GetNode<OptionButton>("Appearance");
        OptionButton background = menu.GetNode<OptionButton>("Background");
        Button confirm = menu.GetNode<Button>("Confirm");
        Label hint = menu.GetNode<Label>("Hint");
        Label status = menu.GetNode<Label>("Status");

        SeekerCustomizationSelectionState rebuiltInitial =
            SeekerCustomizationSelectionStateTransition.Initialize(source.Options);
        SeekerCustomizationSelectionState rebuiltSelected = Select(
            rebuiltInitial,
            initialCharacter.Identity.Id,
            source.Options);
        SeekerCustomizationOptions foreignOptions =
            RepresentativeSeekerCustomizationOptionsSource.Create();
        SeekerCustomizationSelectionState foreignInitial =
            SeekerCustomizationSelectionStateTransition.Initialize(foreignOptions);
        SeekerCustomizationSelectionState foreignSelected = Select(
            foreignInitial,
            initialCharacter.Identity.Id,
            foreignOptions);

        if (applicationCoordinator.TryApply(null, null)
            || applicationCoordinator.TryApply(rebuiltInitial, rebuiltSelected)
            || applicationCoordinator.TryApply(initial, rebuiltSelected)
            || applicationCoordinator.TryApply(initial, foreignSelected)
            || !ReferenceEquals(source.CurrentState, initial)
            || !ReferenceEquals(routeSource.CurrentState, initialRoutes)
            || !ReferenceEquals(characterSource.Character, initialCharacter)
            || characterSource.CustomizationApplication is not null
            || routeAccessSource.CurrentAccess is not null
            || closedPathSource.CurrentAccess is not null)
        {
            Fail("Rejected customization applications changed scene state.");
            return;
        }

        menu._UnhandledInput(Action(UnrelatedAction, pressed: true));
        menu._UnhandledInput(Action(OpenAction, pressed: false));
        if (menu.Visible
            || initial.SelectedProfile is not null
            || !ReferenceEquals(source.CurrentState, initial)
            || !ReferenceEquals(routeSource.CurrentState, initialRoutes)
            || !seeker.IsPhysicsProcessing()
            || appearance.ItemCount != source.Options.AppearanceChoices.Count + 1
            || background.ItemCount != source.Options.BackgroundChoices.Count + 1
            || appearance.Selected != -1
            || background.Selected != -1
            || hint.Text
                != "Selection is one-shot. Each background unlocks one optional route advantage.")
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
        SeekerCustomizedCharacterApplication? application =
            characterSource.CustomizationApplication;
        if (menu.Visible
            || !seeker.IsPhysicsProcessing()
            || status.Text != SeekerCustomizationMenuController.SelectedStatusText
            || selected is null
            || application is null
            || selected.Identity.DisplayName != "Mara"
            || !ReferenceEquals(application.SelectedState, source.CurrentState)
            || !ReferenceEquals(application.CurrentCharacter, initialCharacter)
            || !ReferenceEquals(application.CustomizedCharacter, characterSource.Character)
            || !ReferenceEquals(selected.Identity, characterSource.Character.Identity)
            || !ReferenceEquals(
                selected.Identity.Id,
                characterSource.Character.Identity.Id)
            || selected.Pronouns.Subject != "they"
            || !ReferenceEquals(selected.Appearance, source.Options.AppearanceChoices[1])
            || !ReferenceEquals(selected.Background, source.Options.BackgroundChoices[2])
            || !ReferenceEquals(routeSource.CurrentState.PreviousState, initialRoutes)
            || !ReferenceEquals(
                routeSource.CurrentState.SelectedVariation,
                initialRoutes.Variations[2])
            || !ReferenceEquals(
                routeAccessSource.CurrentAccess?.RouteState,
                routeSource.CurrentState)
            || !ReferenceEquals(
                routeAccessSource.CurrentAccess?.SelectedVariation,
                routeSource.CurrentState.SelectedVariation)
            || !ReferenceEquals(
                routeAccessSource.CurrentAccess?.DestinationAnchor,
                routeSource.CurrentState.SelectedVariation?.DestinationAnchor)
            || closedPathSource.CurrentAccess is not null
            || closedGate.GetChild<CollisionShape2D>(0).Disabled
            || !closedGate.GetChild<Line2D>(1).Visible
            || !closedPathSource.ClosedGateMarker.Visible)
        {
            Fail("The complete modal selection did not retain and install exact input.");
            return;
        }

        SeekerCustomizationSelectionState installed = source.CurrentState;
        SeekerBackgroundRouteVariationState installedRoutes = routeSource.CurrentState;
        SeekerBackgroundRouteAccess installedAccess = routeAccessSource.CurrentAccess!;
        Character installedCharacter = characterSource.Character;
        menu._UnhandledInput(Action(OpenAction, pressed: true));
        confirm.EmitSignal(BaseButton.SignalName.Pressed);
        if (menu.Visible
            || !ReferenceEquals(source.CurrentState, installed)
            || !ReferenceEquals(routeSource.CurrentState, installedRoutes)
            || !ReferenceEquals(routeAccessSource.CurrentAccess, installedAccess)
            || !ReferenceEquals(characterSource.Character, installedCharacter)
            || !ReferenceEquals(characterSource.CustomizationApplication, application)
            || !seeker.IsPhysicsProcessing())
        {
            Fail("Repeated menu input changed the installed selection or movement gate.");
            return;
        }

        GD.Print(
            "Seeker customization menu smoke passed: inert hidden startup, released/unrelated "
            + "input rejection, explicit modal opening with movement suspension, complete-field "
            + "validation, exact appearance/background submission, atomic customized-character "
            + "and route-definition application, current route guidance, unchanged non-Forester "
            + "gate, one-shot closure, movement resumption, and repeated-input preservation.");
        GetTree().Quit();
    }

    private static SeekerCustomizationSelectionState Select(
        SeekerCustomizationSelectionState initial,
        CharacterId characterId,
        SeekerCustomizationOptions options) =>
        SeekerCustomizationSelectionStateTransition.Select(
            initial,
            new CharacterIdentity(characterId, "Mara"),
            new SeekerPronouns("they", "them", "their", "theirs", "themself"),
            options.AppearanceChoices[1],
            options.BackgroundChoices[2]);

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
