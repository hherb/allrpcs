using Allrpcs.Domain.Discovery;
using Allrpcs.Domain.WorldObjects;

namespace Allrpcs.Domain.Characters;

/// <summary>Requires visible authoritative sabotage evidence to recover one concealed target.</summary>
public static class CharacterObjectSabotageConcealmentRecoveryPolicy
{
    /// <summary>Returns whether exact retained evidence permits recovery of its concealed target.</summary>
    public static bool CanRecover(
        WorldObjectConcealmentSnapshot concealment,
        DiscoveryEvidenceSnapshot evidence,
        CharacterEvidenceViewSnapshot evidenceView,
        CharacterObjectSabotageEvidence candidate)
    {
        ArgumentNullException.ThrowIfNull(concealment);
        ArgumentNullException.ThrowIfNull(evidence);
        ArgumentNullException.ThrowIfNull(evidenceView);
        ArgumentNullException.ThrowIfNull(candidate);

        return candidate.Occurrence.Proposal.ActionKind == CharacterSabotageActionKind.Conceal
            && candidate.Evidence.ObjectId == candidate.Occurrence.Target.Identity.Id
            && concealment.ConcealedObjectIds.Any(
                objectId => objectId == candidate.Occurrence.Target.Identity.Id)
            && evidence.Entries.Any(
                definition => ReferenceEquals(definition, candidate.Evidence))
            && evidenceView.EvidenceIds.Any(
                evidenceId => evidenceId == candidate.Evidence.EvidenceId);
    }
}
