using Godot;

namespace Allrpcs.Game;

/// <summary>Places approved structure paint over the existing map's physical owners.</summary>
public partial class MapStructuresPresentation : Node2D
{
    private const float CutawayMargin = 72.0f;
    private CharacterBody2D seeker = null!;
    private CanvasItem bed = null!;
    private CanvasItem patient = null!;
    private readonly List<(Sprite2D Sprite, Rect2 Bounds)> shelters = [];

    /// <summary>Whether a shelter is cut away at the supplied world position.</summary>
    public static bool NeedsCutaway(Rect2 bounds, Vector2 position) =>
        bounds.Grow(CutawayMargin).HasPoint(position);

    /// <inheritdoc />
    public override void _Ready()
    {
        seeker = GetNode<CharacterBody2D>("../Seeker");
        bed = GetNode<CanvasItem>("../OpeningRegionFirstPatientSceneSource/Bed");
        patient = GetNode<CanvasItem>("../OpeningRegionFirstPatientSceneSource/Patient");
        shelters.Add((GetNode<Sprite2D>("Sickhouse"), new Rect2(2480, 1310, 320, 192)));
        shelters.Add((GetNode<Sprite2D>("Laundry"), new Rect2(1420, 120, 160, 240)));
        shelters.Add((GetNode<Sprite2D>("Shrine"), new Rect2(240, 180, 380, 280)));
        foreach (string path in new[]
        {
            "OpeningRegionFirstPatientSceneSource/Floor",
            "OpeningRegionFirstPatientSceneSource/Entrance",
            "OpeningRegionStreamLaundrySceneSource/LaundryArea",
            "OpeningRegionStreamLaundrySceneSource/LaundryWoodlandOpening",
            "OpeningRegionStreamLaundrySceneSource/LaundryStreamOpening",
            "OpeningRegionOldShrineSceneSource/ShrineArea",
            "OpeningRegionOldShrineSceneSource/WoodlandClearingOpening",
            "OpeningRegionOldShrineSceneSource/SouthernRoadOpening",
            "OpeningRegionSouthernRoadWagonSceneSource/WagonShell",
            "OpeningRegionSouthernRoadWagonSceneSource/WagonOpening",
            "OpeningRegionVillageTransitionSceneSource/VillageSquareArea",
            "OpeningRegionVillageTransitionSceneSource/VillageEdgeArea",
            "OpeningRegionVillageTransitionSceneSource/EdgeSquareOpening",
            "OpeningRegionVillageTransitionSceneSource/EdgeStreamOpening",
            "OpeningRegionVillageTransitionSceneSource/EdgeWoodlandOpening",
            "OpeningRegionVillageTransitionSceneSource/SquareGardenOpening",
            "OpeningRegionVillageTransitionSceneSource/SquareEdgeOpening",
        })
        {
            GetNode<CanvasItem>("../" + path).Visible = false;
        }

        GetNode<Label>("../OpeningRegionFirstPatientSceneSource/Label").Position = new Vector2(-150, 124);
        GetNode<Label>("../OpeningRegionStreamLaundrySceneSource/LaundryLabel").Position = new Vector2(1390, 375);
        GetNode<Label>("../OpeningRegionOldShrineSceneSource/ShrineLabel").Position = new Vector2(280, 475);
        GetNode<Label>("../OpeningRegionSouthernRoadWagonSceneSource/WagonLabel").Position = new Vector2(1785, 1770);
        GetNode<Label>("../OpeningRegionVillageTransitionSceneSource/VillageSquareLabel").Position = new Vector2(2085, 1030);
        foreach (Line2D line in GetNode("../OpeningRegionSouthernRoadWagonSceneSource")
            .FindChildren("*", "Line2D", recursive: true, owned: false).OfType<Line2D>())
        {
            line.Visible = false;
        }
        UpdateCutaways();
    }

    /// <inheritdoc />
    public override void _Process(double delta)
    {
        _ = delta;
        UpdateCutaways();
    }

    /// <summary>Updates only paint opacity; never moves actors or changes physical ownership.</summary>
    public void UpdateCutaways()
    {
        foreach ((Sprite2D sprite, Rect2 bounds) in shelters)
        {
            Rect2 paint = sprite.GetRect();
            Rect2 paintedWorld = new(sprite.ToGlobal(paint.Position), paint.Size * sprite.GlobalScale);
            bool cutaway = NeedsCutaway(bounds, seeker.GlobalPosition)
                || paintedWorld.Grow(32).HasPoint(seeker.GlobalPosition);
            ((ShaderMaterial)sprite.Material).SetShaderParameter("cutaway", cutaway ? 1.0f : 0.0f);
            if (sprite.Name == "Sickhouse")
            {
                bed.SelfModulate = patient.SelfModulate = new Color(1, 1, 1, cutaway ? 1 : 0);
            }
        }
    }

    /// <summary>Reports whether this site's opaque exterior has been cut away.</summary>
    public bool IsCutaway(string name) =>
        ((ShaderMaterial)GetNode<Sprite2D>(name).Material).GetShaderParameter("cutaway").AsSingle() > 0.5f;

    /// <summary>Draws low cutaway walls exactly on an existing collision segment.</summary>
    internal static void DrawWall(CanvasItem canvas, Vector2 start, Vector2 end)
    {
        canvas.DrawLine(start + new Vector2(0, 3), end + new Vector2(0, 3), new Color("40382b"), 12, true);
        canvas.DrawLine(start, end, new Color("a99a76"), 8, true);
        canvas.DrawLine(start + new Vector2(0, -2), end + new Vector2(0, -2), new Color("d5c5a1"), 2, true);
    }
}
