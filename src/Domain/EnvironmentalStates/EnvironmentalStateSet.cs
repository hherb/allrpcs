namespace Allrpcs.Domain.EnvironmentalStates;

/// <summary>
/// Contains an ordered, duplicate-free set of authored qualitative environmental states.
/// </summary>
public sealed class EnvironmentalStateSet
{
    /// <summary>
    /// Creates an environmental-state set. An empty sequence explicitly means no states are authored.
    /// </summary>
    public EnvironmentalStateSet(IEnumerable<EnvironmentalState> entries)
    {
        ArgumentNullException.ThrowIfNull(entries);

        List<EnvironmentalState> orderedEntries = new();
        HashSet<string> keys = new(StringComparer.Ordinal);

        foreach (EnvironmentalState entry in entries)
        {
            if (entry is null)
            {
                throw new ArgumentException(
                    "Environmental-state entries cannot contain null values.",
                    nameof(entries));
            }

            if (!keys.Add(entry.Key))
            {
                throw new ArgumentException(
                    "Environmental-state entries cannot contain duplicate canonical keys.",
                    nameof(entries));
            }

            orderedEntries.Add(entry);
        }

        Entries = orderedEntries.AsReadOnly();
    }

    /// <summary>
    /// Gets environmental states in their stable authored order.
    /// </summary>
    public IReadOnlyList<EnvironmentalState> Entries { get; }
}
