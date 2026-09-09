namespace Allrpcs.Application.Dialogue;

/// <summary>
/// Streams provider-neutral updates for one character-model generation request.
/// </summary>
public interface ICharacterModel
{
    /// <summary>
    /// Streams exact text deltas followed by one terminal update, observing caller cancellation.
    /// </summary>
    IAsyncEnumerable<CharacterModelStreamUpdate> StreamResponseAsync(
        CharacterModelGenerationRequest request,
        CancellationToken cancellationToken);
}
