namespace Allrpcs.Application.Discovery;

/// <summary>Authorizes presentation of one exact applied inscription discovery.</summary>
public sealed class OpeningRegionLayeredInscriptionPresentation
{
    internal OpeningRegionLayeredInscriptionPresentation(
        OpeningRegionLayeredInscriptionDiscoveryState discoveryState,
        OpeningRegionLayeredInscriptionResolution resolution)
    {
        DiscoveryState = discoveryState;
        Resolution = resolution;
    }

    /// <summary>Gets the exact applied discovery state.</summary>
    public OpeningRegionLayeredInscriptionDiscoveryState DiscoveryState { get; }

    /// <summary>Gets the exact resolution retained by that state.</summary>
    public OpeningRegionLayeredInscriptionResolution Resolution { get; }
}
