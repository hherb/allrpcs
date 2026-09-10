using Allrpcs.Infrastructure.Dialogue;
using Godot;

namespace Allrpcs.Game;

/// <summary>Checks masked explicit OpenAI credential controls through an injected store.</summary>
public partial class OpenAiCredentialPanelSmokeCheck : Node
{
    private const string CheckEnvironmentVariable = "ALLRPCS_OPENAI_CREDENTIAL_PANEL_CHECK";
    private const string EnabledEnvironmentValue = "1";

    private readonly RecordingCredentialStore store = new(
        ["sk-panel-save", "sk-panel-failure"],
        [OpenAiCredentialWriteStatus.Saved, OpenAiCredentialWriteStatus.Failed],
        [
            OpenAiCredentialDeleteStatus.Deleted,
            OpenAiCredentialDeleteStatus.Missing,
            OpenAiCredentialDeleteStatus.Failed,
        ]);

    private LineEdit apiKey = null!;
    private Button saveApiKey = null!;
    private Button deleteApiKey = null!;
    private Label credentialStatus = null!;

    /// <inheritdoc />
    public override void _Ready()
    {
        bool isEnabled = OS.GetEnvironment(CheckEnvironmentVariable) == EnabledEnvironmentValue;
        SetProcess(isEnabled);
        if (!isEnabled)
        {
            return;
        }

        OpenAiCredentialController controller = GetNode<OpenAiCredentialController>(
            "../OpenAiCredentialController");
        if (!controller.TryInitialize(store))
        {
            Fail("OpenAI credential panel smoke could not install its isolated store.");
            return;
        }

        apiKey = GetNode<LineEdit>("../SettingsOverlay/Panel/OpenAiApiKey");
        saveApiKey = GetNode<Button>("../SettingsOverlay/Panel/SaveOpenAiApiKey");
        deleteApiKey = GetNode<Button>("../SettingsOverlay/Panel/DeleteOpenAiApiKey");
        credentialStatus = GetNode<Label>("../SettingsOverlay/Panel/CredentialStatus");
    }

    /// <inheritdoc />
    public override void _Process(double delta)
    {
        _ = delta;
        SetProcess(false);

        if (!apiKey.Secret
            || apiKey.Text.Length != 0
            || credentialStatus.Text
                != TemporaryCharacterModelSettingsPanel.CredentialNotReadStatusText
            || store.OperationCount != 0)
        {
            Fail("OpenAI credential panel read, displayed, or operated on a startup credential.");
            return;
        }

        saveApiKey.EmitSignal(BaseButton.SignalName.Pressed);
        if (credentialStatus.Text
                != TemporaryCharacterModelSettingsPanel.CredentialFailedStatusText
            || store.OperationCount != 0
            || apiKey.Text.Length != 0)
        {
            Fail("OpenAI credential panel delegated blank credential input.");
            return;
        }

        apiKey.Text = "sk-panel-save";
        saveApiKey.EmitSignal(BaseButton.SignalName.Pressed);
        if (credentialStatus.Text
                != TemporaryCharacterModelSettingsPanel.CredentialSavedStatusText
            || apiKey.Text.Length != 0
            || store.SaveCount != 1
            || !store.ReceivedOnlyExpectedApiKeys)
        {
            Fail("OpenAI credential panel did not clear and save one exact submitted key.");
            return;
        }

        apiKey.Text = "sk-panel-failure";
        saveApiKey.EmitSignal(BaseButton.SignalName.Pressed);
        if (credentialStatus.Text
                != TemporaryCharacterModelSettingsPanel.CredentialFailedStatusText
            || apiKey.Text.Length != 0
            || store.SaveCount != 2
            || !store.ReceivedOnlyExpectedApiKeys)
        {
            Fail("OpenAI credential panel did not redact a failed credential save.");
            return;
        }

        if (!PressDelete(OpenAiCredentialDeleteStatus.Deleted)
            || !PressDelete(OpenAiCredentialDeleteStatus.Missing)
            || !PressDelete(OpenAiCredentialDeleteStatus.Failed)
            || store.DeleteCount != 3
            || store.LoadCount != 0
            || credentialStatus.Text.Contains("sk-", StringComparison.Ordinal))
        {
            Fail("OpenAI credential panel did not preserve redacted delete outcomes.");
            return;
        }

        GD.Print(
            "OpenAI credential panel smoke passed: masked and empty startup input, no credential "
            + "load, immediate input clearing, bounded validation, redacted save/delete outcomes, "
            + "and isolated OS-store delegation were correct.");
        GetTree().Quit();
    }

    private bool PressDelete(OpenAiCredentialDeleteStatus expected)
    {
        deleteApiKey.EmitSignal(BaseButton.SignalName.Pressed);
        string expectedText = expected switch
        {
            OpenAiCredentialDeleteStatus.Deleted =>
                TemporaryCharacterModelSettingsPanel.CredentialDeletedStatusText,
            OpenAiCredentialDeleteStatus.Missing =>
                TemporaryCharacterModelSettingsPanel.CredentialMissingStatusText,
            _ => TemporaryCharacterModelSettingsPanel.CredentialFailedStatusText,
        };
        return credentialStatus.Text == expectedText;
    }

    private void Fail(string message)
    {
        GD.PushError(message);
        GetTree().Quit(exitCode: 1);
    }

    private sealed class RecordingCredentialStore : IOpenAiCredentialStore
    {
        private readonly Queue<string> expectedApiKeys;
        private readonly Queue<OpenAiCredentialWriteStatus> writeStatuses;
        private readonly Queue<OpenAiCredentialDeleteStatus> deleteStatuses;

        public RecordingCredentialStore(
            IEnumerable<string> expectedApiKeys,
            IEnumerable<OpenAiCredentialWriteStatus> writeStatuses,
            IEnumerable<OpenAiCredentialDeleteStatus> deleteStatuses)
        {
            this.expectedApiKeys = new Queue<string>(expectedApiKeys);
            this.writeStatuses = new Queue<OpenAiCredentialWriteStatus>(writeStatuses);
            this.deleteStatuses = new Queue<OpenAiCredentialDeleteStatus>(deleteStatuses);
        }

        public int SaveCount { get; private set; }

        public int LoadCount { get; private set; }

        public int DeleteCount { get; private set; }

        public int OperationCount => SaveCount + LoadCount + DeleteCount;

        public bool ReceivedOnlyExpectedApiKeys { get; private set; } = true;

        public ValueTask<OpenAiCredentialWriteStatus> SaveApiKeyAsync(
            string submittedApiKey,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ReceivedOnlyExpectedApiKeys &= expectedApiKeys.Count > 0
                && submittedApiKey == expectedApiKeys.Dequeue();
            SaveCount++;
            return ValueTask.FromResult(writeStatuses.Dequeue());
        }

        public ValueTask<OpenAiCredentialLoadResult> LoadApiKeyAsync(
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            LoadCount++;
            return ValueTask.FromResult(OpenAiCredentialLoadResult.Failed());
        }

        public ValueTask<OpenAiCredentialDeleteStatus> DeleteApiKeyAsync(
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            DeleteCount++;
            return ValueTask.FromResult(deleteStatuses.Dequeue());
        }
    }
}
