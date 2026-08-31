namespace Allrpcs.Domain.Characters;

/// <summary>
/// Builds immutable authored entry collections with stable ordering and unique keys.
/// </summary>
internal static class AuthoredCharacterEntrySet
{
    internal static IReadOnlyList<TEntry> RequireUniqueOrdered<TEntry>(
        IEnumerable<TEntry> entries,
        Func<TEntry, string> keySelector,
        string parameterName)
        where TEntry : class
    {
        ArgumentNullException.ThrowIfNull(entries, parameterName);

        List<TEntry> orderedEntries = new();
        HashSet<string> keys = new(StringComparer.Ordinal);

        foreach (TEntry entry in entries)
        {
            if (entry is null)
            {
                throw new ArgumentException("Entries cannot contain null values.", parameterName);
            }

            if (!keys.Add(keySelector(entry)))
            {
                throw new ArgumentException(
                    "Entries cannot contain duplicate canonical keys.",
                    parameterName);
            }

            orderedEntries.Add(entry);
        }

        return orderedEntries.AsReadOnly();
    }
}
