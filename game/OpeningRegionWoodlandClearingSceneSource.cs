using Allrpcs.Application.Navigation;
using Godot;

namespace Allrpcs.Game;

/// <summary>Owns the collision-aware clearing and its inert concealed specimen marker.</summary>
public partial class OpeningRegionWoodlandClearingSceneSource : Node2D
{
    /// <summary>Gets the shared width of all authored clearing openings.</summary>
    public const float OpeningWidthPixels = OpeningRegionWoodlandClearingGeometry.OpeningWidthPixels;
    private const float VillageEdgeTransitionFeatherPixels = 64.0f;

    /// <summary>Gets the thickness of each collision-backed tree boundary.</summary>
    public const float WallThicknessPixels =
        OpeningRegionWoodlandClearingGeometry.WallThicknessPixels;

    /// <summary>Gets the exact clearing binding after scene sources become ready.</summary>
    public OpeningRegionWoodlandClearingSceneBinding Binding { get; private set; } = null!;

    /// <summary>Gets the collision-aware clearing layout after scene sources become ready.</summary>
    public OpeningRegionWoodlandClearingLayout Layout { get; private set; } = null!;

    /// <inheritdoc />
    public override void _Ready()
    {
        OpeningRegionProjection projection =
            GetNode<OpeningRegionProjectionSource>("../OpeningRegionProjectionSource").Projection;
        OpeningRegionAnchor clearing = projection.Anchors.Single(anchor =>
            anchor.Location.Identity.Id.Value == "woodland-clearing");
        Binding = new OpeningRegionWoodlandClearingSceneBinding(
            projection,
            clearing,
            OpeningRegionWoodlandClearingGeometry.ClearingBounds);
        Layout = new OpeningRegionWoodlandClearingLayout(
            Binding,
            OpeningWidthPixels,
            WallThicknessPixels,
            OpeningRegionWoodlandClearingGeometry.VillageEdgeOpening,
            OpeningRegionWoodlandClearingGeometry.ClosedForestPathOpening);
        ConfigurePresentation();
        ConfigureBoundaries();
    }

    private void ConfigurePresentation()
    {
        GetNode<Polygon2D>("ClearingArea").Polygon = Rectangle(
            Binding.WoodlandClearingBounds);
        GetNode<Polygon2D>("LaundryOpening").Polygon = VerticalOpening(
            Layout.LaundryOpening);
        GetNode<Polygon2D>("ShrineOpening").Polygon = VerticalOpening(
            Layout.ShrineOpening);
        GetNode<Polygon2D>("VillageEdgeOpening").Polygon = HorizontalOpening(
            Layout.VillageEdgeOpening);
        GetNode<Polygon2D>("ClosedForestPathOpening").Polygon = HorizontalOpening(
            Layout.ClosedForestPathOpening);
        GetNode<Label>("ClearingLabel").Position = new Vector2(
            Binding.WoodlandClearingBounds.Left + 12.0f,
            Binding.WoodlandClearingBounds.Top + 12.0f);
        ConfigureTransitionPerimeter();
    }

    private void ConfigureTransitionPerimeter()
    {
        Sprite2D northTransition = GetNode<Sprite2D>("NorthTransition");
        Sprite2D southTransition = GetNode<Sprite2D>("SouthTransition");
        Sprite2D topLeftCorner = GetNode<Sprite2D>("TopLeftTransitionCorner");
        Sprite2D topRightCorner = GetNode<Sprite2D>("TopRightTransitionCorner");
        Sprite2D bottomLeftCorner = GetNode<Sprite2D>("BottomLeftTransitionCorner");
        Sprite2D bottomRightCorner = GetNode<Sprite2D>("BottomRightTransitionCorner");
        Sprite2D villageEdgeTransition = GetNode<Sprite2D>("VillageEdgeTransition");
        Texture2D[] perimeterTextures =
        [
            RequiredTexture(northTransition),
            RequiredTexture(southTransition),
            RequiredTexture(topLeftCorner),
            RequiredTexture(topRightCorner),
            RequiredTexture(bottomLeftCorner),
            RequiredTexture(bottomRightCorner),
        ];
        float diameter = perimeterTextures[0].GetWidth();
        if (perimeterTextures.Any(texture =>
            texture.GetWidth() != diameter || texture.GetHeight() != diameter))
        {
            throw new InvalidOperationException(
                "The woodland perimeter requires matching square transition textures.");
        }

        NavigationWorldBounds bounds = Binding.WoodlandClearingBounds;
        float boundaryWidth = bounds.Right - bounds.Left;
        float boundaryHeight = bounds.Bottom - bounds.Top;
        float straightSpan = boundaryWidth - diameter;
        if (straightSpan <= 0.0f || boundaryHeight > diameter)
        {
            throw new InvalidOperationException(
                "The woodland clearing must fit one horizontal rounded transition perimeter.");
        }

        float verticalOverhang = (diameter - boundaryHeight) / 2.0f;
        float top = bounds.Top - verticalOverhang;
        float bottom = bounds.Bottom + verticalOverhang;
        float centerX = bounds.Left + (boundaryWidth / 2.0f);
        ConfigureStraightTransition(
            northTransition,
            new Vector2(centerX, top),
            Mathf.Pi / 2.0f,
            diameter,
            straightSpan);
        ConfigureStraightTransition(
            southTransition,
            new Vector2(centerX, bottom),
            -Mathf.Pi / 2.0f,
            diameter,
            straightSpan);
        ConfigureCorner(topLeftCorner, new Vector2(bounds.Left, top), Mathf.Pi / 2.0f);
        ConfigureCorner(topRightCorner, new Vector2(bounds.Right, top), -Mathf.Pi / 2.0f);
        ConfigureCorner(bottomLeftCorner, new Vector2(bounds.Left, bottom), 0.0f);
        ConfigureCorner(bottomRightCorner, new Vector2(bounds.Right, bottom), 0.0f);
        ConfigureVillageEdgeTransition(villageEdgeTransition, bounds);
    }

    private static Texture2D RequiredTexture(Sprite2D sprite) =>
        sprite.Texture
        ?? throw new InvalidOperationException(
            $"The woodland perimeter texture on {sprite.Name} is required.");

    private static void ConfigureStraightTransition(
        Sprite2D transition,
        Vector2 position,
        float rotation,
        float textureSize,
        float span)
    {
        transition.Position = position;
        transition.Rotation = rotation;
        transition.RegionRect = new Rect2(0.0f, 0.0f, textureSize, span);
    }

    private static void ConfigureCorner(Sprite2D corner, Vector2 position, float rotation)
    {
        corner.Position = position;
        corner.Rotation = rotation;
    }

    private static void ConfigureVillageEdgeTransition(
        Sprite2D transition,
        NavigationWorldBounds clearingBounds)
    {
        Texture2D texture = RequiredTexture(transition);
        if (texture.GetWidth() != 1024 || texture.GetHeight() != 512)
        {
            throw new InvalidOperationException(
                "The Village Edge transition texture must remain exactly 1024x512 pixels.");
        }

        transition.Position = new Vector2(
            clearingBounds.Left - VillageEdgeTransitionFeatherPixels,
            clearingBounds.Top - VillageEdgeTransitionFeatherPixels);
        transition.Centered = false;
        transition.Offset = Vector2.Zero;
        transition.RegionEnabled = false;
        transition.Rotation = 0.0f;
        transition.Scale = Vector2.One;
    }

    private void ConfigureBoundaries()
    {
        float half = Layout.OpeningWidthPixels / 2.0f;
        NavigationWorldBounds bounds = Binding.WoodlandClearingBounds;
        List<OpeningRegionBoundarySegment> segments =
        [
            new(new(bounds.Left, bounds.Top), new(Layout.ClosedForestPathOpening.X - half, bounds.Top)),
            new(new(Layout.ClosedForestPathOpening.X + half, bounds.Top), new(bounds.Right, bounds.Top)),
            new(new(bounds.Left, bounds.Top), new(bounds.Left, Layout.ShrineOpening.Y - half)),
            new(new(bounds.Left, Layout.ShrineOpening.Y + half), new(bounds.Left, bounds.Bottom)),
            new(new(bounds.Right, bounds.Top), new(bounds.Right, Layout.LaundryOpening.Y - half)),
            new(new(bounds.Right, Layout.LaundryOpening.Y + half), new(bounds.Right, bounds.Bottom)),
            new(new(bounds.Left, bounds.Bottom), new(Layout.VillageEdgeOpening.X - half, bounds.Bottom)),
            new(new(Layout.VillageEdgeOpening.X + half, bounds.Bottom), new(bounds.Right, bounds.Bottom)),
        ];
        string[] names =
        [
            "ClearingTopLeft",
            "ClearingTopRight",
            "ClearingLeftTop",
            "ClearingLeftBottom",
            "ClearingRightTop",
            "ClearingRightBottom",
            "ClearingBottomLeft",
            "ClearingBottomRight",
        ];
        Node2D owner = GetNode<Node2D>("TreeBoundaries");
        if (owner.GetChildCount() != segments.Count)
        {
            throw new InvalidOperationException(
                "The authored clearing-boundary count must match the derived layout.");
        }

        for (int index = 0; index < segments.Count; index++)
        {
            StaticBody2D body = owner.GetChild<StaticBody2D>(index);
            body.Name = names[index];
            body.Position = Vector2.Zero;
            body.GetChild<CollisionShape2D>(0).Shape = new SegmentShape2D
            {
                A = ToGodot(segments[index].Start),
                B = ToGodot(segments[index].End),
            };
        }
    }

    private static Vector2 ToGodot(System.Numerics.Vector2 position) =>
        new(position.X, position.Y);

    private static Vector2[] Rectangle(NavigationWorldBounds bounds) =>
    [
        new(bounds.Left, bounds.Top),
        new(bounds.Right, bounds.Top),
        new(bounds.Right, bounds.Bottom),
        new(bounds.Left, bounds.Bottom),
    ];

    private static Vector2[] HorizontalOpening(System.Numerics.Vector2 center)
    {
        float half = OpeningWidthPixels / 2.0f;
        return
        [
            new(center.X - half, center.Y - WallThicknessPixels),
            new(center.X + half, center.Y - WallThicknessPixels),
            new(center.X + half, center.Y + WallThicknessPixels),
            new(center.X - half, center.Y + WallThicknessPixels),
        ];
    }

    private static Vector2[] VerticalOpening(System.Numerics.Vector2 center)
    {
        float half = OpeningWidthPixels / 2.0f;
        return
        [
            new(center.X - WallThicknessPixels, center.Y - half),
            new(center.X + WallThicknessPixels, center.Y - half),
            new(center.X + WallThicknessPixels, center.Y + half),
            new(center.X - WallThicknessPixels, center.Y + half),
        ];
    }
}
