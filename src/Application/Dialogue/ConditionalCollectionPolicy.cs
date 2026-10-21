using Allrpcs.Domain.Dialogue;

namespace Allrpcs.Application.Dialogue;

/// <summary>Decides whether a character collects what it has just examined.</summary>
public static class ConditionalCollectionPolicy
{
    /// <summary>
    /// Collects only after a safe result, and only when the turn asked for it: the character
    /// conditionally accepted an examination of this object ("gather them if they are safe"), or
    /// committed to collecting it. A plain accepted examination leaves the object in place.
    /// </summary>
    // shortcut: any conditional acceptance counts as "collect if safe" (the condition text is not
    // interpreted); upgrade when a turn can carry other conditions.
    public static bool Collects(CharacterTurnResponse response, CharacterExaminationResult result)
    {
        ArgumentNullException.ThrowIfNull(response);
        ArgumentNullException.ThrowIfNull(result);

        return result.Safety == CharacterExaminationSafety.SafeToCollect
            && ((response.Disposition == CharacterTurnDisposition.ConditionallyAccept
                    && response.ImmediateIntention.TargetObjectId == result.TargetObjectId)
                || response.Commitments.Any(commitment => commitment.ProposedIntentions.Any(
                    intention => intention.Type == CharacterIntentionType.CollectObject
                        && intention.TargetObjectId == result.TargetObjectId)));
    }
}
