namespace Allrpcs.Application.Map;

/// <summary>Exposes one known map location with only its safe status and optional player note.</summary>
public sealed class SeekerMapPresentationEntry
{
    /// <summary>Creates one immutable safe map-presentation entry.</summary>
    internal SeekerMapPresentationEntry(
        MapLocationDefinition location,
        bool isVerified,
        string? noteText)
    {
        Location = location;
        IsVerified = isVerified;
        NoteText = noteText;
    }

    /// <summary>Gets the exact known canonical map-location definition.</summary>
    public MapLocationDefinition Location { get; }

    /// <summary>Gets whether the Seeker has verified this location rather than only heard it.</summary>
    public bool IsVerified { get; }

    /// <summary>Gets the optional bounded player-authored note text.</summary>
    public string? NoteText { get; }
}
