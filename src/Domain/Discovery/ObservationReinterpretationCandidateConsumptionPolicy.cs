namespace Allrpcs.Domain.Discovery;

/// <summary>
/// Decides whether one complete observation-reinterpretation candidate is retained for consumption
/// without changing the candidate snapshot.
/// </summary>
public static class ObservationReinterpretationCandidateConsumptionPolicy
{
    /// <summary>
    /// Returns true when a value-equal complete candidate is retained in the snapshot.
    /// </summary>
    public static bool CanConsume(
        ObservationReinterpretationCandidate candidate,
        ObservationReinterpretationCandidateSnapshot candidates)
    {
        ArgumentNullException.ThrowIfNull(candidate);

        ObservationReinterpretationCandidate? retained =
            ObservationReinterpretationCandidateLookup.Find(candidate, candidates);

        return retained == candidate;
    }
}
