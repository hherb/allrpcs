using Allrpcs.Application.Navigation;
using Godot;

namespace Allrpcs.Game;

/// <summary>Places approved structure paint over the existing map's physical owners.</summary>
public partial class MapStructuresPresentation : Node2D
{
    private const float CutawayMargin = 72.0f;
    private const float PaintedApproachMargin = 32.0f;
    private CharacterBody2D seeker = null!;
    private CharacterBody2D healer = null!;
    private CanvasItem bed = null!;
    private CanvasItem patient = null!;
    private Color bedColor;
    private Color patientColor;
    private OpeningRegionFirstPatientSceneSource sickhouse = null!;
    private OpeningRegionStreamLaundrySceneSource laundry = null!;
    private OpeningRegionOldShrineSceneSource shrine = null!;

    /// <summary>Whether a shelter is cut away at the supplied world position.</summary>
    public static bool NeedsCutaway(Rect2 bounds, Vector2 position) =>
        bounds.Grow(CutawayMargin).HasPoint(position);

    /// <inheritdoc />
    public override void _Ready()
    {
        seeker = GetNode<CharacterBody2D>("../Seeker");
        healer = GetNode<CharacterBody2D>("../Healer");
        bed = GetNode<CanvasItem>("../OpeningRegionFirstPatientSceneSource/Bed");
        patient = GetNode<CanvasItem>("../OpeningRegionFirstPatientSceneSource/Patient");
        bedColor = bed.SelfModulate;
        patientColor = patient.SelfModulate;
        sickhouse = GetNode<OpeningRegionFirstPatientSceneSource>("../OpeningRegionFirstPatientSceneSource");
        laundry = GetNode<OpeningRegionStreamLaundrySceneSource>("../OpeningRegionStreamLaundrySceneSource");
        shrine = GetNode<OpeningRegionOldShrineSceneSource>("../OpeningRegionOldShrineSceneSource");
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
        UpdateShelter("Sickhouse", sickhouse.Layout?.RoomBounds);
        UpdateShelter("Laundry", laundry.Layout?.SceneBinding.AbandonedLaundryBounds);
        UpdateShelter("Shrine", shrine.Layout?.SceneBinding.OldShrineBounds);
        bool patientVisible = IsCutaway("Sickhouse");
        bed.SelfModulate = new Color(bedColor, patientVisible ? bedColor.A : 0);
        patient.SelfModulate = new Color(patientColor, patientVisible ? patientColor.A : 0);
    }

    private void UpdateShelter(string name, NavigationWorldBounds? room)
    {
        if (room is not { } currentRoom) { return; }
        Sprite2D sprite = GetNode<Sprite2D>(name);
        Rect2 bounds = new(currentRoom.Left, currentRoom.Top,
            currentRoom.Right - currentRoom.Left, currentRoom.Bottom - currentRoom.Top);
        // Test actors in paint space so scaled/rotated artwork keeps the same cutaway rule.
        Vector2 margin = new(PaintedApproachMargin / sprite.GlobalScale.X,
            PaintedApproachMargin / sprite.GlobalScale.Y);
        Rect2 paint = sprite.GetRect().GrowIndividual(margin.X, margin.Y, margin.X, margin.Y);
        bool cutaway = ActorNeedsCutaway(sprite, bounds, paint, seeker)
            || ActorNeedsCutaway(sprite, bounds, paint, healer);
        ((ShaderMaterial)sprite.Material).SetShaderParameter("cutaway", cutaway ? 1.0f : 0.0f);
    }

    private static bool ActorNeedsCutaway(Sprite2D sprite, Rect2 bounds, Rect2 paint, CharacterBody2D actor) =>
        actor.IsVisibleInTree()
        && (NeedsCutaway(bounds, actor.GlobalPosition) || paint.HasPoint(sprite.ToLocal(actor.GlobalPosition)));

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
