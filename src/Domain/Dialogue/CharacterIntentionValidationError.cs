namespace Allrpcs.Domain.Dialogue;

/// <summary>
/// Describes one deterministic structural error in a proposed character intention.
/// </summary>
public sealed record CharacterIntentionValidationError
{
    /// <summary>Creates one immutable intention-validation error.</summary>
    public CharacterIntentionValidationError(CharacterIntentionValidationErrorCode code)
    {
        Code = code;
    }

    /// <summary>Gets the stable kind of structural defect.</summary>
    public CharacterIntentionValidationErrorCode Code { get; }
}
