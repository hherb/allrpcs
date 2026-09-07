using Allrpcs.Domain.Perception;

namespace Allrpcs.Domain.Discovery;

/// <summary>
/// Decides whether one passive-noticing candidate can change perception and then be consumed
/// without changing either input snapshot.
/// </summary>
public static class PassiveNoticingCandidateResolutionPolicy
{
    /// <summary>
    /// Returns true when the candidate would change perception and is retained for consumption.
    /// </summary>
    public static bool CanResolve(
        PassiveNoticingCandidate candidate,
        CharacterPerceptionSnapshot perception,
        PassiveNoticingCandidateSnapshot candidates) =>
        PassiveNoticingCandidateChangePolicy.WouldChange(candidate, perception)
        && PassiveNoticingCandidateConsumptionPolicy.CanConsume(candidate, candidates);
}
