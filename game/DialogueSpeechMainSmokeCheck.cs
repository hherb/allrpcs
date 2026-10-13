using System.Runtime.CompilerServices;
using System.Text.Json;
using Allrpcs.Application.Audio;
using Allrpcs.Application.Dialogue;
using Allrpcs.Application.Speech;
using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Dialogue;
using Godot;

namespace Allrpcs.Game;

/// <summary>Opt-in actual Main integration checks; synthetic inputs do not claim physical-device qualification.</summary>
public partial class DialogueSpeechMainSmokeCheck : Node
{
    private static readonly JsonSerializerOptions ReportOptions = new() { WriteIndented = true };
    private readonly List<string> checks = [];
    private DialogueSpeechController speech = null!;
    private CharacterTurnTranscriptController transcript = null!;
    private TemporaryCharacterModelSource modelSource = null!;
    private DialogueInputController input = null!;
    private LineEdit text = null!;
    private string mode = "headless";
    private readonly RecordingSynthesizer fake = new();
    private bool pocket;
    private int submissions;
    private AudioEffectRecord? recording;
    private int recordEffectIndex;

    /// <inheritdoc />
    public override async void _Ready()
    {
        if (OS.GetEnvironment("ALLRPCS_DIALOGUE_SPEECH_CHECK") != "1") { return; }
        await Frames(5);
        try
        {
            mode = OS.GetEnvironment("ALLRPCS_DIALOGUE_SPEECH_MODE");
            pocket = mode.StartsWith("pocket", StringComparison.Ordinal);
            speech = GetNode<DialogueSpeechController>("../DialogueSpeechController");
            transcript = GetNode<CharacterTurnTranscriptController>("../CharacterTurnTranscriptController");
            modelSource = GetNode<TemporaryCharacterModelSource>("../TemporaryCharacterModelSource");
            input = GetNode<DialogueInputController>("../DialogueInputController");
            text = GetNode<LineEdit>("../DialogueOverlay/Panel/Input");
            CharacterBody2D healer = GetNode<CharacterBody2D>("../Healer");
            CharacterBody2D seeker = GetNode<CharacterBody2D>("../Seeker");
            healer.SetPhysicsProcess(false); seeker.SetPhysicsProcess(false);
            seeker.GlobalPosition = healer.GlobalPosition + new Vector2(12, 0);
            if (!pocket && mode != "unavailable") { speech.Configure(fake); }
            if (mode != "headless" && mode != "unavailable")
            {
                GetWindow().Size = mode.EndsWith("1920", StringComparison.Ordinal) ? new(1920, 1080) : new(1280, 720);
                GetWindow().GrabFocus(); await Frames(8);
            }
            AudioSettingsController audio = GetNode<AudioSettingsController>("../AudioSettingsController");
            audio.SetPreferences(new AudioPreferences(AudioPreferences.Defaults().Channels.Select(channel =>
                new AudioChannelSetting(channel.Category, channel.Gain,
                    channel.Category is not (AudioCategory.Master or AudioCategory.Speech)))));
            if (pocket)
            {
                recording = new(); recordEffectIndex = AudioServer.GetBusEffectCount(0);
                AudioServer.AddBusEffect(0, recording); recording.SetRecordingActive(true);
            }
            await OpenDialogueByKeyboard();
            await Submit("The garden is quiet.");
            Require(speech.PushedSamples == 0 && !speech.Playing, "Disabled speech leaked audio");
            checks.Add("disabled-typed-dialogue-unchanged");
            if (mode == "unavailable")
            {
                speech.SetEnabled(true);
                await Submit("A text reply remains available.");
                Require(speech.Status.Contains("unavailable", StringComparison.Ordinal) && !speech.Playing,
                    "Unavailable runtime lost text fallback");
                checks.Add("missing-runtime-text-fallback");
            }
            else
            {
                if (DisplayServer.GetName() == "headless") { speech.SetEnabled(true); }
                else
                {
                    await Click(text.GetParent().FindChild("SpeakReplies", true, false) as Control
                        ?? throw new InvalidOperationException("Speech toggle missing"));
                    checks.Add("native-pointer-speech-toggle");
                }
                string reply = mode.Contains("german", StringComparison.Ordinal)
                    ? "Bitte warte hier. Wir brauchen sauberes Wasser und Mondraute."
                    : "Please wait here. We need clean water and moonwort.";
                if (mode.Contains("german", StringComparison.Ordinal)) { speech.SetLanguage(SpeechLanguage.German); }
                long before = speech.PushedSamples;
                await Submit(reply, reply);
                await Wait(() => speech.PushedSamples > before);
                Require(speech.Playing, "Committed reply did not start PCM playback");
                checks.Add("committed-reply-pcm-speech-bus");
                object transcriptAfter = transcript.Transcript!;
                if (!pocket)
                {
                    Require(fake.Requests[^1].Text == reply && fake.Requests[^1].VoiceProfile == "default",
                        "Synthesis did not retain exact committed words/profile");
                    await Frames(10);
                    Require(fake.Requests.Count == 1, "Reply was synthesized twice");
                    checks.Add("exact-text-and-once-per-commit");
                    if (DisplayServer.GetName() == "headless") { speech.StopSpeech(); }
                    else
                    {
                        await Click(text.GetParent().FindChild("StopSpeech", true, false) as Control
                            ?? throw new InvalidOperationException("Speech stop missing"));
                        checks.Add("native-pointer-speech-stop");
                    }
                    long stopped = speech.PushedSamples;
                    await Frames(8);
                    Require(!speech.Playing && speech.PushedSamples == stopped && ReferenceEquals(transcript.Transcript, transcriptAfter),
                        "Stop changed transcript or replayed PCM");
                    checks.Add("stop-clears-buffer-preserves-transcript");
                    await Submit("A newer accepted player turn interrupts speech.");
                    await Wait(() => speech.Playing);
                    text.Text = "interrupt";
                    text.EmitSignal(LineEdit.SignalName.TextSubmitted, "interrupt");
                    await Frames(2);
                    Require(!speech.Playing, "New player delivery did not interrupt playback");
                    checks.Add("new-player-submission-interrupts");
                    speech.SetLanguage(SpeechLanguage.German);
                    await Submit("The final response still has its original text.");
                    await Wait(() => fake.Requests.Count >= 3);
                    Require(fake.Requests[^1].Language == SpeechLanguage.German, "Explicit German selection ignored");
                    await Wait(() => speech.Playing);
                    object retainedTranscript = transcript.Transcript!;
                    audio.SetPreferences(new AudioPreferences(AudioPreferences.Defaults().Channels.Select(channel =>
                        new AudioChannelSetting(channel.Category, channel.Gain, channel.Category == AudioCategory.Master))));
                    Require(!speech.Playing && ReferenceEquals(transcript.Transcript, retainedTranscript), "Master mute did not stop speech safely");
                    checks.Add("master-mute-retires-pcm-without-gameplay-change");
                    speech.SetEnabled(false);
                    Require(!speech.Playing, "Disable did not flush playback");
                    checks.Add("language-choice-disable-and-text-fallback");
                }
                else
                {
                    await Wait(() => !speech.Playing, 45000);
                    Require(speech.Status == "Reply speech ready", "Pocket completion failed");
                    checks.Add("real-pocket-complete-and-drain");
                    await Submit(reply + " " + reply + " " + reply, reply + " " + reply + " " + reply);
                    await Wait(() => speech.Playing, 45000);
                    await Frames(8);
                    if (DisplayServer.GetName() == "headless") { speech.StopSpeech(); }
                    else
                    {
                        await Click(text.GetParent().FindChild("StopSpeech", true, false) as Control
                            ?? throw new InvalidOperationException("Speech stop missing"));
                        checks.Add("native-pointer-speech-stop");
                    }
                    long stopped = speech.PushedSamples;
                    await Frames(12);
                    Require(!speech.Playing && speech.PushedSamples == stopped, "Pocket interruption leaked obsolete PCM");
                    checks.Add("real-pocket-interruption");
                }
            }
            GetNode<CanvasLayer>("../DialogueOverlay").Visible = true;
            text.GrabFocus();
            string[] focus = ["Whispering", "SpeakReplies", "SpeechLanguage", "StopSpeech", "Submit", "Close", "Input"];
            foreach (string name in focus)
            {
                await Press(Key.Tab);
                Require(GetViewport().GuiGetFocusOwner()?.Name == name, "Speech/typed focus cycle: " + name);
            }
            Require(!ControlInputContext.WorldAllowed(this), "Speech GUI failed world gate");
            checks.Add("keyboard-speech-and-typed-focus-gate");
            string output = OS.GetEnvironment("ALLRPCS_DIALOGUE_SPEECH_REPORT");
            if (DisplayServer.GetName() != "headless")
            {
                await Frames(90);
                await ToSignal(RenderingServer.Singleton, RenderingServer.SignalName.FramePostDraw);
                GetViewport().GetTexture().GetImage().SavePng(Path.ChangeExtension(output, ".png"));
            }
            if (recording is not null)
            {
                recording.SetRecordingActive(false);
                recording.GetRecording().SaveToWav(Path.ChangeExtension(output, ".wav"));
                AudioServer.RemoveBusEffect(0, recordEffectIndex);
            }
            await Frames(2);
            speech.GetParent().RemoveChild(speech);
            await speech.Shutdown;
            Require(speech.Shutdown.IsCompletedSuccessfully, "Speech worker shutdown failed");
            speech.Free();
            checks.Add("scene-exit-provider-retirement");
            File.WriteAllText(output, JsonSerializer.Serialize(new
            {
                status = "passed",
                mode,
                checks,
                size = new[] { GetWindow().Size.X, GetWindow().Size.Y },
                realPocket = pocket,
                provenance = "synthetic keyboard/typed Main input; actual native PCM playback when rendered; no microphone or physical device test"
            },
                ReportOptions));
            GD.Print("PASS: dialogue speech Main: " + mode); GetTree().Quit();
        }
        catch (Exception exception)
        { GD.PrintErr("FAIL: dialogue speech Main: " + exception.Message); GetTree().Quit(1); }
    }

    private async Task OpenDialogueByKeyboard()
    {
        await Press(Key.Tab); await Press(Key.Tab); await Press(Key.Tab); await Press(Key.Enter);
        Require(GetViewport().GuiGetFocusOwner() == text, "Ordinary Dialogue entry focus");
    }

    private async Task Submit(string playerText, string replyText = "The reply remains in the transcript.")
    {
        int before = transcript.Transcript!.Entries.Count;
        submissions++;
        Require(GetNode<TemporaryCharacterTurnParticipationModelSource>("../TemporaryCharacterTurnParticipationModelSource")
            .TrySupplyModel(new ParticipationModel()), "Participation fixture not available");
        Require(modelSource.TrySupplyModel(new ReplyModel(replyText)), "Model fixture not available at submission " + submissions);
        text.Text = playerText; text.EmitSignal(LineEdit.SignalName.TextSubmitted, playerText);
        await Wait(() => transcript.Transcript!.Entries.Count >= before + 2);
        Require(input.LastSubmittedUtterance?.Text == playerText, "Typed owner changed submitted words");
        await Frames(4);
    }
    private async Task Click(Control control)
    {
        Vector2 point = GetViewport().GetFinalTransform() * control.GetGlobalRect().GetCenter();
        Input.ParseInputEvent(new InputEventMouseMotion { Position = point, GlobalPosition = point });
        await Frames(2);
        Input.ParseInputEvent(new InputEventMouseButton
        {
            Position = point,
            GlobalPosition = point,
            ButtonIndex = MouseButton.Left,
            Pressed = true
        });
        await Frames(2);
        Input.ParseInputEvent(new InputEventMouseButton
        {
            Position = point,
            GlobalPosition = point,
            ButtonIndex = MouseButton.Left,
            Pressed = false
        });
        await Frames(2);
    }
    private async Task Press(Key key)
    {
        Input.ParseInputEvent(new InputEventKey { Keycode = key, PhysicalKeycode = key, Pressed = true });
        await Frames(2);
        Input.ParseInputEvent(new InputEventKey { Keycode = key, PhysicalKeycode = key, Pressed = false });
        await Frames(2);
    }
    private async Task Frames(int count) { for (int index = 0; index < count; index++) { await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame); } }
    private async Task Wait(Func<bool> ready, ulong milliseconds = 15000)
    {
        ulong deadline = Time.GetTicksMsec() + milliseconds;
        while (!ready()) { Require(Time.GetTicksMsec() < deadline, "Fixture deadline"); await Frames(1); }
    }
    private static void Require(bool value, string message) { if (!value) { throw new InvalidOperationException(message); } }

    private sealed class ParticipationModel : ICharacterTurnParticipationModel
    {
        public ValueTask<CharacterTurnParticipationDecision> DecideAsync(CharacterTurnParticipationConsiderationRequest request,
            CancellationToken cancellationToken) => ValueTask.FromResult(new CharacterTurnParticipationDecision(
                request.ScheduledCharacter.Identity.Id, CharacterTurnParticipationKind.Respond, null));
    }
    private sealed class ReplyModel(string reply) : ICharacterModel
    {
        public async IAsyncEnumerable<CharacterModelStreamUpdate> StreamResponseAsync(CharacterModelGenerationRequest request,
            [EnumeratorCancellation] CancellationToken cancellationToken)
        {
            await Task.CompletedTask;
            CharacterTurnResponse response = new(request.TurnRequest, CharacterTurnDisposition.Accept,
                new DialogueDelivery(new DialogueUtterance(request.TurnRequest.RespondingCharacterId, reply), DialogueDeliveryMode.OrdinarySpeech),
                new CharacterIntention(CharacterIntentionType.Wait, null), [], []);
            yield return new CharacterModelTextDelta(reply);
            yield return new CharacterModelStreamTermination(CharacterModelStreamTerminationStatus.Completed, response, null);
        }
    }
    private sealed class RecordingSynthesizer : ISpeechSynthesizer
    {
        internal List<SpeechSynthesisRequest> Requests { get; } = [];
        public async IAsyncEnumerable<SpeechAudioChunk> SynthesizeAsync(SpeechSynthesisRequest request,
            [EnumeratorCancellation] CancellationToken cancellationToken)
        {
            Requests.Add(request);
            for (int block = 0; block < 80; block++)
            {
                await Task.Delay(2, cancellationToken);
                float[] samples = Enumerable.Range(block * 1200, 1200).Select(index => (float)(0.1 * Math.Sin(2 * Math.PI * 440 * index / 24000))).ToArray();
                yield return new SpeechAudioChunk(24000, samples);
            }
        }
    }
}
