using Allrpcs.Domain.Dialogue;
using Allrpcs.Domain.WorldObjects;

namespace Allrpcs.Application.Dialogue;

/// <summary>
/// Validates one complete provider-neutral response against its exact request and authoritative
/// world-object snapshot without executing or changing it.
/// </summary>
public static class CharacterTurnResponseValidationPolicy
{
    /// <summary>Returns every deterministic response error in stable evaluation order.</summary>
    public static CharacterTurnResponseValidationResult Validate(
        CharacterTurnRequest expectedRequest,
        CharacterTurnResponse response,
        WorldObjectSnapshot worldObjects,
        CharacterIntentionAffordanceSnapshot affordances)
    {
        ArgumentNullException.ThrowIfNull(expectedRequest);
        ArgumentNullException.ThrowIfNull(response);
        ArgumentNullException.ThrowIfNull(worldObjects);
        ArgumentNullException.ThrowIfNull(affordances);

        List<CharacterTurnResponseValidationError> errors = new();
        if (!ReferenceEquals(expectedRequest, response.Request))
        {
            errors.Add(new CharacterTurnResponseValidationError(
                CharacterTurnResponseValidationErrorCode.RequestMismatch,
                null));
        }

        bool immediateIsStructurallyValid = ValidateIntention(
            response.ImmediateIntention,
            worldObjects,
            errors);
        if (immediateIsStructurallyValid
            && IsKnownTarget(response.ImmediateIntention, worldObjects)
            && !CharacterTurnImmediateIntentionEligibilityPolicy.IsEligible(response, affordances))
        {
            errors.Add(new CharacterTurnResponseValidationError(
                CharacterTurnResponseValidationErrorCode.UnavailableImmediateIntention,
                response.ImmediateIntention));
        }

        foreach (CharacterCommitment commitment in response.Commitments)
        {
            foreach (CharacterIntention intention in commitment.ProposedIntentions)
            {
                ValidateIntention(intention, worldObjects, errors);
            }
        }

        return new CharacterTurnResponseValidationResult(errors);
    }

    private static bool ValidateIntention(
        CharacterIntention intention,
        WorldObjectSnapshot worldObjects,
        List<CharacterTurnResponseValidationError> errors)
    {
        CharacterIntentionValidationResult structuralResult =
            CharacterIntentionValidationPolicy.Validate(intention);
        foreach (CharacterIntentionValidationError structuralError in structuralResult.Errors)
        {
            errors.Add(new CharacterTurnResponseValidationError(
                Map(structuralError.Code),
                intention));
        }

        if (structuralResult.IsValid
            && intention.TargetObjectId is not null
            && WorldObjectLookup.FindById(intention.TargetObjectId, worldObjects) is null)
        {
            errors.Add(new CharacterTurnResponseValidationError(
                CharacterTurnResponseValidationErrorCode.UnknownTargetObject,
                intention));
        }

        return structuralResult.IsValid;
    }

    private static bool IsKnownTarget(
        CharacterIntention intention,
        WorldObjectSnapshot worldObjects) =>
        intention.TargetObjectId is null
        || WorldObjectLookup.FindById(intention.TargetObjectId, worldObjects) is not null;

    private static CharacterTurnResponseValidationErrorCode Map(
        CharacterIntentionValidationErrorCode code) => code switch
        {
            CharacterIntentionValidationErrorCode.MissingTargetObject =>
                CharacterTurnResponseValidationErrorCode.MissingTargetObject,
            CharacterIntentionValidationErrorCode.UnexpectedTargetObject =>
                CharacterTurnResponseValidationErrorCode.UnexpectedTargetObject,
            _ => throw new ArgumentOutOfRangeException(nameof(code)),
        };
}
