namespace Allrpcs.Domain.Discovery;

/// <summary>
/// Contains an ordered snapshot of duplicate-free observation-reinterpretation candidates.
/// </summary>
public sealed class ObservationReinterpretationCandidateSnapshot
{
    /// <summary>
    /// Creates an observation-reinterpretation candidate snapshot. An empty sequence explicitly
    /// means that no candidates are recorded.
    /// </summary>
    public ObservationReinterpretationCandidateSnapshot(
        IEnumerable<ObservationReinterpretationCandidate> entries)
    {
        ArgumentNullException.ThrowIfNull(entries);

        List<ObservationReinterpretationCandidate> orderedEntries = new();
        HashSet<ObservationReinterpretationCandidate> candidates = new();

        foreach (ObservationReinterpretationCandidate entry in entries)
        {
            if (entry is null)
            {
                throw new ArgumentException(
                    "Observation-reinterpretation candidate entries cannot contain null values.",
                    nameof(entries));
            }

            if (!candidates.Add(entry))
            {
                throw new ArgumentException(
                    "Observation-reinterpretation candidate entries cannot contain duplicate candidates.",
                    nameof(entries));
            }

            orderedEntries.Add(entry);
        }

        Entries = orderedEntries.AsReadOnly();
    }

    /// <summary>
    /// Gets observation-reinterpretation candidates in their stable supplied order.
    /// </summary>
    public IReadOnlyList<ObservationReinterpretationCandidate> Entries { get; }
}
