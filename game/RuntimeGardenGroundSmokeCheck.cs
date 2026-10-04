using Godot;

namespace Allrpcs.Game;

/// <summary>Checks only the approved Garden underlay presentation when explicitly enabled.</summary>
public partial class RuntimeGardenGroundSmokeCheck : Node
{
    private const string CheckEnvironmentVariable = "ALLRPCS_RUNTIME_GARDEN_GROUND_CHECK";
    private static readonly Vector2 ExpectedOrigin = new(2048.0f, 1120.0f);

    /// <inheritdoc />
    public override void _Ready()
    {
        SetProcess(OS.GetEnvironment(CheckEnvironmentVariable) == "1");
    }

    /// <inheritdoc />
    public override void _Process(double delta)
    {
        _ = delta;
        SetProcess(false);
        Node2D? owner = GetNodeOrNull<Node2D>("../GardenGroundPresentation");
        Node2D garden = GetNode<Node2D>("../OpeningRegionRemedyPreparationSceneSource");
        if (owner is null || owner.GetScript().VariantType != Variant.Type.Nil
            || owner.Position != ExpectedOrigin || owner.GlobalPosition != ExpectedOrigin
            || owner.Scale != Vector2.One || !Mathf.IsZeroApprox(owner.Rotation)
            || owner.ZIndex != 0 || !owner.ZAsRelative || owner.GetChildCount() != 2
            || garden.Position != new Vector2(2300.0f, 1300.0f))
        {
            Fail("Garden ground lost independent identity-transform ownership.");
            return;
        }

        foreach ((string name, int depth) in new[] { ("Meadow", -90), ("Soil", -89) })
        {
            Sprite2D? sprite = owner.GetNodeOrNull<Sprite2D>(name);
            string path = $"res://art/runtime/terrain/garden-ground-{name.ToLowerInvariant()}.png";
            if (sprite is null || sprite.Texture?.ResourcePath != path
                || sprite.Texture.GetWidth() != 512 || sprite.Texture.GetHeight() != 384
                || sprite.Position != Vector2.Zero || sprite.Offset != Vector2.Zero || sprite.Centered
                || sprite.Scale != Vector2.One || !Mathf.IsZeroApprox(sprite.Rotation)
                || sprite.ZIndex != depth || !sprite.ZAsRelative || sprite.RegionEnabled
                || sprite.TextureFilter != CanvasItem.TextureFilterEnum.Linear
                || sprite.TextureRepeat != CanvasItem.TextureRepeatEnum.Disabled
                || sprite.Modulate != Colors.White || sprite.SelfModulate != Colors.White
                || sprite.Material is not null || sprite.GetChildCount() != 0 || !sprite.Visible)
            {
                Fail("Garden underlay lost exact geometry, depth, sampling or visual-only state.");
                return;
            }
        }

        GD.Print("Runtime Garden ground smoke passed: two independent exact underlays at retained origin/depth.");
        GetTree().Quit();
    }

    private void Fail(string message)
    {
        GD.PushError(message);
        GetTree().Quit(exitCode: 1);
    }
}
