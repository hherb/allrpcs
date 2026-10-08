using System.Numerics;

namespace Allrpcs.Application.Audio;

/// <summary>The four explicitly authorized presentation materials, without world-material authority.</summary>
public enum FootstepMaterial { Earth, Gravel, Leaves, Timber }

/// <summary>Maps supported projection-local positions through the approved audio registry.</summary>
public static class OpeningRegionFootstepMaterialProjection
{
    /// <summary>Selects one material, or silence for unsupported ground and excluded interiors.</summary>
    public static FootstepMaterial? Select(Vector2 position, bool supported)
    {
        if (!supported || !float.IsFinite(position.X) || !float.IsFinite(position.Y)
            || !Rectangle(position, 0, 0, 3200, 1800)
            || Rectangle(position, 2480, 1310, 2800, 1502)
            || Rectangle(position, 1420, 120, 1580, 360)) { return null; }
        if (Rectangle(position, 1736, 452, 1864, 548)) { return FootstepMaterial.Timber; }
        if (Rectangle(position, 2480, 1214, 2576, 1310)) { return FootstepMaterial.Gravel; }
        if (Capsule(position, new(1000, 350), new(380, 300))
            || Capsule(position, new(1000, 350), new(650, 150))) { return FootstepMaterial.Leaves; }
        return FootstepMaterial.Earth;
    }

    /// <summary>Returns the accepted parent ID, rejecting unknown enum values.</summary>
    public static string? ClipId(FootstepMaterial material) => material switch
    {
        FootstepMaterial.Earth => "W01",
        FootstepMaterial.Gravel => "W02",
        FootstepMaterial.Leaves => "W03",
        FootstepMaterial.Timber => "W04",
        _ => null
    };

    /// <summary>Returns bounded Seeker-only wording without location or perception claims.</summary>
    public static string Caption(FootstepMaterial material) => material switch
    {
        FootstepMaterial.Earth => "Footsteps on earth",
        FootstepMaterial.Gravel => "Footsteps on gravel",
        FootstepMaterial.Leaves => "Footsteps in leaves",
        FootstepMaterial.Timber => "Footsteps on timber",
        _ => string.Empty
    };

    private static bool Rectangle(Vector2 p, float left, float top, float right, float bottom) =>
        p.X >= left && p.X <= right && p.Y >= top && p.Y <= bottom;

    private static bool Capsule(Vector2 p, Vector2 start, Vector2 end)
    {
        Vector2 segment = end - start;
        float t = Math.Clamp(Vector2.Dot(p - start, segment) / segment.LengthSquared(), 0, 1);
        return Vector2.DistanceSquared(p, start + t * segment) <= 24 * 24;
    }
}
