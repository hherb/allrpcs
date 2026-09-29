using Godot;

namespace Allrpcs.Game;

/// <summary>Proves the manifest-bound runtime ground and woodland terrain layers.</summary>
public partial class RuntimeTerrainGroundSmokeCheck : Node
{
    private const string CheckEnvironmentVariable = "ALLRPCS_RUNTIME_TERRAIN_GROUND_CHECK";
    private const string EnabledEnvironmentValue = "1";
    private const string ExpectedGroundTexturePath = "res://art/runtime/terrain/quiet-moss.png";
    private const string ExpectedWoodlandTexturePath =
        "res://art/runtime/terrain/woodland-leaf-litter.png";
    private const string ExpectedWoodlandTransitionTexturePath =
        "res://art/runtime/terrain/woodland-leaf-litter-transition.png";
    private const string ExpectedWoodlandCornerTexturePath =
        "res://art/runtime/terrain/woodland-leaf-litter-corner-transition.png";
    private const string ExpectedWoodlandLeftCornerTexturePath =
        "res://art/runtime/terrain/woodland-leaf-litter-corner-transition-left.png";
    private const string ExpectedStraightPathTexturePath =
        "res://art/runtime/terrain/straight-path.png";
    private const string ExpectedPathEndCapTexturePath =
        "res://art/runtime/terrain/path-end-cap.png";
    private static readonly Rect2 ExpectedWorldRegion = new(0.0f, 0.0f, 3200.0f, 1800.0f);
    private static readonly Rect2 ExpectedWoodlandRegion = new(600.0f, 200.0f, 648.0f, 250.0f);
    private static readonly Rect2 ExpectedStraightTransitionRegion =
        new(0.0f, 0.0f, 256.0f, 392.0f);
    private static readonly Vector2 ExpectedNorthTransitionPosition = new(924.0f, 197.0f);
    private static readonly Vector2 ExpectedSouthTransitionPosition = new(924.0f, 453.0f);
    private static readonly Vector2 ExpectedStraightPathStart = new(1500.0f, 220.0f);
    private static readonly Vector2 ExpectedStraightPathEnd = new(1000.0f, 350.0f);

    /// <inheritdoc />
    public override void _Ready()
    {
        SetProcess(OS.GetEnvironment(CheckEnvironmentVariable) == EnabledEnvironmentValue);
    }

    /// <inheritdoc />
    public override void _Process(double delta)
    {
        _ = delta;
        SetProcess(false);
        RunChecks();
    }

    private void RunChecks()
    {
        Sprite2D ground = GetNode<Sprite2D>("../Ground");
        Polygon2D woodland = GetNode<Polygon2D>(
            "../OpeningRegionWoodlandClearingSceneSource/ClearingArea");
        Sprite2D northTransition = GetNode<Sprite2D>(
            "../OpeningRegionWoodlandClearingSceneSource/NorthTransition");
        Sprite2D southTransition = GetNode<Sprite2D>(
            "../OpeningRegionWoodlandClearingSceneSource/SouthTransition");
        Sprite2D topLeftCorner = GetNode<Sprite2D>(
            "../OpeningRegionWoodlandClearingSceneSource/TopLeftTransitionCorner");
        Sprite2D topRightCorner = GetNode<Sprite2D>(
            "../OpeningRegionWoodlandClearingSceneSource/TopRightTransitionCorner");
        Sprite2D bottomLeftCorner = GetNode<Sprite2D>(
            "../OpeningRegionWoodlandClearingSceneSource/BottomLeftTransitionCorner");
        Sprite2D bottomRightCorner = GetNode<Sprite2D>(
            "../OpeningRegionWoodlandClearingSceneSource/BottomRightTransitionCorner");
        Polygon2D laundryOpening = GetNode<Polygon2D>(
            "../OpeningRegionWoodlandClearingSceneSource/LaundryOpening");
        Line2D regionOutline = GetNode<Line2D>("../RegionOutline");
        Node2D routePresentation = GetNode<Node2D>("../GreyboxNavigation/RoutePresentation");
        Sprite2D straightPath = routePresentation.GetNode<Sprite2D>("LaundryClearingPath");
        Sprite2D pathEndCap = routePresentation.GetNode<Sprite2D>("LaundryClearingPathEndCap");
        Texture2D? texture = ground.Texture;
        if (texture is null
            || texture.ResourcePath != ExpectedGroundTexturePath
            || texture.GetWidth() != 256
            || texture.GetHeight() != 256)
        {
            Fail("Runtime ground lost its exact 256x256 quiet-moss texture identity.");
            return;
        }

        if (!ground.RegionEnabled
            || ground.RegionRect != ExpectedWorldRegion
            || ground.Centered
            || ground.Position != Vector2.Zero
            || ground.Offset != Vector2.Zero
            || ground.TextureRepeat != CanvasItem.TextureRepeatEnum.Enabled)
        {
            Fail("Runtime quiet moss lost repeat-enabled full-world coverage.");
            return;
        }

        Texture2D? woodlandTexture = woodland.Texture;
        if (woodlandTexture is null
            || woodlandTexture.ResourcePath != ExpectedWoodlandTexturePath
            || woodlandTexture.GetWidth() != 256
            || woodlandTexture.GetHeight() != 256)
        {
            Fail("Runtime woodland lost its exact 256x256 leaf-litter texture identity.");
            return;
        }

        if (!IsRectangle(woodland.Polygon, ExpectedWoodlandRegion)
            || woodland.Position != Vector2.Zero
            || woodland.Color != Colors.White
            || woodland.TextureOffset != Vector2.Zero
            || woodland.TextureRotation != 0.0f
            || woodland.TextureScale != Vector2.One
            || woodland.TextureRepeat != CanvasItem.TextureRepeatEnum.Enabled)
        {
            Fail("Runtime leaf litter lost repeat-enabled exact woodland-clearing coverage.");
            return;
        }

        if (!HasExactTexture(northTransition, ExpectedWoodlandTransitionTexturePath)
            || !HasExactTexture(southTransition, ExpectedWoodlandTransitionTexturePath))
        {
            Fail("Runtime woodland straight edges lost their exact 256x256 transition identity.");
            return;
        }

        if (!northTransition.RegionEnabled
            || northTransition.RegionRect != ExpectedStraightTransitionRegion
            || northTransition.Position != ExpectedNorthTransitionPosition
            || !northTransition.Centered
            || northTransition.Offset != Vector2.Zero
            || northTransition.TextureRepeat != CanvasItem.TextureRepeatEnum.Enabled
            || !Mathf.IsEqualApprox(northTransition.Rotation, Mathf.Pi / 2.0f)
            || northTransition.Scale != Vector2.One
            || !southTransition.RegionEnabled
            || southTransition.RegionRect != ExpectedStraightTransitionRegion
            || southTransition.Position != ExpectedSouthTransitionPosition
            || !southTransition.Centered
            || southTransition.Offset != Vector2.Zero
            || southTransition.TextureRepeat != CanvasItem.TextureRepeatEnum.Enabled
            || !Mathf.IsEqualApprox(southTransition.Rotation, -Mathf.Pi / 2.0f)
            || southTransition.Scale != Vector2.One)
        {
            Fail("Runtime woodland straight transitions lost their derived perimeter placement.");
            return;
        }

        if (!HasExactCornerPresentation(
                topLeftCorner,
                ExpectedWoodlandCornerTexturePath,
                new Vector2(600.0f, 197.0f),
                Mathf.Pi / 2.0f)
            || !HasExactCornerPresentation(
                topRightCorner,
                ExpectedWoodlandLeftCornerTexturePath,
                new Vector2(1248.0f, 197.0f),
                -Mathf.Pi / 2.0f)
            || !HasExactCornerPresentation(
                bottomLeftCorner,
                ExpectedWoodlandCornerTexturePath,
                new Vector2(600.0f, 453.0f),
                0.0f)
            || !HasExactCornerPresentation(
                bottomRightCorner,
                ExpectedWoodlandLeftCornerTexturePath,
                new Vector2(1248.0f, 453.0f),
                0.0f))
        {
            Fail("Runtime woodland corners lost their exact rounded-perimeter placement.");
            return;
        }

        Vector2 pathDelta = ExpectedStraightPathEnd - ExpectedStraightPathStart;
        float pathLength = pathDelta.Length();
        if (!HasExactTexture(straightPath, ExpectedStraightPathTexturePath, 96, 256)
            || !straightPath.RegionEnabled
            || straightPath.RegionRect != new Rect2(0.0f, 0.0f, 96.0f, pathLength)
            || straightPath.Position
                != ExpectedStraightPathStart.Lerp(ExpectedStraightPathEnd, 0.5f)
            || !Mathf.IsEqualApprox(
                straightPath.Rotation,
                pathDelta.Angle() - (Mathf.Pi / 2.0f))
            || straightPath.TextureRepeat != CanvasItem.TextureRepeatEnum.Enabled
            || !straightPath.Centered
            || straightPath.Offset != Vector2.Zero
            || straightPath.Scale != Vector2.One
            || !HasExactTexture(pathEndCap, ExpectedPathEndCapTexturePath)
            || pathEndCap.RegionEnabled
            || pathEndCap.Position != ExpectedStraightPathEnd
            || !Mathf.IsEqualApprox(pathEndCap.Rotation, straightPath.Rotation)
            || !pathEndCap.Centered
            || pathEndCap.Offset != Vector2.Zero
            || pathEndCap.Scale != Vector2.One
            || routePresentation.GetChildCount() != 13
            || routePresentation.GetChildren().Count(child => child is Line2D) != 11
            || routePresentation.GetChildren().Count(child => child is Sprite2D) != 2
            || straightPath.GetIndex() >= pathEndCap.GetIndex()
            || routePresentation.GetChildren()
                .Where(child => child is Line2D)
                .Any(child => ((Line2D)child).ZIndex != -75))
        {
            Fail("Runtime straight path lost its exact laundry-clearing presentation.");
            return;
        }

        if (ground.ZIndex != -100
            || woodland.ZIndex != -90
            || northTransition.ZIndex != -80
            || southTransition.ZIndex != -80
            || topLeftCorner.ZIndex != -80
            || topRightCorner.ZIndex != -80
            || bottomLeftCorner.ZIndex != -80
            || bottomRightCorner.ZIndex != -80
            || straightPath.ZIndex != -70
            || pathEndCap.ZIndex != -70
            || ground.ZIndex >= woodland.ZIndex
            || woodland.ZIndex >= northTransition.ZIndex
            || bottomRightCorner.ZIndex >= straightPath.ZIndex
            || ground.GetIndex() >= regionOutline.GetIndex()
            || woodland.GetIndex() >= northTransition.GetIndex()
            || northTransition.GetIndex() >= southTransition.GetIndex()
            || southTransition.GetIndex() >= topLeftCorner.GetIndex()
            || topLeftCorner.GetIndex() >= topRightCorner.GetIndex()
            || topRightCorner.GetIndex() >= bottomLeftCorner.GetIndex()
            || bottomLeftCorner.GetIndex() >= bottomRightCorner.GetIndex()
            || bottomRightCorner.GetIndex() >= laundryOpening.GetIndex()
            || ground.ShowBehindParent
            || woodland.ShowBehindParent
            || northTransition.ShowBehindParent
            || southTransition.ShowBehindParent
            || topLeftCorner.ShowBehindParent
            || topRightCorner.ShowBehindParent
            || bottomLeftCorner.ShowBehindParent
            || bottomRightCorner.ShowBehindParent
            || straightPath.ShowBehindParent
            || pathEndCap.ShowBehindParent)
        {
            Fail("Runtime terrain no longer draws behind the retained greybox presentation.");
            return;
        }

        GD.Print(
            "Runtime terrain ground smoke passed: exact 256x256 quiet moss, leaf litter, straight "
            + "transitions, complementary outer corners, and one straight path with its rounded "
            + "end cap form exact bounded presentation behind the retained greybox.");
        GetTree().Quit();
    }

    private static bool HasExactTexture(
        Sprite2D sprite,
        string expectedPath,
        int expectedWidth = 256,
        int expectedHeight = 256) =>
        sprite.Texture is Texture2D texture
        && texture.ResourcePath == expectedPath
        && texture.GetWidth() == expectedWidth
        && texture.GetHeight() == expectedHeight;

    private static bool HasExactCornerPresentation(
        Sprite2D corner,
        string expectedTexturePath,
        Vector2 expectedPosition,
        float expectedRotation) =>
        HasExactTexture(corner, expectedTexturePath)
        && !corner.RegionEnabled
        && corner.Position == expectedPosition
        && corner.Centered
        && corner.Offset == Vector2.Zero
        && Mathf.IsEqualApprox(corner.Rotation, expectedRotation)
        && corner.Scale == Vector2.One;

    private static bool IsRectangle(Vector2[] polygon, Rect2 bounds) =>
        polygon.Length == 4
        && polygon[0] == bounds.Position
        && polygon[1] == new Vector2(bounds.End.X, bounds.Position.Y)
        && polygon[2] == bounds.End
        && polygon[3] == new Vector2(bounds.Position.X, bounds.End.Y);

    private void Fail(string message)
    {
        GD.PushError(message);
        GetTree().Quit(exitCode: 1);
    }
}
