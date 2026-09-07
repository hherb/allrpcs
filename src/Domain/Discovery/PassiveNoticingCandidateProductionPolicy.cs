using Allrpcs.Domain.Characters;
using Allrpcs.Domain.WorldObjects;

namespace Allrpcs.Domain.Discovery;

/// <summary>
/// Decides whether an authoritative character and world object can produce one existing
/// passive-noticing candidate.
/// </summary>
public static class PassiveNoticingCandidateProductionPolicy
{
    /// <summary>
    /// Returns true when both authoritative identities match and the candidate is not recorded.
    /// </summary>
    public static bool CanProduce(
        PassiveNoticingCandidate candidate,
        Character character,
        WorldObject worldObject,
        PassiveNoticingCandidateSnapshot candidates)
    {
        ArgumentNullException.ThrowIfNull(candidate);
        ArgumentNullException.ThrowIfNull(character);
        ArgumentNullException.ThrowIfNull(worldObject);
        ArgumentNullException.ThrowIfNull(candidates);

        return candidate.ObserverCharacterId == character.Identity.Id
            && candidate.TargetObjectId == worldObject.Identity.Id
            && PassiveNoticingCandidateRecordingPolicy.CanRecord(candidate, candidates);
    }
}
