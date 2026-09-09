using System.Runtime.CompilerServices;
using Allrpcs.Application.Dialogue;
using Godot;

namespace Allrpcs.Game;

/// <summary>Provides one stable credential-free unavailable character model for offline play.</summary>
public partial class TemporaryOfflineCharacterModelSource : Node
{
    /// <summary>Gets the single retained provider-neutral offline model.</summary>
    public ICharacterModel Model { get; } = new UnavailableCharacterModel();

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
