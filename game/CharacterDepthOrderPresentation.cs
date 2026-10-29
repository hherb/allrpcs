using Allrpcs.Application.Presentation;
using Godot;

namespace Allrpcs.Game;

/// <summary>
/// Draws the Village Sage, water cask, Healer and Seeker in depth order: whichever stands further
/// south (nearer the viewer) is drawn in front (#479), so the water cask no longer covers the Sage (#477).
/// Main has no y-sort; enabling it would reorder every z-0 child of Main by its origin Y (the sickhouse
/// scene source sits at y 1390, so its bed and patient could draw over the Seeker in the cutaway),
/// so only these four are ranked.
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

    private Node2D[] items = [];
    private readonly float[] footY = new float[SortedPaths.Length];

    /// <inheritdoc />
    public override void _Ready()
    {
        items = [.. SortedPaths.Select(path => GetNode<Node2D>(path))];
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
            // Every other world drawing in Main is at z <= 0, so starting at 1 keeps all four above it
            // (the Sage and cask now also above later z-0 scene props). The Sage's name label is absolute
            // z 5 in Main.tscn so a character ranked up to 4 never covers it.
            items[i].ZIndex = 1 + ranks[i];
        }
    }
}
