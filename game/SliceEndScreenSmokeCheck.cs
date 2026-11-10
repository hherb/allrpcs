using Allrpcs.Application.Diseases;
using Allrpcs.Application.Input;
using Allrpcs.Application.Persistence;
using Allrpcs.Infrastructure.Persistence;
using Godot;
using static Allrpcs.Game.MainFlowCheckSteps;

namespace Allrpcs.Game;

/// <summary>Exercises ending input, same-session load and a real Main reload with disposable saves.</summary>
public partial class SliceEndScreenSmokeCheck : Node
{
    internal const string EnvironmentVariable = "ALLRPCS_SLICE_END_SCREEN_CHECK";
    private static bool returningToTitle;

    public override async void _Ready()
    {
        if (DebugTestEnvironment.GetEnvironment(EnvironmentVariable) != "1") { return; }
        SceneTree tree = GetTree();
        string savePath = SaveDocumentStorageController.CurrentFilePath();
        string profilePath = NewGameStartPanelController.RememberedProfilePath();
        Node main = GetParent();
        try
        {
            Engine.MaxFps = 60;
            if (!returningToTitle)
            {
                DeleteSave(savePath);
                File.Delete(profilePath);
            }

            // Main initializes the normal launch flow after its hosted checks are readied.
            await Frames(4);
            Require(tree.CurrentScene == main, "The check runs inside the current Main scene.");
            Require(main.GetNode<ControlSettingsController>("ControlSettingsController").Apply(ControlPreferences.Defaults()),
                "Installed the default test bindings in memory.");
            if (returningToTitle)
            {
                await CheckTitle(main);
                DeleteSave(savePath);
                File.Delete(profilePath);
                GD.Print("PASS: slice ending input, restoration and Return to title");
                tree.Quit();
                return;
            }

            Require(main.GetNode<Control>("SeekerCustomizationOverlay/Panel").Visible,
                "The first Main opened fresh customization.");
            Customize(main);
            await Frames(2);
            SaveDocumentStorageController storage = main.GetNode<SaveDocumentStorageController>("SaveDocumentStorageController");
            Button save = main.GetNode<Button>("CommandOverlay/SaveLoadPanel/Save");
            save.EmitSignal(BaseButton.SignalName.Pressed);
            save.EmitSignal(BaseButton.SignalName.Pressed);
            string saved = File.ReadAllText(savePath);
            string backupPath = FileSaveDocumentStore.BackupPathFor(savePath);
            string backup = File.ReadAllText(backupPath);

            SliceEndPanelController panel = main.GetNode<SliceEndPanelController>("SliceEndOverlay/Panel");
            Button title = panel.GetNode<Button>("Box/ReturnToTitle");
            await Lose(main, panel);
            Require(GetViewport().GuiGetFocusOwner() == title, "Loss focuses Return to title.");
            foreach (Key key in new[] { Key.Left, Key.Right, Key.Up, Key.Down, Key.Tab })
            {
                await Tap(key);
                Require(GetViewport().GuiGetFocusOwner() == title, $"Loss keeps focus inside the panel after {key}.");
            }
            await Tap(Key.Tab, shift: true);
            Require(GetViewport().GuiGetFocusOwner() == title, "Shift+Tab stays on Return to title.");

            CanvasLayer dialogue = main.GetNode<CanvasLayer>("DialogueOverlay");
            int originalLayer = dialogue.Layer;
            SpaceHoldSpeechController space = main.GetNode<SpaceHoldSpeechController>("SpaceHoldSpeechController");
            int holds = space.Holds;
            KeyEvent(Key.Space, true);
            await ToSignal(tree.CreateTimer((TapHoldGesture.HoldMilliseconds + 100) / 1000), SceneTreeTimer.SignalName.Timeout);
            Require(!dialogue.Visible && dialogue.Layer == originalLayer && space.Holds == holds,
                "Holding Space leaves the ending in front and does not begin dialogue.");
            KeyEvent(Key.Space, false);
            await Frames(2);
            Require(panel.Visible && GetViewport().GuiGetFocusOwner() == title,
                "Releasing a held Space does not activate the focused button.");

            // Guard the Save callback even if an activation reached the background control.
            save.EmitSignal(BaseButton.SignalName.Pressed);
            Require(File.ReadAllText(savePath) == saved && File.ReadAllText(backupPath) == backup,
                "The ending leaves the in-progress save and its backup intact.");
            main.GetNode<Button>("CommandOverlay/SaveLoadPanel/Load").EmitSignal(BaseButton.SignalName.Pressed);
            Require(panel.Visible, "The background Load button does not dismiss the ending.");

            Require(storage.LoadCurrent() == SaveDocumentReadStatus.Loaded, "Restored the in-progress save in the same Main.");
            await Frames(2);
            Require(!panel.Visible && panel.Shown is null && main.GetNode<Seeker>("Seeker").IsPhysicsProcessing()
                && ControlInputContext.WorldAllowed(main), "Restore clears the ending and releases the Seeker.");

            await Lose(main, panel);
            returningToTitle = true;
            // Tap Space must still activate the focused button. The fresh hosted check finishes the run.
            KeyEvent(Key.Space, true);
            await Frames(2);
            KeyEvent(Key.Space, false);
        }
        catch (Exception error)
        {
            GD.PushError(error.ToString());
            DeleteSave(savePath);
            File.Delete(profilePath);
            tree.Quit(1);
        }
    }

    private async Task Lose(Node main, SliceEndPanelController panel)
    {
        for (int i = 0; i < 2; i++)
        {
            PrepareAtStation(main, 7);
            Administer(main);
            await Frames(2);
        }
        Require(panel.Shown == FirstPatientSliceOutcome.LostAfterDoses, "Two wrong doses opened the loss panel.");
    }

    private async Task CheckTitle(Node main)
    {
        NewGameStartPanelController start = main.GetNode<NewGameStartPanelController>("SeekerCustomizationOverlay/StartPanel");
        Button resume = start.GetNode<Button>("Continue");
        Require(start.Visible && resume.Text == "CONTINUE" && start.GetNode<Button>("NewGame").Visible
            && GetViewport().GuiGetFocusOwner() == resume, "A fresh Main offers Continue and New game with Continue focused.");
        await Tap(Key.Enter);
        Require(!start.Visible, "Continue closes the title: " + start.GetNode<Label>("Status").Text);
        Require(!main.GetNode<CanvasLayer>("DialogueOverlay").Visible && !main.GetNode<Control>("SliceEndOverlay/Panel").Visible
            && main.GetNode<FirstPatientStakesController>("FirstPatientStakesController").Outcome == FirstPatientSliceOutcome.InProgress,
            "Continue restores the in-progress journey.");
        Require(main.GetNode<Seeker>("Seeker").IsPhysicsProcessing(), "Continue releases the Seeker.");
    }

    private async Task Tap(Key key, bool shift = false)
    {
        KeyEvent(key, true, shift);
        await Frames(1);
        KeyEvent(key, false, shift);
        await Frames(2);
    }

    private static void KeyEvent(Key key, bool pressed, bool shift = false) =>
        Input.ParseInputEvent(new InputEventKey { Keycode = key, PhysicalKeycode = key, Pressed = pressed, ShiftPressed = shift });

    private async Task Frames(int count)
    {
        for (int i = 0; i < count; i++) { await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame); }
    }
}
