namespace Allrpcs.Application.Dialogue;

/// <summary>
/// Contains ordered provider-neutral errors for one complete proposed character-turn response.
/// </summary>
public sealed class CharacterTurnResponseValidationResult
{
    /// <summary>Creates an immutable validation result from errors in deterministic order.</summary>
    public CharacterTurnResponseValidationResult(
        IEnumerable<CharacterTurnResponseValidationError> errors)
    {
        ArgumentNullException.ThrowIfNull(errors);

        List<CharacterTurnResponseValidationError> retainedErrors = new();
        foreach (CharacterTurnResponseValidationError error in errors)
        {
            if (error is null)
            {
                throw new ArgumentException(
                    "Response-validation errors cannot contain null values.",
                    nameof(errors));
            }

            retainedErrors.Add(error);
        }

        Errors = retainedErrors.AsReadOnly();
    }

    /// <summary>Gets whether the complete response passed every validation boundary.</summary>
    public bool IsValid => Errors.Count == 0;

    /// <summary>Gets validation errors in deterministic evaluation order.</summary>
    public IReadOnlyList<CharacterTurnResponseValidationError> Errors { get; }
}
