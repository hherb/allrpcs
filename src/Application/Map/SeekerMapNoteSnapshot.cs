using Allrpcs.Domain.Identifiers;

namespace Allrpcs.Application.Map;

/// <summary>Exposes immutable player-authored notes for one exact Seeker map.</summary>
public sealed class SeekerMapNoteSnapshot
{
    /// <summary>Creates one validated immutable map-note snapshot.</summary>
    internal SeekerMapNoteSnapshot(
        OpeningRegionMapLocationCatalogue catalogue,
        CharacterId seekerId,
        IEnumerable<SeekerMapNote> notes)
    {
        Catalogue = catalogue;
        SeekerId = seekerId;
        Notes = notes.ToList().AsReadOnly();
    }

    /// <summary>Gets the exact hidden catalogue used to validate note transitions.</summary>
    internal OpeningRegionMapLocationCatalogue Catalogue { get; }

    /// <summary>Gets the exact Seeker identifier owning these notes.</summary>
    public CharacterId SeekerId { get; }

    /// <summary>Gets player-authored notes in canonical location order.</summary>
    public IReadOnlyList<SeekerMapNote> Notes { get; }
}
