using System.Runtime.CompilerServices;
using System.Text.Json;
using Allrpcs.Application.Dialogue;
using Allrpcs.Application.Speech;
using Allrpcs.Domain.Dialogue;
using Allrpcs.Infrastructure.Speech;
using Godot;

namespace Allrpcs.Game;

/// <summary>Opt-in Main keyboard qualification with synthetic PCM; never opens the physical microphone.</summary>
public partial class SpaceHoldSpeechSmokeCheck : Node
{
    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };
    private readonly Capture capture = new();
    private readonly List<string> checks = [];
    private PlayerSpeechController mic = null!;
    private SpaceHoldSpeechController shortcut = null!;
    private DialogueSpeechController reply = null!;
    private TemporaryDialogueInputPanel panel = null!;
    private LineEdit text = null!;
    private DialogueInputController delivery = null!;
    private CharacterTurnTranscriptController transcript = null!;
    private string report = "", mode = "", recognized = "", stage = "setup";

    /// <inheritdoc />
    public override async void _Ready()
    {
        if (OS.GetEnvironment("ALLRPCS_SPACE_HOLD_CHECK") != "1") { return; }
        Engine.MaxFps = 60; await Frames(6);
        try
        {
            mode = OS.GetEnvironment("ALLRPCS_SPACE_HOLD_MODE"); report = OS.GetEnvironment("ALLRPCS_SPACE_HOLD_REPORT");
            mic = GetNode<PlayerSpeechController>("../PlayerSpeechController");
            shortcut = GetNode<SpaceHoldSpeechController>("../SpaceHoldSpeechController");
            reply = GetNode<DialogueSpeechController>("../DialogueSpeechController");
            panel = GetNode<TemporaryDialogueInputPanel>("../DialogueOverlay/Panel"); text = panel.GetNode<LineEdit>("Input");
            delivery = GetNode<DialogueInputController>("../DialogueInputController");
            transcript = GetNode<CharacterTurnTranscriptController>("../CharacterTurnTranscriptController");
            CharacterBody2D healer = GetNode<CharacterBody2D>("../Healer"), seeker = GetNode<CharacterBody2D>("../Seeker");
            healer.SetPhysicsProcess(false); seeker.SetPhysicsProcess(false); seeker.GlobalPosition = healer.GlobalPosition + new Vector2(12, 0);
            if (DisplayServer.GetName() != "headless")
            { GetWindow().Size = mode.Contains("1920", StringComparison.Ordinal) ? new(1920, 1080) : new(1280, 720); GetWindow().GrabFocus(); await Frames(8); }
            reply.Configure(new Synthesizer());
            bool real = mode.StartsWith("moonshine", StringComparison.Ordinal);
            if (mode != "unavailable")
            {
                ISpeechRecognizer recognizer = new Recognizer();
                if (real)
                {
                    using JsonDocument c = JsonDocument.Parse(File.ReadAllText(OS.GetEnvironment("ALLRPCS_SPACE_HOLD_RUNTIME")));
                    JsonElement v = c.RootElement;
                    recognizer = new MoonshineSpeechRecognizer(new PlayerSpeechWorkerFactory(new(v.GetProperty("pythonPath").GetString()!,
                        v.GetProperty("workerPath").GetString()!, v.GetProperty("englishModelPath").GetString()!,
                        v.GetProperty("germanModelPath").GetString()!, v.GetProperty("manifestPath").GetString()!)));
                    capture.Wave = ReadWave(OS.GetEnvironment("ALLRPCS_SPACE_HOLD_FILE"));
                }
                mic.Configure(capture, recognizer);
            }
            GetNode<Button>("../SettingsOverlay/DialogueEntry").EmitSignal(BaseButton.SignalName.Pressed); await Frames(3);
            text.Text = "helloworld"; text.CaretColumn = 5; await TapSpace();
            Require(text.Text == "hello world" && capture.Opens == 0, "Tap did not insert native space: " + text.Text);
            text.Text = "hello world"; text.CaretColumn = 11; text.Select(5, 11); await TapSpace();
            Require(text.Text == "hello " && capture.Opens == 0, "Native selected-text replacement: " + text.Text);
            text.Text = "ab"; text.CaretColumn = 1;
            KeyEvent(Key.Space, true); KeyEvent(Key.X, true); KeyEvent(Key.X, false); KeyEvent(Key.Space, false); await Frames(3);
            Require(text.Text == "a xb", "Overlapping typing order: " + text.Text);
            checks.Add("native-tap-spacing-selection-and-overlapping-typing-no-mic");
            CheckButton whisper = panel.GetNode<CheckButton>("Whispering"); whisper.GrabFocus(); await TapSpace();
            Require(whisper.ButtonPressed && capture.Opens == 0, "Tap native checkbox activation"); whisper.ButtonPressed = false;
            ((Button)panel.FindChild("HoldToTalk", true, false)).GrabFocus(); await TapSpace();
            Require(capture.Opens == 0, "Brief tap on talk focus opened microphone");
            checks.Add("tap-native-button-semantics-without-brief-talk-capture");
            if (mode == "unavailable")
            {
                text.Text = "Typed fallback"; text.GrabFocus(); await StartHold(false); await ReleaseHold();
                Require(!mic.Listening && text.Text == "Typed fallback" && mic.Status.Contains("unavailable", StringComparison.Ordinal), "Unavailable runtime text fallback");
                checks.Add("held-space-missing-runtime-retains-typed-text");
            }
            else
            {
                stage = "initial-reply"; GD.Print("Space stage: " + stage);
                reply.SetEnabled(true); await Submit("Healer, please reply to this keyboard test."); await Wait(() => reply.Playing);
                int playerEntries = PlayerEntries();
                if (real)
                {
                    bool german = mode.Contains("german", StringComparison.Ordinal);
                    OptionButton language = (OptionButton)panel.FindChild("SpeechLanguage", true, false);
                    language.Select(german ? 1 : 0); language.EmitSignal(OptionButton.SignalName.ItemSelected, german ? 1 : 0); await Frames(3);
                    text.Text = ""; text.GrabFocus(); await StartHold();
                    await Wait(() => text.Text.Contains(german ? "nicht" : "not", StringComparison.OrdinalIgnoreCase));
                    await ReleaseHold(); recognized = text.Text;
                    Require(PlayerEntries() == playerEntries, "Real recognition auto-submitted");
                    checks.Add("actual-local-moonshine-space-hold-partial-final-no-auto-submit");
                }
                else
                {
                    foreach (string name in new[] { "Input", "Whispering", "SpeechLanguage", "Submit", "Close", "HoldToTalk" })
                    {
                        stage = "hold-focus-" + name; GD.Print("Space stage: " + stage);
                        text.Text = "Typed prefix"; ((Control)panel.FindChild(name, true, false)).GrabFocus();
                        int opens = capture.Opens; await StartHold();
                        Require(capture.Opens == opens + 1 && !reply.Playing, "Hold did not interrupt reply/start once: " + name);
                        for (int i = 0; i < 20; i++) { KeyEvent(Key.Space, true, echo: true); }
                        ((Control)panel.FindChild("SpeechLanguage", true, false)).GrabFocus(); await Frames(3);
                        Require(mic.Listening && capture.Opens == opens + 1, "GUI focus/echo restarted or finalized hold");
                        await ReleaseHold();
                        Require(text.Text == "Typed prefix Speech finalized", "Release lost final or leaked spaces: " + text.Text);
                    }
                    Require(PlayerEntries() == playerEntries, "Hold or native control auto-submitted");
                    checks.Add("hold-from-six-focuses-no-repeat-space-button-action-or-auto-submit");
                    stage = "world-hold"; GD.Print("Space stage: " + stage);
                    CanvasLayer overlay = GetNode<CanvasLayer>("../DialogueOverlay"); overlay.Visible = false;
                    GetViewport().GuiGetFocusOwner()?.ReleaseFocus(); text.Text = "World prefix";
                    await StartHold(); Require(overlay.Visible, "World hold did not open dialogue"); await ReleaseHold();
                    checks.Add("world-hold-opens-draft-without-voice-mode");
                    stage = "reading-hold"; GD.Print("Space stage: " + stage);
                    GetNode<AudioSettingsPanel>("../AudioSettingsPanel").Open();
                    await StartHold(); Require(overlay.Layer > GetNode<AudioSettingsPanel>("../AudioSettingsPanel").Layer, "Reading view covered voice draft");
                    await ReleaseHold(); GetNode<AudioSettingsPanel>("../AudioSettingsPanel").Close(false);
                    checks.Add("hold-from-other-reading-view-brings-draft-forward");
                    stage = "popup-hold"; GD.Print("Space stage: " + stage);
                    PopupMenu popup = ((OptionButton)panel.FindChild("SpeechLanguage", true, false)).GetPopup();
                    ((OptionButton)panel.FindChild("SpeechLanguage", true, false)).ShowPopup(); await Frames(3); Require(popup.Visible, "Popup fixture did not open");
                    await StartHold(); await ReleaseHold(); popup.Hide();
                    checks.Add("held-space-from-open-language-popup");
                    stage = "pending-cancel"; GD.Print("Space stage: " + stage);
                    int before = capture.Opens; KeyEvent(Key.Space, true); await Frames(3); mic.StopCapture("fixture-pending-stop");
                    await Delay(400); Require(capture.Opens == before, "Cancelled pending hold opened mic"); KeyEvent(Key.Space, false); await Frames(3);
                    checks.Add("stop-before-threshold-fences-key-until-release");
                    stage = "active-cancel"; GD.Print("Space stage: " + stage);
                    await StartHold(); mic.StopCapture("fixture-stop"); await Delay(350);
                    Require(!mic.Listening, "Stop retained held capture"); await ReleaseHold();
                    await StartHold(); panel.GetNode<Button>("Close").EmitSignal(BaseButton.SignalName.Pressed);
                    await Delay(350); Require(!mic.Listening, "Close retained held capture"); await ReleaseHold();
                    await StartHold(); mic._Notification((int)NotificationApplicationFocusOut); shortcut._Notification((int)NotificationApplicationFocusOut);
                    await Delay(350); Require(!mic.Listening, "Application focus loss retained capture"); KeyEvent(Key.Space, false);
                    shortcut._Notification((int)NotificationApplicationFocusIn); await Frames(3);
                    checks.Add("stop-close-and-application-focus-loss-fence-held-key");
                    await StartHold();
                    // The full gate qualifies real restore roots separately. This keyboard fixture's supplied
                    // dialogue state is not a capturable recovery root, so exercise the exact reset notification seam.
                    SaveSnapshotRestorationController restoration = GetNode<SaveSnapshotRestorationController>("../SaveSnapshotRestorationController");
                    var field = typeof(SaveSnapshotRestorationController).GetField("PresentationReset",
                        System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
                    Action reset = field?.GetValue(restoration) as Action ?? throw new InvalidOperationException("Restore reset notification missing.");
                    reset();
                    await Delay(350); Require(!mic.Listening, "Successful load retained held capture"); await ReleaseHold();
                    checks.Add("injected-successful-load-reset-notification-fences-held-key");
                    recognized = text.Text;
                }
                text.Text = "Corrected voice words remain explicit."; text.EmitSignal(LineEdit.SignalName.TextChanged, text.Text);
                whisper.ButtonPressed = true; await Submit(text.Text);
                Require(delivery.LastSubmittedUtterance?.Text == "Corrected voice words remain explicit." && delivery.LastSubmittedDelivery?.Mode == DialogueDeliveryMode.Whispering,
                    "Explicit corrected Whisper submission changed"); checks.Add("corrected-explicit-submit-existing-whisper-owner");
            }
            if (DisplayServer.GetName() != "headless")
            { text.Text = recognized; text.GrabFocus(); await Frames(3); GetViewport().GetTexture().GetImage().SavePng(Path.ChangeExtension(report, ".png")); }
            if (mode != "unavailable") { await StartHold(); }
            SceneTree tree = GetTree(); Node main = GetParent(); main.RemoveChild(this); tree.Root.AddChild(this); main.QueueFree(); await Frames(5); await mic.Shutdown; await reply.Shutdown;
            Require(capture.Active == 0, "Scene teardown retained capture"); checks.Add("scene-teardown-awaits-synthetic-capture-and-recognition");
            File.WriteAllText(report, JsonSerializer.Serialize(new
            {
                status = "passed",
                mode,
                checks,
                thresholdMilliseconds = 300,
                captureOpens = capture.Opens,
                recognized,
                realMoonshine = real,
                provenance = "Native engine key events and synthetic PCM in Main. No physical microphone opened; no human/echo qualification."
            }, JsonOptions));
            GD.Print("PASS: Space hold Main: " + mode); GetTree().Quit();
        }
        catch (Exception error) { GD.PrintErr("FAIL: Space hold Main: " + error.Message); GetTree().Quit(1); }
    }
    private int PlayerEntries() => transcript.Transcript!.Entries.Count(entry => entry.SpokenStatement?.SpeakerCharacterId == delivery.SpeakerCharacterId);
    private static void KeyEvent(Key key, bool pressed, bool echo = false) => Input.ParseInputEvent(new InputEventKey
    { Keycode = key, PhysicalKeycode = key, Unicode = key == Key.Space ? 32u : key == Key.X ? 120u : 0, Pressed = pressed, Echo = echo });
    private async Task TapSpace() { KeyEvent(Key.Space, true); await Frames(2); KeyEvent(Key.Space, false); await Frames(3); }
    private async Task StartHold(bool available = true)
    {
        int count = shortcut.Holds; KeyEvent(Key.Space, true); await Frames(3);
        Require(shortcut.Holds == count, "Voice started before threshold");
        await Wait(() => shortcut.Holds == count + 1);
        if (available) { await Wait(() => mic.Listening && capture.Active > 0); }
    }
    private async Task ReleaseHold() { KeyEvent(Key.Space, false); await Wait(() => !mic.Listening); await Frames(3); }
    private async Task Submit(string value)
    {
        await Wait(() => CharacterTurnStartPolicy.CanStart(GetNode<CharacterTurnActivityController>("../CharacterTurnActivityController").Activity)
            && GetNode<CharacterTurnParticipationRoundTaskController>("../CharacterTurnParticipationRoundTaskController").Status == CharacterTurnParticipationRoundTaskStatus.Idle);
        Reply model = new();
        Require(GetNode<TemporaryCharacterTurnParticipationModelSource>("../TemporaryCharacterTurnParticipationModelSource").TrySupplyModel(new Participation()), "Participation source collision");
        Require(GetNode<TemporaryCharacterModelSource>("../TemporaryCharacterModelSource").TrySupplyModel(model), "Reply source collision");
        text.Text = value; text.EmitSignal(LineEdit.SignalName.TextSubmitted, value); await Wait(() => model.Calls > 0);
    }
    private async Task Frames(int count) { for (int i = 0; i < count; i++) { await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame); } }
    private async Task Delay(ulong milliseconds) { ulong until = Time.GetTicksMsec() + milliseconds; while (Time.GetTicksMsec() < until) { await Frames(1); } }
    private async Task Wait(Func<bool> predicate)
    { ulong until = Time.GetTicksMsec() + 15000; while (!predicate()) { Require(Time.GetTicksMsec() < until, "Deadline at " + stage + ": " + mic.Status + " stop=" + mic.StopReason + " holds=" + shortcut.Holds + " capture=" + capture.Opens + " active=" + capture.Active + " focus=" + GetViewport().GuiGetFocusOwner()?.Name); await Frames(1); } }
    private static void Require(bool value, string message) { if (!value) { throw new InvalidOperationException(message); } }
    private static float[] ReadWave(string path)
    {
        using BinaryReader reader = new(File.OpenRead(path)); reader.ReadBytes(12);
        while (reader.BaseStream.Position < reader.BaseStream.Length)
        {
            string id = new(reader.ReadChars(4)); int length = reader.ReadInt32(); byte[] bytes = reader.ReadBytes(length);
            if (id == "data") { return Enumerable.Range(0, length / 2).Select(i => System.Buffers.Binary.BinaryPrimitives.ReadInt16LittleEndian(bytes.AsSpan(i * 2)) / 32768f).ToArray(); }
            if (length % 2 != 0) { reader.ReadByte(); }
        }
        throw new IOException("Synthetic PCM fixture missing.");
    }
    private sealed class Capture : IMicrophoneCapture
    {
        internal int Opens, Active; internal float[]? Wave;
        public async IAsyncEnumerable<SpeechAudioChunk> CaptureAsync([EnumeratorCancellation] CancellationToken token)
        {
            Opens++; Active++;
            try
            {
                for (int offset = 0; ; offset += 512)
                {
                    token.ThrowIfCancellationRequested();
                    if (Wave is not null && offset >= Wave.Length) { yield break; }
                    yield return new(16000, Wave is null ? Enumerable.Repeat(0.1f, 512).ToArray() : Wave.Skip(offset).Take(512).ToArray());
                    await Task.Delay(32, token);
                }
            }
            finally { Active--; }
        }
    }
    private sealed class Recognizer : ISpeechRecognizer
    {
        public async IAsyncEnumerable<SpeechRecognitionEvent> RecognizeAsync(SpeechLanguage language, IAsyncEnumerable<SpeechAudioChunk> input,
            [EnumeratorCancellation] CancellationToken token)
        { await foreach (SpeechAudioChunk _ in input.WithCancellation(token)) { yield return new(SpeechRecognitionKind.Partial, "1", "Speech draft"); } yield return new(SpeechRecognitionKind.Final, "1", "Speech finalized"); }
    }
    private sealed class Participation : ICharacterTurnParticipationModel
    {
        public ValueTask<CharacterTurnParticipationDecision> DecideAsync(CharacterTurnParticipationConsiderationRequest request, CancellationToken token)
            => ValueTask.FromResult(new CharacterTurnParticipationDecision(request.ScheduledCharacter.Identity.Id,
                request.IsDirectlyAddressed ? CharacterTurnParticipationKind.Respond : CharacterTurnParticipationKind.RemainSilent,
                request.IsDirectlyAddressed ? null : request.ScheduledCharacter.Goals.Entries[0]));
    }
    private sealed class Reply : ICharacterModel
    {
        internal int Calls;
        public async IAsyncEnumerable<CharacterModelStreamUpdate> StreamResponseAsync(CharacterModelGenerationRequest request, [EnumeratorCancellation] CancellationToken token)
        {
            Calls++; await Task.CompletedTask; token.ThrowIfCancellationRequested(); const string words = "Synthetic reply stays in the transcript.";
            CharacterTurnResponse result = new(request.TurnRequest, CharacterTurnDisposition.Accept,
                new DialogueDelivery(new DialogueUtterance(request.TurnRequest.RespondingCharacterId, words), DialogueDeliveryMode.OrdinarySpeech),
                new CharacterIntention(CharacterIntentionType.Wait, null), [], []);
            yield return new CharacterModelTextDelta(words); yield return new CharacterModelStreamTermination(CharacterModelStreamTerminationStatus.Completed, result, null);
        }
    }
    private sealed class Synthesizer : ISpeechSynthesizer
    {
        public async IAsyncEnumerable<SpeechAudioChunk> SynthesizeAsync(SpeechSynthesisRequest request, [EnumeratorCancellation] CancellationToken token)
        { for (int i = 0; i < 80; i++) { await Task.Delay(2, token); yield return new(24000, Enumerable.Repeat(0.1f, 1200).ToArray()); } }
    }
}
