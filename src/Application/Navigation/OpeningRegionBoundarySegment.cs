using System.Numerics;

namespace Allrpcs.Application.Navigation;

/// <summary>Defines one finite non-zero collision boundary segment.</summary>
public sealed class OpeningRegionBoundarySegment
{
    /// <summary>Creates one boundary between two distinct finite points.</summary>
    public OpeningRegionBoundarySegment(Vector2 start, Vector2 end)
    {
        if (!IsFinite(start) || !IsFinite(end) || start == end)
        {
            throw new ArgumentException("Boundary endpoints must be finite and distinct.");
        }

        Start = start;
        End = end;
    }

    /// <summary>Gets the first boundary endpoint.</summary>
    public Vector2 Start { get; }

    /// <summary>Gets the second boundary endpoint.</summary>
    public Vector2 End { get; }

    private static bool IsFinite(Vector2 point) =>
        float.IsFinite(point.X) && float.IsFinite(point.Y);
}
