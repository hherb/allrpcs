namespace Allrpcs.Domain.Dialogue;

/// <summary>
/// Contains deterministic character-intention validation errors in policy order.
/// </summary>
public sealed class CharacterIntentionValidationResult
{
    /// <summary>Creates an immutable validation result from exact error instances.</summary>
    public CharacterIntentionValidationResult(
        IEnumerable<CharacterIntentionValidationError> errors)
    {
        ArgumentNullException.ThrowIfNull(errors);

        List<CharacterIntentionValidationError> retainedErrors = new();
        foreach (CharacterIntentionValidationError error in errors)
        {
            if (error is null)
            {
                throw new ArgumentException(
                    "Validation errors cannot contain null values.",
                    nameof(errors));
            }

            retainedErrors.Add(error);
        }

        Errors = retainedErrors.AsReadOnly();
    }

    /// <summary>Gets whether validation found no structural defects.</summary>
    public bool IsValid => Errors.Count == 0;

    /// <summary>Gets exact errors in deterministic policy order.</summary>
    public IReadOnlyList<CharacterIntentionValidationError> Errors { get; }
}
