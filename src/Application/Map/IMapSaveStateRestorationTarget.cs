namespace Allrpcs.Application.Map;

/// <summary>Receives one preflighted exact map-restoration composition atomically.</summary>
public interface IMapSaveStateRestorationTarget
{
    /// <summary>Installs the exact shared catalogue, safe map, and note snapshots.</summary>
    void InstallRestored(
        OpeningRegionMapLocationCatalogue catalogue,
        SeekerMapSnapshot map,
        SeekerMapNoteSnapshot notes);
}
