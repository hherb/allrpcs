using Godot;

namespace Allrpcs.Game;

/// <summary>Proves the manifest-bound quiet-moss tile is the complete runtime ground.</summary>
public partial class RuntimeTerrainGroundSmokeCheck : Node
{
    private const string CheckEnvironmentVariable = "ALLRPCS_RUNTIME_TERRAIN_GROUND_CHECK";
    private const string EnabledEnvironmentValue = "1";
    private const string ExpectedTexturePath = "res://art/runtime/terrain/quiet-moss.png";
    private static readonly Rect2 ExpectedWorldRegion = new(0.0f, 0.0f, 3200.0f, 1800.0f);

    /// <inheritdoc />
    public override void _Ready()
    {
        if (OS.GetEnvironment(CheckEnvironmentVariable) != EnabledEnvironmentValue)
        {
            return;
        }

        Sprite2D ground = GetNode<Sprite2D>("../Ground");
        Line2D regionOutline = GetNode<Line2D>("../RegionOutline");
        Texture2D? texture = ground.Texture;
        if (texture is null
            || texture.ResourcePath != ExpectedTexturePath
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

        if (ground.ZIndex != -100
            || ground.GetIndex() >= regionOutline.GetIndex()
            || ground.ShowBehindParent)
        {
            Fail("Runtime quiet moss no longer draws behind the existing greybox presentation.");
            return;
        }

        GD.Print(
            "Runtime terrain ground smoke passed: exact 256x256 quiet moss repeats across "
            + "3200x1800 behind the retained greybox presentation.");
        GetTree().Quit();
    }

    private void Fail(string message)
    {
        GD.PushError(message);
        GetTree().Quit(exitCode: 1);
    }
}
