namespace Allrpcs.Domain.Discovery;

/// <summary>
/// Contains an ordered snapshot of focused-search activities with duplicate-free requests.
/// </summary>
public sealed class FocusedSearchActivitySnapshot
{
    /// <summary>
    /// Creates a focused-search activity snapshot. An empty sequence explicitly means that no
    /// focused searches are active.
    /// </summary>
    public FocusedSearchActivitySnapshot(IEnumerable<FocusedSearchActivity> entries)
    {
        ArgumentNullException.ThrowIfNull(entries);

        List<FocusedSearchActivity> orderedEntries = new();
        HashSet<FocusedSearchRequest> requests = new();

        foreach (FocusedSearchActivity entry in entries)
        {
            if (entry is null)
            {
                throw new ArgumentException(
                    "Focused-search activity entries cannot contain null values.",
                    nameof(entries));
            }

            if (!requests.Add(entry.Request))
            {
                throw new ArgumentException(
                    "Focused-search activity entries cannot contain duplicate requests.",
                    nameof(entries));
            }

            orderedEntries.Add(entry);
        }

        Entries = orderedEntries.AsReadOnly();
    }

    /// <summary>
    /// Gets focused-search activities in their stable supplied order.
    /// </summary>
    public IReadOnlyList<FocusedSearchActivity> Entries { get; }
}
