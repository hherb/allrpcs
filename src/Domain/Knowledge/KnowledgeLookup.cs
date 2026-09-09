using Allrpcs.Domain.Identifiers;

namespace Allrpcs.Domain.Knowledge;

/// <summary>Selects authored knowledge by identifier without changing its snapshot.</summary>
public static class KnowledgeLookup
{
    /// <summary>Returns the exact entry for a value-equal identifier, or null when absent.</summary>
    public static KnowledgeEntry? FindById(KnowledgeId id, KnowledgeSnapshot snapshot)
    {
        ArgumentNullException.ThrowIfNull(id);
        ArgumentNullException.ThrowIfNull(snapshot);

        return snapshot.Entries.FirstOrDefault(entry => entry.Id == id);
    }
}
