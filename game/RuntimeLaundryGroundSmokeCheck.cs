using Godot;

namespace Allrpcs.Game;

/// <summary>Checks the approved independent Laundry underlay only when explicitly enabled.</summary>
public partial class RuntimeLaundryGroundSmokeCheck : Node
{
    /// <inheritdoc />
    public override void _Ready()
    {
        SetProcess(OS.GetEnvironment("ALLRPCS_RUNTIME_LAUNDRY_GROUND_CHECK") == "1");
    }

    /// <inheritdoc />
    public override void _Process(double delta)
    {
        _ = delta;
        SetProcess(false);
        Node2D? owner = GetNodeOrNull<Node2D>("../LaundryGroundPresentation");
        Sprite2D? sprite = owner?.GetNodeOrNull<Sprite2D>("PackedEarth");
        if (owner is null || owner.GetScript().VariantType != Variant.Type.Nil
            || owner.Position != new Vector2(1564, 219) || owner.GlobalPosition != owner.Position
            || owner.Scale != Vector2.One || owner.Rotation != 0 || owner.ZIndex != 0
            || !owner.ZAsRelative || owner.Modulate != Colors.White || owner.SelfModulate != Colors.White
            || owner.Material is not null || !owner.Visible || owner.GetChildCount() != 1
            || sprite is null || sprite.GetScript().VariantType != Variant.Type.Nil
            || sprite.Texture?.ResourcePath != "res://art/runtime/terrain/laundry-entrance-ground.png"
            || sprite.Texture.GetWidth() != 92 || sprite.Texture.GetHeight() != 210
            || sprite.Position != Vector2.Zero || sprite.Offset != Vector2.Zero || sprite.Centered
            || sprite.Scale != Vector2.One || sprite.Rotation != 0 || sprite.ZIndex != -78
            || !sprite.ZAsRelative || sprite.RegionEnabled || !sprite.Visible
            || sprite.TextureFilter != CanvasItem.TextureFilterEnum.Linear
            || sprite.TextureRepeat != CanvasItem.TextureRepeatEnum.Disabled
            || sprite.Modulate != Colors.White || sprite.SelfModulate != Colors.White
            || sprite.Material is not null || sprite.GetChildCount() != 0)
        {
            GD.PushError("Laundry ground lost independent exact visual-only ownership or sampling.");
            GetTree().Quit(1);
            return;
        }

        GD.Print("Runtime Laundry ground smoke passed: one exact independent underlay at retained phase/depth.");
        GetTree().Quit();
    }
}
