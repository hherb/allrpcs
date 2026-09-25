using System.Numerics;
using Allrpcs.Domain.Locations;

namespace Allrpcs.Application.Navigation;

/// <summary>Retains one exact authored opening landmark at one finite scene position.</summary>
public sealed class OpeningRegionAnchor
{
    /// <summary>Creates one immutable scene anchor for an exact authored location.</summary>
    public OpeningRegionAnchor(Location location, Vector2 position)
    {
        ArgumentNullException.ThrowIfNull(location);
        if (!float.IsFinite(position.X) || !float.IsFinite(position.Y))
        {
            throw new ArgumentOutOfRangeException(
                nameof(position),
                position,
                "An opening-region anchor position must be finite.");
        }

        Location = location;
        Position = position;
    }

    /// <summary>Gets the exact authored location represented by this anchor.</summary>
    public Location Location { get; }

    /// <summary>Gets the authored gameplay-scale scene position in pixels.</summary>
    public Vector2 Position { get; }
}
