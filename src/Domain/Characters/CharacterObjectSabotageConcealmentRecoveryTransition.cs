using Allrpcs.Domain.Discovery;
using Allrpcs.Domain.WorldObjects;

namespace Allrpcs.Domain.Characters;

/// <summary>Returns fresh concealment state after evidence-authorized recovery.</summary>
public static class CharacterObjectSabotageConcealmentRecoveryTransition
{
    /// <summary>Returns a fresh snapshot with only the evidence-bound target recovered.</summary>
    public static CharacterObjectSabotageConcealmentRecoveryResult Recover(
        WorldObjectConcealmentSnapshot concealment,
        DiscoveryEvidenceSnapshot evidence,
        CharacterEvidenceViewSnapshot evidenceView,
        CharacterObjectSabotageEvidence candidate)
    {
        ArgumentNullException.ThrowIfNull(concealment);
        ArgumentNullException.ThrowIfNull(evidence);
        ArgumentNullException.ThrowIfNull(evidenceView);
        ArgumentNullException.ThrowIfNull(candidate);

        if (!CharacterObjectSabotageConcealmentRecoveryPolicy.CanRecover(
                concealment,
                evidence,
                evidenceView,
                candidate))
        {
            throw new InvalidOperationException(
                "Only visible authoritative sabotage evidence can recover its concealed target.");
        }

        WorldObjectConcealmentSnapshot resultingConcealment = new(
            concealment.ConcealedObjectIds.Where(
                objectId => objectId != candidate.Occurrence.Target.Identity.Id));

        return new CharacterObjectSabotageConcealmentRecoveryResult(
            resultingConcealment,
            evidenceView,
            candidate);
    }
}
