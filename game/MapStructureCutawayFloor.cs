using Allrpcs.Application.Navigation;
using Godot;

namespace Allrpcs.Game;

/// <summary>Draws open floors and low walls from the authoritative room geometry.</summary>
public partial class MapStructureCutawayFloor : Node2D
{
    private const int MaximumReadinessFrames = 60;
    private int readinessFrames;
    private OpeningRegionSickhouseLayout? sickhouse;
    private OpeningRegionStreamLaundryLayout? laundry;
    private OpeningRegionOldShrineLayout? shrine;
    private (bool Sickhouse, bool Laundry, bool Shrine)? shown;

    /// <summary>Gets or sets the separately authored timber-floor paint.</summary>
    [Export]
    public Texture2D WoodTexture { get; set; } = null!;

    /// <summary>Gets or sets the separately authored stone-floor paint.</summary>
    [Export]
    public Texture2D StoneTexture { get; set; } = null!;

    /// <inheritdoc />
    public override void _Ready()
    {
        ZIndex = -3;
        TextureRepeat = TextureRepeatEnum.Enabled;
        TextureFilter = TextureFilterEnum.Linear;
    }

    /// <inheritdoc />
    public override void _Process(double delta)
    {
        _ = delta;
        sickhouse = GetNode<OpeningRegionFirstPatientSceneSource>("../../OpeningRegionFirstPatientSceneSource").Layout;
        laundry = GetNode<OpeningRegionStreamLaundrySceneSource>("../../OpeningRegionStreamLaundrySceneSource").Layout;
        shrine = GetNode<OpeningRegionOldShrineSceneSource>("../../OpeningRegionOldShrineSceneSource").Layout;
        if (sickhouse is null || laundry is null || shrine is null)
        {
            if (++readinessFrames >= MaximumReadinessFrames)
            {
                GD.PushError("Map structure floors did not receive the existing room layouts.");
                SetProcess(false);
            }
            return;
        }
        MapStructuresPresentation owner = GetParent<MapStructuresPresentation>();
        (bool Sickhouse, bool Laundry, bool Shrine) current =
            (owner.IsCutaway("Sickhouse"), owner.IsCutaway("Laundry"), owner.IsCutaway("Shrine"));
        if (shown != current)
        {
            shown = current;
            QueueRedraw();
        }
    }

    /// <inheritdoc />
    public override void _Draw()
    {
        if (sickhouse is null || laundry is null || shrine is null) { return; }
        if (shown?.Sickhouse == true)
        {
            DrawFloor(sickhouse.RoomBounds, wood: true);
            DrawWalls("../../OpeningRegionFirstPatientSceneSource/Walls");
        }
        if (shown?.Laundry == true)
        {
            DrawFloor(laundry.SceneBinding.AbandonedLaundryBounds, wood: false);
            DrawWalls("../../OpeningRegionStreamLaundrySceneSource/LaundryBoundaries");
        }
        if (shown?.Shrine == true)
        {
            DrawFloor(shrine.SceneBinding.OldShrineBounds, wood: false);
            DrawWalls("../../OpeningRegionOldShrineSceneSource/ShrineBoundaries");
        }
    }

    private void DrawFloor(NavigationWorldBounds bounds, bool wood)
    {
        Rect2 room = new(bounds.Left, bounds.Top, bounds.Right - bounds.Left, bounds.Bottom - bounds.Top);
        Texture2D paint = wood ? WoodTexture : StoneTexture;
        DrawTextureRectRegion(paint, room, new Rect2(Vector2.Zero, room.Size * 3), Colors.White, false, false);
    }

    private void DrawWalls(string path)
    {
        foreach (Node body in GetNode(path).GetChildren())
        {
            CollisionShape2D collision = body.GetNode<CollisionShape2D>("CollisionShape2D");
            if (collision.Shape is SegmentShape2D segment)
            {
                MapStructuresPresentation.DrawWall(this,
                    ToLocal(collision.ToGlobal(segment.A)), ToLocal(collision.ToGlobal(segment.B)));
            }
            else if (collision.Shape is RectangleShape2D rectangle)
            {
                Vector2 center = ToLocal(collision.GlobalPosition);
                Vector2 half = rectangle.Size / 2;
                Vector2 axis = rectangle.Size.X >= rectangle.Size.Y ? new(half.X, 0) : new(0, half.Y);
                MapStructuresPresentation.DrawWall(this, center - axis, center + axis);
            }
        }
    }
}
