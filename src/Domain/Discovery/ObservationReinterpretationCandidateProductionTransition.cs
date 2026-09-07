using Allrpcs.Domain.Characters;
using Allrpcs.Domain.WorldObjects;

namespace Allrpcs.Domain.Discovery;

/// <summary>
/// Records one observation-reinterpretation candidate backed by an authoritative character and
/// world object.
/// </summary>
public static class ObservationReinterpretationCandidateProductionTransition
{
    /// <summary>
    /// Returns a new snapshot with the eligible authoritative candidate appended.
    /// </summary>
    public static ObservationReinterpretationCandidateSnapshot Produce(
        ObservationReinterpretationCandidate candidate,
        Character character,
        WorldObject worldObject,
        ObservationReinterpretationCandidateSnapshot candidates)
    {
        if (!ObservationReinterpretationCandidateProductionPolicy.CanProduce(
                candidate,
                character,
                worldObject,
                candidates))
        {
            throw new InvalidOperationException(
                "The supplied observation-reinterpretation candidate is not eligible for production.");
        }

        return ObservationReinterpretationCandidateRecordingTransition.Record(
            candidate,
            candidates);
    }
}
