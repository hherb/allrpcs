using Godot;

namespace Allrpcs.Game;

/// <summary>Exercises overlapping production modals and successful restore without physical input.</summary>
internal static class ModalInputRecoverySmokeCheck
{
    internal static void Run(Node owner, string mode)
    {
        string[] parts = mode.Split('-');
        bool map = parts[0] == "map";
        CanvasLayer readingPanel = owner.GetNode<CanvasLayer>(map ? "../SeekerMapPanel" : "../JournalSectionPanel");
        bool customizationFirst = parts[1] == "custom";
        bool customizationClosesFirst = parts[2] == "custom";
        bool suspended = parts[3] == "suspended";
        bool restore = parts.Length > 4;
        Seeker seeker = owner.GetNode<Seeker>("../Seeker");
        SeekerCustomizationMenuController menu = owner.GetNode<SeekerCustomizationMenuController>("../SeekerCustomizationOverlay/Panel");
        SeekerMapOpenCloseInputController mapInput = owner.GetNode<SeekerMapOpenCloseInputController>("../SeekerMapOpenCloseInputController");
        JournalOpenCloseInputController journal = owner.GetNode<JournalOpenCloseInputController>("../JournalOpenCloseInputController");
        void ToggleReading()
        {
            if (map) { mapInput._UnhandledInput(Action("map_toggle")); }
            else { journal._UnhandledInput(Action("journal_toggle")); }
        }
        void OpenCustomization() => menu._UnhandledInput(Action("open_seeker_customization"));
        void CloseCustomization()
        {
            foreach ((string name, string text) in new[] { ("DisplayName", "Mara"), ("Subject", "they"),
                ("Objective", "them"), ("PossessiveDeterminer", "their"), ("PossessivePronoun", "theirs"), ("Reflexive", "themself") })
            { menu.GetNode<LineEdit>(name).Text = text; }
            menu.GetNode<OptionButton>("Appearance").Select(1);
            menu.GetNode<OptionButton>("Background").Select(1);
            menu.GetNode<Button>("Confirm").EmitSignal(BaseButton.SignalName.Pressed);
            Require(!menu.Visible, "Customization did not close");
        }
        // Initialize capture prerequisites before choosing the deliberate baseline suspension.
        mapInput._UnhandledInput(Action("map_toggle"));
        mapInput._UnhandledInput(Action("map_toggle"));
        journal._UnhandledInput(Action("journal_toggle"));
        journal._UnhandledInput(Action("journal_toggle"));
        SaveSnapshotJsonController json = owner.GetNode<SaveSnapshotJsonController>("../SaveSnapshotJsonController");
        string snapshot = json.CaptureSerialized() ?? throw new InvalidOperationException("Missing complete capture");
        seeker.SetPhysicsProcess(!suspended);
        if (customizationFirst) { OpenCustomization(); ToggleReading(); }
        else { ToggleReading(); OpenCustomization(); }
        Require(menu.Visible && readingPanel.Visible && !seeker.IsPhysicsProcessing(), "Overlapping modals failed to suspend");
        if (restore)
        {
            Require(json.TryRestoreSerialized(snapshot), "Successful restore rejected");
            Require(menu.Visible && !readingPanel.Visible && !seeker.IsPhysicsProcessing(), "Restore resumed movement beneath customization");
            CloseCustomization();
        }
        else
        {
            if (customizationClosesFirst) { CloseCustomization(); } else { ToggleReading(); }
            Require(!seeker.IsPhysicsProcessing(), "First modal close resumed movement beneath remaining modal");
            if (customizationClosesFirst) { ToggleReading(); } else { CloseCustomization(); }
        }
        Require(!menu.Visible && !readingPanel.Visible, "Modal surface remained open");
        Require(seeker.IsPhysicsProcessing() == !suspended, "Last close lost baseline suspension");
        if (!suspended)
        {
            Require(ControlInputContext.WorldAllowed(owner), "World input failed to recover");
            Input.ActionPress("move_right");
            try
            {
                seeker._PhysicsProcess(1.0 / 60);
                Require(seeker.Velocity.X > 0, "Movement did not resume after final close");
            }
            finally { Input.ActionRelease("move_right"); }
        }
        GD.Print("PASS: modal input recovery " + mode);
        owner.GetTree().Quit();
    }

    private static InputEventAction Action(string name) => new() { Action = name, Pressed = true };
    private static void Require(bool value, string message)
    { if (!value) { throw new InvalidOperationException(message); } }
}
