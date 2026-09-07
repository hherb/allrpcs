using Allrpcs.Domain.Identifiers;
using Allrpcs.Domain.Perception;

namespace Allrpcs.Domain.Discovery;

/// <summary>
/// Selects the next resolvable passive-noticing candidate for one observer.
/// </summary>
public static class PassiveNoticingCandidateResolutionSelection
{
    /// <summary>
    /// Returns the exact first resolvable retained candidate in stable order, or null.
    /// </summary>
    public static PassiveNoticingCandidate? SelectNext(
        CharacterId observerCharacterId,
        CharacterPerceptionSnapshot perception,
        PassiveNoticingCandidateSnapshot candidates)
    {
        ArgumentNullException.ThrowIfNull(observerCharacterId);
        ArgumentNullException.ThrowIfNull(perception);
        ArgumentNullException.ThrowIfNull(candidates);

        return candidates.Entries.FirstOrDefault(
            candidate => candidate.ObserverCharacterId == observerCharacterId
                && PassiveNoticingCandidateResolutionPolicy.CanResolve(
                    candidate,
                    perception,
                    candidates));
    }
}
