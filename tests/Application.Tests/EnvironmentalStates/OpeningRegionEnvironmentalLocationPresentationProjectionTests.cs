using Allrpcs.Application.EnvironmentalStates;
using Allrpcs.Application.Navigation;
using Allrpcs.Application.Tests.Navigation;
using Allrpcs.Domain.EnvironmentalStates;
using Allrpcs.Domain.Locations;

namespace Allrpcs.Application.Tests.EnvironmentalStates;

public sealed class OpeningRegionEnvironmentalLocationPresentationProjectionTests
{
    [Fact]
    public void ProjectsExactLocationRegionAndQualitativeChannels()
    {
        OpeningRegionProjection projection = OpeningRegionOldShrineSceneBindingTests.Projection();
        OpeningRegionEnvironmentalRegionSet regions =
            RepresentativeOpeningRegionEnvironmentalSource.Create(projection.Topology);
        Location shrine = projection.Topology.Locations.Single(location =>
            location.Identity.Id.Value == "old-shrine");

        OpeningRegionEnvironmentalLocationPresentation presentation =
            OpeningRegionEnvironmentalLocationPresentationProjection.Project(regions, shrine);

        OpeningRegionEnvironmentalRegion woodland = regions.Regions.Single(region =>
            region.Key == "woodland-edge");
        Assert.Same(regions, presentation.Regions);
        Assert.Same(shrine, presentation.Location);
        Assert.Same(woodland, presentation.Region);
        Assert.Same(woodland.WeatherState, presentation.WeatherState);
        Assert.Same(woodland.LightingState, presentation.LightingState);
        Assert.Same(woodland.SoundState, presentation.SoundState);
        Assert.Equal(
            "Environment: steady-rain / canopy-muted-light / rain-under-trees",
            presentation.Text);
    }

    [Fact]
    public void RejectsNullAndNonRetainedLocations()
    {
        OpeningRegionProjection projection = OpeningRegionOldShrineSceneBindingTests.Projection();
        OpeningRegionEnvironmentalRegionSet regions =
            RepresentativeOpeningRegionEnvironmentalSource.Create(projection.Topology);
        Location foreignShrine = OpeningRegionOldShrineSceneBindingTests.Projection()
            .Topology.Locations.Single(location => location.Identity.Id.Value == "old-shrine");

        Assert.Throws<ArgumentNullException>(() =>
            OpeningRegionEnvironmentalLocationPresentationProjection.Project(null!, foreignShrine));
        Assert.Throws<ArgumentNullException>(() =>
            OpeningRegionEnvironmentalLocationPresentationProjection.Project(regions, null!));
        Assert.Throws<ArgumentException>(() =>
            OpeningRegionEnvironmentalLocationPresentationProjection.Project(
                regions,
                foreignShrine));
    }
}
