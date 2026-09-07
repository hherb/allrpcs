using Allrpcs.Domain.Identifiers;
using Allrpcs.Domain.Perception;

namespace Allrpcs.Domain.Discovery;

/// <summary>
/// Selects and resolves the next eligible passive-noticing candidate for one observer.
/// </summary>
public static class PassiveNoticingCandidateResolutionSequence
{
    /// <summary>
    /// Returns the atomic resolution of the exact first eligible retained candidate.
    /// </summary>
    public static PassiveNoticingCandidateResolutionResult ResolveNext(
        CharacterId observerCharacterId,
        CharacterPerceptionSnapshot perception,
        PassiveNoticingCandidateSnapshot candidates)
    {
        PassiveNoticingCandidate? candidate =
            PassiveNoticingCandidateResolutionSelection.SelectNext(
                observerCharacterId,
                perception,
                candidates);

        if (candidate is null)
        {
            throw new InvalidOperationException(
                "No passive-noticing candidate can be resolved for the supplied observer.");
        }

        return PassiveNoticingCandidateResolutionTransition.Resolve(
            candidate,
            perception,
            candidates);
    }
}
