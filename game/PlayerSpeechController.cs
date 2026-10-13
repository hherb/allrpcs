using System.Text.Json;
using Allrpcs.Application.Speech;
using Allrpcs.Infrastructure.Speech;
using Godot;

namespace Allrpcs.Game;

/// <summary>Opt-in local microphone drafts; existing explicit Submit remains the only dialogue owner.</summary>
public partial class PlayerSpeechController : Node
{
    private PlayerSpeechSession? session;
    private EditableSpeechDraft? draft;
    private LineEdit input = null!;
    private TemporaryDialogueInputPanel panel = null!;
    private DialogueSpeechController reply = null!;
    private SaveSnapshotRestorationController restoration = null!;
    private DialogueInputController delivery = null!;
    private OptionButton language = null!;
    private CheckButton listen = null!;
    private Button talk = null!;
    private Button stop = null!;
    private Button submit = null!;
    private Label indicator = null!;
    private bool updating;
    private bool finishing;
    private bool held;
    private bool captured;
    private object? lastDelivery;
    internal Task Shutdown { get; private set; } = Task.CompletedTask;
    internal bool Listening => session?.Status == SpeechPlaybackStatus.Generating;
    internal string Status => indicator.Text;
    internal string StopReason { get; private set; } = "none";
    internal int VoiceStarts { get; private set; }
    internal new bool Ready => session is not null;

    /// <inheritdoc />
    public override void _Ready()
    {
        panel = GetNode<TemporaryDialogueInputPanel>("../DialogueOverlay/Panel");
        input = panel.GetNode<LineEdit>("Input"); submit = panel.GetNode<Button>("Submit");
        reply = GetNode<DialogueSpeechController>("../DialogueSpeechController");
        restoration = GetNode<SaveSnapshotRestorationController>("../SaveSnapshotRestorationController");
        delivery = GetNode<DialogueInputController>("../DialogueInputController");
        language = (OptionButton)panel.FindChild("SpeechLanguage", true, false);
        BuildToolbar(); ConfigureLocal();
        input.TextChanged += OnEdited; input.TextSubmitted += OnSubmitted; submit.Pressed += OnSubmit;
        panel.Closed += OnClose; restoration.PresentationReset += OnClose;
        language.ItemSelected += OnLanguage;
        lastDelivery = delivery.LastSubmittedDelivery;
        SetStatus("Microphone off");
    }
    private void ConfigureLocal()
    {
        string root = Path.GetDirectoryName(Path.TrimEndingDirectorySeparator(ProjectSettings.GlobalizePath("res://"))) ?? "";
        string configured = OS.GetEnvironment("ALLRPCS_PLAYER_SPEECH_CONFIG");
        string path = configured.Length == 0 ? Path.Combine(root, "artifacts", "player-speech-runtime.json") : configured;
        try
        {
            if (!Path.IsPathFullyQualified(path) || !File.Exists(path) || new FileInfo(path).Length > 16384) { return; }
            using JsonDocument document = JsonDocument.Parse(File.ReadAllText(path));
            JsonElement value = document.RootElement;
            string[] fields = ["schemaVersion", "pythonPath", "workerPath", "englishModelPath", "germanModelPath", "manifestPath"];
            if (value.GetProperty("schemaVersion").GetInt32() != 1
                || !value.EnumerateObject().Select(p => p.Name).Order(StringComparer.Ordinal).SequenceEqual(fields.Order(StringComparer.Ordinal))) { return; }
            PlayerSpeechWorkerOptions options = new(value.GetProperty("pythonPath").GetString()!, value.GetProperty("workerPath").GetString()!,
                value.GetProperty("englishModelPath").GetString()!, value.GetProperty("germanModelPath").GetString()!, value.GetProperty("manifestPath").GetString()!);
            if (!File.Exists(options.Python) || !File.Exists(options.Worker) || !File.Exists(options.Manifest)
                || !Directory.Exists(options.English) || !Directory.Exists(options.German)) { return; }
            IPlayerSpeechWorkerFactory factory = new PlayerSpeechWorkerFactory(options);
            session = new(new PortAudioMicrophoneCapture(factory), new MoonshineSpeechRecognizer(factory));
        }
        catch (Exception error) when (error is IOException or UnauthorizedAccessException or JsonException or ArgumentException
            or KeyNotFoundException or InvalidOperationException)
        { }
    }
    internal void Configure(IMicrophoneCapture capture, ISpeechRecognizer recognizer)
    { if (session is not null) { throw new InvalidOperationException("Input speech already configured."); } session = new(capture, recognizer); }
    private void BuildToolbar()
    {
        PanelContainer background = new() { Name = "PlayerSpeechToolbar", Position = new(0, -100), Size = new(936, 44) };
        background.AddThemeStyleboxOverride("panel", new StyleBoxFlat
        {
            BgColor = new(0.055f, 0.067f, 0.082f, 0.97f),
            ContentMarginLeft = 10,
            ContentMarginRight = 10,
            ContentMarginTop = 4,
            ContentMarginBottom = 4
        });
        panel.AddChild(background);
        HBoxContainer row = new(); row.AddThemeConstantOverride("separation", 12); background.AddChild(row);
        talk = new() { Name = "HoldToTalk", Text = "Hold to talk", AccessibilityName = "Hold to talk into a draft" };
        talk.AccessibilityDescription = "Hold the button or Space while focused. Release to finalize. Submit sends the editable draft.";
        row.AddChild(talk); talk.ButtonDown += OnTalkDown; talk.ButtonUp += OnTalkUp; talk.FocusExited += OnTalkUp;
        listen = new() { Name = "ListenToPlayer", Text = "Listen (headphones)", AccessibilityName = "Listen for player speech with headphones" };
        listen.AccessibilityDescription = "Opt in for up to 30 seconds. Use headphones to avoid recognizing game speakers. Submit remains explicit.";
        row.AddChild(listen); listen.Toggled += OnListen;
        stop = new() { Name = "StopMicrophone", Text = "Stop mic", AccessibilityName = "Stop microphone and keep draft" };
        row.AddChild(stop); stop.Pressed += OnClose;
        indicator = new()
        {
            Name = "MicrophoneState",
            SizeFlagsHorizontal = Control.SizeFlags.ExpandFill,
            AccessibilityName = "Microphone and recognition state",
            AccessibilityLive = DisplayServer.AccessibilityLiveMode.Polite
        };
        row.AddChild(indicator);
        panel.RegisterSpeechControls([(Control)panel.FindChild("SpeakReplies", true, false),
            language,
            (Control)panel.FindChild("StopSpeech", true, false),
            talk,
            listen,
            stop]);
    }
    private void Start(bool held)
    {
        StopCapture("replacement");
        if (session is null) { SetStatus("Microphone unavailable · type instead"); SetListening(false); return; }
        draft = new(input.Text); finishing = false; captured = false;
        lastDelivery = delivery.LastSubmittedDelivery;
        if (held) { reply.StopSpeech("Reply speech interrupted"); }
        session.Start(language.Selected == 0 ? SpeechLanguage.English : SpeechLanguage.German);
        SetStatus("Starting microphone…");
    }
    private void OnTalkDown() { SetListening(false); Start(true); held = Listening; }
    private void OnTalkUp() { if (held && Listening) { finishing = true; session!.Finish(); SetStatus("Finalizing draft…"); } held = false; }
    private void OnListen(bool value)
    {
        if (updating) { return; }
        if (value) { Start(false); SetListening(session is not null); }
        else { StopCapture(); }
    }
    private void SetListening(bool value) { if (!GodotObject.IsInstanceValid(listen)) { return; } updating = true; listen.ButtonPressed = value; updating = false; }
    private void OnEdited(string text) { if (!updating) { draft?.Edited(text); } }
    private void OnSubmitted(string _) => StopCapture("submitted");
    private void OnSubmit() => StopCapture("submitted");
    private void OnClose() => StopCapture("closed");
    private void OnLanguage(long _) => StopCapture("language");
    internal void StopCapture(string reason = "stop")
    {
        StopReason = reason;
        session?.Stop(); draft = null; finishing = false; held = false; SetListening(false);
        SetStatus(session?.Retiring == true ? "Stopping microphone…" : "Microphone off · draft kept");
    }
    private void SetStatus(string text) { if (!GodotObject.IsInstanceValid(indicator)) { return; } if (indicator.Text != text) { indicator.Text = text; } }
    /// <inheritdoc />
    public override void _Process(double delta)
    {
        _ = delta;
        if (!ReferenceEquals(lastDelivery, delivery.LastSubmittedDelivery))
        { lastDelivery = delivery.LastSubmittedDelivery; StopCapture("delivery"); }
        if (!panel.IsVisibleInTree() && Listening) { StopCapture("hidden"); }
        if (session is null) { return; }
        if (draft is not null)
        {
            while (session.TryRead(out SpeechRecognitionEvent? value))
            {
                if (value!.Kind == SpeechRecognitionKind.CaptureStarted) { captured = true; }
                if (value.Kind == SpeechRecognitionKind.VoiceStarted)
                { VoiceStarts++; reply.StopSpeech("Reply speech interrupted"); }
                try
                {
                    if (draft.Apply(value)) { updating = true; input.Text = draft.Text; updating = false; }
                }
                catch (InvalidOperationException) { StopCapture(); SetStatus("Draft limit reached · text kept"); return; }
            }
            if (session.Status == SpeechPlaybackStatus.Failed)
            { StopCapture(); SetStatus("Microphone unavailable · check permission/device"); }
            else if (session.Status == SpeechPlaybackStatus.Completed)
            { draft = null; SetListening(false); SetStatus(session.Retiring ? "Stopping microphone…" : captured ? "Microphone off · review draft, then Submit" : "Microphone off · no audio captured"); }
            else if (Listening && !finishing)
            { SetStatus(captured ? draft.ManuallyEdited ? "Listening · edits protected" : "Listening · review before Submit" : "Starting microphone…"); }
        }
        else if (indicator.Text == "Stopping microphone…" && !session.Retiring) { SetStatus("Microphone off · draft kept"); }
    }
    /// <inheritdoc />
    public override void _Notification(int what)
    {
        if (indicator is not null && what is (int)NotificationApplicationFocusOut or (int)NotificationWMWindowFocusOut)
        { StopCapture("focus"); }
    }
    /// <inheritdoc />
    public override void _ExitTree()
    {
        if (GodotObject.IsInstanceValid(input)) { input.TextChanged -= OnEdited; input.TextSubmitted -= OnSubmitted; }
        if (GodotObject.IsInstanceValid(submit)) { submit.Pressed -= OnSubmit; }
        panel.Closed -= OnClose; restoration.PresentationReset -= OnClose;
        if (GodotObject.IsInstanceValid(language)) { language.ItemSelected -= OnLanguage; }
        StopCapture(); Shutdown = session is null ? Task.CompletedTask : session.DisposeAsync().AsTask();
    }
}
