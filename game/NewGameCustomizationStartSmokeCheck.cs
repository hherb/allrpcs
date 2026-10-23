using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Identifiers;
using Allrpcs.Domain.Persistence;
using Allrpcs.Infrastructure.Persistence;
using Godot;

namespace Allrpcs.Game;

/// <summary>
/// Proves a new game opens Seeker customization before play, and that confirming the Forester
/// background selects that exact profile, keeps the menu from reopening, resumes the Seeker and
/// opens the closed forest path. Confirming also remembers the profile; with
/// <c>ALLRPCS_NEW_GAME_CUSTOMIZATION_MODE=remembered</c> the check plants that file first and proves
/// the launch starts as the remembered Seeker without opening the menu.
/// </summary>
public partial class NewGameCustomizationStartSmokeCheck : Node
{
    private const string CheckEnvironmentVariable = "ALLRPCS_NEW_GAME_CUSTOMIZATION_CHECK";

    /// <inheritdoc />
    public override void _Ready()
    {
        SetProcess(OS.GetEnvironment(CheckEnvironmentVariable) == "1");
        // Main's _Ready, which reads the remembered profile, runs after this child's.
        // A file left by a failed run would make the plain mode skip the menu.
        if (IsProcessing() && !Remembered) { File.Delete(NewGameStartPanelController.RememberedProfilePath()); }
        if (IsProcessing() && Remembered)
        {
            File.WriteAllText(
                NewGameStartPanelController.RememberedProfilePath(),
                RememberedSeekerProfileJson.Serialize(new SeekerCustomizationSaveState(
                    new CharacterIdentity(new CharacterId("seeker"), "Mara"),
                    new SeekerPronouns("they", "them", "their", "theirs", "themself"),
                    "travelling-cloak",
                    "forester")));
        }
    }

    private static bool Remembered => OS.GetEnvironment("ALLRPCS_NEW_GAME_CUSTOMIZATION_MODE") == "remembered";

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
        if (Remembered)
        {
            CheckRememberedStart(menu, seeker, closedPath, closedGate);
            return;
        }

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

        string path = NewGameStartPanelController.RememberedProfilePath();
        if (!File.Exists(path)
            || RememberedSeekerProfileJson.Deserialize(File.ReadAllText(path)) is not { Identity.DisplayName: "Mara", BackgroundKey: "forester" })
        {
            Fail("Confirming did not remember Mara's Forester profile for the next launch.");
            return;
        }

        File.Delete(path);
        GD.Print(
            "New-game customization start smoke passed: a plain launch opened the menu over a "
            + "suspended Seeker with the name field focused, and confirming Forester selected Mara's "
            + "profile, remembered it, closed the menu for good, resumed movement and opened the closed forest path.");
        GetTree().Quit();
    }

    private void CheckRememberedStart(
        SeekerCustomizationMenuController menu,
        Seeker seeker,
        OpeningRegionClosedForestPathSceneSource closedPath,
        StaticBody2D closedGate)
    {
        File.Delete(NewGameStartPanelController.RememberedProfilePath());
        SeekerCustomizationProfile? profile = GetNode<SeekerCustomizationSelectionSceneSource>(
            "../SeekerCustomizationSelectionSceneSource").CurrentState.SelectedProfile;
        if (menu.Visible
            || !seeker.IsPhysicsProcessing()
            || profile?.Identity.DisplayName != "Mara"
            || profile.Pronouns.Objective != "them"
            || profile.Background.Key != "forester"
            || closedPath.CurrentAccess is null
            || !closedGate.GetChild<CollisionShape2D>(0).Disabled)
        {
            Fail("A remembered profile did not start play as Mara the Forester without the menu.");
            return;
        }

        GD.Print(
            "New-game customization start smoke passed (remembered): a plain launch with a remembered "
            + "profile skipped the menu, selected Mara's Forester profile, kept the Seeker moving and "
            + "opened the closed forest path.");
        GetTree().Quit();
    }

    private void Fail(string message)
    {
        GD.PushError(message);
        GetTree().Quit(exitCode: 1);
    }
}
