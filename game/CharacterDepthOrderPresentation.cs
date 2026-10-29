using Allrpcs.Application.Presentation;
using Godot;

namespace Allrpcs.Game;

/// <summary>
/// Draws the tall figures in the village square in depth order: whichever stands further south
/// (nearer the viewer) is drawn in front (#479), so the water cask no longer covers the Sage (#477).
/// Main has no y-sort; enabling it on Main would reorder every child drawn at origin Y (for example
/// the sickhouse floor would cover the Seeker inside the cutaway), so only these four are ranked.
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
            // Every other world drawing in Main is at z <= 0, so starting at 1 keeps all four above
            // it, as Healer and Seeker already were by tree order; only their order among each other changes.
            items[i].ZIndex = 1 + ranks[i];
        }
    }
}
