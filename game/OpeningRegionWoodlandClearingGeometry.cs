using Allrpcs.Application.Navigation;
using NumericsVector2 = System.Numerics.Vector2;

namespace Allrpcs.Game;

/// <summary>Names the authored woodland-clearing geometry shared by scene and route owners.</summary>
internal static class OpeningRegionWoodlandClearingGeometry
{
    internal const float OpeningWidthPixels = 96.0f;
    internal const float WallThicknessPixels = 8.0f;
    internal const float ClearingLeft = 600.0f;
    internal const float ClearingTop = 200.0f;
    internal const float ClearingRight = 1248.0f;
    internal const float ClearingBottom = 450.0f;
    internal const float VillageEdgeOpeningX = 1128.0f;
    internal const float ClosedForestPathOpeningX = 744.0f;

    internal static NavigationWorldBounds ClearingBounds => new(
        ClearingLeft,
        ClearingTop,
        ClearingRight,
        ClearingBottom);

    internal static NumericsVector2 VillageEdgeOpening => new(
        VillageEdgeOpeningX,
        ClearingBottom);

    internal static NumericsVector2 ClosedForestPathOpening => new(
        ClosedForestPathOpeningX,
        ClearingTop);

    internal static NumericsVector2 RareSpecimenPosition => new(1050.0f, 350.0f);
}
