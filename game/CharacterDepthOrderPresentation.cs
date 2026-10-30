using Allrpcs.Application.Presentation;
using Godot;

namespace Allrpcs.Game;

/// <summary>
/// Draws the Village Sage, water cask, Healer and Seeker in depth order: whichever stands further
/// south (nearer the viewer) is drawn in front (#479), so the water cask no longer covers the Sage (#477).
/// Main has no y-sort; enabling it would reorder every z-0 child of Main by its origin Y (the sickhouse
/// scene source sits at y 1390, so its bed and patient could draw over the Seeker in the cutaway),
/// so only these four are ranked. The fixed herb patch and stone marker are placed between them by their foot Y.
/// </summary>
public partial class CharacterDepthOrderPresentation : Node
{
    // Each node's origin is its feet (the cask's is its centre). List order breaks ties.
    private static readonly string[] SortedPaths =
    [
        "../SageHearingPosition",
        "../InteractionCandidates/WaterCask",
        "../Healer",
        "../Seeker",
    ];

    // Fixed props and how far below their node (the interaction point) their art's feet stand:
    // the herb sprigs' positions y 16, and the tablet's y -17 + pivot 75 - half height 40, in
    // Main.tscn. check-herb-patch-art.gd and check-stone-marker-art.gd tie each offset to its art
    // by checking the depth swap at exactly this foot.
    private static readonly (string Path, float FootOffset)[] FixedProps =
    [
        ("../InteractionCandidates/HerbPatch", 16.0f),
        ("../InteractionCandidates/StoneMarker", 18.0f),
    ];

    private Node2D[] items = [];
    private Node2D[] props = [];
    private readonly float[] footY = new float[SortedPaths.Length];

    /// <inheritdoc />
    public override void _Ready()
    {
        items = [.. SortedPaths.Select(path => GetNode<Node2D>(path))];
        props = [.. FixedProps.Select(prop => GetNode<Node2D>(prop.Path))];
        _Process(0.0);
    }

    /// <inheritdoc />
    public override void _Process(double delta)
    {
        for (int i = 0; i < items.Length; i++)
        {
            footY[i] = items[i].GlobalPosition.Y;
        }

        int[] ranks = DepthDrawOrder.Ranks(footY);
        for (int i = 0; i < items.Length; i++)
        {
            // Every other world drawing in Main except the fixed props (placed below) is at z <= 0, so
            // starting at 1 keeps all four above it (the Sage and cask now also above later z-0 scene
            // props). The Sage's name label is absolute z 5 in Main.tscn so nothing ranked up to 4,
            // the props included, covers it.
            items[i].ZIndex = 1 + ranks[i];
        }

        // Each fixed prop goes between the four without changing their 1..4: in front of those north
        // of it, level with the first one south of it. At equal z Godot draws in tree order. The cask,
        // Healer and Seeker come after both props in Main.tscn, so they draw in front; the fixed Sage
        // comes before them, but the Sage is far from both, so a tie with it never shows (the cask can
        // be moved south of the Sage, which makes that tie possible). The two props are 160 px apart
        // and never overlap, so their order between themselves does not matter.
        for (int i = 0; i < props.Length; i++)
        {
            props[i].ZIndex = 1 + DepthDrawOrder.CountNorthOf(footY, props[i].GlobalPosition.Y + FixedProps[i].FootOffset);
        }
    }
}
