using Allrpcs.Domain.Identifiers;

namespace Allrpcs.Domain.Investigations;

/// <summary>Links one authored evidence identity to an exact hypothesis without evaluating it.</summary>
public sealed class InvestigationEvidenceLink
{
    /// <summary>Creates one immutable authored hypothesis-evidence link.</summary>
    public InvestigationEvidenceLink(
        InvestigationHypothesis hypothesis,
        EvidenceId evidenceId,
        InvestigationEvidenceRelationship relationship)
    {
        ArgumentNullException.ThrowIfNull(hypothesis);
        ArgumentNullException.ThrowIfNull(evidenceId);
        ArgumentNullException.ThrowIfNull(relationship);

        Hypothesis = hypothesis;
        EvidenceId = evidenceId;
        Relationship = relationship;
    }

    /// <summary>Gets the exact hypothesis to which the evidence is linked.</summary>
    public InvestigationHypothesis Hypothesis { get; }

    /// <summary>Gets the exact authored evidence identity.</summary>
    public EvidenceId EvidenceId { get; }

    /// <summary>Gets the exact authored supporting or contradicting relationship.</summary>
    public InvestigationEvidenceRelationship Relationship { get; }
}
