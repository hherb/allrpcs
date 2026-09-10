using System.Runtime.CompilerServices;
using Allrpcs.Application.Dialogue;
using Godot;

namespace Allrpcs.Game;

/// <summary>Supplies one caller-composed scene model or the stable offline default.</summary>
public partial class TemporaryCharacterModelSource : Node
{
    private readonly ICharacterModel offlineModel = new UnavailableCharacterModel();
    private CharacterModelBindingSnapshot? bindings;
    private CharacterModelSettingsSelectionSnapshot? settingsSelection;
    private ICharacterModel? suppliedModel;

    /// <summary>Retains exact configured settings and matching caller-composed bindings once.</summary>
    public bool TryInitializeBindings(
        CharacterModelSettingsSelectionSnapshot selection,
        CharacterModelBindingSnapshot snapshot)
    {
        ArgumentNullException.ThrowIfNull(selection);
        ArgumentNullException.ThrowIfNull(snapshot);

        if (bindings is not null
            || CharacterModelSettingsBindingSelector.Select(selection, snapshot) is null)
        {
            return false;
        }

        settingsSelection = selection;
        bindings = snapshot;
        return true;
    }

    /// <summary>Atomically replaces configured bindings for future model takes only.</summary>
    public bool TryReplaceBindings(
        CharacterModelSettingsSelectionSnapshot selection,
        CharacterModelBindingSnapshot snapshot)
    {
        ArgumentNullException.ThrowIfNull(selection);
        ArgumentNullException.ThrowIfNull(snapshot);

        if (settingsSelection is null
            || bindings is null
            || CharacterModelSettingsBindingSelector.Select(selection, snapshot) is null)
        {
            return false;
        }

        settingsSelection = selection;
        bindings = snapshot;
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

    /// <summary>Retains one exact model for the next scene generation only.</summary>
    public bool TrySupplyModel(ICharacterModel model)
    {
        ArgumentNullException.ThrowIfNull(model);

        if (suppliedModel is not null)
        {
            return false;
        }

        suppliedModel = model;
        return true;
    }

    /// <summary>
    /// Takes an exact one-shot supplied model, the selected bound model, or the offline default.
    /// </summary>
    public ICharacterModel TakeModel()
    {
        ICharacterModel model = suppliedModel ?? SelectBoundModel() ?? offlineModel;
        suppliedModel = null;
        return model;
    }

    private ICharacterModel? SelectBoundModel()
    {
        if (settingsSelection is null || bindings is null)
        {
            return null;
        }

        return CharacterModelSettingsBindingSelector.Select(settingsSelection, bindings)?.Model
            ?? throw new InvalidOperationException(
                "The initialized character-model provider binding became unavailable.");
    }

    private CharacterModelBinding? SelectConfiguredBinding(CharacterModelProvider provider)
    {
        CharacterModelSettings? settings = settingsSelection?.Settings.SingleOrDefault(
            candidate => ReferenceEquals(candidate.Provider, provider));
        if (settings is null || bindings is null)
        {
            return null;
        }

        CharacterModelBinding? binding = CharacterModelBindingSelector.Select(provider, bindings);
        return binding is not null && ReferenceEquals(binding.Settings, settings)
            ? binding
            : null;
    }

    private sealed class UnavailableCharacterModel : ICharacterModel
    {
        public async IAsyncEnumerable<CharacterModelStreamUpdate> StreamResponseAsync(
            CharacterModelGenerationRequest request,
            [EnumeratorCancellation] CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            cancellationToken.ThrowIfCancellationRequested();

            await Task.CompletedTask;
            yield return new CharacterModelStreamTermination(
                CharacterModelStreamTerminationStatus.Failed,
                null,
                new CharacterModelFailure(CharacterModelFailureCode.Unavailable));
        }
    }
}
