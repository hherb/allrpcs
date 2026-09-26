using Allrpcs.Application.Discovery;
using Allrpcs.Domain.EnvironmentalStates;

namespace Allrpcs.Application.EnvironmentalStates;

/// <summary>Retains one pure woodland variation authorized by inscription presentation.</summary>
public sealed class OpeningRegionLayeredInscriptionEnvironmentalVariation
{
    internal OpeningRegionLayeredInscriptionEnvironmentalVariation(
        OpeningRegionEnvironmentalRegionSet baselineRegions,
        OpeningRegionLayeredInscriptionPresentation presentation,
        EnvironmentalState revealedLightingState,
        EnvironmentalState revealedSoundState,
        OpeningRegionEnvironmentalRegionSet variedRegions)
    {
        BaselineRegions = baselineRegions;
        Presentation = presentation;
        RevealedLightingState = revealedLightingState;
        RevealedSoundState = revealedSoundState;
        VariedRegions = variedRegions;
    }

    /// <summary>Gets the exact unchanged baseline environmental regions.</summary>
    public OpeningRegionEnvironmentalRegionSet BaselineRegions { get; }

    /// <summary>Gets the exact inscription presentation authorizing the variation.</summary>
    public OpeningRegionLayeredInscriptionPresentation Presentation { get; }

    /// <summary>Gets the exact replacement woodland lighting state.</summary>
    public EnvironmentalState RevealedLightingState { get; }

    /// <summary>Gets the exact replacement woodland sound state.</summary>
    public EnvironmentalState RevealedSoundState { get; }

    /// <summary>Gets the varied region set without scene or playback mutation.</summary>
    public OpeningRegionEnvironmentalRegionSet VariedRegions { get; }
}
