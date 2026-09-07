using Allrpcs.Domain.Characters;
using Allrpcs.Domain.WorldObjects;

namespace Allrpcs.Domain.Discovery;

/// <summary>
/// Records one passive-noticing candidate backed by an authoritative character and world object.
/// </summary>
public static class PassiveNoticingCandidateProductionTransition
{
    /// <summary>
    /// Returns a new snapshot with the eligible authoritative candidate appended.
    /// </summary>
    public static PassiveNoticingCandidateSnapshot Produce(
        PassiveNoticingCandidate candidate,
        Character character,
        WorldObject worldObject,
        PassiveNoticingCandidateSnapshot candidates)
    {
        if (!PassiveNoticingCandidateProductionPolicy.CanProduce(
                candidate,
                character,
                worldObject,
                candidates))
        {
            throw new InvalidOperationException(
                "The supplied passive-noticing candidate is not eligible for production.");
        }

        return PassiveNoticingCandidateRecordingTransition.Record(candidate, candidates);
    }
}
