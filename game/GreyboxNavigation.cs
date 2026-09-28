using Godot;

namespace Allrpcs.Game;

/// <summary>Authors navigation and collision for the approved 96-pixel route network.</summary>
public partial class GreyboxNavigation : NavigationRegion2D
{
    private const float HalfPathWidthPixels = OpeningRegionLayoutGeometry.PathWidthPixels / 2.0f;
    private static readonly int ColumnCount = (int)(
        OpeningRegionLayoutGeometry.WorldWidthPixels
        / OpeningRegionLayoutGeometry.RouteCellSizePixels);
    private static readonly int RowCount = (int)(
        OpeningRegionLayoutGeometry.WorldHeightPixels
        / OpeningRegionLayoutGeometry.RouteCellSizePixels);

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
        AddRoutePresentation();
        AddCollisionBoundary(physicalRouteCells);
    }

    private void AddRoutePresentation()
    {
        Node2D owner = new() { Name = "RoutePresentation" };
        AddChild(owner);
        foreach ((string firstId, string secondId) in OpeningRegionLayoutGeometry.Connections)
        {
            owner.AddChild(new Line2D
            {
                Points =
                [
                    OpeningRegionLayoutGeometry.Anchors[firstId],
                    OpeningRegionLayoutGeometry.Anchors[secondId],
                ],
                Width = OpeningRegionLayoutGeometry.PathWidthPixels,
                DefaultColor = new Color(0.447f, 0.392f, 0.302f),
                Antialiased = true,
            });
        }
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

        if (addAnchorClearance)
        {
            foreach (Vector2 anchor in OpeningRegionLayoutGeometry.Anchors.Values)
            {
                AddAnchorClearanceCells(cells, anchor, halfWidthPixels);
            }
        }

        return cells;
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
