using Allrpcs.Application.Discovery;
using Allrpcs.Domain.EnvironmentalStates;

namespace Allrpcs.Application.EnvironmentalStates;

/// <summary>Checks exact one-shot authority to install an inscription environment variation.</summary>
public static class OpeningRegionLayeredInscriptionEnvironmentalVariationInstallationPolicy
{
    /// <summary>Returns whether one exact baseline can be replaced by its exact derived variation.</summary>
    public static bool CanInstall(
        OpeningRegionEnvironmentalRegionSet? baselineRegions,
        OpeningRegionEnvironmentalRegionSet? currentRegions,
        OpeningRegionLayeredInscriptionPresentation? presentation,
        OpeningRegionLayeredInscriptionEnvironmentalVariation? variation) =>
        baselineRegions is not null
        && ReferenceEquals(currentRegions, baselineRegions)
        && presentation is not null
        && variation is not null
        && ReferenceEquals(variation.BaselineRegions, baselineRegions)
        && ReferenceEquals(variation.Presentation, presentation)
        && ReferenceEquals(variation.VariedRegions.Topology, baselineRegions.Topology)
        && !ReferenceEquals(variation.VariedRegions, currentRegions);
}
