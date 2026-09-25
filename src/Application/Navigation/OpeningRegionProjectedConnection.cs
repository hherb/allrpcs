using Allrpcs.Domain.Locations;

namespace Allrpcs.Application.Navigation;

/// <summary>Retains one exact topology edge and its exact projected endpoint anchors.</summary>
public sealed class OpeningRegionProjectedConnection
{
    internal OpeningRegionProjectedConnection(
        OpeningRegionConnection topologyConnection,
        OpeningRegionAnchor firstAnchor,
        OpeningRegionAnchor secondAnchor)
    {
        TopologyConnection = topologyConnection;
        FirstAnchor = firstAnchor;
        SecondAnchor = secondAnchor;
    }

    /// <summary>Gets the exact authored connection represented by this projection.</summary>
    public OpeningRegionConnection TopologyConnection { get; }

    /// <summary>Gets the exact anchor for the authored first endpoint.</summary>
    public OpeningRegionAnchor FirstAnchor { get; }

    /// <summary>Gets the exact anchor for the authored second endpoint.</summary>
    public OpeningRegionAnchor SecondAnchor { get; }
}
