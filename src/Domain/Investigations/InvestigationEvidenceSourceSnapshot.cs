using Allrpcs.Domain.Identifiers;

namespace Allrpcs.Domain.Investigations;

/// <summary>Contains bounded evidence-source associations in stable authored order.</summary>
public sealed class InvestigationEvidenceSourceSnapshot
{
    /// <summary>Gets the maximum number of retained evidence-source associations.</summary>
    public const int MaximumSourceCount = 256;

    /// <summary>Creates an immutable copy with one source association per evidence identity.</summary>
    public InvestigationEvidenceSourceSnapshot(IEnumerable<InvestigationEvidenceSource> sources)
    {
        ArgumentNullException.ThrowIfNull(sources);

        List<InvestigationEvidenceSource> retainedSources = [];
        HashSet<EvidenceId> evidenceIds = [];
        foreach (InvestigationEvidenceSource? source in sources)
        {
            if (source is null)
            {
                throw new ArgumentException("Evidence sources cannot contain null.", nameof(sources));
            }

            if (!evidenceIds.Add(source.EvidenceId))
            {
                throw new ArgumentException(
                    "An evidence identity can have only one authored source.",
                    nameof(sources));
            }

            retainedSources.Add(source);
            if (retainedSources.Count > MaximumSourceCount)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(sources),
                    $"A snapshot accepts at most {MaximumSourceCount} evidence sources.");
            }
        }

        Sources = retainedSources.AsReadOnly();
    }

    /// <summary>Gets exact evidence-source associations in stable authored order.</summary>
    public IReadOnlyList<InvestigationEvidenceSource> Sources { get; }
}
