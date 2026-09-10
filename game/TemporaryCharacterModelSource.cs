using System.Runtime.CompilerServices;
using Allrpcs.Application.Dialogue;
using Godot;

namespace Allrpcs.Game;

/// <summary>Supplies one caller-composed scene model or the stable offline default.</summary>
public partial class TemporaryCharacterModelSource : Node
{
    private readonly ICharacterModel offlineModel = new UnavailableCharacterModel();
    private CharacterModelBindingSnapshot? bindings;
    private CharacterModelProvider? selectedProvider;
    private ICharacterModel? suppliedModel;

    /// <summary>Retains one exact binding snapshot and available initial provider once.</summary>
    public bool TryInitializeBindings(
        CharacterModelBindingSnapshot snapshot,
        CharacterModelProvider initialProvider)
    {
        ArgumentNullException.ThrowIfNull(snapshot);
        ArgumentNullException.ThrowIfNull(initialProvider);

        if (bindings is not null
            || CharacterModelBindingSelector.Select(initialProvider, snapshot) is null)
        {
            return false;
        }

        bindings = snapshot;
        selectedProvider = initialProvider;
        return true;
    }

    /// <summary>Selects one exact available canonical provider without fallback.</summary>
    public bool TrySelectProvider(CharacterModelProvider provider)
    {
        ArgumentNullException.ThrowIfNull(provider);

        if (bindings is null
            || CharacterModelBindingSelector.Select(provider, bindings) is null)
        {
            return false;
        }

        selectedProvider = provider;
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
        if (bindings is null || selectedProvider is null)
        {
            return null;
        }

        return CharacterModelBindingSelector.Select(selectedProvider, bindings)?.Model
            ?? throw new InvalidOperationException(
                "The initialized character-model provider binding became unavailable.");
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
