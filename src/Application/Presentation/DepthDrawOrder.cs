namespace Allrpcs.Application.Presentation;

/// <summary>
/// Orders top-down world items for drawing so that whatever stands further south (nearer the
/// viewer) is drawn in front, without depending on the presentation engine.
/// </summary>
public static class DepthDrawOrder
{
    /// <summary>
    /// Ranks items by the world Y of their feet: rank 0 is drawn first (furthest back).
    /// </summary>
    /// <param name="footY">Each item's foot Y in world pixels; larger Y is further south.</param>
    /// <returns>
    /// One distinct rank per item, in input order. Equal Y keeps input order, so ties never
    /// flicker between frames.
    /// </returns>
    public static int[] Ranks(IReadOnlyList<float> footY)
    {
        ArgumentNullException.ThrowIfNull(footY);
        int[] byDepth = Enumerable.Range(0, footY.Count).OrderBy(i => footY[i]).ToArray();
        int[] ranks = new int[footY.Count];
        for (int rank = 0; rank < byDepth.Length; rank++)
        {
            ranks[byDepth[rank]] = rank;
        }

        return ranks;
    }

    /// <summary>
    /// Counts the ranked items whose feet are strictly north of a fixed prop's foot, i.e. the items
    /// the prop must be drawn in front of. Ranked items at z <c>1 + rank</c> leave the prop at z
    /// <c>1 + count</c>: above every item north of it, level with the first item south of it.
    /// </summary>
    /// <param name="footY">The ranked items' foot Y in world pixels.</param>
    /// <param name="propFootY">The prop's foot Y; an item level with it counts as in front of it.</param>
    public static int CountNorthOf(IReadOnlyList<float> footY, float propFootY)
    {
        ArgumentNullException.ThrowIfNull(footY);
        return footY.Count(y => y < propFootY);
    }
}
