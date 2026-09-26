namespace Allrpcs.Application.Map;

/// <summary>Retains one bounded player-authored note for one known map location.</summary>
public sealed class SeekerMapNote
{
    /// <summary>Gets the maximum accepted note length in UTF-16 code units.</summary>
    public const int MaximumTextLength = 1_000;

    /// <summary>Creates one validated player-authored map note.</summary>
    internal SeekerMapNote(MapLocationDefinition location, string text)
    {
        Location = location;
        Text = text;
    }

    /// <summary>Gets the exact canonical map location receiving the note.</summary>
    public MapLocationDefinition Location { get; }

    /// <summary>Gets the exact bounded player-authored text.</summary>
    public string Text { get; }
}
