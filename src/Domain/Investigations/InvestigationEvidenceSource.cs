using Allrpcs.Domain.Identifiers;

namespace Allrpcs.Domain.Investigations;

/// <summary>Associates one evidence identity with its authored source and cooperation boundary.</summary>
public sealed class InvestigationEvidenceSource
{
    /// <summary>Creates one immutable evidence-source association.</summary>
    public InvestigationEvidenceSource(
        InvestigationEvidenceSourceId sourceId,
        EvidenceId evidenceId,
        bool requiresCooperation)
    {
        ArgumentNullException.ThrowIfNull(sourceId);
        ArgumentNullException.ThrowIfNull(evidenceId);

        SourceId = sourceId;
        EvidenceId = evidenceId;
        RequiresCooperation = requiresCooperation;
    }

    /// <summary>Gets the exact canonical source identity.</summary>
    public InvestigationEvidenceSourceId SourceId { get; }

    /// <summary>Gets the exact evidence identity supplied by this source.</summary>
    public EvidenceId EvidenceId { get; }

    /// <summary>Gets whether obtaining this evidence depends on character cooperation.</summary>
    public bool RequiresCooperation { get; }
}
