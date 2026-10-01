using Godot;

namespace Allrpcs.Game;

/// <summary>Authors navigation and collision for the approved 96-pixel route network.</summary>
public partial class GreyboxNavigation : NavigationRegion2D
{
    private const string StraightPathFirstId = "abandoned-laundry";
    private const string StraightPathSecondId = "woodland-clearing";
    private const string StraightPathNodeName = "LaundryClearingPath";
    private const string PathEndCapNodeName = "LaundryClearingPathEndCap";
    private const string OpeningJunctionNodeName = "OpeningJunction";
    private const string OpeningJunctionAnchorId = "abandoned-laundry";
    private const string VillageSquareJunctionNodeName = "VillageSquareJunction";
    private const string VillageSquareJunctionAnchorId = "village-square";
    private const int StraightPathModuleLengthPixels = 256;
    private const int JunctionSizePixels = 512;
    private const int OpeningJunctionPivotPixels = 224;
    private const float OpeningJunctionRotationDegrees = 133.025066f;
    private static readonly Vector2 OpeningJunctionPivotOffset = new(
        (JunctionSizePixels / 2.0f) - OpeningJunctionPivotPixels,
        (JunctionSizePixels / 2.0f) - OpeningJunctionPivotPixels);
    private const float HalfPathWidthPixels = OpeningRegionLayoutGeometry.PathWidthPixels / 2.0f;
    private static readonly int ColumnCount = (int)(
        OpeningRegionLayoutGeometry.WorldWidthPixels
        / OpeningRegionLayoutGeometry.RouteCellSizePixels);
    private static readonly int RowCount = (int)(
        OpeningRegionLayoutGeometry.WorldHeightPixels
        / OpeningRegionLayoutGeometry.RouteCellSizePixels);

    /// <summary>Gets or sets the one bounded runtime straight-path texture.</summary>
    [Export]
    public Texture2D? StraightPathTexture { get; set; }

    /// <summary>Gets or sets the matching rounded endpoint for the bounded path.</summary>
    [Export]
    public Texture2D? PathEndCapTexture { get; set; }

    /// <summary>Gets or sets the exact live-angle Stream-Laundry-Woodland junction.</summary>
    [Export]
    public Texture2D? OpeningJunctionTexture { get; set; }

    /// <summary>Gets or sets the exact live-angle Village Square junction.</summary>
    [Export]
    public Texture2D? VillageSquareJunctionTexture { get; set; }

    /// <inheritdoc />
    public override void _EnterTree()
    {
        HashSet<Cell> navigationCells = CreateRouteCells(
            HalfPathWidthPixels - OpeningRegionProjectionSource.MinimumPathClearancePixels,
            addAnchorClearance: true);
        HashSet<Cell> physicalRouteCells = CreateRouteCells(
            HalfPathWidthPixels,
            addAnchorClearance: false);
        NavigationPolygon = CreateNavigationPolygon(navigationCells);
        AddCollisionBoundary(physicalRouteCells);
    }

    /// <inheritdoc />
    public override void _Ready()
    {
        AddRoutePresentation();
    }

    private void AddRoutePresentation()
    {
        Node2D owner = new() { Name = "RoutePresentation" };
        AddChild(owner);
        foreach ((string firstId, string secondId) in OpeningRegionLayoutGeometry.Connections)
        {
            Vector2 start = OpeningRegionLayoutGeometry.Anchors[firstId];
            Vector2 end = OpeningRegionLayoutGeometry.Anchors[secondId];
            if (firstId == StraightPathFirstId && secondId == StraightPathSecondId)
            {
                Sprite2D path = CreateStraightPathPresentation(start, end);
                owner.AddChild(path);
                owner.AddChild(CreatePathEndCapPresentation(end, path.Rotation));
                continue;
            }

            if (firstId == VillageSquareJunctionAnchorId)
            {
                start = FindSquareBoundaryExit(start, end, JunctionSizePixels / 2.0f);
            }

            owner.AddChild(new Line2D
            {
                Points = [start, end],
                Width = OpeningRegionLayoutGeometry.PathWidthPixels,
                DefaultColor = new Color(0.447f, 0.392f, 0.302f),
                Antialiased = true,
                ZIndex = -75,
            });
        }

        owner.AddChild(CreateOpeningJunctionPresentation());
        owner.AddChild(CreateVillageSquareJunctionPresentation());
    }

    private Sprite2D CreateStraightPathPresentation(Vector2 start, Vector2 end)
    {
        Texture2D texture = StraightPathTexture
            ?? throw new InvalidOperationException("The bounded straight-path texture is required.");
        if (texture.GetWidth() != OpeningRegionLayoutGeometry.PathWidthPixels
            || texture.GetHeight() != StraightPathModuleLengthPixels)
        {
            throw new InvalidOperationException(
                "The bounded straight-path texture must retain its exact 96x256 geometry.");
        }

        Vector2 delta = end - start;
        return new Sprite2D
        {
            Name = StraightPathNodeName,
            Texture = texture,
            TextureRepeat = TextureRepeatEnum.Enabled,
            RegionEnabled = true,
            RegionRect = new Rect2(0.0f, 0.0f, texture.GetWidth(), delta.Length()),
            Position = start.Lerp(end, 0.5f),
            Rotation = delta.Angle() - (Mathf.Pi / 2.0f),
            ZIndex = -70,
        };
    }

    private Sprite2D CreatePathEndCapPresentation(Vector2 position, float rotation)
    {
        Texture2D texture = PathEndCapTexture
            ?? throw new InvalidOperationException("The bounded path end-cap texture is required.");
        if (texture.GetWidth() != StraightPathModuleLengthPixels
            || texture.GetHeight() != StraightPathModuleLengthPixels)
        {
            throw new InvalidOperationException(
                "The bounded path end-cap texture must retain its exact 256x256 geometry.");
        }

        return new Sprite2D
        {
            Name = PathEndCapNodeName,
            Texture = texture,
            Position = position,
            Rotation = rotation,
            ZIndex = -70,
        };
    }

    private Sprite2D CreateOpeningJunctionPresentation()
    {
        Texture2D texture = OpeningJunctionTexture
            ?? throw new InvalidOperationException("The bounded opening-junction texture is required.");
        if (texture.GetWidth() != JunctionSizePixels
            || texture.GetHeight() != JunctionSizePixels)
        {
            throw new InvalidOperationException(
                "The bounded opening-junction texture must retain its exact 512x512 geometry.");
        }

        return new Sprite2D
        {
            Name = OpeningJunctionNodeName,
            Texture = texture,
            Position = OpeningRegionLayoutGeometry.Anchors[OpeningJunctionAnchorId],
            RotationDegrees = OpeningJunctionRotationDegrees,
            Offset = OpeningJunctionPivotOffset,
            ZIndex = -70,
        };
    }

    private Sprite2D CreateVillageSquareJunctionPresentation()
    {
        Texture2D texture = VillageSquareJunctionTexture
            ?? throw new InvalidOperationException(
                "The bounded Village Square junction texture is required.");
        if (texture.GetWidth() != JunctionSizePixels
            || texture.GetHeight() != JunctionSizePixels)
        {
            throw new InvalidOperationException(
                "The bounded Village Square junction texture must retain its exact 512x512 geometry.");
        }

        return new Sprite2D
        {
            Name = VillageSquareJunctionNodeName,
            Texture = texture,
            Position = OpeningRegionLayoutGeometry.Anchors[VillageSquareJunctionAnchorId],
            ZIndex = -70,
        };
    }

    private static Vector2 FindSquareBoundaryExit(
        Vector2 start,
        Vector2 end,
        float halfSizePixels)
    {
        Vector2 direction = (end - start).Normalized();
        float distance = halfSizePixels
            / Mathf.Max(Mathf.Abs(direction.X), Mathf.Abs(direction.Y));
        return start + (direction * distance);
    }

    private static HashSet<Cell> CreateRouteCells(
        float halfWidthPixels,
        bool addAnchorClearance)
    {
        HashSet<Cell> cells = [];
        foreach ((string firstId, string secondId) in OpeningRegionLayoutGeometry.Connections)
        {
            AddRouteCells(
                cells,
                OpeningRegionLayoutGeometry.Anchors[firstId],
                OpeningRegionLayoutGeometry.Anchors[secondId],
                halfWidthPixels);
        }

        AddVillageEdgeAreaCells(cells, addAnchorClearance);
        AddSickhouseAreaCells(cells, addAnchorClearance);

        if (addAnchorClearance)
        {
            foreach (Vector2 anchor in OpeningRegionLayoutGeometry.Anchors.Values)
            {
                AddAnchorClearanceCells(cells, anchor, halfWidthPixels);
            }
        }

        return cells;
    }

    private static void AddVillageEdgeAreaCells(
        HashSet<Cell> cells,
        bool retainNavigationClearance)
    {
        float clearance = retainNavigationClearance
            ? OpeningRegionProjectionSource.MinimumPathClearancePixels
            : 0.0f;
        AddAreaCells(
            cells,
            OpeningRegionVillageTransitionGeometry.VillageEdgeLeft + clearance,
            OpeningRegionVillageTransitionGeometry.VillageEdgeTop + clearance,
            OpeningRegionVillageTransitionGeometry.VillageEdgeRight - clearance,
            OpeningRegionVillageTransitionGeometry.VillageEdgeBottom - clearance);
    }

    private static void AddSickhouseAreaCells(
        HashSet<Cell> cells,
        bool retainNavigationClearance)
    {
        float clearance = retainNavigationClearance
            ? OpeningRegionProjectionSource.MinimumPathClearancePixels
            : 0.0f;
        Vector2 anchor = OpeningRegionLayoutGeometry.Anchors["sickhouse"];
        AddAreaCells(
            cells,
            anchor.X - OpeningRegionFirstPatientSceneSource.RoomHalfWidthPixels + clearance,
            anchor.Y - OpeningRegionFirstPatientSceneSource.RoomTopOffsetPixels + clearance,
            anchor.X + OpeningRegionFirstPatientSceneSource.RoomHalfWidthPixels - clearance,
            anchor.Y + OpeningRegionFirstPatientSceneSource.RoomBottomOffsetPixels - clearance);
    }

    private static void AddAreaCells(
        HashSet<Cell> cells,
        float left,
        float top,
        float right,
        float bottom)
    {
        float cellSize = OpeningRegionLayoutGeometry.RouteCellSizePixels;
        int firstColumn = Math.Clamp((int)MathF.Floor(left / cellSize), 0, ColumnCount - 1);
        int lastColumn = Math.Clamp((int)MathF.Floor(right / cellSize), 0, ColumnCount - 1);
        int firstRow = Math.Clamp((int)MathF.Floor(top / cellSize), 0, RowCount - 1);
        int lastRow = Math.Clamp((int)MathF.Floor(bottom / cellSize), 0, RowCount - 1);
        for (int row = firstRow; row <= lastRow; row++)
        {
            for (int column = firstColumn; column <= lastColumn; column++)
            {
                Vector2 center = new(
                    (column + 0.5f) * cellSize,
                    (row + 0.5f) * cellSize);
                if (center.X >= left
                    && center.X <= right
                    && center.Y >= top
                    && center.Y <= bottom)
                {
                    cells.Add(new Cell(column, row));
                }
            }
        }
    }

    private static void AddAnchorClearanceCells(
        HashSet<Cell> cells,
        Vector2 anchor,
        float clearancePixels)
    {
        float cellSize = OpeningRegionLayoutGeometry.RouteCellSizePixels;
        float coveringRadius = clearancePixels + (cellSize * MathF.Sqrt(2.0f) / 2.0f);
        int firstColumn = Math.Clamp(
            (int)MathF.Floor((anchor.X - coveringRadius) / cellSize),
            0,
            ColumnCount - 1);
        int lastColumn = Math.Clamp(
            (int)MathF.Floor((anchor.X + coveringRadius) / cellSize),
            0,
            ColumnCount - 1);
        int firstRow = Math.Clamp(
            (int)MathF.Floor((anchor.Y - coveringRadius) / cellSize),
            0,
            RowCount - 1);
        int lastRow = Math.Clamp(
            (int)MathF.Floor((anchor.Y + coveringRadius) / cellSize),
            0,
            RowCount - 1);
        for (int row = firstRow; row <= lastRow; row++)
        {
            for (int column = firstColumn; column <= lastColumn; column++)
            {
                Vector2 center = new(
                    (column + 0.5f) * cellSize,
                    (row + 0.5f) * cellSize);
                if (center.DistanceTo(anchor) <= coveringRadius)
                {
                    cells.Add(new Cell(column, row));
                }
            }
        }
    }

    private static void AddRouteCells(
        HashSet<Cell> cells,
        Vector2 start,
        Vector2 end,
        float halfWidthPixels)
    {
        float cellSize = OpeningRegionLayoutGeometry.RouteCellSizePixels;
        float minimumX = MathF.Max(0.0f, MathF.Min(start.X, end.X) - halfWidthPixels);
        float maximumX = MathF.Min(
            OpeningRegionLayoutGeometry.WorldWidthPixels,
            MathF.Max(start.X, end.X) + halfWidthPixels);
        float minimumY = MathF.Max(0.0f, MathF.Min(start.Y, end.Y) - halfWidthPixels);
        float maximumY = MathF.Min(
            OpeningRegionLayoutGeometry.WorldHeightPixels,
            MathF.Max(start.Y, end.Y) + halfWidthPixels);
        int firstColumn = Math.Clamp((int)MathF.Floor(minimumX / cellSize), 0, ColumnCount - 1);
        int lastColumn = Math.Clamp((int)MathF.Floor(maximumX / cellSize), 0, ColumnCount - 1);
        int firstRow = Math.Clamp((int)MathF.Floor(minimumY / cellSize), 0, RowCount - 1);
        int lastRow = Math.Clamp((int)MathF.Floor(maximumY / cellSize), 0, RowCount - 1);
        for (int row = firstRow; row <= lastRow; row++)
        {
            for (int column = firstColumn; column <= lastColumn; column++)
            {
                Vector2 center = new(
                    (column + 0.5f) * cellSize,
                    (row + 0.5f) * cellSize);
                if (DistanceToSegment(center, start, end) <= halfWidthPixels)
                {
                    cells.Add(new Cell(column, row));
                }
            }
        }
    }

    private static float DistanceToSegment(Vector2 point, Vector2 start, Vector2 end)
    {
        Vector2 segment = end - start;
        float lengthSquared = segment.LengthSquared();
        float progress = lengthSquared == 0.0f
            ? 0.0f
            : Math.Clamp((point - start).Dot(segment) / lengthSquared, 0.0f, 1.0f);
        return point.DistanceTo(start + (segment * progress));
    }

    private static NavigationPolygon CreateNavigationPolygon(HashSet<Cell> cells)
    {
        float cellSize = OpeningRegionLayoutGeometry.RouteCellSizePixels;
        Dictionary<Vector2, int> vertexIndices = [];
        List<Vector2> vertices = [];
        List<int[]> polygons = [];
        foreach (Cell cell in cells.OrderBy(cell => cell.Row).ThenBy(cell => cell.Column))
        {
            float left = cell.Column * cellSize;
            float top = cell.Row * cellSize;
            Vector2[] corners =
            [
                new(left, top),
                new(left + cellSize, top),
                new(left + cellSize, top + cellSize),
                new(left, top + cellSize),
            ];
            polygons.Add(corners.Select(corner => VertexIndex(
                corner,
                vertexIndices,
                vertices)).ToArray());
        }

        NavigationPolygon polygon = new() { Vertices = [.. vertices] };
        foreach (int[] indices in polygons)
        {
            polygon.AddPolygon(indices);
        }

        return polygon;
    }

    private static int VertexIndex(
        Vector2 vertex,
        Dictionary<Vector2, int> indices,
        List<Vector2> vertices)
    {
        if (indices.TryGetValue(vertex, out int existing))
        {
            return existing;
        }

        int index = vertices.Count;
        vertices.Add(vertex);
        indices.Add(vertex, index);
        return index;
    }

    private void AddCollisionBoundary(HashSet<Cell> cells)
    {
        Node2D owner = new() { Name = "RouteBoundaries" };
        AddChild(owner);
        foreach ((Vector2 start, Vector2 end) in CreateBoundarySegments(cells))
        {
            StaticBody2D body = new();
            body.AddChild(new CollisionShape2D
            {
                Shape = new SegmentShape2D { A = start, B = end },
            });
            owner.AddChild(body);
        }
    }

    private static List<(Vector2 Start, Vector2 End)> CreateBoundarySegments(
        HashSet<Cell> cells)
    {
        float size = OpeningRegionLayoutGeometry.RouteCellSizePixels;
        List<(Vector2 Start, Vector2 End)> segments = [];
        foreach (Cell cell in cells)
        {
            float left = cell.Column * size;
            float top = cell.Row * size;
            if (!cells.Contains(new Cell(cell.Column, cell.Row - 1)))
            {
                segments.Add((new(left, top), new(left + size, top)));
            }

            if (!cells.Contains(new Cell(cell.Column + 1, cell.Row)))
            {
                segments.Add((new(left + size, top), new(left + size, top + size)));
            }

            if (!cells.Contains(new Cell(cell.Column, cell.Row + 1)))
            {
                segments.Add((new(left + size, top + size), new(left, top + size)));
            }

            if (!cells.Contains(new Cell(cell.Column - 1, cell.Row)))
            {
                segments.Add((new(left, top + size), new(left, top)));
            }
        }

        return segments;
    }

    private readonly record struct Cell(int Column, int Row);
}
