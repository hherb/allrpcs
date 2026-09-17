using System.Runtime.CompilerServices;
using Allrpcs.Application.Characters;
using Allrpcs.Application.Dialogue;
using Allrpcs.Infrastructure.Characters;
using Allrpcs.Infrastructure.Dialogue;
using Godot;

namespace Allrpcs.Game;

/// <summary>Checks persisted provider composition without credentials or network input/output.</summary>
public partial class TemporaryCharacterModelCompositionControllerSmokeCheck : Node
{
    private const string CheckEnvironmentVariable =
        "ALLRPCS_CHARACTER_MODEL_COMPOSITION_CHECK";
    private const string EnabledEnvironmentValue = "1";

    private TemporaryCharacterModelCompositionController controller = null!;
    private CharacterModelSettingsController settingsController = null!;
    private CharacterModelSettingsStorageController storage = null!;
    private TemporaryCharacterModelSource modelSource = null!;
    private TemporaryCharacterObjectSabotageConcealmentModelSource concealmentModelSource = null!;
    private TemporaryCharacterObjectSabotageMoveModelSource moveModelSource = null!;
    private TemporaryCharacterObjectSabotageSubstitutionModelSource substitutionModelSource = null!;
    private TemporaryCharacterObjectSabotageContaminationModelSource contaminationModelSource = null!;
    private TemporaryCharacterTargetSabotageInfluenceModelSource influenceModelSource = null!;
    private TemporaryCharacterTargetSabotageInfluenceRecoveryModelSource recoveryModelSource = null!;
    private TemporaryCharacterTurnParticipationModelSource participationModelSource = null!;

    /// <inheritdoc />
    public override void _Ready()
    {
        bool isEnabled = OS.GetEnvironment(CheckEnvironmentVariable) == EnabledEnvironmentValue;
        SetProcess(isEnabled);
        if (!isEnabled)
        {
            return;
        }

        controller = GetNode<TemporaryCharacterModelCompositionController>(
            "../TemporaryCharacterModelCompositionController");
        settingsController = GetNode<CharacterModelSettingsController>(
            "../CharacterModelSettingsController");
        storage = GetNode<CharacterModelSettingsStorageController>(
            "../CharacterModelSettingsStorageController");
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
        influenceModelSource = GetNode<TemporaryCharacterTargetSabotageInfluenceModelSource>(
            "../TemporaryCharacterTargetSabotageInfluenceModelSource");
        recoveryModelSource =
            GetNode<TemporaryCharacterTargetSabotageInfluenceRecoveryModelSource>(
                "../TemporaryCharacterTargetSabotageInfluenceRecoveryModelSource");
        participationModelSource = GetNode<TemporaryCharacterTurnParticipationModelSource>(
            "../TemporaryCharacterTurnParticipationModelSource");
    }

    /// <inheritdoc />
    public override void _Process(double delta)
    {
        _ = delta;
        SetProcess(false);

        if (controller.TryReplaceCurrentBindings())
        {
            Fail("Provider composition replaced bindings without current settings.");
            return;
        }

        CharacterModelSettings anthropicSettings = new(
            CharacterModelProvider.Anthropic,
            "claude-test");
        CharacterModelSettingsSelectionSnapshot unsupported = new(
            [anthropicSettings],
            CharacterModelProvider.Anthropic);
        ICharacterModel offline = modelSource.TakeModel();
        ICharacterTurnParticipationModel offlineParticipation = participationModelSource.TakeModel();
        ICharacterObjectSabotageConcealmentDecisionModel offlineConcealment =
            concealmentModelSource.TakeModel();
        ICharacterObjectSabotageMoveDecisionModel offlineMove = moveModelSource.TakeModel();
        ICharacterObjectSabotageSubstitutionDecisionModel offlineSubstitution =
            substitutionModelSource.TakeModel();
        ICharacterObjectSabotageContaminationDecisionModel offlineContamination =
            contaminationModelSource.TakeModel();
        ICharacterTargetSabotageInfluenceDecisionModel offlineInfluence =
            influenceModelSource.TakeModel();
        ICharacterTargetSabotageInfluenceRecoveryDecisionModel offlineRecovery =
            recoveryModelSource.TakeModel();
        if (controller.TryInitialize(unsupported)
            || !ReferenceEquals(modelSource.TakeModel(), offline)
            || !ReferenceEquals(participationModelSource.TakeModel(), offlineParticipation)
            || !ReferenceEquals(concealmentModelSource.TakeModel(), offlineConcealment)
            || !ReferenceEquals(moveModelSource.TakeModel(), offlineMove)
            || !ReferenceEquals(substitutionModelSource.TakeModel(), offlineSubstitution)
            || !ReferenceEquals(contaminationModelSource.TakeModel(), offlineContamination)
            || !ReferenceEquals(influenceModelSource.TakeModel(), offlineInfluence)
            || !ReferenceEquals(recoveryModelSource.TakeModel(), offlineRecovery))
        {
            Fail("Provider composition did not preserve offline behavior for Anthropic.");
            return;
        }

        CharacterModelSettings ollamaSettings = new(
            CharacterModelProvider.Ollama,
            "gemma3");
        CharacterModelSettingsSelectionSnapshot ollama = new(
            [ollamaSettings],
            CharacterModelProvider.Ollama);
        if (storage.SaveCurrent(ollama) != CharacterModelSettingsDocumentWriteResult.Saved
            || !controller.TryLoadCurrent()
            || settingsController.Snapshot is not { } loaded
            || !ReferenceEquals(loaded.SelectedProvider, CharacterModelProvider.Ollama)
            || loaded.Settings.Count != 1
            || controller.TryInitialize(ollama))
        {
            Fail("Provider composition did not initialize the persisted Ollama selection once.");
            return;
        }

        ICharacterModel first = modelSource.TakeModel();
        ICharacterModel second = modelSource.TakeModel();
        if (first is not OllamaCharacterModel
            || !ReferenceEquals(first, second)
            || !ReferenceEquals(participationModelSource.TakeModel(), offlineParticipation)
            || !ReferenceEquals(concealmentModelSource.TakeModel(), offlineConcealment)
            || !ReferenceEquals(moveModelSource.TakeModel(), offlineMove)
            || !ReferenceEquals(substitutionModelSource.TakeModel(), offlineSubstitution)
            || !ReferenceEquals(contaminationModelSource.TakeModel(), offlineContamination)
            || !ReferenceEquals(influenceModelSource.TakeModel(), offlineInfluence)
            || !ReferenceEquals(recoveryModelSource.TakeModel(), offlineRecovery))
        {
            Fail("Provider composition did not retain its exact persisted Ollama model.");
            return;
        }

        ScriptedModel retained = new();
        CharacterModelSettings retainedSettings = new(
            CharacterModelProvider.Ollama,
            "retained-test");
        CharacterModelSettingsSelectionSnapshot retainedSelection = new(
            [retainedSettings],
            CharacterModelProvider.Ollama);
        CharacterModelBinding retainedBinding = Binding(retainedSettings, retained);
        if (!modelSource.TryReplaceBindings(
                retainedSelection,
                new CharacterModelBindingSnapshot([retainedBinding])))
        {
            Fail("The model source rejected one exact replacement binding.");
            return;
        }

        ICharacterModel alreadyTaken = modelSource.TakeModel();
        ScriptedModel replacement = new();
        ScriptedModel oneShot = new();
        CharacterModelSettings replacementSettings = new(
            CharacterModelProvider.Ollama,
            "replacement-test");
        CharacterModelSettingsSelectionSnapshot replacementSelection = new(
            [replacementSettings],
            CharacterModelProvider.Ollama);
        CharacterModelBinding replacementBinding = Binding(replacementSettings, replacement);
        CharacterModelBinding mismatchedBinding = Binding(
            new CharacterModelSettings(CharacterModelProvider.Ollama, "replacement-test"),
            replacement);
        if (!modelSource.TrySupplyModel(oneShot)
            || modelSource.TryReplaceBindings(
                replacementSelection,
                new CharacterModelBindingSnapshot([mismatchedBinding]))
            || !ReferenceEquals(modelSource.TakeModel(), oneShot)
            || !ReferenceEquals(modelSource.TakeModel(), retained)
            || !modelSource.TrySupplyModel(oneShot)
            || !modelSource.TryReplaceBindings(
                replacementSelection,
                new CharacterModelBindingSnapshot([replacementBinding]))
            || !ReferenceEquals(alreadyTaken, retained)
            || !ReferenceEquals(modelSource.TakeModel(), oneShot)
            || !ReferenceEquals(modelSource.TakeModel(), replacement)
            || !ReferenceEquals(modelSource.TakeModel(), replacement))
        {
            Fail("The model source replacement was not atomic, stable, or one-shot preserving.");
            return;
        }

        CharacterModelSettings activatedSettings = new(
            CharacterModelProvider.Ollama,
            "activated-test");
        if (!settingsController.TryApply(activatedSettings)
            || !controller.TryReplaceCurrentBindings())
        {
            Fail("Provider composition did not replace the exact current Ollama binding.");
            return;
        }

        ICharacterModel activated = modelSource.TakeModel();
        if (activated is not OllamaCharacterModel
            || ReferenceEquals(activated, first)
            || !ReferenceEquals(modelSource.TakeModel(), activated)
            || !settingsController.TryApply(anthropicSettings)
            || controller.TryReplaceCurrentBindings()
            || !ReferenceEquals(modelSource.TakeModel(), activated))
        {
            Fail("Provider composition changed the model after an unsupported replacement.");
            return;
        }

        CharacterModelSettings openAiSettings = new(
            CharacterModelProvider.OpenAi,
            "gpt-test");
        if (!settingsController.TryApply(openAiSettings)
            || !controller.TryReplaceCurrentBindings())
        {
            Fail("Provider composition did not activate the current OpenAI bindings.");
            return;
        }

        ICharacterModel openAiModel = modelSource.TakeModel();
        ICharacterTurnParticipationModel openAiParticipation = participationModelSource.TakeModel();
        ICharacterObjectSabotageConcealmentDecisionModel openAiConcealment =
            concealmentModelSource.TakeModel();
        ICharacterObjectSabotageMoveDecisionModel openAiMove = moveModelSource.TakeModel();
        ICharacterObjectSabotageSubstitutionDecisionModel openAiSubstitution =
            substitutionModelSource.TakeModel();
        ICharacterObjectSabotageContaminationDecisionModel openAiContamination =
            contaminationModelSource.TakeModel();
        ICharacterTargetSabotageInfluenceDecisionModel openAiInfluence =
            influenceModelSource.TakeModel();
        ICharacterTargetSabotageInfluenceRecoveryDecisionModel openAiRecovery =
            recoveryModelSource.TakeModel();
        CharacterModelSettings replacementOpenAiSettings = new(
            CharacterModelProvider.OpenAi,
            "gpt-replacement-test");
        if (openAiModel is not OpenAiCharacterModel
            || openAiParticipation is not OpenAiCharacterTurnParticipationModel
            || openAiConcealment is not OpenAiCharacterObjectSabotageConcealmentModel
            || openAiMove is not OpenAiCharacterObjectSabotageMoveModel
            || openAiSubstitution is not OpenAiCharacterObjectSabotageSubstitutionModel
            || openAiContamination is not OpenAiCharacterObjectSabotageContaminationModel
            || openAiInfluence is not OpenAiCharacterTargetSabotageInfluenceModel
            || openAiRecovery is not OpenAiCharacterTargetSabotageInfluenceRecoveryModel
            || !ReferenceEquals(modelSource.TakeModel(), openAiModel)
            || !ReferenceEquals(participationModelSource.TakeModel(), openAiParticipation)
            || !ReferenceEquals(concealmentModelSource.TakeModel(), openAiConcealment)
            || !ReferenceEquals(moveModelSource.TakeModel(), openAiMove)
            || !ReferenceEquals(substitutionModelSource.TakeModel(), openAiSubstitution)
            || !ReferenceEquals(contaminationModelSource.TakeModel(), openAiContamination)
            || !ReferenceEquals(influenceModelSource.TakeModel(), openAiInfluence)
            || !ReferenceEquals(recoveryModelSource.TakeModel(), openAiRecovery)
            || !settingsController.TryApply(replacementOpenAiSettings)
            || !controller.TryReplaceCurrentBindings()
            || concealmentModelSource.TakeModel()
                is not OpenAiCharacterObjectSabotageConcealmentModel replacementConcealment
            || ReferenceEquals(replacementConcealment, openAiConcealment)
            || !ReferenceEquals(concealmentModelSource.TakeModel(), replacementConcealment)
            || moveModelSource.TakeModel()
                is not OpenAiCharacterObjectSabotageMoveModel replacementMove
            || ReferenceEquals(replacementMove, openAiMove)
            || !ReferenceEquals(moveModelSource.TakeModel(), replacementMove)
            || substitutionModelSource.TakeModel()
                is not OpenAiCharacterObjectSabotageSubstitutionModel replacementSubstitution
            || ReferenceEquals(replacementSubstitution, openAiSubstitution)
            || !ReferenceEquals(
                substitutionModelSource.TakeModel(),
                replacementSubstitution)
            || contaminationModelSource.TakeModel()
                is not OpenAiCharacterObjectSabotageContaminationModel replacementContamination
            || ReferenceEquals(replacementContamination, openAiContamination)
            || !ReferenceEquals(
                contaminationModelSource.TakeModel(),
                replacementContamination)
            || influenceModelSource.TakeModel()
                is not OpenAiCharacterTargetSabotageInfluenceModel replacementInfluence
            || ReferenceEquals(replacementInfluence, openAiInfluence)
            || !ReferenceEquals(influenceModelSource.TakeModel(), replacementInfluence)
            || recoveryModelSource.TakeModel()
                is not OpenAiCharacterTargetSabotageInfluenceRecoveryModel replacementRecovery
            || ReferenceEquals(replacementRecovery, openAiRecovery)
            || !ReferenceEquals(recoveryModelSource.TakeModel(), replacementRecovery)
            || !settingsController.TryApply(activatedSettings)
            || !controller.TryReplaceCurrentBindings()
            || modelSource.TakeModel() is not OllamaCharacterModel
            || !ReferenceEquals(participationModelSource.TakeModel(), offlineParticipation)
            || !ReferenceEquals(concealmentModelSource.TakeModel(), offlineConcealment)
            || !ReferenceEquals(moveModelSource.TakeModel(), offlineMove)
            || !ReferenceEquals(substitutionModelSource.TakeModel(), offlineSubstitution)
            || !ReferenceEquals(contaminationModelSource.TakeModel(), offlineContamination)
            || !ReferenceEquals(influenceModelSource.TakeModel(), offlineInfluence)
            || !ReferenceEquals(recoveryModelSource.TakeModel(), offlineRecovery))
        {
            Fail("Provider composition did not activate and safely clear specialized bindings.");
            return;
        }

        GD.Print(
            "Temporary character-model composition smoke passed: unsupported Anthropic remained "
            + "offline, one exact persisted Ollama snapshot composed without input/output, "
            + "replacement was atomic and one-shot preserving, the current Ollama setting "
            + "activated as one stable future model, OpenAI activated exact ordinary and "
            + "specialized bindings with exact concealment, move, substitution, contamination, "
            + "influence, and recovery replacement, and Ollama restored fail-closed "
            + "participation, concealment, move, substitution, contamination, influence, and "
            + "recovery decisions.");
        GetTree().Quit();
    }

    private static CharacterModelBinding Binding(
        CharacterModelSettings settings,
        ICharacterModel model) =>
        new(settings.Provider, settings, model, new ScriptedConnectionTester());

    private sealed class ScriptedModel : ICharacterModel
    {
        public async IAsyncEnumerable<CharacterModelStreamUpdate> StreamResponseAsync(
            CharacterModelGenerationRequest request,
            [EnumeratorCancellation]
            CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            cancellationToken.ThrowIfCancellationRequested();
            await Task.CompletedTask;
            yield break;
        }
    }

    private sealed class ScriptedConnectionTester : ICharacterModelConnectionTester
    {
        public ValueTask<CharacterModelConnectionTestResult> TestConnectionAsync(
            CharacterModelSettings settings,
            CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(settings);
            cancellationToken.ThrowIfCancellationRequested();
            return ValueTask.FromResult(CharacterModelConnectionTestResult.Connected());
        }
    }

    private void Fail(string message)
    {
        GD.PushError(message);
        GetTree().Quit(exitCode: 1);
    }
}
