namespace Allrpcs.Domain.Dialogue;

/// <summary>
/// Identifies one closed structural defect in the initial character-intention schema.
/// </summary>
public enum CharacterIntentionValidationErrorCode
{
    /// <summary>A targeted intention does not identify its target world object.</summary>
    MissingTargetObject,

    /// <summary>A wait intention unexpectedly identifies a target world object.</summary>
    UnexpectedTargetObject,
}
