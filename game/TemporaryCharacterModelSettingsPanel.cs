using Allrpcs.Application.Dialogue;
using Allrpcs.Infrastructure.Dialogue;
using Godot;

namespace Allrpcs.Game;

/// <summary>Controls secret-free model settings and the OS-stored OpenAI credential.</summary>
public partial class TemporaryCharacterModelSettingsPanel : Control
{
    /// <summary>Displayed after a valid setting is applied only in memory.</summary>
    public const string AppliedStatusText = "Provider settings: APPLIED · SAVE TO KEEP";

    /// <summary>Displayed after current settings are explicitly saved.</summary>
    public const string SavedStatusText = "Provider settings: SAVED · RESTART TO APPLY";

    /// <summary>Displayed after provider or model input is rejected.</summary>
    public const string RejectedStatusText = "Provider settings: REJECTED";

    /// <summary>Displayed after one unselected provider setting is removed in memory.</summary>
    public const string RemovedStatusText = "Provider settings: REMOVED · SAVE TO KEEP";

    /// <summary>Displayed when no initialized settings can be saved.</summary>
    public const string SaveFailedStatusText = "Provider settings: SAVE FAILED";

    /// <summary>Displayed before any credential operation; no credential is loaded for display.</summary>
    public const string CredentialNotReadStatusText = "OpenAI credential: NOT READ";

    /// <summary>Displayed after the OS credential store reports a successful save.</summary>
    public const string CredentialSavedStatusText = "OpenAI credential: SAVED IN OS STORE";

    /// <summary>Displayed after the OS credential store reports a successful delete.</summary>
    public const string CredentialDeletedStatusText = "OpenAI credential: DELETED";

    /// <summary>Displayed when no OS-stored OpenAI credential exists to delete.</summary>
    public const string CredentialMissingStatusText = "OpenAI credential: MISSING";

    /// <summary>Displayed when a credential operation fails without exposing details.</summary>
    public const string CredentialFailedStatusText = "OpenAI credential: FAILED";

    /// <summary>Displayed before the player explicitly tests the selected provider setting.</summary>
    public const string ConnectionNotTestedStatusText = "Connection: NOT TESTED";

    /// <summary>Displayed after the exact selected provider setting connects successfully.</summary>
    public const string ConnectionConnectedStatusText = "Connection: CONNECTED";

    /// <summary>Displayed after any redacted connection-test failure.</summary>
    public const string ConnectionFailedStatusText = "Connection: FAILED";

    private const string EmptyStatusText = "Provider settings: NONE";
    private const string LoadedStatusText = "Provider settings: LOADED";
    private const string CredentialBusyStatusText = "OpenAI credential: BUSY";
    private const string ConnectionBusyStatusText = "Connection: BUSY";

    private static readonly string[] ProviderLabels = ["OpenAI", "Anthropic", "Ollama"];

    private CharacterModelSettingsController controller = null!;
    private OpenAiCredentialController credentialController = null!;
    private TemporaryCharacterModelCompositionController compositionController = null!;
    private OptionButton provider = null!;
    private LineEdit modelName = null!;
    private Label status = null!;
    private LineEdit apiKey = null!;
    private Label credentialStatus = null!;
    private Label connectionStatus = null!;
    private readonly CancellationTokenSource panelOperationCancellation = new();
    private Task<OpenAiCredentialWriteStatus>? pendingCredentialSave;
    private Task<OpenAiCredentialDeleteStatus>? pendingCredentialDelete;
    private Task<CharacterModelConnectionTestResult>? pendingConnectionTest;

    /// <inheritdoc />
    public override void _Ready()
    {
        controller = GetNode<CharacterModelSettingsController>(
            "../../CharacterModelSettingsController");
        credentialController = GetNode<OpenAiCredentialController>(
            "../../OpenAiCredentialController");
        compositionController = GetNode<TemporaryCharacterModelCompositionController>(
            "../../TemporaryCharacterModelCompositionController");
        provider = GetNode<OptionButton>("Provider");
        modelName = GetNode<LineEdit>("ModelName");
        status = GetNode<Label>("Status");
        apiKey = GetNode<LineEdit>("OpenAiApiKey");
        credentialStatus = GetNode<Label>("CredentialStatus");
        connectionStatus = GetNode<Label>("ConnectionStatus");
        apiKey.Clear();
        credentialStatus.Text = CredentialNotReadStatusText;
        connectionStatus.Text = ConnectionNotTestedStatusText;

        PopulateProviderChoices();
        PresentCurrent();

        provider.ItemSelected += OnProviderSelected;
        GetNode<Button>("Apply").Pressed += OnApplyPressed;
        GetNode<Button>("Remove").Pressed += OnRemovePressed;
        GetNode<Button>("Save").Pressed += OnSavePressed;
        GetNode<Button>("SaveOpenAiApiKey").Pressed += OnSaveOpenAiApiKeyPressed;
        GetNode<Button>("DeleteOpenAiApiKey").Pressed += OnDeleteOpenAiApiKeyPressed;
        GetNode<Button>("TestConnection").Pressed += OnTestConnectionPressed;
        SetProcess(false);
    }

    /// <inheritdoc />
    public override void _Process(double delta)
    {
        _ = delta;
        CompleteOperation();
    }

    /// <inheritdoc />
    public override void _ExitTree()
    {
        panelOperationCancellation.Cancel();
        panelOperationCancellation.Dispose();
    }

    private void PopulateProviderChoices()
    {
        if (ProviderLabels.Length != CharacterModelProvider.All.Count)
        {
            throw new InvalidOperationException(
                "Every canonical character-model provider requires one display label.");
        }

        provider.AddItem("Choose provider");
        provider.SetItemDisabled(0, true);
        foreach (string label in ProviderLabels)
        {
            provider.AddItem(label);
        }

        provider.Select(-1);
    }

    private void PresentCurrent()
    {
        CharacterModelSettingsSelectionSnapshot? snapshot = controller.Snapshot;
        if (snapshot is null)
        {
            status.Text = EmptyStatusText;
            return;
        }

        int providerIndex = CharacterModelProvider.All
            .Select((candidate, index) => (candidate, index))
            .Single(pair => ReferenceEquals(pair.candidate, snapshot.SelectedProvider))
            .index;
        CharacterModelSettings selected = snapshot.Settings.Single(
            settings => ReferenceEquals(settings.Provider, snapshot.SelectedProvider));
        provider.Select(providerIndex + 1);
        modelName.Text = selected.ModelName;
        status.Text = LoadedStatusText;
    }

    private void OnProviderSelected(long selectedIndex)
    {
        CharacterModelProvider? selectedProvider = ProviderAt(selectedIndex);
        CharacterModelSettings? settings = controller.Snapshot?.Settings.SingleOrDefault(
            candidate => ReferenceEquals(candidate.Provider, selectedProvider));
        modelName.Text = settings?.ModelName ?? string.Empty;
    }

    private void OnApplyPressed()
    {
        if (OperationIsActive())
        {
            status.Text = RejectedStatusText;
            return;
        }

        CharacterModelProvider? selectedProvider = ProviderAt(provider.Selected);
        if (selectedProvider is null)
        {
            status.Text = RejectedStatusText;
            return;
        }

        try
        {
            CharacterModelSettings settings = new(selectedProvider, modelName.Text);
            bool applied = controller.Snapshot is null
                ? controller.TryInitialize(
                    new CharacterModelSettingsSelectionSnapshot([settings], selectedProvider))
                : controller.TryApply(settings);
            status.Text = applied ? AppliedStatusText : RejectedStatusText;
            if (applied)
            {
                connectionStatus.Text = ConnectionNotTestedStatusText;
            }
        }
        catch (ArgumentException)
        {
            status.Text = RejectedStatusText;
        }
    }

    private void OnSavePressed()
    {
        status.Text = controller.SaveCurrent() == CharacterModelSettingsDocumentWriteResult.Saved
            ? SavedStatusText
            : SaveFailedStatusText;
    }

    private void OnRemovePressed()
    {
        if (OperationIsActive())
        {
            status.Text = RejectedStatusText;
            return;
        }

        CharacterModelProvider? selectedProvider = ProviderAt(provider.Selected);
        if (selectedProvider is null || !controller.TryRemove(selectedProvider))
        {
            status.Text = RejectedStatusText;
            return;
        }

        modelName.Clear();
        status.Text = RemovedStatusText;
        connectionStatus.Text = ConnectionNotTestedStatusText;
    }

    private void OnSaveOpenAiApiKeyPressed()
    {
        if (OperationIsActive())
        {
            credentialStatus.Text = CredentialBusyStatusText;
            return;
        }

        string submittedApiKey = apiKey.Text;
        apiKey.Clear();
        pendingCredentialSave = credentialController
            .SaveApiKeyAsync(submittedApiKey, panelOperationCancellation.Token)
            .AsTask();
        submittedApiKey = string.Empty;
        CompleteOperation();
    }

    private void OnDeleteOpenAiApiKeyPressed()
    {
        if (OperationIsActive())
        {
            credentialStatus.Text = CredentialBusyStatusText;
            return;
        }

        pendingCredentialDelete = credentialController
            .DeleteApiKeyAsync(panelOperationCancellation.Token)
            .AsTask();
        CompleteOperation();
    }

    private void OnTestConnectionPressed()
    {
        if (OperationIsActive())
        {
            connectionStatus.Text = ConnectionBusyStatusText;
            return;
        }

        pendingConnectionTest = compositionController
            .TestCurrentConnectionAsync(panelOperationCancellation.Token)
            .AsTask();
        CompleteOperation();
    }

    private void CompleteOperation()
    {
        if (pendingCredentialSave is { IsCompleted: true } saveTask)
        {
            credentialStatus.Text = CredentialSaveSucceeded(saveTask)
                    ? CredentialSavedStatusText
                    : CredentialFailedStatusText;
            connectionStatus.Text = ConnectionNotTestedStatusText;
            pendingCredentialSave = null;
        }
        else if (pendingCredentialDelete is { IsCompleted: true } deleteTask)
        {
            credentialStatus.Text = CredentialDeleteStatusText(deleteTask);
            connectionStatus.Text = ConnectionNotTestedStatusText;
            pendingCredentialDelete = null;
        }
        else if (pendingConnectionTest is { IsCompleted: true } connectionTask)
        {
            connectionStatus.Text = ConnectionTestSucceeded(connectionTask)
                ? ConnectionConnectedStatusText
                : ConnectionFailedStatusText;
            pendingConnectionTest = null;
        }

        SetProcess(OperationIsActive());
    }

    private bool OperationIsActive() =>
        pendingCredentialSave is not null
        || pendingCredentialDelete is not null
        || pendingConnectionTest is not null;

    private static bool CredentialSaveSucceeded(Task<OpenAiCredentialWriteStatus> task)
    {
        if (task.IsCompletedSuccessfully)
        {
            return task.Result == OpenAiCredentialWriteStatus.Saved;
        }

        _ = task.Exception;
        return false;
    }

    private static string CredentialDeleteStatusText(
        Task<OpenAiCredentialDeleteStatus> task)
    {
        if (task.IsCompletedSuccessfully)
        {
            return DeleteStatusText(task.Result);
        }

        _ = task.Exception;
        return CredentialFailedStatusText;
    }

    private static bool ConnectionTestSucceeded(Task<CharacterModelConnectionTestResult> task)
    {
        if (task.IsCompletedSuccessfully)
        {
            return task.Result.IsConnected;
        }

        _ = task.Exception;
        return false;
    }

    private static string DeleteStatusText(OpenAiCredentialDeleteStatus status) => status switch
    {
        OpenAiCredentialDeleteStatus.Deleted => CredentialDeletedStatusText,
        OpenAiCredentialDeleteStatus.Missing => CredentialMissingStatusText,
        OpenAiCredentialDeleteStatus.Failed => CredentialFailedStatusText,
        _ => CredentialFailedStatusText,
    };

    private static CharacterModelProvider? ProviderAt(long selectedIndex)
    {
        long providerIndex = selectedIndex - 1;
        return providerIndex >= 0 && providerIndex < CharacterModelProvider.All.Count
            ? CharacterModelProvider.All[(int)providerIndex]
            : null;
    }
}
