using Allrpcs.Application.Dialogue;
using Allrpcs.Domain.Dialogue;

namespace Allrpcs.Infrastructure.Dialogue;

/// <summary>
/// Maps one bounded JSON response document to a provider-neutral terminal stream update.
/// </summary>
public static class CharacterModelJsonResponseCompletion
{
    /// <summary>
    /// Returns an exact completed response or a redacted invalid-response failure.
    /// </summary>
    public static CharacterModelStreamTermination Create(
        CharacterTurnRequest request,
        string serialized)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(serialized);

        try
        {
            CharacterTurnResponse response = CharacterTurnResponseJsonDeserializer.Deserialize(
                request,
                serialized);
            return new CharacterModelStreamTermination(
                CharacterModelStreamTerminationStatus.Completed,
                response,
                null);
        }
        catch (ArgumentException)
        {
            return new CharacterModelStreamTermination(
                CharacterModelStreamTerminationStatus.Failed,
                null,
                new CharacterModelFailure(CharacterModelFailureCode.InvalidResponse));
        }
    }
}
