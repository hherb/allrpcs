using Allrpcs.Application.Discovery;

namespace Allrpcs.Application.EnvironmentalStates;

/// <summary>Retains one exact completed inscription-to-environment presentation graph.</summary>
public sealed class OpeningRegionLayeredInscriptionEnvironmentalFlow
{
    /// <summary>Creates a token only for one complete exact flow graph.</summary>
    public OpeningRegionLayeredInscriptionEnvironmentalFlow(
        OpeningRegionLayeredInscriptionPresentation presentation,
        OpeningRegionLayeredInscriptionEnvironmentalVariation variation,
        OpeningRegionEnvironmentalLocationPresentation locationPresentation)
    {
        ArgumentNullException.ThrowIfNull(presentation);
        ArgumentNullException.ThrowIfNull(variation);
        ArgumentNullException.ThrowIfNull(locationPresentation);
        if (!ReferenceEquals(variation.Presentation, presentation))
        {
            throw new ArgumentException(
                "The environmental flow requires the variation's exact presentation.",
                nameof(presentation));
        }

        if (!ReferenceEquals(locationPresentation.Regions, variation.VariedRegions))
        {
            throw new ArgumentException(
                "The environmental flow requires a projection of the exact varied regions.",
                nameof(locationPresentation));
        }

        Presentation = presentation;
        Variation = variation;
        LocationPresentation = locationPresentation;
    }

    /// <summary>Gets the exact inscription presentation that authorized the flow.</summary>
    public OpeningRegionLayeredInscriptionPresentation Presentation { get; }

    /// <summary>Gets the exact installed qualitative environmental variation.</summary>
    public OpeningRegionLayeredInscriptionEnvironmentalVariation Variation { get; }

    /// <summary>Gets the exact rendered old-shrine environmental presentation.</summary>
    public OpeningRegionEnvironmentalLocationPresentation LocationPresentation { get; }
}
