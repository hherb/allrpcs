using Allrpcs.Domain.Identifiers;

namespace Allrpcs.Application.Map;

/// <summary>Exposes only one Seeker's known opening-region locations for presentation.</summary>
public sealed class SeekerMapPresentationSnapshot
{
    /// <summary>Creates one immutable safe map-presentation snapshot.</summary>
    internal SeekerMapPresentationSnapshot(
        SeekerMapSnapshot map,
        SeekerMapNoteSnapshot notes,
        IEnumerable<SeekerMapPresentationEntry> locations)
    {
        Map = map;
        Notes = notes;
        SeekerId = map.SeekerId;
        Locations = locations.ToList().AsReadOnly();
    }

    /// <summary>Gets the exact hidden source map used to produce this presentation.</summary>
    internal SeekerMapSnapshot Map { get; }

    /// <summary>Gets the exact hidden source notes used to produce this presentation.</summary>
    internal SeekerMapNoteSnapshot Notes { get; }

    /// <summary>Gets the exact Seeker identifier owning the presented map.</summary>
    public CharacterId SeekerId { get; }

    /// <summary>Gets only known safe locations in canonical opening-region order.</summary>
    public IReadOnlyList<SeekerMapPresentationEntry> Locations { get; }
}
