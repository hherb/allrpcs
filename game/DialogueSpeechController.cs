using System.Text.Json;
using Allrpcs.Application.Audio;
using Allrpcs.Application.Speech;
using Allrpcs.Domain.Dialogue;
using Allrpcs.Infrastructure.Speech;
using Godot;

namespace Allrpcs.Game;

/// <summary>Optional exact committed-reply synthesis; audio failure never changes dialogue authority.</summary>
public partial class DialogueSpeechController : Node
{
    private const int SpeechRate = 24000;
    private const float BufferSeconds = 0.25f;
    private TemporaryCharacterTurnResponseSpeechRecorder recorder = null!;
    private DialogueInputController input = null!;
    private SaveSnapshotRestorationController restoration = null!;
    private AudioSettingsController audio = null!;
    private CharacterBody2D seeker = null!;
    private AudioStreamPlayer player = null!;
    private SpeechPlaybackSession? session;
    private PocketSpeechSynthesizer? providerOwner;
    private AudioStreamGeneratorPlayback? playback;
    private SpeechAudioChunk? chunk;
    private int offset;
    private int capacity;
    private ulong finalDrainDeadline;
    private long? duckToken;
    private DialogueDelivery? lastReply;
    private DialogueDelivery? lastPlayerDelivery;
    private RecordedReplyPresentation? active;
    private Label status = null!;
    private CheckButton enabled = null!;
    private OptionButton language = null!;
    private string outputDevice = "";
    internal long PushedSamples { get; private set; }
    internal bool Playing => player.Playing;
    internal Task Shutdown { get; private set; } = Task.CompletedTask;
    internal string Status => status.Text;

    /// <inheritdoc />
    public override void _Ready()
    {
        recorder = GetNode<TemporaryCharacterTurnResponseSpeechRecorder>("../TemporaryCharacterTurnResponseSpeechRecorder");
        input = GetNode<DialogueInputController>("../DialogueInputController");
        restoration = GetNode<SaveSnapshotRestorationController>("../SaveSnapshotRestorationController");
        audio = GetNode<AudioSettingsController>("../AudioSettingsController");
        seeker = GetNode<CharacterBody2D>("../Seeker");
        player = new() { Name = "ReplySpeech", Bus = "Speech" }; AddChild(player);
        BuildToolbar();
        recorder.ReplyRecorded += Receive;
        restoration.PresentationReset += OnRestored;
        audio.Changed += OnAudioChanged;
        outputDevice = AudioServer.OutputDevice;
        ConfigureLocalRuntime();
        SetStatus("Reply speech off");
    }

    private void ConfigureLocalRuntime()
    {
        string root = Path.GetDirectoryName(Path.TrimEndingDirectorySeparator(ProjectSettings.GlobalizePath("res://"))) ?? "";
        string configured = OS.GetEnvironment("ALLRPCS_SPEECH_CONFIG");
        string path = string.IsNullOrEmpty(configured) ? Path.Combine(root, "artifacts", "speech-runtime.json") : configured;
        try
        {
            if (!Path.IsPathFullyQualified(path) || !File.Exists(path) || new FileInfo(path).Length > 16384) { return; }
            using JsonDocument document = JsonDocument.Parse(File.ReadAllText(path));
            JsonElement value = document.RootElement;
            string[] fields = ["schemaVersion", "pythonPath", "workerPath", "assetsPath", "profilesPath"];
            string[] found = value.EnumerateObject().Select(property => property.Name).ToArray();
            if (value.GetProperty("schemaVersion").GetInt32() != 1 || found.Length != fields.Length
                || !found.Order(StringComparer.Ordinal).SequenceEqual(fields.Order(StringComparer.Ordinal))) { return; }
            PocketSpeechWorkerOptions options = new(value.GetProperty("pythonPath").GetString()!,
                value.GetProperty("workerPath").GetString()!, value.GetProperty("assetsPath").GetString()!,
                value.GetProperty("profilesPath").GetString()!);
            if (!File.Exists(options.PythonPath) || !File.Exists(options.WorkerPath)
                || !Directory.Exists(options.AssetsPath) || !File.Exists(options.ProfilesPath)) { return; }
            PocketSpeechSynthesizer provider = new(new PocketSpeechWorkerProcessFactory(options));
            session = new(provider); providerOwner = provider;
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException or JsonException
            or ArgumentException or KeyNotFoundException or InvalidOperationException)
        { }
    }

    internal void Configure(ISpeechSynthesizer provider)
    {
        if (session is not null) { throw new InvalidOperationException("Speech is already configured."); }
        session = new(provider);
    }

    internal void SetEnabled(bool value) { enabled.ButtonPressed = value; OnEnabledChanged(value); }
    internal void SetLanguage(SpeechLanguage value) { language.Select(value == SpeechLanguage.English ? 0 : 1); StopSpeech("Speech language changed"); }

    private void Receive(RecordedReplyPresentation notice)
    {
        if (ReferenceEquals(lastReply, notice.Delivery) || !ReferenceEquals(notice.Event.Delivery, notice.Delivery)
            || !notice.Event.Audience.ListenerCharacterIds.Contains(notice.SeekerId)
            || input.SpeakerCharacterId != notice.SeekerId || !GodotObject.IsInstanceValid(notice.Speaker)) { return; }
        lastReply = notice.Delivery;
        StopSpeech("Reply speech off");
        if (!enabled.ButtonPressed) { return; }
        if (session is null) { SetStatus("Speech unavailable · text retained"); return; }
        if (!Audible() || seeker.GlobalPosition.DistanceTo(notice.Speaker.GlobalPosition) > notice.Range)
        { SetStatus("Reply speech not audible"); return; }
        active = notice;
        try
        {
            session.Start(new(notice.Delivery.Utterance.Text,
                language.Selected == 0 ? SpeechLanguage.English : SpeechLanguage.German, "default"));
            SetStatus("Preparing reply speech");
        }
        catch (Exception exception) when (exception is ArgumentException or InvalidOperationException or ObjectDisposedException)
        { StopSpeech("Speech unavailable · text retained"); }
    }

    /// <inheritdoc />
    public override void _Process(double delta)
    {
        _ = delta;
        if (!ReferenceEquals(input.LastSubmittedDelivery, lastPlayerDelivery))
        {
            lastPlayerDelivery = input.LastSubmittedDelivery;
            if (active is not null) { StopSpeech("Reply speech interrupted"); }
        }
        if (AudioServer.OutputDevice != outputDevice)
        { outputDevice = AudioServer.OutputDevice; StopSpeech("Audio device changed · text retained"); }
        if (active is null || session is null) { return; }
        if (playback is not null && !player.Playing)
        { StopSpeech("Audio unavailable · text retained"); return; }
        if (!GodotObject.IsInstanceValid(active.Speaker) || !Audible()
            || seeker.GlobalPosition.DistanceTo(active.Speaker.GlobalPosition) > active.Range)
        { StopSpeech("Reply speech not audible"); return; }
        if (session.Status == SpeechPlaybackStatus.Failed)
        { StopSpeech("Speech unavailable · text retained"); return; }
        try
        {
            while (true)
            {
                if (chunk is null && !session.TryRead(out chunk)) { break; }
                if (chunk!.SampleRate != SpeechRate) { StopSpeech("Speech unavailable · text retained"); return; }
                if (playback is null) { BeginPlayback(); }
                int count = Math.Min(playback!.GetFramesAvailable(), chunk.Samples.Count - offset);
                if (count == 0) { break; }
                Vector2[] frames = new Vector2[count];
                for (int index = 0; index < count; index++) { float sample = chunk.Samples[offset + index]; frames[index] = new(sample, sample); }
                if (!playback.PushBuffer(frames)) { StopSpeech("Speech unavailable · text retained"); return; }
                PushedSamples += count; offset += count;
                finalDrainDeadline = Time.GetTicksMsec() + (ulong)(1000 * BufferSeconds + 100);
                if (offset == chunk.Samples.Count) { chunk = null; offset = 0; }
            }
            if (session.Status == SpeechPlaybackStatus.Completed && chunk is null && playback is not null
                && playback.GetFramesAvailable() >= capacity && Time.GetTicksMsec() >= finalDrainDeadline)
            { StopSpeech("Reply speech ready"); }
        }
        catch (Exception exception) when (exception is InvalidOperationException or ObjectDisposedException)
        { StopSpeech("Speech unavailable · text retained"); }
    }

    private void BeginPlayback()
    {
        player.Stream = new AudioStreamGenerator
        {
            MixRate = SpeechRate,
            MixRateMode = AudioStreamGenerator.AudioStreamGeneratorMixRate.Custom,
            BufferLength = BufferSeconds
        };
        player.Play();
        playback = player.GetStreamPlayback() as AudioStreamGeneratorPlayback
            ?? throw new InvalidOperationException("Speech playback unavailable.");
        capacity = playback.GetFramesAvailable();
        duckToken = audio.Ducking.Begin(AudioForeground.Speech);
        SetStatus("Speaking reply");
    }

    internal void StopSpeech(string message = "Reply speech stopped")
    {
        session?.Stop();
        player.Stop();
        player.Stream = null;
        playback = null; chunk = null; offset = 0; active = null;
        if (duckToken is long token) { audio.Ducking.End(token); duckToken = null; }
        SetStatus(message);
    }

    private bool Audible() => audio.Preferences.Channels.Where(channel => channel.Category is AudioCategory.Master or AudioCategory.Speech)
        .All(channel => !channel.Muted && channel.Gain > 0) && AudioServer.GetBusIndex("Speech") >= 0;
    private void OnEnabledChanged(bool value) => StopSpeech(value
        ? session is null ? "Speech unavailable · text retained" : "Reply speech ready"
        : "Reply speech off");
    private void OnRestored() => StopSpeech("Reply speech stopped");
    private void OnAudioChanged() { if (!Audible()) { StopSpeech("Reply speech not audible"); } }
    private void SetStatus(string message) { if (status.Text != message) { status.Text = message; } }

    /// <inheritdoc />
    public override void _Input(InputEvent @event)
    {
        if (ControlInputContext.WorldAllowed(this) && ControlInputMatcher.Pressed(@event, "character_turn_cancel"))
        { StopSpeech("Reply speech interrupted"); }
    }

    /// <inheritdoc />
    public override void _Notification(int what)
    {
        if (player is not null && what is (int)NotificationApplicationFocusOut or (int)NotificationWMWindowFocusOut)
        { StopSpeech("Reply speech interrupted"); }
    }

    private void BuildToolbar()
    {
        TemporaryDialogueInputPanel panel = GetNode<TemporaryDialogueInputPanel>("../DialogueOverlay/Panel");
        PanelContainer background = new() { Name = "SpeechToolbar", Position = new(0, -52), Size = new(936, 44) };
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
        enabled = new() { Name = "SpeakReplies", Text = "Speak replies", AccessibilityName = "Speak character replies" };
        row.AddChild(enabled); enabled.Toggled += OnEnabledChanged;
        language = new() { Name = "SpeechLanguage", AccessibilityName = "Speech language" };
        language.AddItem("English"); language.AddItem("German"); row.AddChild(language);
        language.ItemSelected += _ => StopSpeech("Speech language changed");
        Button stop = new() { Name = "StopSpeech", Text = "Stop speech", AccessibilityName = "Stop reply speech" };
        row.AddChild(stop); stop.Pressed += () => StopSpeech();
        status = new()
        {
            Name = "SpeechStatus",
            SizeFlagsHorizontal = Control.SizeFlags.ExpandFill,
            AccessibilityName = "Reply speech status",
            AccessibilityLive = DisplayServer.AccessibilityLiveMode.Polite
        };
        row.AddChild(status);
        panel.RegisterSpeechControls([enabled, language, stop]);
    }

    /// <inheritdoc />
    public override void _ExitTree()
    {
        recorder.ReplyRecorded -= Receive;
        restoration.PresentationReset -= OnRestored;
        audio.Changed -= OnAudioChanged;
        StopSpeech("Reply speech stopped");
        Shutdown = ShutdownAsync();
    }

    private async Task ShutdownAsync()
    {
        if (session is not null) { await session.DisposeAsync().ConfigureAwait(false); }
        if (providerOwner is not null) { await providerOwner.DisposeAsync().ConfigureAwait(false); }
    }
}
