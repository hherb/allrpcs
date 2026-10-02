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
    private const string ExpectedVillageEdgeTransitionTexturePath =
        "res://art/runtime/terrain/woodland-village-edge-transition.png";
    private const string ExpectedStraightPathTexturePath =
        "res://art/runtime/terrain/straight-path.png";
    private const string ExpectedPathEndCapTexturePath =
        "res://art/runtime/terrain/path-end-cap.png";
    private const string ExpectedOpeningJunctionTexturePath =
        "res://art/runtime/terrain/opening-junction.png";
    private const string ExpectedVillageSquareJunctionTexturePath =
        "res://art/runtime/terrain/village-square-junction.png";
    private const float ExpectedOpeningJunctionRotationDegrees = 133.025066f;
    private const string ExpectedVillageEdgeSquareConnectorTexturePath =
        "res://art/runtime/terrain/village-edge-square-connector.png";
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
        Sprite2D villageEdgeTransition = GetNode<Sprite2D>(
            "../OpeningRegionWoodlandClearingSceneSource/VillageEdgeTransition");
        Polygon2D laundryOpening = GetNode<Polygon2D>(
            "../OpeningRegionWoodlandClearingSceneSource/LaundryOpening");
        Polygon2D shrineOpening = GetNode<Polygon2D>(
            "../OpeningRegionWoodlandClearingSceneSource/ShrineOpening");
        Polygon2D villageOpening = GetNode<Polygon2D>(
            "../OpeningRegionWoodlandClearingSceneSource/VillageEdgeOpening");
        Polygon2D forestOpening = GetNode<Polygon2D>(
            "../OpeningRegionWoodlandClearingSceneSource/ClosedForestPathOpening");
        Line2D regionOutline = GetNode<Line2D>("../RegionOutline");
        Node2D routePresentation = GetNode<Node2D>("../GreyboxNavigation/RoutePresentation");
        Sprite2D straightPath = routePresentation.GetNode<Sprite2D>("LaundryClearingPath");
        Sprite2D pathEndCap = routePresentation.GetNode<Sprite2D>("LaundryClearingPathEndCap");
        Sprite2D openingJunction = routePresentation.GetNode<Sprite2D>("OpeningJunction");
        Sprite2D villageSquareJunction =
            routePresentation.GetNode<Sprite2D>("VillageSquareJunction");
        Sprite2D connector = routePresentation.GetNode<Sprite2D>("VillageEdgeSquareConnector");
        Sprite2D gardenPath = routePresentation.GetNode<Sprite2D>("SquareGardenPath");
        Sprite2D sickhousePath = routePresentation.GetNode<Sprite2D>("SquareSickhousePath");
        Sprite2D southernPath = routePresentation.GetNode<Sprite2D>("SquareSouthernRoadPath");
        Sprite2D wagonPath = routePresentation.GetNode<Sprite2D>("SouthernRoadWagonPath");
        Line2D roadFill = GetNode<Line2D>("../OpeningRegionSouthernRoadWagonSceneSource/RoadArea");
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

        if (!HasExactTexture(
                villageEdgeTransition,
                ExpectedVillageEdgeTransitionTexturePath,
                1024,
                512)
            || villageEdgeTransition.RegionEnabled
            || villageEdgeTransition.Position != new Vector2(536.0f, 136.0f)
            || villageEdgeTransition.Centered
            || villageEdgeTransition.Offset != Vector2.Zero
            || !Mathf.IsZeroApprox(villageEdgeTransition.Rotation)
            || villageEdgeTransition.Scale != Vector2.One
            || villageOpening.Visible
            || !laundryOpening.Visible
            || !shrineOpening.Visible
            || !forestOpening.Visible)
        {
            Fail("Runtime Village Edge transition lost its exact full-clearing feather presentation.");
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
            || !HasExactTexture(openingJunction, ExpectedOpeningJunctionTexturePath, 512, 512)
            || openingJunction.RegionEnabled
            || openingJunction.Position != ExpectedStraightPathStart
            || !Mathf.IsEqualApprox(
                openingJunction.RotationDegrees,
                ExpectedOpeningJunctionRotationDegrees)
            || !openingJunction.Centered
            || openingJunction.Offset != new Vector2(32.0f, 32.0f)
            || openingJunction.Scale != Vector2.One
            || !HasExactTexture(
                villageSquareJunction,
                ExpectedVillageSquareJunctionTexturePath,
                512,
                512)
            || villageSquareJunction.RegionEnabled
            || villageSquareJunction.Position != new Vector2(2200.0f, 900.0f)
            || !Mathf.IsZeroApprox(villageSquareJunction.Rotation)
            || !villageSquareJunction.Centered
            || villageSquareJunction.Offset != Vector2.Zero
            || villageSquareJunction.Scale != Vector2.One
            || !HasExactTexture(connector, ExpectedVillageEdgeSquareConnectorTexturePath, 124, 96)
            || connector.Position != new Vector2(1882.0f, 900.0f)
            || connector.Scale != Vector2.One
            || !Mathf.IsZeroApprox(connector.Rotation)
            || !connector.Centered
            || connector.Offset != Vector2.Zero
            || connector.RegionEnabled
            || connector.GetChildCount() != 0
            || routePresentation.GetChildCount() != 16
            || !HasExactTexture(gardenPath, "res://art/runtime/terrain/square-garden-path.png", 200, 220)
            || gardenPath.Position != new Vector2(2300.0f, 1250.0f)
            || gardenPath.Scale != Vector2.One
            || !Mathf.IsZeroApprox(gardenPath.Rotation)
            || !gardenPath.Centered
            || gardenPath.Offset != Vector2.Zero
            || gardenPath.RegionEnabled
            || gardenPath.GetChildCount() != 0
            || !HasExactTexture(sickhousePath, "res://art/runtime/terrain/square-sickhouse-path.png", 380, 360)
            || sickhousePath.Position != new Vector2(2510.0f, 1320.0f)
            || sickhousePath.Scale != Vector2.One
            || !Mathf.IsZeroApprox(sickhousePath.Rotation)
            || !sickhousePath.Centered
            || sickhousePath.Offset != Vector2.Zero
            || sickhousePath.TextureRepeat != CanvasItem.TextureRepeatEnum.Enabled
            || sickhousePath.RegionEnabled
            || sickhousePath.GetChildCount() != 0
            || !HasExactTexture(southernPath, "res://art/runtime/terrain/square-southern-road-path.png", 520, 416)
            || southernPath.Position != new Vector2(1700.0f, 1232.0f)
            || southernPath.Scale != Vector2.One
            || !Mathf.IsZeroApprox(southernPath.Rotation)
            || !southernPath.Centered
            || southernPath.Offset != Vector2.Zero
            || southernPath.TextureRepeat != CanvasItem.TextureRepeatEnum.Disabled
            || southernPath.RegionEnabled
            || southernPath.GetChildCount() != 0
            || !HasExactTexture(wagonPath, "res://art/runtime/terrain/southern-road-wagon-path.png", 560, 432)
            || wagonPath.Position != new Vector2(1680.0f, 1496.0f)
            || wagonPath.Scale != Vector2.One
            || !Mathf.IsZeroApprox(wagonPath.Rotation)
            || !wagonPath.Centered
            || wagonPath.Offset != Vector2.Zero
            || wagonPath.TextureRepeat != CanvasItem.TextureRepeatEnum.Disabled
            || wagonPath.RegionEnabled
            || wagonPath.GetChildCount() != 0
            || !roadFill.Points.SequenceEqual([new Vector2(572.0f, 460.0f), new Vector2(1500.0f, 1400.0f)])
            || roadFill.Width != 96.0f
            || roadFill.Visible
            || routePresentation.GetChildren().OfType<Line2D>().Any(line =>
                line.Points.SequenceEqual([new Vector2(1500.0f, 1400.0f), new Vector2(1900.0f, 1640.0f)]))
            || routePresentation.GetChildren().Count(child => child is Line2D) != 6
            || routePresentation.GetChildren().Count(child => child is Sprite2D) != 9
            || routePresentation.GetNode<Node2D>("ShrineSouthernRoadPath").GetChildCount() != 2
            || routePresentation.GetChildren().OfType<Line2D>()
                .Any(line => line.Points.Contains(new Vector2(2300.0f, 1300.0f))
                    || line.Points.Contains(new Vector2(2640.0f, 1450.0f))
                    || (line.Points.Length == 2
                        && line.Points[0].IsEqualApprox(new Vector2(1944.0f, 1082.857143f))
                        && line.Points[1] == new Vector2(1500.0f, 1400.0f)))
            || straightPath.GetIndex() >= pathEndCap.GetIndex()
            || pathEndCap.GetIndex() >= openingJunction.GetIndex()
            || openingJunction.GetIndex() >= villageSquareJunction.GetIndex()
            || villageSquareJunction.GetIndex() >= connector.GetIndex()
            || routePresentation.GetChildren()
                .Where(child => child is Line2D)
                .Any(child => ((Line2D)child).ZIndex != -75)
            || !HasExactVillageSquareRouteContinuations(routePresentation))
        {
            Fail("Runtime path art lost its exact bounded opening-region presentation.");
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
            || villageEdgeTransition.ZIndex != -79
            || straightPath.ZIndex != -70
            || pathEndCap.ZIndex != -70
            || openingJunction.ZIndex != -70
            || villageSquareJunction.ZIndex != -70
            || connector.ZIndex != -70
            || gardenPath.ZIndex != -71
            || gardenPath.ZIndex >= villageSquareJunction.ZIndex
            || gardenPath.ZIndex >= GetNode<CanvasItem>(
                "../OpeningRegionRemedyPreparationSceneSource/PreparationTable").ZIndex
            || gardenPath.ZIndex >= GetNode<CanvasItem>(
                "../OpeningRegionRemedyPreparationSceneSource/HerbBedLeft").ZIndex
            || gardenPath.ZIndex >= GetNode<CanvasItem>(
                "../OpeningRegionRemedyPreparationSceneSource/HerbBedRight").ZIndex
            || gardenPath.ZIndex >= GetNode<CanvasItem>(
                "../OpeningRegionRemedyPreparationSceneSource/Label").ZIndex
            || gardenPath.ShowBehindParent
            || sickhousePath.ZIndex != -71
            || sickhousePath.ZIndex >= villageSquareJunction.ZIndex
            || sickhousePath.ZIndex >= GetNode<CanvasItem>(
                "../OpeningRegionFirstPatientSceneSource/Floor").ZIndex
            || sickhousePath.ZIndex >= GetNode<CanvasItem>(
                "../OpeningRegionFirstPatientSceneSource/Entrance").ZIndex
            || sickhousePath.ZIndex >= GetNode<CanvasItem>(
                "../OpeningRegionFirstPatientSceneSource/Patient").ZIndex
            || sickhousePath.ZIndex >= GetNode<CanvasItem>(
                "../OpeningRegionFirstPatientSceneSource/Label").ZIndex
            || sickhousePath.ShowBehindParent
            || wagonPath.ZIndex != -70
            || wagonPath.ShowBehindParent
            || wagonPath.ZIndex >= roadFill.ZIndex
            || wagonPath.ZIndex >= GetNode<CanvasItem>(
                "../OpeningRegionSouthernRoadWagonSceneSource/WagonShell").ZIndex
            || southernPath.ZIndex != -71
            || southernPath.ZIndex >= villageSquareJunction.ZIndex
            || southernPath.ShowBehindParent
            || southernPath.ZIndex >= GetNode<CanvasItem>(
                "../OpeningRegionSouthernRoadWagonSceneSource/RoadArea").ZIndex
            || southernPath.ZIndex >= GetNode<CanvasItem>(
                "../OpeningRegionSouthernRoadWagonSceneSource/RoadLabel").ZIndex
            || southernPath.ZIndex >= regionOutline.ZIndex
            || connector.ZIndex >= GetNode<CanvasItem>(
                "../OpeningRegionVillageTransitionSceneSource/SquareEdgeOpening").ZIndex
            || connector.ZIndex >= GetNode<CanvasItem>(
                "../OpeningRegionVillageTransitionSceneSource/EdgeSquareOpening").ZIndex
            || connector.ZIndex >= GetNode<CanvasItem>(
                "../OpeningRegionVillageTransitionSceneSource/VillageEdgeLabel").ZIndex
            || connector.ZIndex >= GetNode<CanvasItem>(
                "../OpeningRegionVillageTransitionSceneSource/VillageSquareLabel").ZIndex
            || ground.ZIndex >= woodland.ZIndex
            || woodland.ZIndex >= northTransition.ZIndex
            || bottomRightCorner.ZIndex >= villageEdgeTransition.ZIndex
            || villageEdgeTransition.ZIndex >= straightPath.ZIndex
            || bottomRightCorner.ZIndex >= straightPath.ZIndex
            || ground.GetIndex() >= regionOutline.GetIndex()
            || woodland.GetIndex() >= northTransition.GetIndex()
            || northTransition.GetIndex() >= southTransition.GetIndex()
            || southTransition.GetIndex() >= topLeftCorner.GetIndex()
            || topLeftCorner.GetIndex() >= topRightCorner.GetIndex()
            || topRightCorner.GetIndex() >= bottomLeftCorner.GetIndex()
            || bottomLeftCorner.GetIndex() >= bottomRightCorner.GetIndex()
            || bottomRightCorner.GetIndex() >= villageEdgeTransition.GetIndex()
            || villageEdgeTransition.GetIndex() >= laundryOpening.GetIndex()
            || ground.ShowBehindParent
            || woodland.ShowBehindParent
            || northTransition.ShowBehindParent
            || southTransition.ShowBehindParent
            || topLeftCorner.ShowBehindParent
            || topRightCorner.ShowBehindParent
            || bottomLeftCorner.ShowBehindParent
            || bottomRightCorner.ShowBehindParent
            || villageEdgeTransition.ShowBehindParent
            || straightPath.ShowBehindParent
            || pathEndCap.ShowBehindParent
            || openingJunction.ShowBehindParent
            || villageSquareJunction.ShowBehindParent
            || connector.ShowBehindParent)
        {
            Fail("Runtime terrain no longer draws behind the retained greybox presentation.");
            return;
        }

        GD.Print(
            "Runtime terrain ground smoke passed: exact 256x256 quiet moss, leaf litter, straight "
            + "transitions, complementary outer corners, one softly feathered Village Edge "
            + "transition, and one "
            + "straight path with its rounded end cap plus the exact live-angle opening junction "
            + "and tailored Village Square junction with its 124x96 Village Edge connector "
            + "form bounded presentation behind the retained "
            + "greybox.");
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

    private static bool HasExactVillageSquareRouteContinuations(Node2D routePresentation)
    {
        Vector2 square = new(2200.0f, 900.0f);
        Vector2[] destinations =
        [
            new(1700.0f, 900.0f),
        ];
        Line2D[] routes = routePresentation.GetChildren()
            .OfType<Line2D>()
            .Where(line =>
                line.Points.Length == 2
                && destinations.Contains(line.Points[1])
                && Mathf.IsEqualApprox(
                    Mathf.Max(
                        Mathf.Abs(line.Points[0].X - square.X),
                        Mathf.Abs(line.Points[0].Y - square.Y)),
                    380.0f))
            .ToArray();
        if (routes.Length != destinations.Length)
        {
            return false;
        }

        if (!routes.Select(route => route.Points[1]).ToHashSet().SetEquals(destinations))
        {
            return false;
        }

        foreach (Line2D route in routes)
        {
            Vector2 start = route.Points[0];
            Vector2 end = route.Points[1];
            Vector2 expectedDirection = (end - square).Normalized();
            Vector2 retainedDirection = (end - start).Normalized();
            Vector2 offset = start - square;
            const float expectedBoundaryDistance = 380.0f;
            if (route.Width != 96.0f
                || !expectedDirection.IsEqualApprox(retainedDirection)
                || !Mathf.IsEqualApprox(
                    Mathf.Max(Mathf.Abs(offset.X), Mathf.Abs(offset.Y)), expectedBoundaryDistance))
            {
                return false;
            }
        }

        return true;
    }

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
