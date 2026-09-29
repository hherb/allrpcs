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
    private static readonly Rect2 ExpectedWorldRegion = new(0.0f, 0.0f, 3200.0f, 1800.0f);
    private static readonly Rect2 ExpectedWoodlandRegion = new(600.0f, 200.0f, 648.0f, 250.0f);
    private static readonly Rect2 ExpectedNorthTransitionRegion =
        new(0.0f, 0.0f, 256.0f, 648.0f);
    private static readonly Vector2 ExpectedNorthTransitionPosition = new(924.0f, 200.0f);

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
        Polygon2D laundryOpening = GetNode<Polygon2D>(
            "../OpeningRegionWoodlandClearingSceneSource/LaundryOpening");
        Line2D regionOutline = GetNode<Line2D>("../RegionOutline");
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

        Texture2D? northTransitionTexture = northTransition.Texture;
        if (northTransitionTexture is null
            || northTransitionTexture.ResourcePath != ExpectedWoodlandTransitionTexturePath
            || northTransitionTexture.GetWidth() != 256
            || northTransitionTexture.GetHeight() != 256)
        {
            Fail("Runtime woodland north edge lost its exact 256x256 transition identity.");
            return;
        }

        if (!northTransition.RegionEnabled
            || northTransition.RegionRect != ExpectedNorthTransitionRegion
            || northTransition.Position != ExpectedNorthTransitionPosition
            || !northTransition.Centered
            || northTransition.Offset != Vector2.Zero
            || northTransition.TextureRepeat != CanvasItem.TextureRepeatEnum.Enabled
            || !Mathf.IsEqualApprox(northTransition.Rotation, Mathf.Pi / 2.0f)
            || northTransition.Scale != Vector2.One)
        {
            Fail("Runtime woodland transition lost its repeated exact north-edge placement.");
            return;
        }

        if (ground.ZIndex != -100
            || woodland.ZIndex != -90
            || northTransition.ZIndex != -80
            || ground.ZIndex >= woodland.ZIndex
            || woodland.ZIndex >= northTransition.ZIndex
            || ground.GetIndex() >= regionOutline.GetIndex()
            || woodland.GetIndex() >= northTransition.GetIndex()
            || northTransition.GetIndex() >= laundryOpening.GetIndex()
            || ground.ShowBehindParent
            || woodland.ShowBehindParent
            || northTransition.ShowBehindParent)
        {
            Fail("Runtime terrain no longer draws behind the retained greybox presentation.");
            return;
        }

        GD.Print(
            "Runtime terrain ground smoke passed: exact 256x256 quiet moss, leaf litter, and "
            + "north-edge transition repeat across bounded regions behind the retained greybox "
            + "presentation.");
        GetTree().Quit();
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
