namespace Allrpcs.Application.Map;

/// <summary>Retains one freshly restored safe opening-region map and its notes.</summary>
public sealed class MapSaveStateRestoration
{
    internal MapSaveStateRestoration(
        SeekerMapSnapshot map,
        SeekerMapNoteSnapshot notes)
    {
        Map = map;
        Notes = notes;
    }

    /// <summary>Gets the fresh safe Seeker-owned map snapshot.</summary>
    public SeekerMapSnapshot Map { get; }

    /// <summary>Gets the fresh canonical map catalogue used by the restored snapshots.</summary>
    public OpeningRegionMapLocationCatalogue Catalogue => Map.Catalogue;

    /// <summary>Gets the fresh safe player-authored map-note snapshot.</summary>
    public SeekerMapNoteSnapshot Notes { get; }

    /// <summary>Installs this exact preflighted composition into one trusted runtime target.</summary>
    public void InstallInto(IMapSaveStateRestorationTarget target)
    {
        ArgumentNullException.ThrowIfNull(target);

        target.InstallRestored(Map.Catalogue, Map, Notes);
    }
}
