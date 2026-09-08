using Allrpcs.Domain.Identifiers;

namespace Allrpcs.Domain.Discovery;

/// <summary>Contains authored discovery evidence in stable duplicate-free identifier order.</summary>
public sealed class DiscoveryEvidenceSnapshot
{
    /// <summary>Creates an immutable snapshot of authored evidence definitions.</summary>
    public DiscoveryEvidenceSnapshot(IEnumerable<DiscoveryEvidence> entries)
    {
        ArgumentNullException.ThrowIfNull(entries);

        List<DiscoveryEvidence> orderedEntries = new();
        HashSet<EvidenceId> evidenceIds = new();
        foreach (DiscoveryEvidence entry in entries)
        {
            if (entry is null)
            {
                throw new ArgumentException(
                    "Discovery-evidence entries cannot contain null values.",
                    nameof(entries));
            }

            if (!evidenceIds.Add(entry.EvidenceId))
            {
                throw new ArgumentException(
                    "Discovery-evidence entries cannot contain duplicate evidence identifiers.",
                    nameof(entries));
            }

            orderedEntries.Add(entry);
        }

        Entries = orderedEntries.AsReadOnly();
    }

    /// <summary>Gets the exact evidence definitions in stable authored order.</summary>
    public IReadOnlyList<DiscoveryEvidence> Entries { get; }
}
