using Allrpcs.Domain.Identifiers;

namespace Allrpcs.Domain.Persistence;

/// <summary>Retains one character body's exact finite world position for persistence.</summary>
public sealed record CharacterPositionSaveState
{
    /// <summary>Creates one finite world position for an exact character.</summary>
    public CharacterPositionSaveState(CharacterId characterId, float x, float y)
    {
        ArgumentNullException.ThrowIfNull(characterId);
        if (!float.IsFinite(x) || !float.IsFinite(y))
        {
            throw new ArgumentException("A saved position must be finite.", nameof(x));
        }

        CharacterId = characterId;
        X = x;
        Y = y;
    }

    /// <summary>Gets the exact character identity of the positioned body.</summary>
    public CharacterId CharacterId { get; }

    /// <summary>Gets the world X coordinate in pixels.</summary>
    public float X { get; }

    /// <summary>Gets the world Y coordinate in pixels.</summary>
    public float Y { get; }
}
