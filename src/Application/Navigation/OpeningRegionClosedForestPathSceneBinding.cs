namespace Allrpcs.Application.Navigation;

/// <summary>Binds the exact closed deeper-forest anchor to the retained clearing layout.</summary>
public sealed class OpeningRegionClosedForestPathSceneBinding
{
    private const string ClosedForestPathLocationId = "closed-deeper-forest-path";

    /// <summary>Creates one exact closed-path binding without opening the deeper forest.</summary>
    public OpeningRegionClosedForestPathSceneBinding(
        OpeningRegionWoodlandClearingLayout woodlandClearingLayout,
        OpeningRegionAnchor closedForestPathAnchor)
    {
        ArgumentNullException.ThrowIfNull(woodlandClearingLayout);
        ArgumentNullException.ThrowIfNull(closedForestPathAnchor);
        OpeningRegionProjection projection = woodlandClearingLayout.SceneBinding.Projection;
        if (!projection.Anchors.Any(candidate => ReferenceEquals(
                candidate,
                closedForestPathAnchor))
            || closedForestPathAnchor.Location.Identity.Id.Value != ClosedForestPathLocationId)
        {
            throw new ArgumentException(
                "The closed path must be the projection's exact retained anchor in its role.",
                nameof(closedForestPathAnchor));
        }

        OpeningRegionAnchor clearing =
            woodlandClearingLayout.SceneBinding.WoodlandClearingAnchor;
        if (!projection.Connections.Any(connection =>
            (ReferenceEquals(connection.FirstAnchor, clearing)
                && ReferenceEquals(connection.SecondAnchor, closedForestPathAnchor))
            || (ReferenceEquals(connection.FirstAnchor, closedForestPathAnchor)
                && ReferenceEquals(connection.SecondAnchor, clearing))))
        {
            throw new ArgumentException(
                "The closed path must retain its exact projected clearing connection.",
                nameof(closedForestPathAnchor));
        }

        WoodlandClearingLayout = woodlandClearingLayout;
        Projection = projection;
        ClosedForestPathAnchor = closedForestPathAnchor;
    }

    /// <summary>Gets the exact retained woodland-clearing layout.</summary>
    public OpeningRegionWoodlandClearingLayout WoodlandClearingLayout { get; }

    /// <summary>Gets the exact projection containing the retained path anchor.</summary>
    public OpeningRegionProjection Projection { get; }

    /// <summary>Gets the exact closed deeper-forest path anchor.</summary>
    public OpeningRegionAnchor ClosedForestPathAnchor { get; }
}
