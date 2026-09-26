using Allrpcs.Domain.EnvironmentalStates;
using Allrpcs.Domain.Locations;

namespace Allrpcs.Domain.Tests.EnvironmentalStates;

public sealed class RepresentativeOpeningRegionEnvironmentalSourceTests
{
    [Fact]
    public void AuthorsStableWeatherLightingAndSoundRegionsAcrossExactTopology()
    {
        OpeningRegionSnapshot topology = RepresentativeOpeningRegionSource.Create();

        OpeningRegionEnvironmentalRegionSet regions =
            RepresentativeOpeningRegionEnvironmentalSource.Create(topology);

        Assert.Same(topology, regions.Topology);
        Assert.Equal(
            ["village-shelter", "waterway", "woodland-edge", "southern-road"],
            regions.Regions.Select(region => region.Key));
        Assert.Equal(
            [
                "sickhouse",
                "healer-garden",
                "village-square",
                "village-edge",
                "stream",
                "abandoned-laundry",
                "woodland-clearing",
                "old-shrine",
                "closed-deeper-forest-path",
                "southern-road",
                "abandoned-wagon",
            ],
            regions.Regions.SelectMany(region => region.Locations)
                .Select(location => location.Identity.Id.Value));
        Assert.All(topology.Locations, location => Assert.NotNull(
            OpeningRegionEnvironmentalRegionLookup.FindByLocation(regions, location)));
        Assert.All(regions.Regions, region =>
        {
            Assert.Contains(region.WeatherState, regions.EnvironmentalStates.Entries);
            Assert.Contains(region.LightingState, regions.EnvironmentalStates.Entries);
            Assert.Contains(region.SoundState, regions.EnvironmentalStates.Entries);
        });
    }

    [Fact]
    public void RejectsNullTopology()
    {
        Assert.Throws<ArgumentNullException>(() =>
            RepresentativeOpeningRegionEnvironmentalSource.Create(null!));
    }
}
