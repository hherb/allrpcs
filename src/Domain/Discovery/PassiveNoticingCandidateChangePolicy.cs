using Allrpcs.Domain.Perception;

namespace Allrpcs.Domain.Discovery;

/// <summary>
/// Decides whether one passive-noticing candidate would change existing perception.
/// </summary>
public static class PassiveNoticingCandidateChangePolicy
{
    /// <summary>
    /// Returns true only when the candidate applies and its retained target is unnoticed.
    /// </summary>
    public static bool WouldChange(
        PassiveNoticingCandidate candidate,
        CharacterPerceptionSnapshot perception)
    {
        ArgumentNullException.ThrowIfNull(candidate);
        ArgumentNullException.ThrowIfNull(perception);

        if (!PassiveNoticingCandidateApplicabilityPolicy.CanApply(candidate, perception))
        {
            return false;
        }

        WorldObjectVisibility target = CharacterPerceptionLookup.FindByObject(
            candidate.TargetObjectId,
            perception)!;

        return target.VisibilityState == VisibilityState.Unnoticed;
    }
}
