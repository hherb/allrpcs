using Allrpcs.Domain.Characters;
using Godot;

namespace Allrpcs.Game;

/// <summary>
/// Proves a new game opens Seeker customization before play, and that confirming the Forester
/// background selects that exact profile, keeps the menu from reopening, resumes the Seeker and
/// opens the closed forest path.
/// </summary>
public partial class NewGameCustomizationStartSmokeCheck : Node
{
    private const string CheckEnvironmentVariable = "ALLRPCS_NEW_GAME_CUSTOMIZATION_CHECK";

    /// <inheritdoc />
    public override void _Ready()
    {
        SetProcess(OS.GetEnvironment(CheckEnvironmentVariable) == "1");
    }

    /// <inheritdoc />
    public override void _Process(double delta)
    {
        _ = delta;
        SetProcess(false);
        SeekerCustomizationMenuController menu =
            GetNode<SeekerCustomizationMenuController>("../SeekerCustomizationOverlay/Panel");
        Seeker seeker = GetNode<Seeker>("../Seeker");
        OpeningRegionClosedForestPathSceneSource closedPath =
            GetNode<OpeningRegionClosedForestPathSceneSource>("../OpeningRegionClosedForestPathSceneSource");
        StaticBody2D closedGate = closedPath.GetNode<StaticBody2D>("ClosedGate");
        LineEdit displayName = menu.GetNode<LineEdit>("DisplayName");
        if (!menu.Visible
            || seeker.IsPhysicsProcessing()
            || GetViewport().GuiGetFocusOwner() != displayName
            || closedPath.CurrentAccess is not null
            || closedGate.GetChild<CollisionShape2D>(0).Disabled)
        {
            Fail("A new game did not open customization over a suspended Seeker and closed gate.");
            return;
        }

        displayName.Text = "Mara";
        menu.GetNode<LineEdit>("Subject").Text = "they";
        menu.GetNode<LineEdit>("Objective").Text = "them";
        menu.GetNode<LineEdit>("PossessiveDeterminer").Text = "their";
        menu.GetNode<LineEdit>("PossessivePronoun").Text = "theirs";
        menu.GetNode<LineEdit>("Reflexive").Text = "themself";
        menu.GetNode<OptionButton>("Appearance").Select(1);
        // Item 0 is the disabled prompt; item 1 is the first background, Forester.
        menu.GetNode<OptionButton>("Background").Select(1);
        menu.GetNode<Button>("Confirm").EmitSignal(BaseButton.SignalName.Pressed);
        SeekerCustomizationProfile? profile = GetNode<SeekerCustomizationSelectionSceneSource>(
            "../SeekerCustomizationSelectionSceneSource").CurrentState.SelectedProfile;
        if (profile?.Identity.DisplayName != "Mara"
            || profile.Background.Key != "forester"
            || menu.GetNode<Label>("Status").Text != SeekerCustomizationMenuController.SelectedStatusText
            || menu.TryOpen())
        {
            Fail("Confirming Forester did not select Mara's Forester profile, or the menu could reopen.");
            return;
        }

        if (menu.Visible
            || !seeker.IsPhysicsProcessing()
            || closedPath.CurrentAccess is null
            || !closedGate.GetChild<CollisionShape2D>(0).Disabled
            || closedPath.ClosedGateMarker.Visible)
        {
            Fail("Confirming Forester did not resume the Seeker and open the closed forest path.");
            return;
        }

        GD.Print(
            "New-game customization start smoke passed: a plain launch opened the menu over a "
            + "suspended Seeker with the name field focused, and confirming Forester selected Mara's "
            + "profile, closed the menu for good, resumed movement and opened the closed forest path.");
        GetTree().Quit();
    }

    private void Fail(string message)
    {
        GD.PushError(message);
        GetTree().Quit(exitCode: 1);
    }
}
