using Godot;

namespace Allrpcs.Game;

/// <summary>Reuses approved deck paint at the retained crossing without changing any physical owner.</summary>
public partial class RetainedStreamCrossingPresentation : Node
{
    private const string DeckPath = "res://art/runtime/terrain/channel-decks.png";
    private const float BankApron = 16;
    private static readonly Vector2 SourceCenter = new(1382.1818f, 388);

    /// <inheritdoc />
    public override void _Ready()
    {
        OpeningRegionStreamLaundrySceneSource stream =
            GetNode<OpeningRegionStreamLaundrySceneSource>("../OpeningRegionStreamLaundrySceneSource");
        var bounds = stream.Binding.StreamBounds;
        // Keep the scene primitives and their geometry for their original owners; retire only their paint.
        foreach (string name in new[] { "StreamCrossingArea", "NorthWestCap", "NorthEastCap",
            "SouthWestCap", "SouthEastCap" })
        {
            stream.GetNode<CanvasItem>(name).SelfModulate = new Color(1, 1, 1, 0);
        }
        Vector2 along = new Vector2(700, 550).Normalized() * 64;
        Vector2 across = new Vector2(-550, 700).Normalized() * 44;
        stream.AddChild(new Polygon2D
        {
            Name = "PaintedRetainedCrossing",
            Polygon = [new(bounds.Left, bounds.Top - BankApron),
                new(bounds.Right, bounds.Top - BankApron),
                new(bounds.Right, bounds.Bottom + BankApron),
                new(bounds.Left, bounds.Bottom + BankApron)],
            UV = [SourceCenter - across - along,
                SourceCenter + across - along,
                SourceCenter + across + along,
                SourceCenter - across + along],
            Texture = GD.Load<Texture2D>(DeckPath),
            TextureFilter = CanvasItem.TextureFilterEnum.Linear,
            TextureRepeat = CanvasItem.TextureRepeatEnum.Disabled,
        });
    }
}
