using Godot;
using static Allrpcs.Game.OpeningRegionVillageTransitionGeometry;

namespace Allrpcs.Game;

/// <summary>
/// Authors the deterministic navigation surface for the temporary greybox region.
/// </summary>
public partial class GreyboxNavigation : NavigationRegion2D
{
    private const float WorldInset = 24.0f;
    private const float WorldRight = 1576.0f;
    private const float WorldBottom = 876.0f;
    private const float BridgeLeft = 776.0f;
    private const float BridgeRight = 840.0f;
    private const float WestStreamCollisionEnd = 752.0f;
    private const float EastStreamCollisionStart = 864.0f;
    private const float LowerFieldTop = 286.0f;
    private const float TreeClearanceLeft = 190.0f;
    private const float TreeClearanceRight = 434.0f;
    private const float TreeClearanceBottom = 466.0f;
    private const float VillageClearanceLeft = 1016.0f;
    private const float VillageClearanceRight = 1480.0f;
    private const float VillageClearanceTop = 480.0f;
    private const float VillageClearanceBottom = 800.0f;
    private const float SickhouseClearanceLeft = 800.0f;
    private const float SickhouseInteriorLeft = 848.0f;
    private const float SickhouseEntranceLeft = 896.0f;
    private const float SickhouseEntranceSubdivisionLeft = 904.0f;
    private const float SickhouseEntranceSubdivisionRight = 936.0f;
    private const float SickhouseEntranceRight = 944.0f;
    private const float SickhouseInteriorRight = 992.0f;
    private const float SickhouseClearanceRight = 1040.0f;
    private const float SickhouseClearanceTop = 656.0f;
    private const float SickhouseInteriorTop = 704.0f;
    private const float SickhouseInteriorBottom = 848.0f;
    private static readonly Vector2[] UpperBank =
    [
        new(WorldInset, 128.0f),
        new(320.0f, 158.0f),
        new(624.0f, 78.0f),
        new(BridgeLeft, 95.0f),
        new(BridgeRight, 105.0f),
        new(920.0f, 118.0f),
        new(1248.0f, 54.0f),
        new(WorldRight, 84.0f),
    ];

    private static readonly Vector2[] LowerBankWest =
    [
        new(WorldInset, 244.0f),
        new(TreeClearanceLeft, 261.0f),
        new(320.0f, 274.0f),
        new(TreeClearanceRight, 244.0f),
        new(624.0f, 194.0f),
        new(WestStreamCollisionEnd, 211.0f),
    ];

    private static readonly Vector2[] LowerBankEast =
    [
        new(EastStreamCollisionStart, 226.0f),
        new(SickhouseEntranceLeft, 230.57143f),
        new(920.0f, 234.0f),
        new(SickhouseEntranceRight, 229.25f),
        new(SickhouseInteriorRight, 219.75f),
        new(VillageClearanceLeft, 215.0f),
        new(SickhouseClearanceRight, 210.34483f),
        new(1248.0f, 170.0f),
        new(VillageClearanceRight, 191.0f),
        new(WorldRight, 200.0f),
    ];

    /// <inheritdoc />
    public override void _EnterTree()
    {
        NavigationPolygon = CreateNavigationPolygon();
    }

    private static NavigationPolygon CreateNavigationPolygon()
    {
        NavigationPolygonBuilder builder = new();
        AddUpperGround(builder);
        AddBridge(builder);
        AddLowerBank(builder, LowerBankWest);
        AddLowerBank(builder, LowerBankEast);
        AddLowerField(builder);
        AddSickhouseInterior(builder);
        return builder.Build();
    }

    private static void AddUpperGround(NavigationPolygonBuilder builder)
    {
        for (int index = 0; index < UpperBank.Length - 1; index++)
        {
            Vector2 leftBank = UpperBank[index];
            Vector2 rightBank = UpperBank[index + 1];
            if (index == 3)
            {
                Vector2 splitBank = GetBridgeSplitUpperBank();
                builder.AddQuadrilateral(
                    new Vector2(leftBank.X, WorldInset),
                    new Vector2(SickhouseClearanceLeft, WorldInset),
                    splitBank,
                    leftBank);
                builder.AddQuadrilateral(
                    new Vector2(SickhouseClearanceLeft, WorldInset),
                    new Vector2(rightBank.X, WorldInset),
                    rightBank,
                    splitBank);
                continue;
            }

            builder.AddQuadrilateral(
                new Vector2(leftBank.X, WorldInset),
                new Vector2(rightBank.X, WorldInset),
                rightBank,
                leftBank);
        }
    }

    private static void AddBridge(NavigationPolygonBuilder builder)
    {
        Vector2 splitUpperBank = GetBridgeSplitUpperBank();
        builder.AddQuadrilateral(
            UpperBank[3],
            splitUpperBank,
            new Vector2(SickhouseClearanceLeft, LowerFieldTop),
            new Vector2(BridgeLeft, LowerFieldTop));
        builder.AddQuadrilateral(
            splitUpperBank,
            UpperBank[4],
            new Vector2(BridgeRight, LowerFieldTop),
            new Vector2(SickhouseClearanceLeft, LowerFieldTop));
    }

    private static Vector2 GetBridgeSplitUpperBank() => UpperBank[3].Lerp(
        UpperBank[4],
        (SickhouseClearanceLeft - BridgeLeft) / (BridgeRight - BridgeLeft));

    private static void AddLowerBank(NavigationPolygonBuilder builder, Vector2[] bank)
    {
        for (int index = 0; index < bank.Length - 1; index++)
        {
            Vector2 leftBank = bank[index];
            Vector2 rightBank = bank[index + 1];
            builder.AddQuadrilateral(
                leftBank,
                rightBank,
                new Vector2(rightBank.X, LowerFieldTop),
                new Vector2(leftBank.X, LowerFieldTop));
        }
    }

    private static void AddLowerField(NavigationPolygonBuilder builder)
    {
        float[] horizontalEdges =
        [
            WorldInset,
            TreeClearanceLeft,
            320.0f,
            TreeClearanceRight,
            VillageEdgeClearanceLeft,
            VillageEdgeInteriorLeft,
            624.0f,
            VillageEdgeNorthOpeningLeft,
            VillageEdgeNorthOpeningRight,
            VillageEdgeInteriorRight,
            WestStreamCollisionEnd,
            VillageEdgeClearanceRight,
            BridgeLeft,
            SickhouseClearanceLeft,
            VillageSquareClearanceLeft,
            BridgeRight,
            SickhouseInteriorLeft,
            EastStreamCollisionStart,
            VillageSquareSouthOpeningLeft,
            VillageSquareSouthOpeningRight,
            SickhouseEntranceLeft,
            SickhouseEntranceSubdivisionLeft,
            920.0f,
            SickhouseEntranceSubdivisionRight,
            SickhouseEntranceRight,
            VillageSquareClearanceRight,
            SickhouseInteriorRight,
            VillageClearanceLeft,
            SickhouseClearanceRight,
            1248.0f,
            VillageClearanceRight,
            WorldRight,
        ];
        float[] verticalEdges =
        [
            LowerFieldTop,
            VillageTransitionClearanceTop,
            VillageTransitionInteriorTop,
            VillageTransitionSideOpeningTop,
            VillageTransitionSideOpeningBottom,
            VillageTransitionInteriorBottom,
            TreeClearanceBottom,
            VillageClearanceTop,
            VillageTransitionClearanceBottom,
            SickhouseClearanceTop,
            SickhouseInteriorTop,
            VillageClearanceBottom,
            SickhouseInteriorBottom,
            WorldBottom,
        ];

        for (int row = 0; row < verticalEdges.Length - 1; row++)
        {
            for (int column = 0; column < horizontalEdges.Length - 1; column++)
            {
                float left = horizontalEdges[column];
                float top = verticalEdges[row];
                float right = horizontalEdges[column + 1];
                float bottom = verticalEdges[row + 1];
                if (IsObstacleCell(left, top, right, bottom))
                {
                    continue;
                }

                builder.AddRectangle(left, top, right, bottom);
            }
        }
    }

    private static bool IsObstacleCell(float left, float top, float right, float bottom)
    {
        bool isTreeClearance = left >= TreeClearanceLeft
            && right <= TreeClearanceRight
            && top >= LowerFieldTop
            && bottom <= TreeClearanceBottom;
        bool isVillageClearance = left >= VillageClearanceLeft
            && right <= VillageClearanceRight
            && top >= VillageClearanceTop
            && bottom <= VillageClearanceBottom;
        bool isSickhouseClearance = left >= SickhouseClearanceLeft
            && right <= SickhouseClearanceRight
            && top >= SickhouseClearanceTop
            && bottom <= WorldBottom;
        bool isVillageTransitionClearance = IsVillageTransitionClearance(
            left,
            top,
            right,
            bottom);
        return isTreeClearance
            || isVillageClearance
            || isSickhouseClearance
            || isVillageTransitionClearance;
    }

    private static bool IsVillageTransitionClearance(
        float left,
        float top,
        float right,
        float bottom)
    {
        bool isInsideEdgeClearance = IsInside(
            left,
            top,
            right,
            bottom,
            VillageEdgeClearanceLeft,
            VillageTransitionClearanceTop,
            VillageEdgeClearanceRight,
            VillageTransitionClearanceBottom);
        bool isInsideEdgeSurface = IsInside(
                left,
                top,
                right,
                bottom,
                VillageEdgeInteriorLeft,
                VillageTransitionInteriorTop,
                VillageEdgeInteriorRight,
                VillageTransitionInteriorBottom)
            || IsInside(
                left,
                top,
                right,
                bottom,
                VillageEdgeNorthOpeningLeft,
                VillageTransitionClearanceTop,
                VillageEdgeNorthOpeningRight,
                VillageTransitionInteriorTop)
            || IsInside(
                left,
                top,
                right,
                bottom,
                VillageEdgeClearanceLeft,
                VillageTransitionSideOpeningTop,
                VillageEdgeInteriorLeft,
                VillageTransitionSideOpeningBottom)
            || IsInside(
                left,
                top,
                right,
                bottom,
                VillageEdgeInteriorRight,
                VillageTransitionSideOpeningTop,
                VillageEdgeClearanceRight,
                VillageTransitionSideOpeningBottom);
        bool isInsideSquareClearance = IsInside(
            left,
            top,
            right,
            bottom,
            VillageSquareClearanceLeft,
            VillageTransitionClearanceTop,
            VillageSquareClearanceRight,
            VillageTransitionClearanceBottom);
        bool isInsideSquareSurface = IsInside(
                left,
                top,
                right,
                bottom,
                VillageSquareInteriorLeft,
                VillageTransitionInteriorTop,
                VillageSquareInteriorRight,
                VillageTransitionInteriorBottom)
            || IsInside(
                left,
                top,
                right,
                bottom,
                VillageSquareClearanceLeft,
                VillageTransitionSideOpeningTop,
                VillageSquareInteriorLeft,
                VillageTransitionSideOpeningBottom)
            || IsInside(
                left,
                top,
                right,
                bottom,
                VillageSquareSouthOpeningLeft,
                VillageTransitionInteriorBottom,
                VillageSquareSouthOpeningRight,
                VillageTransitionClearanceBottom);
        return (isInsideEdgeClearance && !isInsideEdgeSurface)
            || (isInsideSquareClearance && !isInsideSquareSurface);
    }

    private static bool IsInside(
        float left,
        float top,
        float right,
        float bottom,
        float areaLeft,
        float areaTop,
        float areaRight,
        float areaBottom) =>
        left >= areaLeft
        && right <= areaRight
        && top >= areaTop
        && bottom <= areaBottom;

    private static void AddSickhouseInterior(NavigationPolygonBuilder builder)
    {
        builder.AddRectangle(
            SickhouseInteriorLeft,
            SickhouseInteriorTop,
            SickhouseEntranceLeft,
            SickhouseInteriorBottom);
        builder.AddRectangle(
            SickhouseEntranceLeft,
            SickhouseInteriorTop,
            SickhouseEntranceSubdivisionLeft,
            SickhouseInteriorBottom);
        builder.AddRectangle(
            SickhouseEntranceSubdivisionLeft,
            SickhouseInteriorTop,
            920.0f,
            SickhouseInteriorBottom);
        builder.AddRectangle(
            920.0f,
            SickhouseInteriorTop,
            SickhouseEntranceSubdivisionRight,
            SickhouseInteriorBottom);
        builder.AddRectangle(
            SickhouseEntranceSubdivisionRight,
            SickhouseInteriorTop,
            SickhouseEntranceRight,
            SickhouseInteriorBottom);
        builder.AddRectangle(
            SickhouseEntranceRight,
            SickhouseInteriorTop,
            SickhouseInteriorRight,
            SickhouseInteriorBottom);
        builder.AddRectangle(
            SickhouseEntranceLeft,
            SickhouseClearanceTop,
            SickhouseEntranceSubdivisionLeft,
            SickhouseInteriorTop);
        builder.AddRectangle(
            SickhouseEntranceSubdivisionLeft,
            SickhouseClearanceTop,
            920.0f,
            SickhouseInteriorTop);
        builder.AddRectangle(
            920.0f,
            SickhouseClearanceTop,
            SickhouseEntranceSubdivisionRight,
            SickhouseInteriorTop);
        builder.AddRectangle(
            SickhouseEntranceSubdivisionRight,
            SickhouseClearanceTop,
            SickhouseEntranceRight,
            SickhouseInteriorTop);
    }

    private sealed class NavigationPolygonBuilder
    {
        private readonly Dictionary<Vector2, int> vertexIndices = [];
        private readonly List<Vector2> vertices = [];
        private readonly List<int[]> polygons = [];

        public void AddRectangle(float left, float top, float right, float bottom)
        {
            AddQuadrilateral(
                new Vector2(left, top),
                new Vector2(right, top),
                new Vector2(right, bottom),
                new Vector2(left, bottom));
        }

        public void AddQuadrilateral(
            Vector2 topLeft,
            Vector2 topRight,
            Vector2 bottomRight,
            Vector2 bottomLeft)
        {
            polygons.Add(
            [
                GetVertexIndex(topLeft),
                GetVertexIndex(topRight),
                GetVertexIndex(bottomRight),
                GetVertexIndex(bottomLeft),
            ]);
        }

        public NavigationPolygon Build()
        {
            NavigationPolygon navigationPolygon = new()
            {
                Vertices = [.. vertices],
            };
            foreach (int[] polygon in polygons)
            {
                navigationPolygon.AddPolygon(polygon);
            }

            return navigationPolygon;
        }

        private int GetVertexIndex(Vector2 vertex)
        {
            if (vertexIndices.TryGetValue(vertex, out int index))
            {
                return index;
            }

            int newIndex = vertices.Count;
            vertices.Add(vertex);
            vertexIndices.Add(vertex, newIndex);
            return newIndex;
        }
    }
}
