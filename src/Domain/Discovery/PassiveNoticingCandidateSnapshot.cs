namespace Allrpcs.Domain.Discovery;

/// <summary>
/// Contains an ordered snapshot of duplicate-free passive-noticing candidates.
/// </summary>
public sealed class PassiveNoticingCandidateSnapshot
{
    /// <summary>
    /// Creates a passive-noticing candidate snapshot. An empty sequence explicitly means that no
    /// candidates are recorded.
    /// </summary>
    public PassiveNoticingCandidateSnapshot(IEnumerable<PassiveNoticingCandidate> entries)
    {
        ArgumentNullException.ThrowIfNull(entries);

        List<PassiveNoticingCandidate> orderedEntries = new();
        HashSet<PassiveNoticingCandidate> candidates = new();

        foreach (PassiveNoticingCandidate entry in entries)
        {
            if (entry is null)
            {
                throw new ArgumentException(
                    "Passive-noticing candidate entries cannot contain null values.",
                    nameof(entries));
            }

            if (!candidates.Add(entry))
            {
                throw new ArgumentException(
                    "Passive-noticing candidate entries cannot contain duplicate candidates.",
                    nameof(entries));
            }

            orderedEntries.Add(entry);
        }

        Entries = orderedEntries.AsReadOnly();
    }

    /// <summary>
    /// Gets passive-noticing candidates in their stable supplied order.
    /// </summary>
    public IReadOnlyList<PassiveNoticingCandidate> Entries { get; }
}
