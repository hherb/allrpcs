using Allrpcs.Domain.Discovery;
using Allrpcs.Domain.WorldObjects;

namespace Allrpcs.Domain.Characters;

/// <summary>Returns fresh contamination state after evidence-authorized recovery.</summary>
public static class CharacterObjectSabotageContaminationRecoveryTransition
{
    /// <summary>Returns a fresh snapshot with only the evidence-bound target recovered.</summary>
    public static CharacterObjectSabotageContaminationRecoveryResult Recover(
        WorldObjectContaminationSnapshot contamination,
        DiscoveryEvidenceSnapshot evidence,
        CharacterEvidenceViewSnapshot evidenceView,
        CharacterObjectSabotageEvidence candidate)
    {
        ArgumentNullException.ThrowIfNull(contamination);
        ArgumentNullException.ThrowIfNull(evidence);
        ArgumentNullException.ThrowIfNull(evidenceView);
        ArgumentNullException.ThrowIfNull(candidate);

        if (!CharacterObjectSabotageContaminationRecoveryPolicy.CanRecover(
                contamination,
                evidence,
                evidenceView,
                candidate))
        {
            throw new InvalidOperationException(
                "Only visible authoritative sabotage evidence can recover its contaminated target.");
        }

        WorldObjectContaminationSnapshot resultingContamination = new(
            contamination.ContaminatedObjectIds.Where(
                objectId => objectId != candidate.Occurrence.Target.Identity.Id));

        return new CharacterObjectSabotageContaminationRecoveryResult(
            resultingContamination,
            evidenceView,
            candidate);
    }
}
