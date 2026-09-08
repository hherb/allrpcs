namespace Allrpcs.Domain.Dialogue;

/// <summary>
/// Validates the target shape of one proposed initial character intention without executing it.
/// </summary>
public static class CharacterIntentionValidationPolicy
{
    /// <summary>Returns the deterministic structural validation result.</summary>
    public static CharacterIntentionValidationResult Validate(CharacterIntention intention)
    {
        ArgumentNullException.ThrowIfNull(intention);

        if (intention.Type == CharacterIntentionType.Wait)
        {
            return intention.TargetObjectId is null
                ? new CharacterIntentionValidationResult([])
                : ResultWith(CharacterIntentionValidationErrorCode.UnexpectedTargetObject);
        }

        return intention.TargetObjectId is null
            ? ResultWith(CharacterIntentionValidationErrorCode.MissingTargetObject)
            : new CharacterIntentionValidationResult([]);
    }

    private static CharacterIntentionValidationResult ResultWith(
        CharacterIntentionValidationErrorCode code)
    {
        return new CharacterIntentionValidationResult(
            [new CharacterIntentionValidationError(code)]);
    }
}
