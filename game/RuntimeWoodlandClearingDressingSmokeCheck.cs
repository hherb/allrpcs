using Godot;

namespace Allrpcs.Game;

/// <summary>Checks only the exact grouped presentation when explicitly enabled.</summary>
public partial class RuntimeWoodlandClearingDressingSmokeCheck : Node
{
    private static readonly (string Name, string Texture, Vector2 Origin, Vector2I Size)[] Placements =
    [
        ("shrubs_01", "res://art/runtime/plants/woodland-shrubs-01.png", new Vector2(920, 132), new Vector2I(125, 88)),
        ("fallen_wood_07", "res://art/runtime/objects/woodland-fallen-wood-07.png", new Vector2(1050, 192), new Vector2I(97, 64)),
        ("rocks_08", "res://art/runtime/objects/woodland-rocks-08.png", new Vector2(628, 382), new Vector2I(77, 40)),
        ("environmental_details_01", "res://art/runtime/plants/woodland-environmental-details-01.png", new Vector2(1160, 156), new Vector2I(78, 56)),
        ("environmental_details_05", "res://art/runtime/plants/woodland-environmental-details-05.png", new Vector2(795, 395), new Vector2I(37, 22)),
        ("environmental_details_06", "res://art/runtime/plants/woodland-environmental-details-06.png", new Vector2(836, 170), new Vector2I(40, 24)),
        ("environmental_details_03", "res://art/runtime/plants/woodland-environmental-details-03.png", new Vector2(1140, 80), new Vector2I(58, 48)),
        ("environmental_details_09", "res://art/runtime/plants/woodland-environmental-details-09.png", new Vector2(912, 206), new Vector2I(44, 28)),
        ("shrubs_08", "res://art/runtime/plants/woodland-shrubs-08.png", new Vector2(1054, 114), new Vector2I(115, 80)),
        ("fallen_wood_03", "res://art/runtime/objects/woodland-fallen-wood-03.png", new Vector2(968, 186), new Vector2I(110, 72)),
        ("rocks_03", "res://art/runtime/objects/woodland-rocks-03.png", new Vector2(1128, 94), new Vector2I(98, 72)),
        ("environmental_details_02", "res://art/runtime/plants/woodland-environmental-details-02.png", new Vector2(860, 118), new Vector2I(102, 72)),
        ("environmental_details_07", "res://art/runtime/plants/woodland-environmental-details-07.png", new Vector2(954, 226), new Vector2I(50, 32)),
        ("fallen_wood_09", "res://art/runtime/objects/woodland-fallen-wood-09.png", new Vector2(670, 394), new Vector2I(75, 40)),
        ("environmental_details_08", "res://art/runtime/plants/woodland-environmental-details-08.png", new Vector2(772, 400), new Vector2I(59, 36)),
        ("shrubs_07", "res://art/runtime/plants/woodland-shrubs-07.png", new Vector2(1020, 96), new Vector2I(76, 72)),
    ];

    /// <inheritdoc />
    public override void _Ready()
    {
        SetProcess(OS.GetEnvironment("ALLRPCS_RUNTIME_WOODLAND_CLEARING_DRESSING_CHECK") == "1");
    }

    /// <inheritdoc />
    public override void _Process(double delta)
    {
        _ = delta;
        SetProcess(false);
        Node2D? owner = GetNodeOrNull<Node2D>("../WoodlandClearingDressingPresentation");
        if (owner is null || owner.GetScript().VariantType != Variant.Type.Nil
            || owner.Position != Vector2.Zero || owner.GlobalPosition != Vector2.Zero
            || owner.Scale != Vector2.One || owner.Rotation != 0 || owner.ZIndex != 0
            || !owner.ZAsRelative || owner.Modulate != Colors.White || owner.SelfModulate != Colors.White
            || owner.Material is not null || !owner.Visible || owner.YSortEnabled
            || owner.GetChildCount() != Placements.Length
            || GetParent().GetChild(GetParent().GetChildCount() - 1).Name != "WholeChannelPresentation")
        {
            Fail("Grouped dressing lost its independent, ordered visual-only owner.");
            return;
        }

        for (int index = 0; index < Placements.Length; index++)
        {
            var expected = Placements[index];
            if (owner.GetChild(index) is not Sprite2D sprite || sprite.Name != expected.Name
                || sprite.GetScript().VariantType != Variant.Type.Nil
                || sprite.Texture is not CompressedTexture2D || sprite.Texture.ResourcePath != expected.Texture
                || sprite.Texture.GetWidth() != expected.Size.X || sprite.Texture.GetHeight() != expected.Size.Y
                || sprite.Position != expected.Origin || sprite.GlobalPosition != expected.Origin
                || sprite.Offset != Vector2.Zero || sprite.Centered || sprite.Scale != Vector2.One
                || sprite.Rotation != 0 || sprite.ZIndex != -60 || !sprite.ZAsRelative
                || sprite.RegionEnabled || sprite.FlipH || sprite.FlipV || !sprite.Visible
                || sprite.TextureFilter != CanvasItem.TextureFilterEnum.Linear
                || sprite.TextureRepeat != CanvasItem.TextureRepeatEnum.Disabled
                || sprite.Modulate != Colors.White || sprite.SelfModulate != Colors.White
                || sprite.Material is not null || sprite.GetChildCount() != 0)
            {
                Fail("Grouped dressing placement/import drift: " + expected.Name);
                return;
            }
        }

        GD.Print("Runtime Woodland Clearing dressing smoke passed: sixteen exact imported grouped placements.");
        GetTree().Quit();
    }

    /// <summary>Fail visibly and return a nonzero exit for an enabled contract check.</summary>
    private void Fail(string message)
    {
        GD.PushError(message);
        GetTree().Quit(1);
    }
}
