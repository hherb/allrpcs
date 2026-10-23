using System.Text.Json;
using Allrpcs.Application.Input;
using Allrpcs.Infrastructure.Input;
using Godot;

namespace Allrpcs.Game;

/// <summary>Opt-in genuine Main controls fixture using synthetic events with explicit provenance.</summary>
public partial class ControlSettingsMainSmokeCheck : Node
{
    private readonly List<string> passed = [];
    private ControlSettingsController controller = null!;
    private ControlSettingsPanel panel = null!;
    private ControlInputMapAdapter adapter = null!;
    private ControlInputContext context = null!;
    private Seeker seeker = null!;
    private ControlPreferences defaults = null!;
    private string mode = "headless";
    /// <inheritdoc />
    public override async void _Ready()
    {
        if (OS.GetEnvironment("ALLRPCS_CONTROLS_MAIN_CHECK") != "1") { return; }
        mode = OS.GetEnvironment("ALLRPCS_CONTROLS_MODE");
        if (string.IsNullOrEmpty(mode)) { mode = "headless"; }
        await Frames(3);
        if (mode.StartsWith("native", StringComparison.Ordinal)) { Engine.MaxFps = 60; GetWindow().Size = mode == "native-1920" ? new(1920, 1080) : new(1280, 720); GetWindow().GrabFocus(); await Frames(8); }
        try
        {
            controller = GetNode<ControlSettingsController>("../ControlSettingsController");
            panel = GetNode<ControlSettingsPanel>("../ControlSettingsPanel");
            adapter = GetNode<ControlInputMapAdapter>("../ControlInputMapAdapter");
            context = GetNode<ControlInputContext>("../ControlInputContext"); seeker = GetNode<Seeker>("../Seeker");
            defaults = ControlPreferences.Defaults();
            Require(ControlSettingsController.Same(defaults, controller.Preferences), "Initial controls not complete defaults");
            await CheckAdapter(); await CheckMovement(); await CheckRouting(); await CheckPanel(); await CheckCapture(); await CheckContexts();
            await CheckContextualPanels(); await CheckPersistence();
            GD.Print("Controls product checks complete: " + passed.Count);
            if (mode.StartsWith("native", StringComparison.Ordinal)) { await NativeCapture(); }
            string report = OS.GetEnvironment("ALLRPCS_CONTROLS_REPORT");
            if (!string.IsNullOrEmpty(report))
            {
                File.WriteAllText(report, JsonSerializer.Serialize(new
                { status = "passed", mode, actions = 29, checks = passed, provenance = "synthetic Godot input events and actual Main owners; no physical controller qualification", engine = Engine.GetVersionInfo()["string"].AsString() }));
            }
            GD.Print("PASS: controls Main: " + mode + " · " + passed.Count + " checks"); GetTree().Quit();
        }
        catch (Exception error) { GD.PrintErr("FAIL: controls Main: " + error); GetTree().Quit(1); }
        finally { File.Delete(Path.Combine(Path.GetTempPath(), $"allrpcs-controls-{OS.GetProcessId()}.json")); }
    }
    private async Task CheckAdapter()
    {
        StringName unrelated = new("controls_test_unrelated"); InputMap.AddAction(unrelated);
        InputMap.ActionAddEvent(unrelated, new InputEventKey { PhysicalKeycode = Key.F24 });
        string before = MapSnapshot(); adapter.FailAfterAction = 3;
        ControlPreferences changed = ControlBindingEdit.Set(defaults, "save_current", new("mouseButton", 8));
        Require(!adapter.Apply(changed, out _) && MapSnapshot() == before, "Adapter rollback changed prior events");
        adapter.FailAfterAction = null;
        Require(adapter.Apply(changed, out _), "Adapter installation failed");
        Require(InputMap.ActionHasEvent(unrelated, new InputEventKey { PhysicalKeycode = Key.F24 }), "Unrelated action deleted");
        Require(controller.Apply(defaults), "Defaults could not be restored"); InputMap.EraseAction(unrelated);
        foreach (ControlActionDefinition action in ControlActionCatalog.Actions)
        {
            foreach (ControlBinding binding in action.Defaults)
            {
                using InputEvent value = ControlInputMapAdapter.ToEvent(binding);
                if (value is InputEventJoypadMotion axis) { axis.AxisValue = binding.Sign; }
                else if (value is InputEventKey key) { key.Pressed = true; }
                else if (value is InputEventJoypadButton button) { button.Pressed = true; }
                Require(ControlInputMatcher.Pressed(value, action.Id), "Default binding not matched: " + action.Id + "/" + binding.Identity);
            }
        }
        InputEventKey modified = KeyEvent(Key.Q, true, ctrl: true);
        Require(!ControlInputMatcher.Pressed(modified, "save_current"), "Modifier superset admitted save");
        modified.Pressed = false; Require(ControlInputMatcher.Released(modified, "save_current"), "Release lost after modifier change");
        Require(controller.Save(defaults), "Fixture default save failed");
        ControlPreferences layoutConflict = ControlBindingEdit.Set(defaults, "save_current", new("key", (int)Key.F12, "logical"), 0);
        var storage = GetNode<ControlSettingsStorageController>("../ControlSettingsStorageController");
        Require(storage.Save(layoutConflict), "Fixture layout document save failed");
        adapter.LayoutResolverForCheck = key => key == Key.A ? Key.F12 : key;
        controller._Ready();
        Require(ControlSettingsController.Same(controller.Preferences, defaults) && MapSnapshot() == before,
            "Startup layout rejection did not install complete default events");
        Require(ControlSettingsController.Same(storage.Load().Preferences, layoutConflict), "Startup rejection rewrote the document");
        adapter.LayoutResolverForCheck = null; Require(storage.Save(defaults), "Fixture storage reset failed");
        passed.Add("29-action defaults, exact modifiers, unrelated action retention, rollback and layout-rejected startup defaults"); await Frames(1);
    }
    private async Task CheckMovement()
    {
        Vector2 retained = seeker.Position;
        if (mode.StartsWith("native", StringComparison.Ordinal)) { GetWindow().GrabFocus(); await Frames(6); }
        seeker.GlobalPosition = new Vector2(1800, 500); seeker.Velocity = Vector2.Zero; await PhysicsFrames(3);
        foreach (var item in new[] { (Key.A, new Vector2(-1, 0)), (Key.D, new Vector2(1, 0)), (Key.W, new Vector2(0, -1)), (Key.S, new Vector2(0, 1)) })
        {
            Vector2 start = seeker.Position; Input.ParseInputEvent(KeyEvent(item.Item1, true)); await PhysicsFrames(4);
            Require((seeker.Position - start).Dot(item.Item2) > 1, "Physical movement direction not routed: " + item.Item1);
            Require(Math.Abs(seeker.Velocity.Length() - 240) < 0.1, "Cardinal speed changed");
            Input.ParseInputEvent(KeyEvent(item.Item1, false)); await PhysicsFrames(2);
        }
        Input.ParseInputEvent(KeyEvent(Key.W, true)); Input.ParseInputEvent(KeyEvent(Key.D, true)); await PhysicsFrames(4);
        Require(Math.Abs(seeker.Velocity.Length() - 240) < 0.1, "Diagonal movement not normalized: velocity=" + seeker.Velocity + ", position=" + seeker.GlobalPosition + ", windowFocus=" + GetWindow().HasFocus());
        Input.ParseInputEvent(KeyEvent(Key.W, false)); Input.ParseInputEvent(KeyEvent(Key.D, false)); await PhysicsFrames(2);
        foreach (var item in new[] { (JoyAxis.LeftX, -1f, new Vector2(-1, 0)), (JoyAxis.LeftX, 1f, new Vector2(1, 0)), (JoyAxis.LeftY, -1f, new Vector2(0, -1)), (JoyAxis.LeftY, 1f, new Vector2(0, 1)) })
        {
            Vector2 start = seeker.Position; Input.ParseInputEvent(new InputEventJoypadMotion { Device = 7, Axis = item.Item1, AxisValue = item.Item2 }); await PhysicsFrames(4);
            Require((seeker.Position - start).Dot(item.Item3) > 1, "Signed controller movement not routed");
            Input.ParseInputEvent(new InputEventJoypadMotion { Device = 7, Axis = item.Item1, AxisValue = 0 }); await PhysicsFrames(2);
        }
        seeker.Position = retained; seeker.Velocity = Vector2.Zero; passed.Add("four physical-key/signed-axis directions and unchanged normalized 240px/s movement");
    }
    private async Task CheckRouting()
    {
        Main main = GetParent<Main>(); Healer healer = GetNode<Healer>("../Healer");
        var prior = healer.CommandMode;
        ControlPreferences remapped = ControlBindingEdit.Set(defaults, "healer_toggle_follow_wait", new("key", (int)Key.F12, "physical"), 0);
        Require(controller.Apply(remapped), "Remap failed");
        main._UnhandledInput(KeyEvent(Key.X, true)); Require(healer.CommandMode == prior, "Old key remained active");
        await Press(Key.F12); Require(healer.CommandMode != prior, "New key did not reach actual Main command");
        await Press(Key.F12); Require(healer.CommandMode == prior, "Command did not toggle back");
        remapped = ControlBindingEdit.Set(defaults, "healer_toggle_follow_wait", new("mouseButton", 8), 0); controller.Apply(remapped);
        Input.ParseInputEvent(new InputEventMouseButton { ButtonIndex = MouseButton.Xbutton1, Pressed = true, Position = new Vector2(800, 550) }); await Frames(2);
        Require(healer.CommandMode != prior, "Mouse command not routed");
        Input.ParseInputEvent(new InputEventMouseButton { ButtonIndex = MouseButton.Xbutton1, Pressed = false, Position = new Vector2(800, 550) }); await Frames(1);
        remapped = ControlBindingEdit.Set(defaults, "healer_toggle_follow_wait", new("joyButton", 20), 0); controller.Apply(remapped);
        Input.ParseInputEvent(new InputEventJoypadButton { Device = 7, ButtonIndex = (JoyButton)20, Pressed = true }); await Frames(2);
        Require(healer.CommandMode == prior, "Controller command not routed");
        Input.ParseInputEvent(new InputEventJoypadButton { Device = 7, ButtonIndex = (JoyButton)20, Pressed = false }); await Frames(1);
        controller.Apply(defaults); passed.Add("keyboard/mouse/controller remaps through actual Main Healer owner");
        SaveLoadInputController save = GetNode<SaveLoadInputController>("../SaveLoadInputController");
        save._UnhandledInput(KeyEvent(Key.Q, true));
        Require(GetNode<Label>("../CommandOverlay/SaveLoadPanel/Status").Text == SaveLoadInputController.SavedStatusText
            && File.Exists(SaveDocumentStorageController.CurrentFilePath()), "Save binding did not save");
        File.Delete(SaveDocumentStorageController.CurrentFilePath());
        ControlPreferences newSave = ControlBindingEdit.Set(defaults, "save_current", new("key", (int)Key.F12, "physical"), 0); controller.Apply(newSave);
        Require(GetNode<Label>("../CommandOverlay/Panel/RecoveryStatus").Text.Contains("F12", StringComparison.Ordinal), "Remapped recovery prompt stayed stale");
        Require(GetNode<Button>("../CommandOverlay/SaveLoadPanel/Save").Text.Contains("F12", StringComparison.Ordinal), "Save button binding stale");
        controller.Apply(defaults); passed.Add("existing save prerequisite retained and live prompts updated");
    }
    private async Task CheckPanel()
    {
        await Press(Key.Tab);
        Button entry = GetNode<Button>("../SettingsOverlay/ControlsEntry");
        Require(entry.IsVisibleInTree() && entry.HasFocus(), "Controls entry not reachable by keyboard"); await Press(Key.Enter); await Frames(2);
        Require(panel.Visible && panel.GetNode<ItemList>("Shade/Panel/Content/Columns/Actions").HasFocus(), "Panel initial focus missing");
        ItemList list = panel.GetNode<ItemList>("Shade/Panel/Content/Columns/Actions");
        Require(list.ItemCount == 29, "Rows incomplete");
        await Press(Key.Tab); Require(panel.Visible && GetViewport().GuiGetFocusOwner() != list, "Tab closed instead of traversed Controls");
        await Press(Key.Tab, shift: true); Require(panel.Visible && list.HasFocus(), "Shift-Tab failed");
        panel.SelectAction("ui_focus_prev"); await Frames(1); Require(list.GetSelectedItems().Single() == 28, "Last row unreachable");
        panel.SelectAction("save_current");
        ControlPreferences draft = ControlBindingEdit.Set(defaults, "save_current", new("mouseButton", 8)); panel.SetDraft(draft);
        panel.Close(); Require(panel.Visible, "Unapplied draft silently discarded");
        Button discard = panel.GetNode<Button>("Shade/Panel/Content/Confirmation/Discard"); discard.EmitSignal(BaseButton.SignalName.Pressed); await Frames(1);
        Require(!panel.Visible && entry.HasFocus(), "Discard/return focus failed");
        await Press(Key.Tab); await Frames(2); Require(ControlInputContext.WorldAllowed(this), "Overlay close left world blocked");
        Input.ParseInputEvent(new InputEventJoypadButton { Device = 7, ButtonIndex = JoyButton.Start, Pressed = true }); await Frames(2);
        Input.ParseInputEvent(new InputEventJoypadButton { Device = 7, ButtonIndex = JoyButton.Start, Pressed = false }); await Frames(1);
        Require(entry.HasFocus(), "Controller Start did not reach Controls entry");
        Input.ParseInputEvent(new InputEventJoypadButton { Device = 7, ButtonIndex = JoyButton.A, Pressed = true }); await Frames(2);
        Input.ParseInputEvent(new InputEventJoypadButton { Device = 7, ButtonIndex = JoyButton.A, Pressed = false }); await Frames(1);
        Require(panel.Visible, "Controller accept did not open Controls"); panel.Close(false); await Frames(2);
        passed.Add("keyboard/controller entry, 29-row scrolling, Tab/Shift-Tab, draft discard and return focus");
    }
    private async Task CheckCapture()
    {
        panel.Open(); panel.SelectAction("save_current"); string original = ControlPreferencesJson.Serialize(panel.Draft);
        panel.BeginCapture("Keyboard", 0); await Frames(2);
        await Press(Key.Q); Require(!panel.Capture.Active && panel.Draft.Get("save_current").Bindings.Single().Code == (int)Key.Q, "Keyboard capture not completed");
        Require(!File.Exists(SaveDocumentStorageController.CurrentFilePath()), "Captured save leaked into gameplay");
        panel.BeginCapture("Mouse"); await Frames(2);
        Input.ParseInputEvent(new InputEventMouseButton { ButtonIndex = MouseButton.Xbutton1, Pressed = true, Position = new Vector2(800, 550) }); await Frames(2);
        Input.ParseInputEvent(new InputEventMouseButton { ButtonIndex = MouseButton.Xbutton1, Pressed = false, Position = new Vector2(800, 550) }); await Frames(1);
        Require(panel.Draft.Get("save_current").Bindings.Any(b => b.Kind == "mouseButton" && b.Code == 8), "Mouse capture missing");
        panel.BeginCapture("Controller"); await Frames(2);
        Input.ParseInputEvent(new InputEventJoypadButton { Device = 7, ButtonIndex = (JoyButton)20, Pressed = true }); await Frames(2);
        Input.ParseInputEvent(new InputEventJoypadButton { Device = 7, ButtonIndex = (JoyButton)20, Pressed = false }); await Frames(1);
        Require(panel.Draft.Get("save_current").Bindings.Any(b => b.Kind == "joyButton" && b.Code == 20), "Controller capture missing");
        panel.BeginCapture("Keyboard", 0); await Frames(2); await Press(Key.L);
        Require(panel.Draft.Get("save_current").Bindings.Any(b => b.Kind == "key" && b.Code == 81), "Conflicting capture mutated draft");
        Require(panel.GetNode<Label>("Shade/Panel/Content/Status").Text.Contains("conflict", StringComparison.OrdinalIgnoreCase), "Conflict not explained");
        panel.SelectAction("move_left"); panel.BeginCapture("Controller"); await Frames(2);
        Input.ParseInputEvent(new InputEventJoypadMotion { Device = 7, Axis = JoyAxis.RightX, AxisValue = 0.9f }); await Frames(1);
        Require(panel.Capture.Active, "Held axis captured before neutral");
        Input.ParseInputEvent(new InputEventJoypadMotion { Device = 7, Axis = JoyAxis.RightX, AxisValue = 0 }); await Frames(1);
        Input.ParseInputEvent(new InputEventJoypadMotion { Device = 7, Axis = JoyAxis.RightX, AxisValue = -0.8f }); await Frames(2);
        Input.ParseInputEvent(new InputEventJoypadMotion { Device = 7, Axis = JoyAxis.RightX, AxisValue = 0 }); await Frames(1);
        Require(panel.Draft.Get("move_left").Bindings.Any(b => b.Kind == "joyAxis" && b.Code == 2 && b.Sign == -1), "Axis crossing not captured");
        panel.BeginCapture("Keyboard"); await Frames(2); panel.Capture.Advance(10); panel._Process(0);
        Require(!panel.Capture.Active, "Capture timeout missing");
        panel.BeginCapture("Keyboard"); await Frames(2); await Press(Key.Escape); Require(!panel.Capture.Active && panel.Visible, "Escape failed to cancel capture");
        panel.BeginCapture("Keyboard"); panel._Notification((int)NotificationApplicationFocusOut); Require(!panel.Capture.Active, "Focus loss left capture active");
        panel.SetDraft(defaults); panel.Close(false); await Frames(2);
        Require(ControlInputContext.WorldAllowed(this), "Capture close left release fence stuck");
        passed.Add("three device classes, conflict rejection, axis neutral/crossing, timeout, Escape and focus loss");
    }
    private async Task CheckContexts()
    {
        Vector2 start = seeker.Position;
        Healer autonomous = GetNode<Healer>("../Healer");
        Require(GetParent().IsProcessing() && autonomous.IsPhysicsProcessing() && !GetTree().Paused, "Fixture simulation not active before modal checks");
        panel.Open(); Input.ActionPress("move_right"); await PhysicsFrames(5);
        Require(seeker.Position.DistanceTo(start) < 0.01, "World moved during Controls");
        Require(GetParent().IsProcessing() && autonomous.IsPhysicsProcessing() && !GetTree().Paused, "Controls paused world/NPC simulation"); Input.ActionRelease("move_right");
        Main main = GetParent<Main>(); Healer healer = GetNode<Healer>("../Healer"); var modeBefore = healer.CommandMode;
        main._UnhandledInput(new InputEventAction { Action = "healer_toggle_follow_wait", Pressed = true }); Require(healer.CommandMode == modeBefore, "World command escaped Controls");
        panel.Close(false); await Frames(2);
        LineEdit field = GetNode<LineEdit>("../DialogueOverlay/Panel/Input");
        await Press(Key.Tab); field.GrabFocus(); Input.ActionPress("move_right"); await PhysicsFrames(5);
        Require(seeker.Position.DistanceTo(start) < 0.01, "World moved during text entry"); Input.ActionRelease("move_right");
        main._UnhandledInput(KeyEvent(Key.X, true)); Require(healer.CommandMode == modeBefore, "World command escaped text focus");
        await Press(Key.Tab); await Frames(2);
        Input.ParseInputEvent(KeyEvent(Key.D, true)); await PhysicsFrames(3); Input.ParseInputEvent(KeyEvent(Key.D, false)); await Frames(2);
        Require(seeker.Position.DistanceTo(start) > 1, "Ordinary movement not restored");
        panel.Open(); Input.ParseInputEvent(KeyEvent(Key.D, true)); await Frames(2);
        Button close = (Button)panel.FindChild("Close", true, false);
        await Pointer(close.GetGlobalRect().GetCenter());
        Require(!panel.Visible, "Held gameplay input prevented native Close");
        await Press(Key.Tab); await PhysicsFrames(4);
        Vector2 frozen = seeker.Position; await PhysicsFrames(4); Require(seeker.Position.DistanceTo(frozen) < 0.01, "Held captured key resumed after close");
        Input.ParseInputEvent(KeyEvent(Key.D, false)); await Frames(2);
        context._Notification((int)NotificationApplicationFocusOut); Input.ActionPress("move_right"); await PhysicsFrames(3);
        Require(seeker.Position.DistanceTo(frozen) < 0.01, "Movement escaped lost focus"); Input.ActionRelease("move_right");
        context._Notification((int)NotificationApplicationFocusIn); context.Disconnect(7); await Frames(2);
        Require(ControlInputContext.WorldAllowed(this), "Focus/disconnect recovery failed");
        passed.Add("modal/text/focus movement and command gates, held-key release fence, recovery");
    }
    private async Task CheckContextualPanels()
    {
        // Match the accepted owner fixtures' deterministic positive-case arrangement.
        // The earlier modal checks run with normal simulation and prove its flags stay enabled.
        Node simulation = GetParent(); Node completion = GetNode<Node>("../TemporaryCharacterTurnCompletionController");
        Healer npc = GetNode<Healer>("../Healer");
        bool simulationWas = simulation.IsProcessing(), completionWas = completion.IsProcessing(), npcWas = npc.IsPhysicsProcessing();
        Vector2 retained = seeker.GlobalPosition;
        simulation.SetProcess(false); completion.SetProcess(false); npc.SetPhysicsProcess(false);
        try
        {
            if (mode.StartsWith("native", StringComparison.Ordinal)) { GetWindow().GrabFocus(); await Frames(10); }
            await Press(Key.V); // Existing player cancellation, never a forged turn/admission state.
            var water = GetNode<OpeningRegionStreamWaterContactCoordinator>("../OpeningRegionStreamWaterContactCoordinator");
            var cask = GetNode<OpeningRegionCaskCoverHandlingCoordinator>("../OpeningRegionCaskCoverHandlingCoordinator");
            var tool = GetNode<OpeningRegionHandToolTapCoordinator>("../OpeningRegionHandToolTapCoordinator");
            await Contextual("StreamWaterContactPanel", "TouchWater", new(1756, 484), () => water.CommitCount, water.ResetPresentation, water._Process);
            await Contextual("CaskCoverHandlingPanel", "AdjustCaskCover", new(2168, 900), () => cask.CommitCount, cask.ResetPresentation, cask._Process);
            await Contextual("HandToolTapPanel", "TapPracticeBlock", new(1732, 900), () => tool.CommitCount, tool.ResetPresentation, tool._Process);
            passed.Add("three qualified contextual owners with unrelated NPC ticking isolated: keyboard/controller once per press, held suppression, native pointer and Controls navigation isolation");
        }
        finally
        {
            seeker.GlobalPosition = retained; simulation.SetProcess(simulationWas);
            completion.SetProcess(completionWas); npc.SetPhysicsProcess(npcWas);
        }
        await Frames(8);
    }
    private async Task Contextual(string name, string buttonName, Vector2 position, Func<long> count, Action reset, Action<double> advance)
    {
        if (mode.StartsWith("native", StringComparison.Ordinal)) { GetWindow().GrabFocus(); await Frames(10); Require(GetWindow().HasFocus(), "Native window focus unavailable"); }
        seeker.GlobalPosition = position; await Frames(8); reset(); await Frames(2);
        CanvasLayer view = GetNode<CanvasLayer>("../" + name);
        Button button = (Button)view.FindChild(buttonName, true, false);
        Require(view.Visible && !button.Disabled, "Contextual owner not qualified: " + name + ", windowFocus=" + GetWindow().HasFocus() + ", position=" + seeker.GlobalPosition + ", physics=" + seeker.IsPhysicsProcessing() + ", GUI=" + GetViewport().GuiGetFocusOwner()?.Name);
        panel.Open(); await Press(Key.Tab);
        Require(panel.Visible && !button.HasFocus(), "Contextual owner stole Controls navigation: " + name);
        panel.Close(false); await Frames(2);
        foreach (bool joy in new[] { false, true })
        {
            reset(); await Frames(2); button.GrabFocus(); long before = count();
            InputEvent down = joy ? new InputEventJoypadButton { Device = 7, ButtonIndex = JoyButton.A, Pressed = true } : KeyEvent(Key.Enter, true);
            Input.ParseInputEvent(down); await Frames(2); Require(count() == before + 1, "Contextual activation failed: " + name + ", joy=" + joy + ", windowFocus=" + GetWindow().HasFocus() + ", buttonFocus=" + button.HasFocus() + ", disabled=" + button.Disabled + ", turn=" + GetNode<CharacterTurnActivityController>("../CharacterTurnActivityController").Activity?.State);
            for (int tick = 0; tick < 20; tick++) { advance(0.25); }
            Input.ParseInputEvent(down); await Frames(2); Require(count() == before + 1, "Held activation duplicated: " + name);
            if (down is InputEventJoypadButton joyDown) { joyDown.Pressed = false; } else { ((InputEventKey)down).Pressed = false; }
            Input.ParseInputEvent(down); await Frames(2);
            if (down is InputEventJoypadButton joyUp) { joyUp.Pressed = true; } else { ((InputEventKey)down).Pressed = true; }
            Input.ParseInputEvent(down); await Frames(2); Require(count() == before + 2, "Released activation failed: " + name);
            if (down is InputEventJoypadButton joyLast) { joyLast.Pressed = false; } else { ((InputEventKey)down).Pressed = false; }
            Input.ParseInputEvent(down); await Frames(2); button.ReleaseFocus();
        }
        reset(); await Frames(2); long pointerBefore = count();
        Vector2 center = GetViewport().GetFinalTransform() * button.GetGlobalRect().GetCenter();
        Input.ParseInputEvent(new InputEventMouseMotion { Position = center, GlobalPosition = center }); await Frames(1);
        Input.ParseInputEvent(new InputEventMouseButton { ButtonIndex = MouseButton.Left, Position = center, GlobalPosition = center, Pressed = true }); await Frames(1);
        Input.ParseInputEvent(new InputEventMouseButton { ButtonIndex = MouseButton.Left, Position = center, GlobalPosition = center, Pressed = false }); await Frames(2);
        Require(count() == pointerBefore + 1, "Contextual pointer activation failed: " + name + ", windowFocus=" + GetWindow().HasFocus() + ", turn=" + GetNode<CharacterTurnActivityController>("../CharacterTurnActivityController").Activity?.State); button.ReleaseFocus(); reset();
        controller.Apply(ControlBindingEdit.Set(defaults, "ui_accept", new("mouseButton", 1)));
        reset(); await Frames(2); button.GrabFocus(); long outsideBefore = count();
        await Pointer(new Vector2(800, 650));
        Require(count() == outsideBefore, "Mapped primary click bypassed native hit testing: " + name);
        await Pointer(button.GetGlobalRect().GetCenter());
        Require(count() == outsideBefore + 1, "Mapped primary click duplicated/lost native activation: " + name);
        button.ReleaseFocus(); reset(); controller.Apply(defaults);
    }
    private async Task CheckPersistence()
    {
        panel.Open(); ControlPreferences changed = ControlBindingEdit.Set(defaults, "save_current", new("mouseButton", 8)); panel.SetDraft(changed);
        Require(panel.ApplyDraft(true), "Explicit Save failed");
        var loaded = GetNode<ControlSettingsStorageController>("../ControlSettingsStorageController").Load();
        Require(loaded.Status == ControlPreferencesLoadStatus.Loaded && ControlSettingsController.Same(loaded.Preferences, changed), "Restart snapshot changed");
        panel.SetDraft(defaults); Require(panel.ApplyDraft(true), "Complete default reset could not persist");
        panel.Close(false); await Frames(2); passed.Add("explicit complete Apply/Save/reload/default reset in disposable storage");
    }
    private async Task NativeCapture()
    {
        Vector2I size = mode == "native-1920" ? new(1920, 1080) : new(1280, 720); DisplayServer.WindowSetSize(size); await Frames(5);
        panel.Open(); panel.SelectAction("save_current");
        ControlPreferences multi = ControlBindingEdit.Set(defaults, "save_current", new("mouseButton", 8));
        multi = ControlBindingEdit.Set(multi, "save_current", new("joyButton", 20)); panel.SetDraft(multi); await Frames(5);
        string path = OS.GetEnvironment("ALLRPCS_CONTROLS_SCREENSHOT");
        if (!string.IsNullOrEmpty(path))
        {
            await Screenshot(path);
            panel.SelectAction("ui_focus_prev"); await Frames(5); await Screenshot(Path.ChangeExtension(path, null) + "-last-row.png");
            panel.SelectAction("save_current"); panel.BeginCapture("Mouse"); await Frames(5);
            await Screenshot(Path.ChangeExtension(path, null) + "-capture.png");
            Button cancel = (Button)panel.FindChild("CancelCapture", true, false);
            string beforeCancel = ControlPreferencesJson.Serialize(panel.Draft);
            Vector2 center = GetViewport().GetFinalTransform() * cancel.GetGlobalRect().GetCenter();
            Input.ParseInputEvent(new InputEventMouseMotion { Position = center, GlobalPosition = center }); await Frames(1);
            Input.ParseInputEvent(new InputEventMouseButton { ButtonIndex = MouseButton.Left, Position = center, GlobalPosition = center, Pressed = true }); await Frames(1);
            Input.ParseInputEvent(new InputEventMouseButton { ButtonIndex = MouseButton.Left, Position = center, GlobalPosition = center, Pressed = false }); await Frames(2);
            Require(!panel.Capture.Active && panel.Capture.Candidate is null && ControlPreferencesJson.Serialize(panel.Draft) == beforeCancel,
                "Native pointer Cancel captured a binding or failed to cancel: " + size);
            ControlPreferences active = controller.Preferences;
            panel.BeginCapture("Mouse"); await Frames(5);
            await Pointer(((Button)panel.FindChild("Close", true, false)).GetGlobalRect().GetCenter());
            Require(panel.Visible && !panel.Capture.Active && panel.Capture.Candidate is null
                && ((Control)panel.FindChild("Confirmation", true, false)).Visible,
                "Native Close during capture did not offer unapplied draft discard");
            await Pointer(((Button)panel.FindChild("KeepEditing", true, false)).GetGlobalRect().GetCenter());
            Require(ControlPreferencesJson.Serialize(panel.Draft) == beforeCancel, "Keep editing discarded the draft");
            panel.BeginCapture("Mouse"); await Frames(5);
            await Pointer(((Button)panel.FindChild("Defaults", true, false)).GetGlobalRect().GetCenter());
            Require(!panel.Capture.Active && panel.Capture.Candidate is null && ControlSettingsController.Same(panel.Draft, defaults)
                && ControlSettingsController.Same(controller.Preferences, active), "Native Restore defaults during capture did not retain explicit Apply semantics");
            panel.BeginCapture("Mouse"); await Frames(5);
            await Pointer(((Button)panel.FindChild("Close", true, false)).GetGlobalRect().GetCenter());
            Require(!panel.Visible && !panel.Capture.Active && panel.Capture.Candidate is null,
                "Native Close during capture captured a binding or did not close");
            ControlPreferences mouseCancel = ControlBindingEdit.Set(defaults, "ui_cancel", new("mouseButton", 1), 0);
            mouseCancel = ControlBindingEdit.Set(mouseCancel, "optional_overlays_toggle", new("mouseButton", 1), 0);
            Require(controller.Apply(mouseCancel), "Mouse cancel/toggle configuration failed");
            await Pointer(GetNode<Button>("../SettingsOverlay/ControlsEntry").GetGlobalRect().GetCenter());
            Require(panel.Visible, "Remapped primary-pointer overlay toggle stole the Controls entry"); await Frames(3);
            await Pointer(((Button)panel.FindChild("Defaults", true, false)).GetGlobalRect().GetCenter());
            Require(panel.Visible && ControlSettingsController.Same(panel.Draft, defaults)
                && ControlSettingsController.Same(controller.Preferences, mouseCancel),
                "Remapped primary-pointer UI cancel stole visible Restore defaults");
            Require(controller.Apply(defaults), "Default active map restoration failed");
            await Pointer(((Button)panel.FindChild("Close", true, false)).GetGlobalRect().GetCenter());
            Require(!panel.Visible, "Primary-pointer Close fallback failed");
        }
        passed.Add("native multi-device row/last-row/capture screenshots " + size); panel.SetDraft(defaults); panel.Close(false);
    }
    private async Task Pointer(Vector2 canvasPoint)
    {
        Vector2 point = GetViewport().GetFinalTransform() * canvasPoint;
        Input.ParseInputEvent(new InputEventMouseMotion { Position = point, GlobalPosition = point }); await Frames(1);
        Input.ParseInputEvent(new InputEventMouseButton { ButtonIndex = MouseButton.Left, Position = point, GlobalPosition = point, Pressed = true }); await Frames(1);
        Input.ParseInputEvent(new InputEventMouseButton { ButtonIndex = MouseButton.Left, Position = point, GlobalPosition = point, Pressed = false }); await Frames(2);
    }
    private async Task Screenshot(string path)
    { var drawn = ToSignal(RenderingServer.Singleton, RenderingServer.SignalName.FramePostDraw); RenderingServer.ForceDraw(); await drawn; Require(GetViewport().GetTexture().GetImage().SavePng(path) == Error.Ok, "Native screenshot failed"); }
    private static string MapSnapshot() => string.Join("|", ControlActionCatalog.Actions.Select(a => a.Id + ":" + InputMap.ActionGetDeadzone(a.Id) + ":" + string.Join("/", InputMap.ActionGetEvents(a.Id).Select(e => e.AsText()))));
    private async Task Press(Key key, bool shift = false)
    { Input.ParseInputEvent(KeyEvent(key, true, shift: shift)); await Frames(2); Input.ParseInputEvent(KeyEvent(key, false, shift: shift)); await Frames(1); }
    private static InputEventKey KeyEvent(Key key, bool pressed, bool shift = false, bool ctrl = false) => new() { Keycode = key, PhysicalKeycode = key, Pressed = pressed, ShiftPressed = shift, CtrlPressed = ctrl };
    private async Task Frames(int count) { for (int i = 0; i < count; i++) { await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame); } }
    private async Task PhysicsFrames(int count) { for (int i = 0; i < count; i++) { await ToSignal(GetTree(), SceneTree.SignalName.PhysicsFrame); } }
    private static void Require(bool value, string message) { if (!value) { throw new InvalidOperationException(message); } }
}
