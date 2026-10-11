using Allrpcs.Application.Navigation;
using Allrpcs.Domain.Actions;
using Godot;

namespace Allrpcs.Game;

/// <summary>Retains the exact existing Village Edge sources for the newly granted fixed site.</summary>
internal sealed class OpeningRegionHandToolTapSiteBinding
{
    /// <summary>Binds only the unchanged approved anchor and village layout; creates no inventory object.</summary>
    internal OpeningRegionHandToolTapSiteBinding(OpeningRegionProjectionSource projection, OpeningRegionVillageTransitionSceneSource village)
    {
        Projection = projection.Projection; Village = village.Layout;
        Anchor = Projection.Anchors.Single(a => a.Location.Identity.Id.Value == "village-edge");
        Marker = projection.GetNode<Marker2D>("village-edge");
        Site = new(HandToolTapSite.SiteKey, Anchor.Position);
        ProjectionTransform = projection.GlobalTransform; VillageTransform = village.GlobalTransform;
        MarkerTransform = Marker.Transform;
    }
    /// <summary>The immutable approved action-only material/site declaration.</summary>
    internal HandToolTapSite Site { get; }
    /// <summary>The exact retained projection.</summary>
    internal OpeningRegionProjection Projection { get; }
    /// <summary>The exact retained physical village layout.</summary>
    internal OpeningRegionVillageTransitionLayout Village { get; }
    /// <summary>The exact retained Village Edge location anchor.</summary>
    internal OpeningRegionAnchor Anchor { get; }
    /// <summary>The exact existing marker node.</summary>
    internal Marker2D Marker { get; }
    /// <summary>The unchanged projection transform.</summary>
    internal Transform2D ProjectionTransform { get; }
    /// <summary>The unchanged physical village source transform.</summary>
    internal Transform2D VillageTransform { get; }
    /// <summary>The unchanged local marker transform.</summary>
    internal Transform2D MarkerTransform { get; }
}
