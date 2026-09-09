using System.Runtime.CompilerServices;
using Allrpcs.Application.Dialogue;
using Godot;

namespace Allrpcs.Game;

/// <summary>Supplies one caller-composed scene model or the stable offline default.</summary>
public partial class TemporaryCharacterModelSource : Node
{
    private readonly ICharacterModel offlineModel = new UnavailableCharacterModel();
    private ICharacterModel? suppliedModel;

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

    /// <summary>Takes the exact supplied model once, or returns the stable offline default.</summary>
    public ICharacterModel TakeModel()
    {
        ICharacterModel model = suppliedModel ?? offlineModel;
        suppliedModel = null;
        return model;
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
