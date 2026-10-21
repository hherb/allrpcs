using Allrpcs.Application.Navigation;
using Godot;

namespace Allrpcs.Game;

/// <summary>Shared steps for flow checks that drive a real Main through slice play.</summary>
internal static class MainFlowCheckSteps
{
    // A saved profile lets Continue resume play instead of reopening customization.
    internal static void Customize(Node main)
    {
        SeekerCustomizationMenuController menu =
            main.GetNode<SeekerCustomizationMenuController>("SeekerCustomizationOverlay/Panel");
        Require(menu.TryOpen(), "The first session opened customization.");
        menu.GetNode<LineEdit>("DisplayName").Text = "Ren";
        menu.GetNode<LineEdit>("Subject").Text = "they";
        menu.GetNode<LineEdit>("Objective").Text = "them";
        menu.GetNode<LineEdit>("PossessiveDeterminer").Text = "their";
        menu.GetNode<LineEdit>("PossessivePronoun").Text = "theirs";
        menu.GetNode<LineEdit>("Reflexive").Text = "themself";
        menu.GetNode<OptionButton>("Appearance").Select(1);
        menu.GetNode<OptionButton>("Background").Select(1);
        menu.GetNode<Button>("Confirm").EmitSignal(BaseButton.SignalName.Pressed);
        Require(!menu.Visible, "The first session customized the Seeker.");
    }

    // Start, four steps, completion and the batch: seven presses at the station.
    internal static void PrepareAtStation(Node main, int presses)
    {
        OpeningRegionRemedyPreparationSceneBinding station = main.GetNode<OpeningRegionRemedyPreparationSceneSource>(
            "OpeningRegionRemedyPreparationSceneSource").Binding!;
        main.GetNode<Node2D>("Seeker").GlobalPosition = new(station.StationPosition.X, station.StationPosition.Y);
        for (int index = 0; index < presses; index++)
        {
            Press(main, "OpeningRegionRemedyPreparationInputCoordinator", "remedy_prepare");
        }
    }

    internal static void Administer(Node main)
    {
        OpeningRegionFirstPatientSceneBinding patient = main.GetNode<OpeningRegionFirstPatientSceneSource>(
            "OpeningRegionFirstPatientSceneSource").Binding!;
        main.GetNode<Node2D>("Seeker").GlobalPosition = new(patient.PatientPosition.X, patient.PatientPosition.Y);
        Press(main, "OpeningRegionFirstPatientAdministrationInputCoordinator", "remedy_administer");
    }

    internal static void Press(Node main, string coordinator, string action) =>
        main.GetNode<Node>(coordinator)._UnhandledInput(new InputEventAction { Action = action, Pressed = true });

    internal static void InitializeMap(Node main)
    {
        SeekerMapOpenCloseInputController map = main.GetNode<SeekerMapOpenCloseInputController>("SeekerMapOpenCloseInputController");
        map._UnhandledInput(new InputEventAction { Action = "map_toggle", Pressed = true });
        map._UnhandledInput(new InputEventAction { Action = "map_toggle", Pressed = true });
        Require(main.GetNode<SeekerMapController>("SeekerMapController").Map is not null, "The first session initialized the map.");
    }

    internal static void Require(bool condition, string message)
    {
        if (!condition) { throw new InvalidOperationException("Failed: " + message); }
    }
}
