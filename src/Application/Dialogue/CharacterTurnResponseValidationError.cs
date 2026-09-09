using Allrpcs.Domain.Dialogue;

namespace Allrpcs.Application.Dialogue;

/// <summary>
/// Retains one deterministic complete-response validation error and its exact offending intention.
/// </summary>
public sealed record CharacterTurnResponseValidationError
{
    /// <summary>Creates an immutable response-validation error.</summary>
    public CharacterTurnResponseValidationError(
        CharacterTurnResponseValidationErrorCode code,
        CharacterIntention? intention)
    {
        if (!Enum.IsDefined(code))
        {
            throw new ArgumentOutOfRangeException(nameof(code));
        }

        if ((code == CharacterTurnResponseValidationErrorCode.RequestMismatch) != (intention is null))
        {
            throw new ArgumentException(
                "Only a request-mismatch error can omit the offending intention.",
                nameof(intention));
        }

        Code = code;
        Intention = intention;
    }

    /// <summary>Gets the stable validation-error kind.</summary>
    public CharacterTurnResponseValidationErrorCode Code { get; }

    /// <summary>Gets the exact offending intention, or null for a request mismatch.</summary>
    public CharacterIntention? Intention { get; }
}
