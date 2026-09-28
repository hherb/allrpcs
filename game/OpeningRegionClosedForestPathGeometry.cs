using NumericsVector2 = System.Numerics.Vector2;

namespace Allrpcs.Game;

/// <summary>Names the authored closed-forest approach and gate geometry.</summary>
internal static class OpeningRegionClosedForestPathGeometry
{
    internal const float ApproachHalfWidthPixels = 48.0f;
    internal const float BoundaryThicknessPixels = 8.0f;
    internal const float ApproachLengthPixels = 84.0f;

    internal static NumericsVector2 GateCenter => new(
        OpeningRegionWoodlandClearingGeometry.ClosedForestPathOpeningX,
        OpeningRegionWoodlandClearingGeometry.ClearingTop);
}
