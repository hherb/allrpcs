using Allrpcs.Domain.Characters;

namespace Allrpcs.Application.Navigation;

/// <summary>Retains one exact Seeker background and its optional-route destination.</summary>
public sealed class SeekerBackgroundRouteVariation
{
    /// <summary>Creates one immutable scene-neutral route-variation definition.</summary>
    public SeekerBackgroundRouteVariation(
        SeekerBackgroundChoice background,
        OpeningRegionAnchor destinationAnchor)
    {
        ArgumentNullException.ThrowIfNull(background);
        ArgumentNullException.ThrowIfNull(destinationAnchor);
        Background = background;
        DestinationAnchor = destinationAnchor;
    }

    /// <summary>Gets the exact representative background choice.</summary>
    public SeekerBackgroundChoice Background { get; }

    /// <summary>Gets the exact existing opening-region destination anchor.</summary>
    public OpeningRegionAnchor DestinationAnchor { get; }
}
