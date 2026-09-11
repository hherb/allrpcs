using Allrpcs.Domain.Identifiers;

namespace Allrpcs.Domain.Investigations;

/// <summary>Contains bounded authored hypothesis-evidence links in stable supplied order.</summary>
public sealed class InvestigationEvidenceLinkSnapshot
{
    /// <summary>Gets the maximum number of retained evidence links.</summary>
    public const int MaximumLinkCount = 256;

    /// <summary>Creates an immutable copy of supplied evidence links.</summary>
    public InvestigationEvidenceLinkSnapshot(IEnumerable<InvestigationEvidenceLink> links)
    {
        ArgumentNullException.ThrowIfNull(links);

        List<InvestigationEvidenceLink> retainedLinks = [];
        HashSet<(InvestigationHypothesisId HypothesisId, EvidenceId EvidenceId)> identities = [];
        foreach (InvestigationEvidenceLink? link in links)
        {
            if (link is null)
            {
                throw new ArgumentException("Evidence links cannot contain null.", nameof(links));
            }

            if (!identities.Add((link.Hypothesis.HypothesisId, link.EvidenceId)))
            {
                throw new ArgumentException(
                    "A hypothesis and evidence identity can have only one relationship.",
                    nameof(links));
            }

            retainedLinks.Add(link);
            if (retainedLinks.Count > MaximumLinkCount)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(links),
                    $"A snapshot accepts at most {MaximumLinkCount} evidence links.");
            }
        }

        Links = retainedLinks.AsReadOnly();
    }

    /// <summary>Gets exact evidence links in stable supplied order.</summary>
    public IReadOnlyList<InvestigationEvidenceLink> Links { get; }
}
