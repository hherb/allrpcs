namespace Allrpcs.Domain.Characters;

/// <summary>Validates one exact evidence selection or abstention against its request.</summary>
public static class CharacterDialogueMisquotationEvidenceDecisionPolicy
{
    /// <summary>Returns whether the exact actor may make the supplied current decision.</summary>
    public static bool CanAccept(
        CharacterDialogueMisquotationEvidenceConsiderationRequest request,
        CharacterDialogueMisquotationEvidenceDecision decision)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(decision);

        if (!ReferenceEquals(decision.ActorId, request.Actor.Identity.Id))
        {
            return false;
        }

        if (decision.SelectedEvidence is null)
        {
            return true;
        }

        return request.Candidates.Any(candidate =>
                ReferenceEquals(candidate, decision.SelectedEvidence))
            && CharacterDialogueMisquotationEvidencePolicy.CanObserve(
                request.Actor,
                decision.SelectedEvidence);
    }
}
