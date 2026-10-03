using Godot;

namespace Allrpcs.Game;

/// <summary>Owns only the approved channel atlases and retained Stream overlap masking.</summary>
public partial class WholeChannelPresentation : Node2D
{
    private const int TextureWidth = 2604;
    private const int TextureHeight = 620;
    private const string TerrainRoot = "res://art/runtime/terrain/";
    private const string ShaderPath = TerrainRoot + "channel-retained-overlap.gdshader";
    private const string ExpectedShaderCode = "shader_type canvas_item; varying vec2 world_position; "
        + "void vertex() { world_position = (MODEL_MATRIX * vec4(VERTEX, 0.0, 1.0)).xy; } "
        + "void fragment() { if (world_position.x < 1664.0) { discard; } }";
    private static readonly Vector2 SpriteCenter = new(1238, 522);

    /// <summary>Gets or sets the exact approved channel water atlas.</summary>
    [Export] public Texture2D? WaterTexture { get; set; }

    /// <summary>Gets or sets the exact approved bank atlas.</summary>
    [Export] public Texture2D? BanksTexture { get; set; }

    /// <summary>Gets or sets the exact approved rail-free bridge deck atlas.</summary>
    [Export] public Texture2D? DecksTexture { get; set; }

    /// <summary>Gets or sets the exact approved spring detail atlas.</summary>
    [Export] public Texture2D? DetailTexture { get; set; }

    /// <summary>Gets or sets the bounded accepted ground-under-bank fringe correction.</summary>
    [Export] public Texture2D? FringeTexture { get; set; }

    /// <summary>Gets or sets the approved fragment-only overlap shader.</summary>
    [Export] public Shader? OverlapClipShader { get; set; }

    /// <inheritdoc />
    public override void _Ready()
    {
        (string Name, Texture2D? Texture, int Z)[] layers =
        [
            ("water", WaterTexture, -68),
            ("banks", BanksTexture, -67),
            ("decks", DecksTexture, -66),
            ("detail", DetailTexture, -66),
            ("fringe", FringeTexture, -65),
        ];
        if (GetChildCount() != 0 || Position != Vector2.Zero || Scale != Vector2.One
            || Rotation != 0 || ZIndex != 0 || Modulate != Colors.White || SelfModulate != Colors.White)
        {
            throw new InvalidOperationException("The channel owner requires its exact empty world-origin presentation.");
        }

        foreach ((string name, Texture2D? texture, _) in layers)
        {
            if (texture is null || texture.GetWidth() != TextureWidth || texture.GetHeight() != TextureHeight
                || texture.ResourcePath != TerrainRoot + $"channel-{name}.png")
            {
                throw new InvalidOperationException("Channel textures require their exact approved source and geometry.");
            }
        }

        Shader shader = OverlapClipShader
            ?? throw new InvalidOperationException("The approved channel overlap shader is required.");
        if (shader.ResourcePath != ShaderPath || shader.Code.Trim() != ExpectedShaderCode)
        {
            throw new InvalidOperationException("The channel overlap shader differs from the approved fragment mask.");
        }

        Node2D stream = GetNode<Node2D>("../OpeningRegionStreamLaundrySceneSource");
        Polygon2D water = stream.GetNode<Polygon2D>("StreamWater");
        Sprite2D north = stream.GetNode<Sprite2D>("NorthWestBank");
        Sprite2D south = stream.GetNode<Sprite2D>("SouthWestBank");
        ValidateStream(stream, water, north, south);
        ShaderMaterial clip = new() { Shader = shader };
        foreach ((string name, Texture2D? texture, int z) in layers)
        {
            AddChild(new Sprite2D
            {
                Name = name,
                Texture = texture,
                Position = SpriteCenter,
                ZIndex = z,
                TextureRepeat = TextureRepeatEnum.Disabled,
                TextureFilter = TextureFilterEnum.Linear,
            });
        }

        // Original geometry/UV interpolation stays native; suppress only duplicate visual fragments.
        water.Material = clip;
        north.Material = clip;
        south.Material = clip;
    }

    private static void ValidateStream(Node2D stream, Polygon2D water, Sprite2D north, Sprite2D south)
    {
        Vector2[] polygon = [new(1600, 452), new(2000, 452), new(2000, 548), new(1600, 548)];
        Vector2[] uv = [new(0, 0), new(400, 0), new(400, 96), new(0, 96)];
        if (stream.Position != Vector2.Zero || stream.Scale != Vector2.One || stream.Rotation != 0
            || stream.Material is not null || stream.ZIndex != 0
            || !water.Polygon.SequenceEqual(polygon) || !water.UV.SequenceEqual(uv)
            || water.Texture?.ResourcePath != TerrainRoot + "stream-water.png"
            || water.Position != Vector2.Zero || water.Scale != Vector2.One || water.Rotation != 0
            || water.Color != Colors.White || water.Material is not null || water.ZIndex != 0)
        {
            throw new InvalidOperationException("The retained Stream water geometry, phase or ownership differs.");
        }

        ValidateBank(north, new Vector2(1600, 452), false);
        ValidateBank(south, new Vector2(1600, 516), true);
    }

    private static void ValidateBank(Sprite2D bank, Vector2 position, bool flipV)
    {
        if (bank.Texture?.ResourcePath != TerrainRoot + "stream-bank.png" || bank.Position != position
            || bank.Scale != Vector2.One || bank.Rotation != 0 || bank.Offset != Vector2.Zero
            || bank.Centered || !bank.RegionEnabled || bank.RegionRect != new Rect2(60, 0, 68, 32)
            || bank.FlipH || bank.FlipV != flipV || bank.Material is not null || bank.ZIndex != 0
            || bank.Modulate != Colors.White || bank.SelfModulate != Colors.White)
        {
            throw new InvalidOperationException("The retained western bank geometry or source ownership differs.");
        }
    }
}
