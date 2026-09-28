using Allrpcs.Application.Navigation;
using NumericsVector2 = System.Numerics.Vector2;

namespace Allrpcs.Game;

/// <summary>Names the authored village-transition geometry shared by scene and navigation owners.</summary>
internal static class OpeningRegionVillageTransitionGeometry
{
    internal const float OpeningWidthPixels = 96.0f;
    internal const float WallThicknessPixels = 8.0f;
    internal const float VillageEdgeLeft = 1580.0f;
    internal const float VillageEdgeTop = 700.0f;
    internal const float VillageEdgeRight = 1820.0f;
    internal const float VillageEdgeBottom = 1020.0f;
    internal const float VillageEdgeAnchorX = 1700.0f;
    internal const float VillageSquareLeft = 2080.0f;
    internal const float VillageSquareTop = 780.0f;
    internal const float VillageSquareRight = 2320.0f;
    internal const float VillageSquareBottom = 1020.0f;
    internal const float VillageSquareAnchorX = 2200.0f;
    internal const float TransitionAnchorY = 900.0f;

    private const float RouteClearancePixels =
        OpeningRegionProjectionSource.MinimumPathClearancePixels;
    private const float NavigationOpeningHalfWidthPixels =
        (OpeningWidthPixels / 2.0f) - RouteClearancePixels;

    internal const float VillageEdgeClearanceLeft = VillageEdgeLeft - RouteClearancePixels;
    internal const float VillageEdgeInteriorLeft = VillageEdgeLeft + RouteClearancePixels;
    internal const float VillageEdgeNorthOpeningLeft =
        VillageEdgeAnchorX - NavigationOpeningHalfWidthPixels;
    internal const float VillageEdgeNorthOpeningRight =
        VillageEdgeAnchorX + NavigationOpeningHalfWidthPixels;
    internal const float VillageEdgeInteriorRight = VillageEdgeRight - RouteClearancePixels;
    internal const float VillageEdgeClearanceRight = VillageEdgeRight + RouteClearancePixels;
    internal const float VillageSquareClearanceLeft = VillageSquareLeft - RouteClearancePixels;
    internal const float VillageSquareInteriorLeft = VillageSquareLeft + RouteClearancePixels;
    internal const float VillageSquareSouthOpeningLeft =
        VillageSquareAnchorX - NavigationOpeningHalfWidthPixels;
    internal const float VillageSquareSouthOpeningRight =
        VillageSquareAnchorX + NavigationOpeningHalfWidthPixels;
    internal const float VillageSquareInteriorRight = VillageSquareRight - RouteClearancePixels;
    internal const float VillageSquareClearanceRight = VillageSquareRight + RouteClearancePixels;
    internal const float VillageTransitionClearanceTop = VillageEdgeTop - RouteClearancePixels;
    internal const float VillageTransitionInteriorTop = VillageEdgeTop + RouteClearancePixels;
    internal const float VillageTransitionSideOpeningTop =
        TransitionAnchorY - NavigationOpeningHalfWidthPixels;
    internal const float VillageTransitionSideOpeningBottom =
        TransitionAnchorY + NavigationOpeningHalfWidthPixels;
    internal const float VillageTransitionInteriorBottom = VillageEdgeBottom - RouteClearancePixels;
    internal const float VillageTransitionClearanceBottom = VillageEdgeBottom + RouteClearancePixels;

    internal static NavigationWorldBounds VillageSquareBounds => new(
        VillageSquareLeft,
        VillageSquareTop,
        VillageSquareRight,
        VillageSquareBottom);

    internal static NavigationWorldBounds VillageEdgeBounds => new(
        VillageEdgeLeft,
        VillageEdgeTop,
        VillageEdgeRight,
        VillageEdgeBottom);

    internal static NumericsVector2 GardenApproach => new(2300.0f, 1300.0f);
}
