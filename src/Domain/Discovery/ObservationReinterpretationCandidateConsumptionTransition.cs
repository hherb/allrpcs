namespace Allrpcs.Domain.Discovery;

/// <summary>
/// Removes one eligible observation-reinterpretation candidate from a candidate snapshot.
/// </summary>
public static class ObservationReinterpretationCandidateConsumptionTransition
{
    /// <summary>
    /// Returns a new snapshot without the value-equal complete retained candidate.
    /// </summary>
    public static ObservationReinterpretationCandidateSnapshot Consume(
        ObservationReinterpretationCandidate candidate,
        ObservationReinterpretationCandidateSnapshot candidates)
    {
        if (!ObservationReinterpretationCandidateConsumptionPolicy.CanConsume(
                candidate,
                candidates))
        {
            throw new InvalidOperationException(
                "Only a complete retained observation-reinterpretation candidate may be consumed.");
        }

        return new ObservationReinterpretationCandidateSnapshot(
            candidates.Entries.Where(entry => entry != candidate));
    }
}
