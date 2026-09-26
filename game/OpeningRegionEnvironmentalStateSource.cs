using Allrpcs.Application.Discovery;
using Allrpcs.Application.EnvironmentalStates;
using Allrpcs.Domain.EnvironmentalStates;
using Godot;

namespace Allrpcs.Game;

/// <summary>Owns the exact qualitative opening-region baseline and current snapshots.</summary>
public partial class OpeningRegionEnvironmentalStateSource : Node
{
    /// <summary>Gets the immutable representative baseline created at ordinary startup.</summary>
    public OpeningRegionEnvironmentalRegionSet BaselineRegions { get; private set; } = null!;

    /// <summary>Gets the exact current qualitative environmental-region snapshot.</summary>
    public OpeningRegionEnvironmentalRegionSet CurrentRegions { get; private set; } = null!;

    /// <inheritdoc />
    public override void _Ready()
    {
        OpeningRegionProjectionSource projectionSource =
            GetNode<OpeningRegionProjectionSource>("../OpeningRegionProjectionSource");
        BaselineRegions = RepresentativeOpeningRegionEnvironmentalSource.Create(
            projectionSource.Projection.Topology);
        CurrentRegions = BaselineRegions;
    }

    /// <summary>Installs one exact derived variation through baseline/current compare-and-swap.</summary>
    public bool TryInstallVariation(
        OpeningRegionEnvironmentalRegionSet? expectedBaseline,
        OpeningRegionEnvironmentalRegionSet? expectedCurrent,
        OpeningRegionLayeredInscriptionPresentation? presentation,
        OpeningRegionLayeredInscriptionEnvironmentalVariation? variation)
    {
        if (!ReferenceEquals(BaselineRegions, expectedBaseline)
            || !ReferenceEquals(CurrentRegions, expectedCurrent)
            || !OpeningRegionLayeredInscriptionEnvironmentalVariationInstallationPolicy.CanInstall(
                expectedBaseline,
                expectedCurrent,
                presentation,
                variation))
        {
            return false;
        }

        CurrentRegions = variation!.VariedRegions;
        return true;
    }
}
