namespace Allrpcs.Application.Investigations;

/// <summary>Retains immutable explicit progress for one exact route catalogue.</summary>
public sealed class OpeningRegionInvestigationRouteProgressSnapshot
{
    internal OpeningRegionInvestigationRouteProgressSnapshot(
        OpeningRegionInvestigationRouteCatalogue catalogue,
        IReadOnlyList<OpeningRegionInvestigationRouteProgress> entries)
    {
        Catalogue = catalogue;
        Entries = entries;
    }

    /// <summary>Gets the exact route catalogue used by every progress entry.</summary>
    public OpeningRegionInvestigationRouteCatalogue Catalogue { get; }

    /// <summary>Gets one exact progress entry per route in catalogue order.</summary>
    public IReadOnlyList<OpeningRegionInvestigationRouteProgress> Entries { get; }
}
