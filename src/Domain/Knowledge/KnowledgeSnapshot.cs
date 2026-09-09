using Allrpcs.Domain.Identifiers;

namespace Allrpcs.Domain.Knowledge;

/// <summary>Contains authored knowledge entries in stable duplicate-free identifier order.</summary>
public sealed class KnowledgeSnapshot
{
    /// <summary>Creates an immutable authored knowledge snapshot.</summary>
    public KnowledgeSnapshot(IEnumerable<KnowledgeEntry> entries)
    {
        ArgumentNullException.ThrowIfNull(entries);

        List<KnowledgeEntry> retainedEntries = new();
        HashSet<KnowledgeId> identifiers = new();
        foreach (KnowledgeEntry entry in entries)
        {
            if (entry is null)
            {
                throw new ArgumentException(
                    "Knowledge entries cannot contain null values.",
                    nameof(entries));
            }

            if (!identifiers.Add(entry.Id))
            {
                throw new ArgumentException(
                    "Knowledge entries cannot contain duplicate identifiers.",
                    nameof(entries));
            }

            retainedEntries.Add(entry);
        }

        Entries = retainedEntries.AsReadOnly();
    }

    /// <summary>Gets exact entries in stable authored order.</summary>
    public IReadOnlyList<KnowledgeEntry> Entries { get; }
}
