namespace Allrpcs.Domain.Discovery;

/// <summary>
/// Contains an ordered snapshot of focused-search outcomes with duplicate-free requests.
/// </summary>
public sealed class FocusedSearchOutcomeSnapshot
{
    /// <summary>
    /// Creates a focused-search outcome snapshot. An empty sequence explicitly means that no
    /// focused-search outcomes are recorded.
    /// </summary>
    public FocusedSearchOutcomeSnapshot(IEnumerable<FocusedSearchOutcome> entries)
    {
        ArgumentNullException.ThrowIfNull(entries);

        List<FocusedSearchOutcome> orderedEntries = new();
        HashSet<FocusedSearchRequest> requests = new();

        foreach (FocusedSearchOutcome entry in entries)
        {
            if (entry is null)
            {
                throw new ArgumentException(
                    "Focused-search outcome entries cannot contain null values.",
                    nameof(entries));
            }

            if (!requests.Add(entry.Request))
            {
                throw new ArgumentException(
                    "Focused-search outcome entries cannot contain duplicate requests.",
                    nameof(entries));
            }

            orderedEntries.Add(entry);
        }

        Entries = orderedEntries.AsReadOnly();
    }

    /// <summary>
    /// Gets focused-search outcomes in their stable supplied order.
    /// </summary>
    public IReadOnlyList<FocusedSearchOutcome> Entries { get; }
}
