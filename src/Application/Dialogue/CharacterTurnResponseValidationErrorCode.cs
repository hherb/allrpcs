namespace Allrpcs.Application.Dialogue;

/// <summary>
/// Identifies one provider-neutral defect in a complete proposed character-turn response.
/// </summary>
public enum CharacterTurnResponseValidationErrorCode
{
    /// <summary>The response does not retain the exact request expected by the caller.</summary>
    RequestMismatch,

    /// <summary>A targeted intention omits its required target object.</summary>
    MissingTargetObject,

    /// <summary>A wait intention unexpectedly names a target object.</summary>
    UnexpectedTargetObject,

    /// <summary>A structurally valid targeted intention names no authoritative world object.</summary>
    UnknownTargetObject,

    /// <summary>The structurally valid known immediate intention is not currently permitted.</summary>
    UnavailableImmediateIntention,
}
