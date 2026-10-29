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
}
