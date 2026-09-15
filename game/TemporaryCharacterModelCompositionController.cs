using Allrpcs.Application.Characters;
using Allrpcs.Application.Dialogue;
using Allrpcs.Infrastructure.Characters;
using Allrpcs.Infrastructure.Dialogue;
using Godot;
using NetHttpClient = System.Net.Http.HttpClient;

namespace Allrpcs.Game;

/// <summary>
/// Composes one persisted selected provider binding while preserving offline fallback behavior.
/// </summary>
public partial class TemporaryCharacterModelCompositionController : Node
{
    internal const string AutomaticCompositionDisabledEnvironmentVariable =
        "ALLRPCS_CHARACTER_MODEL_COMPOSITION_DISABLED";

    private const string DisabledEnvironmentValue = "1";

    private CharacterModelSettingsController settingsController = null!;
    private TemporaryCharacterModelSource modelSource = null!;
    private TemporaryCharacterObjectSabotageConcealmentModelSource concealmentModelSource = null!;
    private TemporaryCharacterObjectSabotageMoveModelSource moveModelSource = null!;
    private TemporaryCharacterObjectSabotageSubstitutionModelSource substitutionModelSource = null!;
    private TemporaryCharacterObjectSabotageContaminationModelSource contaminationModelSource = null!;
    private TemporaryCharacterTurnParticipationModelSource participationModelSource = null!;
    private NetHttpClient client = null!;
    private bool isInitialized;
    private CharacterObjectSabotageConcealmentModelBinding? concealmentBinding;
    private CharacterObjectSabotageMoveModelBinding? moveBinding;
    private CharacterObjectSabotageSubstitutionModelBinding? substitutionBinding;
    private CharacterObjectSabotageContaminationModelBinding? contaminationBinding;
    private bool participationBindingsInitialized;
    private CharacterModelBindingSnapshot? connectionTestBindings;

    /// <inheritdoc />
    public override void _Ready()
    {
        settingsController = GetNode<CharacterModelSettingsController>(
            "../CharacterModelSettingsController");
        modelSource = GetNode<TemporaryCharacterModelSource>("../TemporaryCharacterModelSource");
        concealmentModelSource =
            GetNode<TemporaryCharacterObjectSabotageConcealmentModelSource>(
                "../TemporaryCharacterObjectSabotageConcealmentModelSource");
        moveModelSource = GetNode<TemporaryCharacterObjectSabotageMoveModelSource>(
            "../TemporaryCharacterObjectSabotageMoveModelSource");
        substitutionModelSource =
            GetNode<TemporaryCharacterObjectSabotageSubstitutionModelSource>(
                "../TemporaryCharacterObjectSabotageSubstitutionModelSource");
        contaminationModelSource =
            GetNode<TemporaryCharacterObjectSabotageContaminationModelSource>(
                "../TemporaryCharacterObjectSabotageContaminationModelSource");
        participationModelSource = GetNode<TemporaryCharacterTurnParticipationModelSource>(
            "../TemporaryCharacterTurnParticipationModelSource");
        client = new NetHttpClient
        {
            Timeout = Timeout.InfiniteTimeSpan,
        };
        if (OS.GetEnvironment(AutomaticCompositionDisabledEnvironmentVariable)
            != DisabledEnvironmentValue)
        {
            TryLoadCurrent();
        }
    }

    /// <summary>Loads and composes the current supported selected provider once.</summary>
    public bool TryLoadCurrent()
    {
        CharacterModelSettingsSelectionSnapshot? selection = settingsController.Snapshot;
        if (selection is null)
        {
            if (!settingsController.TryLoadCurrent())
            {
                return false;
            }

            selection = settingsController.Snapshot
                ?? throw new InvalidOperationException(
                    "Loaded character-model settings were not retained.");
        }

        return TryInitialize(selection);
    }

    /// <summary>Composes one exact supported selected provider without input/output.</summary>
    public bool TryInitialize(CharacterModelSettingsSelectionSnapshot selection)
    {
        ArgumentNullException.ThrowIfNull(selection);
        if (isInitialized)
        {
            return false;
        }

        CharacterModelSettings selectedSettings = selection.Settings.Single(
            settings => ReferenceEquals(settings.Provider, selection.SelectedProvider));
        CharacterModelBinding? binding = CreateBinding(selectedSettings);
        if (binding is null
            || !modelSource.TryInitializeBindings(
                selection,
                new CharacterModelBindingSnapshot([binding]))
            || !TryApplyParticipationBindings(selection)
            || !TryApplyConcealmentBinding(selection)
            || !TryApplyMoveBinding(selection)
            || !TryApplySubstitutionBinding(selection)
            || !TryApplyContaminationBinding(selection))
        {
            return false;
        }

        isInitialized = true;
        return true;
    }

    /// <summary>Composes and replaces the exact current supported binding for future turns.</summary>
    public bool TryReplaceCurrentBindings()
    {
        CharacterModelSettingsSelectionSnapshot? selection = settingsController.Snapshot;
        if (!isInitialized || selection is null)
        {
            return false;
        }

        CharacterModelBindingSnapshot? bindings = CreateCurrentBindingSnapshot(selection);
        return bindings is not null
            && modelSource.TryReplaceBindings(selection, bindings)
            && TryApplyParticipationBindings(selection)
            && TryApplyConcealmentBinding(selection)
            && TryApplyMoveBinding(selection)
            && TryApplySubstitutionBinding(selection)
            && TryApplyContaminationBinding(selection);
    }

    /// <summary>Initializes or replaces the exact current supported binding for future turns.</summary>
    public bool TryActivateCurrentBindings()
    {
        CharacterModelSettingsSelectionSnapshot? selection = settingsController.Snapshot;
        if (selection is null)
        {
            return false;
        }

        return isInitialized ? TryReplaceCurrentBindings() : TryInitialize(selection);
    }

    /// <summary>Tests only the exact current selected setting through a redacted binding.</summary>
    public ValueTask<CharacterModelConnectionTestResult> TestCurrentConnectionAsync(
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        CharacterModelSettingsSelectionSnapshot? selection = settingsController.Snapshot;
        if (selection is null)
        {
            return ValueTask.FromResult(
                CharacterModelConnectionTestResult.Failed(
                    CharacterModelFailureCode.Unavailable));
        }

        CharacterModelBindingSnapshot? bindings = connectionTestBindings
            ?? CreateCurrentBindingSnapshot(selection);
        if (bindings is null)
        {
            return ValueTask.FromResult(
                CharacterModelConnectionTestResult.Failed(
                    CharacterModelFailureCode.Unavailable));
        }

        return CharacterModelBindingConnectionTestController.TestConfiguredAsync(
            selection,
            bindings,
            cancellationToken);
    }

    internal bool TryInitializeConnectionTestBindings(CharacterModelBindingSnapshot bindings)
    {
        ArgumentNullException.ThrowIfNull(bindings);
        if (connectionTestBindings is not null)
        {
            return false;
        }

        connectionTestBindings = bindings;
        return true;
    }

    /// <inheritdoc />
    public override void _ExitTree()
    {
        client?.Dispose();
    }

    private CharacterModelBinding? CreateBinding(CharacterModelSettings settings)
    {
        if (ReferenceEquals(settings.Provider, CharacterModelProvider.Ollama))
        {
            return OllamaCharacterModelBindingFactory.Create(
                settings,
                client,
                OllamaEndpoint.Default);
        }

        if (!ReferenceEquals(settings.Provider, CharacterModelProvider.OpenAi))
        {
            return null;
        }

        IOpenAiCredentialSource? credentialSource = CreateOpenAiCredentialSource();
        return credentialSource is null
            ? null
            : OpenAiCharacterModelBindingFactory.Create(settings, client, credentialSource);
    }

    private CharacterModelBindingSnapshot? CreateCurrentBindingSnapshot(
        CharacterModelSettingsSelectionSnapshot selection)
    {
        CharacterModelSettings selectedSettings = selection.Settings.Single(
            settings => ReferenceEquals(settings.Provider, selection.SelectedProvider));
        CharacterModelBinding? binding = CreateBinding(selectedSettings);
        return binding is null ? null : new CharacterModelBindingSnapshot([binding]);
    }

    private bool TryApplyParticipationBindings(
        CharacterModelSettingsSelectionSnapshot selection)
    {
        CharacterModelSettings selectedSettings = selection.Settings.Single(
            settings => ReferenceEquals(settings.Provider, selection.SelectedProvider));
        if (!ReferenceEquals(selectedSettings.Provider, CharacterModelProvider.OpenAi))
        {
            if (!participationBindingsInitialized)
            {
                return true;
            }

            bool cleared = participationModelSource.TryClearBindings();
            if (cleared)
            {
                participationBindingsInitialized = false;
            }

            return cleared;
        }

        IOpenAiCredentialSource? credentialSource = CreateOpenAiCredentialSource();
        if (credentialSource is null)
        {
            return false;
        }

        CharacterTurnParticipationModelBinding binding =
            OpenAiCharacterTurnParticipationModelBindingFactory.Create(
                selectedSettings,
                client,
                credentialSource);
        CharacterTurnParticipationModelBindingSnapshot bindings = new([binding]);
        bool applied = participationBindingsInitialized
            ? participationModelSource.TryReplaceBindings(selection, bindings)
            : participationModelSource.TryInitializeBindings(selection, bindings);
        if (applied)
        {
            participationBindingsInitialized = true;
        }

        return applied;
    }

    private bool TryApplyConcealmentBinding(
        CharacterModelSettingsSelectionSnapshot selection)
    {
        CharacterModelSettings selectedSettings = selection.Settings.Single(
            settings => ReferenceEquals(settings.Provider, selection.SelectedProvider));
        if (!ReferenceEquals(selectedSettings.Provider, CharacterModelProvider.OpenAi))
        {
            if (concealmentBinding is null)
            {
                return true;
            }

            bool cleared = concealmentModelSource.TryClearBinding(concealmentBinding);
            if (cleared)
            {
                concealmentBinding = null;
            }

            return cleared;
        }

        IOpenAiCredentialSource? credentialSource = CreateOpenAiCredentialSource();
        if (credentialSource is null)
        {
            return false;
        }

        CharacterObjectSabotageConcealmentModelBinding replacement =
            OpenAiCharacterObjectSabotageConcealmentModelBindingFactory.Create(
                selectedSettings,
                client,
                credentialSource);
        bool applied = concealmentBinding is null
            ? concealmentModelSource.TryInitializeBinding(replacement)
            : concealmentModelSource.TryReplaceBinding(concealmentBinding, replacement);
        if (applied)
        {
            concealmentBinding = replacement;
        }

        return applied;
    }

    private bool TryApplyMoveBinding(CharacterModelSettingsSelectionSnapshot selection)
    {
        CharacterModelSettings selectedSettings = selection.Settings.Single(
            settings => ReferenceEquals(settings.Provider, selection.SelectedProvider));
        if (!ReferenceEquals(selectedSettings.Provider, CharacterModelProvider.OpenAi))
        {
            if (moveBinding is null)
            {
                return true;
            }

            bool cleared = moveModelSource.TryClearBinding(moveBinding);
            if (cleared)
            {
                moveBinding = null;
            }

            return cleared;
        }

        IOpenAiCredentialSource? credentialSource = CreateOpenAiCredentialSource();
        if (credentialSource is null)
        {
            return false;
        }

        CharacterObjectSabotageMoveModelBinding replacement =
            OpenAiCharacterObjectSabotageMoveModelBindingFactory.Create(
                selectedSettings,
                client,
                credentialSource);
        bool applied = moveBinding is null
            ? moveModelSource.TryInitializeBinding(replacement)
            : moveModelSource.TryReplaceBinding(moveBinding, replacement);
        if (applied)
        {
            moveBinding = replacement;
        }

        return applied;
    }

    private bool TryApplySubstitutionBinding(
        CharacterModelSettingsSelectionSnapshot selection)
    {
        CharacterModelSettings selectedSettings = selection.Settings.Single(
            settings => ReferenceEquals(settings.Provider, selection.SelectedProvider));
        if (!ReferenceEquals(selectedSettings.Provider, CharacterModelProvider.OpenAi))
        {
            if (substitutionBinding is null)
            {
                return true;
            }

            bool cleared = substitutionModelSource.TryClearBinding(substitutionBinding);
            if (cleared)
            {
                substitutionBinding = null;
            }

            return cleared;
        }

        IOpenAiCredentialSource? credentialSource = CreateOpenAiCredentialSource();
        if (credentialSource is null)
        {
            return false;
        }

        CharacterObjectSabotageSubstitutionModelBinding replacement =
            OpenAiCharacterObjectSabotageSubstitutionModelBindingFactory.Create(
                selectedSettings,
                client,
                credentialSource);
        bool applied = substitutionBinding is null
            ? substitutionModelSource.TryInitializeBinding(replacement)
            : substitutionModelSource.TryReplaceBinding(substitutionBinding, replacement);
        if (applied)
        {
            substitutionBinding = replacement;
        }

        return applied;
    }

    private bool TryApplyContaminationBinding(
        CharacterModelSettingsSelectionSnapshot selection)
    {
        CharacterModelSettings selectedSettings = selection.Settings.Single(
            settings => ReferenceEquals(settings.Provider, selection.SelectedProvider));
        if (!ReferenceEquals(selectedSettings.Provider, CharacterModelProvider.OpenAi))
        {
            if (contaminationBinding is null)
            {
                return true;
            }

            bool cleared = contaminationModelSource.TryClearBinding(contaminationBinding);
            if (cleared)
            {
                contaminationBinding = null;
            }

            return cleared;
        }

        IOpenAiCredentialSource? credentialSource = CreateOpenAiCredentialSource();
        if (credentialSource is null)
        {
            return false;
        }

        CharacterObjectSabotageContaminationModelBinding replacement =
            OpenAiCharacterObjectSabotageContaminationModelBindingFactory.Create(
                selectedSettings,
                client,
                credentialSource);
        bool applied = contaminationBinding is null
            ? contaminationModelSource.TryInitializeBinding(replacement)
            : contaminationModelSource.TryReplaceBinding(contaminationBinding, replacement);
        if (applied)
        {
            contaminationBinding = replacement;
        }

        return applied;
    }

    private static IOpenAiCredentialSource? CreateOpenAiCredentialSource()
    {
        if (OperatingSystem.IsMacOS())
        {
            return new MacOsOpenAiCredentialStore(new MacOsKeychainProcessRunner());
        }

        if (OperatingSystem.IsLinux())
        {
            return new LinuxSecretServiceOpenAiCredentialStore(
                new LinuxSecretServiceProcessRunner());
        }

        return OperatingSystem.IsWindows()
            ? new WindowsOpenAiCredentialStore(new WindowsCredentialManager())
            : null;
    }
}
