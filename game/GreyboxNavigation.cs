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

    /// <summary>Gets or sets the accepted Village Edge–Square connector presentation.</summary>
    [Export]
    public Texture2D? VillageEdgeSquareConnectorTexture { get; set; }

    /// <summary>Gets or sets the accepted Square–Garden path presentation.</summary>
    [Export]
    public Texture2D? SquareGardenPathTexture { get; set; }

    /// <summary>Gets or sets the accepted Square–Sickhouse path presentation.</summary>
    [Export]
    public Texture2D? SquareSickhousePathTexture { get; set; }

    /// <summary>Gets or sets the accepted Square–Southern Road path presentation.</summary>
    [Export]
    public Texture2D? SquareSouthernRoadPathTexture { get; set; }

    /// <summary>Gets or sets the accepted Southern Road–Wagon bent-road presentation.</summary>
    [Export]
    public Texture2D? SouthernRoadWagonPathTexture { get; set; }

    /// <summary>Gets or sets the accepted incoming Shrine–Southern Road texture.</summary>
    [Export]
    public Texture2D? ShrineSouthernRoadPathTexture { get; set; }

    /// <summary>Gets or sets the accepted local Shrine–Road endpoint blend.</summary>
    [Export]
    public Texture2D? ShrineSouthernRoadEndpointBlendTexture { get; set; }

    /// <summary>Gets or sets the accepted Woodland–Shrine path presentation.</summary>
    [Export]
    public Texture2D? WoodlandShrinePathTexture { get; set; }

    /// <summary>Gets or sets the accepted Woodland Clearing–Village Edge path presentation.</summary>
    [Export]
    public Texture2D? WoodlandVillageEdgePathTexture { get; set; }

    /// <summary>Gets or sets the accepted Village Edge–Stream path presentation.</summary>
    [Export]
    public Texture2D? VillageEdgeStreamPathTexture { get; set; }

    /// <summary>Gets or sets the accepted Stream–Laundry path presentation.</summary>
    [Export]
    public Texture2D? StreamLaundryPathTexture { get; set; }

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
        Sprite2D? woodlandVillageEdgePath = null;
        Sprite2D? villageEdgeStreamPath = null;
        Sprite2D? streamLaundryPath = null;
        foreach ((string firstId, string secondId) in OpeningRegionLayoutGeometry.Connections)
        {
            Vector2 start = OpeningRegionLayoutGeometry.Anchors[firstId];
            Vector2 end = OpeningRegionLayoutGeometry.Anchors[secondId];
            if (firstId == "stream" && secondId == "abandoned-laundry")
            {
                if (streamLaundryPath is not null)
                {
                    throw new InvalidOperationException("The Stream–Laundry route must occur exactly once.");
                }

                streamLaundryPath = CreateStreamLaundryPathPresentation(start, end);
                continue;
            }

            if (firstId == "village-edge" && secondId == "stream")
            {
                if (villageEdgeStreamPath is not null)
                {
                    throw new InvalidOperationException("The Village Edge–Stream route must occur exactly once.");
                }

                villageEdgeStreamPath = CreateVillageEdgeStreamPathPresentation(start, end);
                continue;
            }

            if (firstId == "village-edge" && secondId == "woodland-clearing")
            {
                if (woodlandVillageEdgePath is not null)
                {
                    throw new InvalidOperationException("The Woodland–Village Edge route must occur exactly once.");
                }

                woodlandVillageEdgePath = CreateWoodlandVillageEdgePathPresentation(start, end);
                continue;
            }

            if (firstId == StraightPathFirstId && secondId == StraightPathSecondId)
            {
                Sprite2D path = CreateStraightPathPresentation(start, end);
                owner.AddChild(path);
                owner.AddChild(CreatePathEndCapPresentation(end, path.Rotation));
                continue;
            }

            if (firstId == "woodland-clearing" && secondId == "old-shrine")
            {
                owner.AddChild(CreateWoodlandShrinePathPresentation(start, end));
                continue;
            }

            if (firstId == "old-shrine" && secondId == "southern-road")
            {
                owner.AddChild(CreateShrineSouthernRoadPathPresentation(start, end));
                continue;
            }

            if (firstId == "southern-road" && secondId == "abandoned-wagon")
            {
                // Replace only the route line; navigation and collisions retain the straight route.
                owner.AddChild(CreateSouthernRoadWagonPathPresentation());
                continue;
            }

            if (firstId == VillageSquareJunctionAnchorId)
            {
                if (secondId == "healer-garden")
                {
                    // Replace only presentation; physical routes retain their original anchors.
                    owner.AddChild(CreateSquareGardenPathPresentation());
                    continue;
                }

                if (secondId == "sickhouse")
                {
                    owner.AddChild(CreateSquareSickhousePathPresentation());
                    continue;
                }

                if (secondId == "southern-road")
                {
                    owner.AddChild(CreateSquareSouthernRoadPathPresentation());
                    continue;
                }

                start = FindSquareBoundaryExit(start, end, JunctionSizePixels / 2.0f);
                if (secondId == "village-edge")
                {
                    // Presentation only: the physical route still uses the original anchors.
                    start = new Vector2(1820.0f, 900.0f);
                }
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
        owner.AddChild(CreateVillageEdgeSquareConnectorPresentation());
        // Preserve the accepted proofs' relative sibling order against other z=-75 lines.
        owner.AddChild(woodlandVillageEdgePath
            ?? throw new InvalidOperationException("The Woodland–Village Edge route is required."));
        owner.AddChild(villageEdgeStreamPath
            ?? throw new InvalidOperationException("The Village Edge–Stream route is required."));
        owner.AddChild(streamLaundryPath
            ?? throw new InvalidOperationException("The Stream–Laundry route is required."));
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

    private Sprite2D CreateVillageEdgeSquareConnectorPresentation()
    {
        Texture2D texture = VillageEdgeSquareConnectorTexture
            ?? throw new InvalidOperationException("The Village Edge–Square connector is required.");
        if (texture.GetWidth() != 124 || texture.GetHeight() != 96)
        {
            throw new InvalidOperationException("The connector must retain its exact 124x96 geometry.");
        }

        return new Sprite2D
        {
            Name = "VillageEdgeSquareConnector",
            Texture = texture,
            Position = new Vector2(1882.0f, 900.0f),
            ZIndex = -70,
        };
    }

    private Sprite2D CreateSquareGardenPathPresentation()
    {
        Texture2D texture = SquareGardenPathTexture
            ?? throw new InvalidOperationException("The Square–Garden path is required.");
        if (texture.GetWidth() != 200 || texture.GetHeight() != 220)
        {
            throw new InvalidOperationException("The path must retain its exact 200x220 geometry.");
        }

        return new Sprite2D
        {
            Name = "SquareGardenPath",
            Texture = texture,
            Position = new Vector2(2300.0f, 1250.0f),
            ZIndex = -71,
        };
    }

    private Sprite2D CreateSquareSickhousePathPresentation()
    {
        Texture2D texture = SquareSickhousePathTexture
            ?? throw new InvalidOperationException("The Square–Sickhouse path is required.");
        if (texture.GetWidth() != 380 || texture.GetHeight() != 360)
        {
            throw new InvalidOperationException("The path must retain its exact 380x360 geometry.");
        }

        return new Sprite2D
        {
            Name = "SquareSickhousePath",
            Texture = texture,
            TextureRepeat = CanvasItem.TextureRepeatEnum.Enabled,
            Position = new Vector2(2510.0f, 1320.0f),
            ZIndex = -71,
        };
    }

    private Sprite2D CreateSquareSouthernRoadPathPresentation()
    {
        Texture2D texture = SquareSouthernRoadPathTexture
            ?? throw new InvalidOperationException("The Square–Southern Road path is required.");
        if (texture.GetWidth() != 520 || texture.GetHeight() != 416)
        {
            throw new InvalidOperationException("The path must retain its exact 520x416 geometry.");
        }

        return new Sprite2D
        {
            Name = "SquareSouthernRoadPath",
            Texture = texture,
            TextureRepeat = CanvasItem.TextureRepeatEnum.Disabled,
            Position = new Vector2(1700.0f, 1232.0f),
            ZIndex = -71,
        };
    }

    private Sprite2D CreateSouthernRoadWagonPathPresentation()
    {
        Texture2D texture = SouthernRoadWagonPathTexture
            ?? throw new InvalidOperationException("The Southern Road–Wagon path is required.");
        if (texture.GetWidth() != 560 || texture.GetHeight() != 432)
        {
            throw new InvalidOperationException("The path must retain its exact 560x432 geometry.");
        }

        return new Sprite2D
        {
            Name = "SouthernRoadWagonPath",
            Texture = texture,
            TextureRepeat = CanvasItem.TextureRepeatEnum.Disabled,
            Position = new Vector2(1680.0f, 1496.0f),
            ZIndex = -70,
        };
    }

    private Node2D CreateShrineSouthernRoadPathPresentation(Vector2 start, Vector2 end)
    {
        if (start != new Vector2(380.0f, 300.0f) || end != new Vector2(1500.0f, 1400.0f)
            || OpeningRegionLayoutGeometry.PathWidthPixels != 96)
        {
            throw new InvalidOperationException("The Shrine path requires its exact retained 96px route.");
        }

        Texture2D incoming = ShrineSouthernRoadPathTexture
            ?? throw new InvalidOperationException("The incoming Shrine path is required.");
        Texture2D blend = ShrineSouthernRoadEndpointBlendTexture
            ?? throw new InvalidOperationException("The Shrine endpoint blend is required.");
        if (incoming.GetWidth() != 1152 || incoming.GetHeight() != 1080
            || blend.GetWidth() != 160 || blend.GetHeight() != 160)
        {
            throw new InvalidOperationException("The Shrine path textures require their exact accepted geometry.");
        }

        Node2D owner = new() { Name = "ShrineSouthernRoadPath" };
        owner.AddChild(new Sprite2D
        {
            Name = "IncomingPath",
            Texture = incoming,
            TextureRepeat = CanvasItem.TextureRepeatEnum.Disabled,
            Position = new Vector2(1008.0f, 940.0f),
            ZIndex = -72,
        });
        owner.AddChild(new Sprite2D
        {
            Name = "EndpointBlend",
            Texture = blend,
            TextureRepeat = CanvasItem.TextureRepeatEnum.Disabled,
            Position = new Vector2(1480.0f, 1360.0f),
            ZIndex = -69,
        });
        return owner;
    }

    private Sprite2D CreateWoodlandShrinePathPresentation(Vector2 start, Vector2 end)
    {
        if (start != new Vector2(1000.0f, 350.0f) || end != new Vector2(380.0f, 300.0f)
            || OpeningRegionLayoutGeometry.PathWidthPixels != 96)
        {
            throw new InvalidOperationException("The Woodland–Shrine path requires its exact retained 96px route.");
        }

        Texture2D texture = WoodlandShrinePathTexture
            ?? throw new InvalidOperationException("The Woodland–Shrine path texture is required.");
        if (texture.GetWidth() != 704 || texture.GetHeight() != 208)
        {
            throw new InvalidOperationException("The Woodland–Shrine path requires its exact 704x208 geometry.");
        }

        return new Sprite2D
        {
            Name = "WoodlandShrinePath",
            Texture = texture,
            TextureRepeat = CanvasItem.TextureRepeatEnum.Disabled,
            Position = new Vector2(704.0f, 328.0f),
            ZIndex = -74,
        };
    }

    private Sprite2D CreateWoodlandVillageEdgePathPresentation(Vector2 start, Vector2 end)
    {
        if (start != new Vector2(1700.0f, 900.0f) || end != new Vector2(1000.0f, 350.0f)
            || OpeningRegionLayoutGeometry.PathWidthPixels != 96)
        {
            throw new InvalidOperationException("The Woodland–Village Edge path requires its retained 96px route.");
        }

        Texture2D texture = WoodlandVillageEdgePathTexture
            ?? throw new InvalidOperationException("The Woodland–Village Edge path texture is required.");
        if (texture.GetWidth() != 816 || texture.GetHeight() != 688)
        {
            throw new InvalidOperationException("The Woodland–Village Edge path requires its exact 816x688 geometry.");
        }

        return new Sprite2D
        {
            Name = "WoodlandVillageEdgePath",
            Texture = texture,
            TextureRepeat = CanvasItem.TextureRepeatEnum.Disabled,
            Position = new Vector2(1352.0f, 632.0f),
            ZIndex = -75,
        };
    }

    private Sprite2D CreateVillageEdgeStreamPathPresentation(Vector2 start, Vector2 end)
    {
        if (start != new Vector2(1700.0f, 900.0f) || end != new Vector2(1800.0f, 500.0f)
            || OpeningRegionLayoutGeometry.PathWidthPixels != 96)
        {
            throw new InvalidOperationException("The Village Edge–Stream path requires its retained 96px route.");
        }

        Texture2D texture = VillageEdgeStreamPathTexture
            ?? throw new InvalidOperationException("The Village Edge–Stream path texture is required.");
        if (texture.GetWidth() != 224 || texture.GetHeight() != 528)
        {
            throw new InvalidOperationException("The Village Edge–Stream path requires its exact 224x528 geometry.");
        }

        return new Sprite2D
        {
            Name = "VillageEdgeStreamPath",
            Texture = texture,
            TextureRepeat = CanvasItem.TextureRepeatEnum.Disabled,
            Position = new Vector2(1752.0f, 704.0f),
            ZIndex = -75,
        };
    }

    private Sprite2D CreateStreamLaundryPathPresentation(Vector2 start, Vector2 end)
    {
        if (start != new Vector2(1800.0f, 500.0f) || end != new Vector2(1500.0f, 220.0f)
            || OpeningRegionLayoutGeometry.PathWidthPixels != 96)
        {
            throw new InvalidOperationException("The Stream–Laundry path requires its retained 96px route.");
        }

        Texture2D texture = StreamLaundryPathTexture
            ?? throw new InvalidOperationException("The Stream–Laundry path texture is required.");
        if (texture.GetWidth() != 432 || texture.GetHeight() != 416)
        {
            throw new InvalidOperationException("The Stream–Laundry path requires its exact 432x416 geometry.");
        }

        return new Sprite2D
        {
            Name = "StreamLaundryPath",
            Texture = texture,
            TextureRepeat = CanvasItem.TextureRepeatEnum.Disabled,
            Position = new Vector2(1640.0f, 360.0f),
            ZIndex = -75,
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
