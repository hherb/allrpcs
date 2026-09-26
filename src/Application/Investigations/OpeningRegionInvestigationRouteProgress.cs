using Allrpcs.Domain.Identifiers;

namespace Allrpcs.Application.Investigations;

/// <summary>Retains explicit ID-only observations for one exact investigation route.</summary>
public sealed class OpeningRegionInvestigationRouteProgress
{
    internal OpeningRegionInvestigationRouteProgress(
        OpeningRegionInvestigationRoute route,
        IReadOnlyList<LocationId> recordedLocationIds,
        IReadOnlyList<EvidenceId> recordedEvidenceIds)
    {
        Route = route;
        RecordedLocationIds = recordedLocationIds;
        RecordedEvidenceIds = recordedEvidenceIds;
    }

    /// <summary>Gets the exact authored route whose progress is retained.</summary>
    public OpeningRegionInvestigationRoute Route { get; }

    /// <summary>Gets explicitly recorded exact route locations in record order.</summary>
    public IReadOnlyList<LocationId> RecordedLocationIds { get; }

    /// <summary>Gets explicitly recorded exact route evidence in record order.</summary>
    public IReadOnlyList<EvidenceId> RecordedEvidenceIds { get; }
}
