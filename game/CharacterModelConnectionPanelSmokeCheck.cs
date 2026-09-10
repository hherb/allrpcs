using System.Runtime.CompilerServices;
using Allrpcs.Application.Dialogue;
using Godot;

namespace Allrpcs.Game;

/// <summary>Checks explicit selected-setting connection testing without provider access.</summary>
public partial class CharacterModelConnectionPanelSmokeCheck : Node
{
    private const string CheckEnvironmentVariable =
        "ALLRPCS_CHARACTER_MODEL_CONNECTION_PANEL_CHECK";
    private const string EnabledEnvironmentValue = "1";

    private readonly NeverUsedCharacterModel model = new();
    private readonly RecordingConnectionTester tester = new(
        [
            CharacterModelConnectionTestResult.Connected(),
            CharacterModelConnectionTestResult.Failed(CharacterModelFailureCode.Authentication),
        ]);

    private CharacterModelSettingsController settingsController = null!;
    private TemporaryCharacterModelCompositionController compositionController = null!;
    private CharacterModelSettingsSelectionSnapshot selection = null!;
    private Button testConnection = null!;
    private Label connectionStatus = null!;

    /// <inheritdoc />
    public override void _Ready()
    {
        bool isEnabled = OS.GetEnvironment(CheckEnvironmentVariable) == EnabledEnvironmentValue;
        SetProcess(isEnabled);
        if (!isEnabled)
        {
            return;
        }

        settingsController = GetNode<CharacterModelSettingsController>(
            "../CharacterModelSettingsController");
        compositionController = GetNode<TemporaryCharacterModelCompositionController>(
            "../TemporaryCharacterModelCompositionController");
        CharacterModelSettings settings = new(CharacterModelProvider.OpenAi, "gpt-test");
        selection = new CharacterModelSettingsSelectionSnapshot(
            [settings],
            CharacterModelProvider.OpenAi);
        CharacterModelBinding binding = new(
            CharacterModelProvider.OpenAi,
            settings,
            model,
            tester);
        if (!compositionController.TryInitializeConnectionTestBindings(
                new CharacterModelBindingSnapshot([binding])))
        {
            Fail("Character-model connection smoke could not install its isolated binding.");
            return;
        }

        testConnection = GetNode<Button>("../SettingsOverlay/Panel/TestConnection");
        connectionStatus = GetNode<Label>("../SettingsOverlay/Panel/ConnectionStatus");
    }

    /// <inheritdoc />
    public override void _Process(double delta)
    {
        _ = delta;
        SetProcess(false);

        if (settingsController.Snapshot is not null
            || connectionStatus.Text
                != TemporaryCharacterModelSettingsPanel.ConnectionNotTestedStatusText
            || tester.CallCount != 0
            || model.InvocationCount != 0)
        {
            Fail("Character-model connection panel tested or generated during startup.");
            return;
        }

        testConnection.EmitSignal(BaseButton.SignalName.Pressed);
        if (connectionStatus.Text
                != TemporaryCharacterModelSettingsPanel.ConnectionFailedStatusText
            || tester.CallCount != 0
            || !settingsController.TryInitialize(selection))
        {
            Fail("Character-model connection panel did not fail closed without settings.");
            return;
        }

        testConnection.EmitSignal(BaseButton.SignalName.Pressed);
        if (connectionStatus.Text
                != TemporaryCharacterModelSettingsPanel.ConnectionConnectedStatusText
            || tester.CallCount != 1
            || !ReferenceEquals(tester.LastSettings, selection.Settings[0]))
        {
            Fail("Character-model connection panel did not test the exact selected setting.");
            return;
        }

        testConnection.EmitSignal(BaseButton.SignalName.Pressed);
        if (connectionStatus.Text
                != TemporaryCharacterModelSettingsPanel.ConnectionFailedStatusText
            || tester.CallCount != 2
            || !ReferenceEquals(settingsController.Snapshot, selection)
            || model.InvocationCount != 0)
        {
            Fail("Character-model connection panel exposed failure detail or generated content.");
            return;
        }

        using CancellationTokenSource cancellation = new();
        cancellation.Cancel();
        try
        {
            compositionController.TestCurrentConnectionAsync(cancellation.Token)
                .AsTask()
                .GetAwaiter()
                .GetResult();
            Fail("Character-model connection controller ignored cancellation.");
            return;
        }
        catch (OperationCanceledException)
        {
        }

        if (tester.CallCount != 2
            || !ReferenceEquals(settingsController.Snapshot, selection)
            || model.InvocationCount != 0)
        {
            Fail("Character-model connection cancellation changed scene state.");
            return;
        }

        GD.Print(
            "Character-model connection panel smoke passed: zero-I/O startup, missing-settings "
            + "failure, exact selected-setting testing, redacted connected/failed presentation, "
            + "cancellation, and absence of generation or scene mutation were correct.");
        GetTree().Quit();
    }

    private void Fail(string message)
    {
        GD.PushError(message);
        GetTree().Quit(exitCode: 1);
    }

    private sealed class RecordingConnectionTester(
        IEnumerable<CharacterModelConnectionTestResult> results)
        : ICharacterModelConnectionTester
    {
        private readonly Queue<CharacterModelConnectionTestResult> results = new(results);

        public int CallCount { get; private set; }

        public CharacterModelSettings? LastSettings { get; private set; }

        public ValueTask<CharacterModelConnectionTestResult> TestConnectionAsync(
            CharacterModelSettings settings,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            LastSettings = settings;
            CallCount++;
            return ValueTask.FromResult(results.Dequeue());
        }
    }

    private sealed class NeverUsedCharacterModel : ICharacterModel
    {
        public int InvocationCount { get; private set; }

        public async IAsyncEnumerable<CharacterModelStreamUpdate> StreamResponseAsync(
            CharacterModelGenerationRequest request,
            [EnumeratorCancellation] CancellationToken cancellationToken)
        {
            _ = request;
            cancellationToken.ThrowIfCancellationRequested();
            InvocationCount++;
            await Task.CompletedTask;
            yield break;
        }
    }
}
