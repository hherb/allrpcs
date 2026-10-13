using System.Runtime.CompilerServices;
using System.Text.Json;
using Allrpcs.Application.Dialogue;
using Allrpcs.Application.Speech;
using Allrpcs.Domain.Dialogue;
using Allrpcs.Infrastructure.Speech;
using Godot;

namespace Allrpcs.Game;

/// <summary>Opt-in ordinary Main qualification; all capture is synthetic and never opens a physical mic.</summary>
public partial class PlayerSpeechMainSmokeCheck : Node
{
    private static readonly JsonSerializerOptions ReportOptions = new() { WriteIndented = true };
    private PlayerSpeechController mic = null!;
    private DialogueSpeechController reply = null!;
    private TemporaryDialogueInputPanel panel = null!;
    private LineEdit text = null!;
    private CharacterTurnTranscriptController transcript = null!;
    private DialogueInputController input = null!;
    private Button talk = null!;
    private CheckButton listen = null!;
    private OptionButton language = null!;
    private readonly SyntheticCapture capture = new();
    private readonly List<string> checks = [];
    private string mode = "";
    private bool real;
    private string report = "";
    private string recognized = "";
    private int submissions;

    /// <inheritdoc />
    public override async void _Ready()
    {
        if (OS.GetEnvironment("ALLRPCS_PLAYER_ASR_CHECK") != "1") { return; }
        Engine.MaxFps = 60;
        await Frames(5);
        try
        {
            mode = OS.GetEnvironment("ALLRPCS_PLAYER_ASR_MODE"); real = mode.StartsWith("moonshine", StringComparison.Ordinal);
            report = OS.GetEnvironment("ALLRPCS_PLAYER_ASR_REPORT");
            mic = GetNode<PlayerSpeechController>("../PlayerSpeechController"); reply = GetNode<DialogueSpeechController>("../DialogueSpeechController");
            panel = GetNode<TemporaryDialogueInputPanel>("../DialogueOverlay/Panel"); text = panel.GetNode<LineEdit>("Input");
            transcript = GetNode<CharacterTurnTranscriptController>("../CharacterTurnTranscriptController"); input = GetNode<DialogueInputController>("../DialogueInputController");
            talk = (Button)panel.FindChild("HoldToTalk", true, false); listen = (CheckButton)panel.FindChild("ListenToPlayer", true, false);
            language = (OptionButton)panel.FindChild("SpeechLanguage", true, false);
            CharacterBody2D healer = GetNode<CharacterBody2D>("../Healer"); CharacterBody2D seeker = GetNode<CharacterBody2D>("../Seeker");
            healer.SetPhysicsProcess(false); seeker.SetPhysicsProcess(false); seeker.GlobalPosition = healer.GlobalPosition + new Vector2(12, 0);
            if (DisplayServer.GetName() != "headless")
            { Engine.MaxFps = 60; GetWindow().Size = mode.EndsWith("1920", StringComparison.Ordinal) ? new(1920, 1080) : new(1280, 720); GetWindow().GrabFocus(); await Frames(8); }
            reply.Configure(new Synthesizer());
            if (mode != "unavailable")
            {
                ISpeechRecognizer recognizer = new DraftRecognizer();
                if (real)
                {
                    using JsonDocument config = JsonDocument.Parse(File.ReadAllText(OS.GetEnvironment("ALLRPCS_PLAYER_ASR_RUNTIME")));
                    JsonElement v = config.RootElement;
                    recognizer = new MoonshineSpeechRecognizer(new PlayerSpeechWorkerFactory(new(v.GetProperty("pythonPath").GetString()!,
                        v.GetProperty("workerPath").GetString()!, v.GetProperty("englishModelPath").GetString()!,
                        v.GetProperty("germanModelPath").GetString()!, v.GetProperty("manifestPath").GetString()!)));
                    capture.Wave = File.ReadAllBytes(OS.GetEnvironment("ALLRPCS_PLAYER_ASR_FILE"));
                }
                mic.Configure(capture, recognizer);
            }
            await Press(Key.Tab); await Press(Key.Tab); await Press(Key.Tab); await Press(Key.Enter);
            Require(GetViewport().GuiGetFocusOwner() == text, "Dialogue entry focus");
            Require(!mic.Listening && capture.Opens == 0 && mic.Status.StartsWith("Microphone off", StringComparison.Ordinal), "Capture default-off: " + capture.Opens + "/" + mic.Status);
            await Submit("Typed dialogue remains authoritative.");
            Require(capture.Opens == 0, "Typed Submit opened microphone"); checks.Add("default-off-typed-dialogue-parity");
            reply.SetEnabled(true); await Submit("A reply for interruption."); await Wait(() => reply.Playing);
            int before = PlayerEntries(); object delivery = input.LastSubmittedDelivery!;
            if (mode == "unavailable")
            {
                text.Text = "Typed fallback stays available";
                talk.EmitSignal(BaseButton.SignalName.ButtonDown); await Frames(3);
                Require(!mic.Listening && mic.Status.Contains("unavailable", StringComparison.Ordinal) && text.Text == "Typed fallback stays available",
                    "Missing recognizer lost text"); checks.Add("missing-runtime-text-fallback");
            }
            else
            {
                if (mode.Contains("german", StringComparison.Ordinal)) { language.Select(1); language.EmitSignal(OptionButton.SignalName.ItemSelected, 1); }
                text.Text = "";
                await TalkDown(); GD.Print("ASR stage talk down fps=" + Engine.MaxFps + " tick=" + Time.GetTicksMsec());
                Require(!reply.Playing, "Hold-to-talk did not synchronously flush reply PCM"); checks.Add("hold-to-talk-reply-buffer-interruption");
                await Wait(() => text.Text.Length > 0, 20000); GD.Print("ASR stage partial " + mic.Status);
                Require(PlayerEntries() == before && ReferenceEquals(delivery, input.LastSubmittedDelivery), "Recognition automatically submitted dialogue");
                checks.Add("partial-draft-no-automatic-dialogue");
                if (!real)
                {
                    text.GrabFocus(); text.Text = "Do not take it yet"; text.EmitSignal(LineEdit.SignalName.TextChanged, text.Text);
                    await Frames(12);
                    Require(text.Text == "Do not take it yet", "Recognition overwrote manual correction"); checks.Add("manual-correction-fences-late-partials");
                    await TalkUp(); await Wait(() => !mic.Listening);
                    Require(text.Text == "Do not take it yet", "Final overwrote correction"); checks.Add("release-finalizes-without-overwriting-correction");
                }
                else
                {
                    await Wait(() => !mic.Listening, 25000); GD.Print("ASR stage recognition ended " + mic.Status + " reason=" + mic.StopReason);
                    recognized = text.Text;
                    Require(recognized.Contains(mode.Contains("german", StringComparison.Ordinal) ? "nicht" : "not", StringComparison.OrdinalIgnoreCase), "Actual recognition status=" + mic.Status + " text=[" + recognized + "] opens=" + capture.Opens);
                    checks.Add("real-moonshine-partial-final-local-model");
                    await TalkUp();
                }
                string corrected = text.Text;
                panel.GetNode<CheckButton>("Whispering").ButtonPressed = true;
                await Submit(corrected);
                Require(input.LastSubmittedUtterance!.Text == corrected && input.LastSubmittedDelivery!.Mode == DialogueDeliveryMode.Whispering,
                    "Explicit corrected Submit changed words/delivery"); checks.Add("explicit-submit-exact-words-existing-whisper-owner");
                panel.GetNode<CheckButton>("Whispering").ButtonPressed = false;
                await Wait(() => reply.Playing);
                int starts = mic.VoiceStarts; before = PlayerEntries();
                await ToggleListening(); GD.Print("ASR stage listen " + mic.Status);
                await Wait(() => mic.VoiceStarts > starts, 15000); GD.Print("ASR stage activity " + mic.Status);
                Require(!reply.Playing && PlayerEntries() == before, "Voice-start did not retire PCM independently of dialogue");
                checks.Add("capture-activity-interruption-no-domain-action");
                Button stop = (Button)panel.FindChild("StopMicrophone", true, false);
                if (DisplayServer.GetName() == "headless") { stop.EmitSignal(BaseButton.SignalName.Pressed); }
                else { await Click(stop); }
                GD.Print("ASR stage stop clicked " + mic.Status + " reason=" + mic.StopReason);
                string kept = text.Text;
                await Frames(8); Require(!mic.Listening && text.Text == kept, "Stop status=" + mic.Status + " kept=[" + kept + "] current=[" + text.Text + "] listen=" + listen.ButtonPressed); checks.Add("stop-mic-preserves-draft-rejects-retired-events");
                if (!real)
                {
                    capture.Fail = true; text.Text = "Typed input survives denied or lost input";
                    await ToggleListening(); await Wait(() => mic.Status.Contains("unavailable", StringComparison.Ordinal));
                    Require(text.Text == "Typed input survives denied or lost input" && !mic.Listening, "Unavailable capture changed input");
                    checks.Add("injected-permission-device-failure-text-fallback"); capture.Fail = false;
                    await ToggleListening(); await Wait(() => mic.Status.StartsWith("Listening", StringComparison.Ordinal));
                    language.Select(1); language.EmitSignal(OptionButton.SignalName.ItemSelected, 1);
                    Require(!mic.Listening, "Language switch retained old capture"); checks.Add("language-switch-retires-generation");
                    await ToggleListening(); await Wait(() => mic.Status.StartsWith("Listening", StringComparison.Ordinal));
                    mic._Notification((int)NotificationWMWindowFocusOut);
                    Require(!mic.Listening, "Focus loss retained capture"); checks.Add("injected-focus-loss-retires-generation");
                }
            }
            GD.Print("ASR stage focus cycle"); text.GrabFocus();
            foreach (string name in new[] { "Whispering", "SpeakReplies", "SpeechLanguage", "StopSpeech", "HoldToTalk", "ListenToPlayer", "StopMicrophone", "Submit", "Close", "Input" })
            { await Press(Key.Tab); Require(GetViewport().GuiGetFocusOwner()?.Name == name, "Combined focus cycle: " + name); }
            Require(!ControlInputContext.WorldAllowed(this), "Voice GUI leaked world controls"); checks.Add("combined-ten-control-focus-and-world-gate");
            if (DisplayServer.GetName() != "headless")
            { await Frames(90); await ToSignal(RenderingServer.Singleton, RenderingServer.SignalName.FramePostDraw); GetViewport().GetTexture().GetImage().SavePng(Path.ChangeExtension(report, ".png")); }
            if (mode != "unavailable") { await ToggleListening(); await Wait(() => mic.Status.StartsWith("Listening", StringComparison.Ordinal)); }
            GD.Print("ASR stage retiring scene " + mic.Status); mic.GetParent().RemoveChild(mic); await mic.Shutdown; Require(mic.Shutdown.IsCompletedSuccessfully, "Capture/model scene retirement"); mic.Free();
            checks.Add("scene-exit-awaits-capture-and-recognizer-retirement");
            File.WriteAllText(report, JsonSerializer.Serialize(new
            {
                status = "passed",
                mode,
                checks,
                realMoonshine = real,
                recognized,
                captureOpens = capture.Opens,
                size = new[] { GetWindow().Size.X, GetWindow().Size.Y },
                provenance = "Synthetic PCM and engine UI input in ordinary Main. Actual local Moonshine when selected. No physical microphone, human acoustic trial or echo qualification."
            }, ReportOptions));
            GD.Print("PASS: player ASR Main: " + mode); GetTree().Quit();
        }
        catch (Exception error)
        {
            if (DisplayServer.GetName() != "headless" && report.Length > 0) { GetViewport().GetTexture().GetImage().SavePng(Path.ChangeExtension(report, ".failed.png")); }
            GD.PrintErr("FAIL: player ASR Main: " + error.Message); GetTree().Quit(1);
        }
    }
    private int PlayerEntries() => transcript.Transcript!.Entries.Count(entry => entry.SpokenStatement?.SpeakerCharacterId == input.SpeakerCharacterId);
    private async Task TalkDown()
    {
        talk.GrabFocus();
        if (DisplayServer.GetName() == "headless") { talk.EmitSignal(BaseButton.SignalName.ButtonDown); }
        else { Input.ParseInputEvent(new InputEventKey { Keycode = Key.Space, PhysicalKeycode = Key.Space, Pressed = true }); }
        await Frames(2);
    }
    private async Task TalkUp()
    {
        if (DisplayServer.GetName() == "headless") { talk.EmitSignal(BaseButton.SignalName.ButtonUp); }
        else { Input.ParseInputEvent(new InputEventKey { Keycode = Key.Space, PhysicalKeycode = Key.Space, Pressed = false }); }
        await Frames(3);
    }
    private async Task ToggleListening()
    {
        if (DisplayServer.GetName() == "headless") { listen.ButtonPressed = !listen.ButtonPressed; }
        else { await Click(listen); }
        await Frames(2);
    }
    private async Task Submit(string value)
    {
        await Wait(() => CharacterTurnStartPolicy.CanStart(GetNode<CharacterTurnActivityController>("../CharacterTurnActivityController").Activity)
            && GetNode<CharacterTurnParticipationRoundTaskController>("../CharacterTurnParticipationRoundTaskController").Status == CharacterTurnParticipationRoundTaskStatus.Idle);
        int count = transcript.Transcript!.Entries.Count;
        submissions++;
        Reply model = new();
        GD.Print("ASR fixture submit " + submissions + " count=" + count + " mic=" + mic.Status);
        Require(GetNode<TemporaryCharacterTurnParticipationModelSource>("../TemporaryCharacterTurnParticipationModelSource").TrySupplyModel(new Participation()), "Participation model fixture collision");
        Require(GetNode<TemporaryCharacterModelSource>("../TemporaryCharacterModelSource").TrySupplyModel(model), "Reply model fixture collision at submission " + submissions);
        text.Text = value; text.EmitSignal(LineEdit.SignalName.TextSubmitted, value);
        Require(input.LastSubmittedUtterance?.Text == value, "Typed Submit rejected at " + submissions);
        await Frames(3);
        GD.Print("ASR fixture state calls=" + model.Calls + " entries=" + transcript.Transcript!.Entries.Count
            + " round=" + GetNode<CharacterTurnParticipationRoundTaskController>("../CharacterTurnParticipationRoundTaskController").Status
            + " activity=" + GetNode<CharacterTurnActivityController>("../CharacterTurnActivityController").Activity?.State
            + " distance=" + GetNode<Node2D>("../Seeker").GlobalPosition.DistanceTo(GetNode<Node2D>("../Healer").GlobalPosition));
        await Wait(() => model.Calls > 0 && transcript.Transcript!.Entries.Count >= count + 2); await Frames(5);
    }
    private async Task Click(Control control)
    {
        Vector2 point = GetViewport().GetFinalTransform() * control.GetGlobalRect().GetCenter();
        Input.ParseInputEvent(new InputEventMouseMotion { Position = point, GlobalPosition = point });
        Input.ParseInputEvent(new InputEventMouseButton { Position = point, GlobalPosition = point, ButtonIndex = MouseButton.Left, Pressed = true }); await Frames(2);
        Input.ParseInputEvent(new InputEventMouseButton { Position = point, GlobalPosition = point, ButtonIndex = MouseButton.Left, Pressed = false }); await Frames(2);
    }
    private async Task Press(Key key)
    {
        Input.ParseInputEvent(new InputEventKey { Keycode = key, PhysicalKeycode = key, Pressed = true }); await Frames(2);
        Input.ParseInputEvent(new InputEventKey { Keycode = key, PhysicalKeycode = key, Pressed = false }); await Frames(2);
    }
    private async Task Frames(int count) { for (int index = 0; index < count; index++) { await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame); } }
    private async Task Wait(Func<bool> ready, ulong milliseconds = 15000)
    { ulong until = Time.GetTicksMsec() + milliseconds; while (!ready()) { Require(Time.GetTicksMsec() < until, "Deadline: " + mic.Status + " reason=" + mic.StopReason + " windowFocus=" + GetWindow().HasFocus() + " gui=" + GetViewport().GuiGetFocusOwner()?.Name); await Frames(1); } }
    private static void Require(bool value, string message) { if (!value) { throw new InvalidOperationException(message); } }
    private sealed class SyntheticCapture : IMicrophoneCapture
    {
        internal int Opens; internal bool Fail; internal byte[]? Wave;
        public async IAsyncEnumerable<SpeechAudioChunk> CaptureAsync([EnumeratorCancellation] CancellationToken token)
        {
            Opens++;
            if (Fail) { await Task.FromException(new IOException("synthetic denied/device lost")); yield break; }
            if (Wave is not null)
            {
                using MemoryStream memory = new(Wave); using BinaryReader reader = new(memory);
                Require(new string(reader.ReadChars(4)) == "RIFF", "Wave container"); reader.ReadInt32(); Require(new string(reader.ReadChars(4)) == "WAVE", "Wave format");
                byte[] data = []; int rate = 0;
                while (memory.Position < memory.Length)
                {
                    string id = new(reader.ReadChars(4)); int length = reader.ReadInt32(); long end = memory.Position + length;
                    if (id == "fmt ") { Require(reader.ReadInt16() == 1 && reader.ReadInt16() == 1, "Fixture requires mono PCM16"); rate = reader.ReadInt32(); reader.ReadInt32(); reader.ReadInt16(); Require(reader.ReadInt16() == 16, "Fixture PCM16"); }
                    if (id == "data") { data = reader.ReadBytes(length); }
                    memory.Position = end + length % 2;
                }
                Require(rate == 16000 && data.Length > 0, "Fixture 16kHz wave");
                for (int offset = 0; offset < data.Length; offset += 1024)
                {
                    token.ThrowIfCancellationRequested(); int count = Math.Min(512, (data.Length - offset) / 2); float[] samples = new float[count];
                    for (int index = 0; index < count; index++) { samples[index] = System.Buffers.Binary.BinaryPrimitives.ReadInt16LittleEndian(data.AsSpan(offset + index * 2)) / 32768f; }
                    yield return new(16000, samples); await Task.Delay(32, token);
                }
            }
            else
            {
                while (true) { token.ThrowIfCancellationRequested(); yield return new(16000, Enumerable.Repeat(0.1f, 1600).ToArray()); await Task.Delay(20, token); }
            }
        }
    }
    private sealed class DraftRecognizer : ISpeechRecognizer
    {
        public async IAsyncEnumerable<SpeechRecognitionEvent> RecognizeAsync(SpeechLanguage language,
            IAsyncEnumerable<SpeechAudioChunk> input, [EnumeratorCancellation] CancellationToken token)
        {
            await foreach (SpeechAudioChunk _ in input.WithCancellation(token)) { yield return new(SpeechRecognitionKind.Partial, "1", "Do not take it"); }
            yield return new(SpeechRecognitionKind.Final, "1", "Do not take the moonwort");
        }
    }
    private sealed class Participation : ICharacterTurnParticipationModel
    {
        public ValueTask<CharacterTurnParticipationDecision> DecideAsync(CharacterTurnParticipationConsiderationRequest request,
            CancellationToken token) => ValueTask.FromResult(new CharacterTurnParticipationDecision(request.ScheduledCharacter.Identity.Id, request.IsDirectlyAddressed ? CharacterTurnParticipationKind.Respond : CharacterTurnParticipationKind.RemainSilent,
                request.IsDirectlyAddressed ? null : request.ScheduledCharacter.Goals.Entries[0]));
    }
    private sealed class Reply : ICharacterModel
    {
        internal int Calls;
        public async IAsyncEnumerable<CharacterModelStreamUpdate> StreamResponseAsync(CharacterModelGenerationRequest request,
            [EnumeratorCancellation] CancellationToken token)
        {
            Calls++;
            await Task.CompletedTask; token.ThrowIfCancellationRequested(); const string words = "The reply remains in the transcript.";
            CharacterTurnResponse result = new(request.TurnRequest, CharacterTurnDisposition.Accept,
                new DialogueDelivery(new DialogueUtterance(request.TurnRequest.RespondingCharacterId, words), DialogueDeliveryMode.OrdinarySpeech),
                new CharacterIntention(CharacterIntentionType.Wait, null), [], []);
            yield return new CharacterModelTextDelta(words); yield return new CharacterModelStreamTermination(CharacterModelStreamTerminationStatus.Completed, result, null);
        }
    }
    private sealed class Synthesizer : ISpeechSynthesizer
    {
        public async IAsyncEnumerable<SpeechAudioChunk> SynthesizeAsync(SpeechSynthesisRequest request, [EnumeratorCancellation] CancellationToken token)
        {
            for (int block = 0; block < 80; block++)
            { await Task.Delay(2, token); yield return new(24000, Enumerable.Range(block * 1200, 1200).Select(i => (float)(0.1 * Math.Sin(2 * Math.PI * 440 * i / 24000))).ToArray()); }
        }
    }
}
