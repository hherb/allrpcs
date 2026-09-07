namespace Allrpcs.Domain.Discovery;

/// <summary>
/// Records one eligible observation-reinterpretation candidate in a candidate snapshot.
/// </summary>
public static class ObservationReinterpretationCandidateRecordingTransition
{
    /// <summary>
    /// Returns a new snapshot with the supplied candidate appended after all retained candidates.
    /// </summary>
    public static ObservationReinterpretationCandidateSnapshot Record(
        ObservationReinterpretationCandidate candidate,
        ObservationReinterpretationCandidateSnapshot candidates)
    {
        if (!ObservationReinterpretationCandidateRecordingPolicy.CanRecord(candidate, candidates))
        {
            throw new InvalidOperationException(
                "The supplied observation-reinterpretation candidate is already recorded.");
        }

        return new ObservationReinterpretationCandidateSnapshot(
            candidates.Entries.Append(candidate));
    }
}
