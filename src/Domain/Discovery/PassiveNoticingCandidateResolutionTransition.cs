using Allrpcs.Domain.Perception;

namespace Allrpcs.Domain.Discovery;

/// <summary>
/// Applies and consumes one eligible passive-noticing candidate as a single pure resolution.
/// </summary>
public static class PassiveNoticingCandidateResolutionTransition
{
    /// <summary>
    /// Returns the perception and candidate snapshots produced by resolving the supplied candidate.
    /// </summary>
    public static PassiveNoticingCandidateResolutionResult Resolve(
        PassiveNoticingCandidate candidate,
        CharacterPerceptionSnapshot perception,
        PassiveNoticingCandidateSnapshot candidates)
    {
        if (!PassiveNoticingCandidateResolutionPolicy.CanResolve(
                candidate,
                perception,
                candidates))
        {
            throw new InvalidOperationException(
                "Only a perception-changing retained passive-noticing candidate may be resolved.");
        }

        CharacterPerceptionSnapshot resolvedPerception =
            PassiveNoticingCandidateApplicationTransition.Apply(candidate, perception);
        PassiveNoticingCandidateSnapshot remainingCandidates =
            PassiveNoticingCandidateConsumptionTransition.Consume(candidate, candidates);

        return new PassiveNoticingCandidateResolutionResult(
            resolvedPerception,
            remainingCandidates);
    }
}
