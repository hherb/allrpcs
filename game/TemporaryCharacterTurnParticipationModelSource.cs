using Allrpcs.Application.Dialogue;
using Allrpcs.Domain.Dialogue;
using Godot;

namespace Allrpcs.Game;

/// <summary>Supplies one caller-composed participation model or one stable offline default.</summary>
public partial class TemporaryCharacterTurnParticipationModelSource : Node
{
    private readonly ICharacterTurnParticipationModel offlineModel =
        new UnavailableParticipationModel();
    private CharacterTurnParticipationModelBindingSnapshot? bindings;
    private CharacterModelSettingsSelectionSnapshot? settingsSelection;
    private ICharacterTurnParticipationModel? suppliedModel;

    /// <summary>Retains exact configured settings and matching participation bindings once.</summary>
    public bool TryInitializeBindings(
        CharacterModelSettingsSelectionSnapshot selection,
        CharacterTurnParticipationModelBindingSnapshot snapshot)
    {
        ArgumentNullException.ThrowIfNull(selection);
        ArgumentNullException.ThrowIfNull(snapshot);

        if (bindings is not null
            || CharacterTurnParticipationModelSettingsBindingSelector.Select(
                selection,
                snapshot) is null)
        {
            return false;
        }

        settingsSelection = selection;
        bindings = snapshot;
        return true;
    }

    /// <summary>Atomically replaces configured participation bindings for future rounds.</summary>
    public bool TryReplaceBindings(
        CharacterModelSettingsSelectionSnapshot selection,
        CharacterTurnParticipationModelBindingSnapshot snapshot)
    {
        ArgumentNullException.ThrowIfNull(selection);
        ArgumentNullException.ThrowIfNull(snapshot);

        if (settingsSelection is null
            || bindings is null
            || CharacterTurnParticipationModelSettingsBindingSelector.Select(
                selection,
                snapshot) is null)
        {
            return false;
        }

        settingsSelection = selection;
        bindings = snapshot;
        return true;
    }

    /// <summary>Clears configured participation bindings while preserving any one-shot model.</summary>
    public bool TryClearBindings()
    {
        if (settingsSelection is null || bindings is null)
        {
            return false;
        }

        settingsSelection = null;
        bindings = null;
        return true;
    }

    /// <summary>Selects one exact available canonical provider without fallback.</summary>
    public bool TrySelectProvider(CharacterModelProvider provider)
    {
        ArgumentNullException.ThrowIfNull(provider);

        if (SelectConfiguredBinding(provider) is null)
        {
            return false;
        }

        settingsSelection = CharacterModelSettingsSelectionTransition.Select(
            provider,
            settingsSelection!);
        return true;
    }

    /// <summary>Retains one exact participation model for the next scene round only.</summary>
    public bool TrySupplyModel(ICharacterTurnParticipationModel model)
    {
        ArgumentNullException.ThrowIfNull(model);

        if (suppliedModel is not null)
        {
            return false;
        }

        suppliedModel = model;
        return true;
    }

    /// <summary>Takes an exact one-shot, selected bound, or stable offline model.</summary>
    public ICharacterTurnParticipationModel TakeModel()
    {
        ICharacterTurnParticipationModel model = suppliedModel ?? SelectBoundModel() ?? offlineModel;
        suppliedModel = null;
        return model;
    }

    private ICharacterTurnParticipationModel? SelectBoundModel()
    {
        if (settingsSelection is null || bindings is null)
        {
            return null;
        }

        return CharacterTurnParticipationModelSettingsBindingSelector.Select(
                settingsSelection,
                bindings)?.Model
            ?? throw new InvalidOperationException(
                "The initialized participation-model provider binding became unavailable.");
    }

    private CharacterTurnParticipationModelBinding? SelectConfiguredBinding(
        CharacterModelProvider provider)
    {
        CharacterModelSettings? settings = settingsSelection?.Settings.SingleOrDefault(
            candidate => ReferenceEquals(candidate.Provider, provider));
        if (settings is null || bindings is null)
        {
            return null;
        }

        CharacterTurnParticipationModelBinding? binding =
            CharacterTurnParticipationModelBindingSelector.Select(provider, bindings);
        return binding is not null && ReferenceEquals(binding.Settings, settings)
            ? binding
            : null;
    }

    private sealed class UnavailableParticipationModel : ICharacterTurnParticipationModel
    {
        public ValueTask<CharacterTurnParticipationDecision> DecideAsync(
            CharacterTurnParticipationConsiderationRequest request,
            CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            cancellationToken.ThrowIfCancellationRequested();
            return ValueTask.FromException<CharacterTurnParticipationDecision>(
                new InvalidOperationException("Participation decisions are unavailable."));
        }
    }
}
