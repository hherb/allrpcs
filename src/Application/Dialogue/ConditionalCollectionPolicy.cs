using Allrpcs.Domain.Dialogue;

namespace Allrpcs.Application.Dialogue;

/// <summary>Decides whether a character collects what it has just examined.</summary>
public static class ConditionalCollectionPolicy
{
    /// <summary>
    /// Collects only after a safe result, and only when the turn asked for it: the character
    /// conditionally accepted ("gather them if they are safe"), or committed to collecting the
    /// examined object. A plain accepted examination leaves the object in place.
    /// </summary>
    public static bool Collects(CharacterTurnResponse response, CharacterExaminationResult result)
    {
        ArgumentNullException.ThrowIfNull(response);
        ArgumentNullException.ThrowIfNull(result);

        return result.Safety == CharacterExaminationSafety.SafeToCollect
            && (response.Disposition == CharacterTurnDisposition.ConditionallyAccept
                || response.Commitments.Any(commitment => commitment.ProposedIntentions.Any(
                    intention => intention.Type == CharacterIntentionType.CollectObject
                        && intention.TargetObjectId == result.TargetObjectId)));
    }
}
