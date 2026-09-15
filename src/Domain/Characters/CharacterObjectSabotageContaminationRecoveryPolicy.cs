using Allrpcs.Domain.Discovery;
using Allrpcs.Domain.WorldObjects;

namespace Allrpcs.Domain.Characters;

/// <summary>Requires visible authoritative sabotage evidence to recover one contaminated target.</summary>
public static class CharacterObjectSabotageContaminationRecoveryPolicy
{
    /// <summary>Returns whether exact retained evidence permits recovery of its contaminated target.</summary>
    public static bool CanRecover(
        WorldObjectContaminationSnapshot contamination,
        DiscoveryEvidenceSnapshot evidence,
        CharacterEvidenceViewSnapshot evidenceView,
        CharacterObjectSabotageEvidence candidate)
    {
        ArgumentNullException.ThrowIfNull(contamination);
        ArgumentNullException.ThrowIfNull(evidence);
        ArgumentNullException.ThrowIfNull(evidenceView);
        ArgumentNullException.ThrowIfNull(candidate);

        return candidate.Occurrence.Proposal.ActionKind == CharacterSabotageActionKind.Contaminate
            && candidate.Evidence.ObjectId == candidate.Occurrence.Target.Identity.Id
            && contamination.IsContaminated(candidate.Occurrence.Target.Identity.Id)
            && evidence.Entries.Any(definition => ReferenceEquals(definition, candidate.Evidence))
            && evidenceView.EvidenceIds.Any(
                evidenceId => evidenceId == candidate.Evidence.EvidenceId);
    }
}
