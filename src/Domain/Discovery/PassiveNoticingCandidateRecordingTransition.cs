namespace Allrpcs.Domain.Discovery;

/// <summary>
/// Records one eligible passive-noticing candidate in a candidate snapshot.
/// </summary>
public static class PassiveNoticingCandidateRecordingTransition
{
    /// <summary>
    /// Returns a new snapshot with the supplied candidate appended after all retained candidates.
    /// </summary>
    public static PassiveNoticingCandidateSnapshot Record(
        PassiveNoticingCandidate candidate,
        PassiveNoticingCandidateSnapshot candidates)
    {
        if (!PassiveNoticingCandidateRecordingPolicy.CanRecord(candidate, candidates))
        {
            throw new InvalidOperationException(
                "The supplied passive-noticing candidate is already recorded.");
        }

        return new PassiveNoticingCandidateSnapshot(candidates.Entries.Append(candidate));
    }
}
