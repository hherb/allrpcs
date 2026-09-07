using Allrpcs.Domain.Perception;

namespace Allrpcs.Domain.Discovery;

/// <summary>
/// Decides whether one passive-noticing candidate applies to one character perception snapshot
/// without changing perception.
/// </summary>
public static class PassiveNoticingCandidateApplicabilityPolicy
{
    /// <summary>
    /// Returns true when the snapshot represents the candidate's observer and contains its target.
    /// </summary>
    public static bool CanApply(
        PassiveNoticingCandidate candidate,
        CharacterPerceptionSnapshot perception)
    {
        ArgumentNullException.ThrowIfNull(candidate);
        ArgumentNullException.ThrowIfNull(perception);

        return perception.CharacterId == candidate.ObserverCharacterId
            && CharacterPerceptionLookup.FindByObject(
                candidate.TargetObjectId,
                perception) is not null;
    }
}
