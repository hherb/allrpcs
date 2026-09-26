using Allrpcs.Domain.EnvironmentalStates;
using Allrpcs.Domain.Locations;

namespace Allrpcs.Domain.Tests.EnvironmentalStates;

public sealed class OpeningRegionEnvironmentalRegionTests
{
    [Fact]
    public void RetainsAuthoredRegionLocationsAndEnvironmentalChannels()
    {
        OpeningRegionSnapshot topology = RepresentativeOpeningRegionSource.Create();
        EnvironmentalState weather = State("steady-rain");
        EnvironmentalState lighting = State("canopy-muted-light");
        EnvironmentalState sound = State("rain-under-trees");

        OpeningRegionEnvironmentalRegion region = new(
            "woodland-edge",
            [topology.Locations[6], topology.Locations[7], topology.Locations[10]],
            weather,
            lighting,
            sound);

        Assert.Equal("woodland-edge", region.Key);
        Assert.Collection(
            region.Locations,
            location => Assert.Same(topology.Locations[6], location),
            location => Assert.Same(topology.Locations[7], location),
            location => Assert.Same(topology.Locations[10], location));
        Assert.Same(weather, region.WeatherState);
        Assert.Same(lighting, region.LightingState);
        Assert.Same(sound, region.SoundState);
    }

    [Fact]
    public void RejectsInvalidKeysLocationsAndChannels()
    {
        OpeningRegionSnapshot topology = RepresentativeOpeningRegionSource.Create();
        EnvironmentalState state = State("steady-rain");

        Assert.Throws<ArgumentException>(() => new OpeningRegionEnvironmentalRegion(
            "Woodland",
            [topology.Locations[6]],
            state,
            state,
            state));
        Assert.Throws<ArgumentException>(() => new OpeningRegionEnvironmentalRegion(
            "woodland",
            [],
            state,
            state,
            state));
        Assert.Throws<ArgumentException>(() => new OpeningRegionEnvironmentalRegion(
            "woodland",
            [topology.Locations[6], topology.Locations[6]],
            state,
            state,
            state));
        Assert.Throws<ArgumentException>(() => new OpeningRegionEnvironmentalRegion(
            "woodland",
            [topology.Locations[6], null!],
            state,
            state,
            state));
        Assert.Throws<ArgumentNullException>(() => new OpeningRegionEnvironmentalRegion(
            "woodland",
            [topology.Locations[6]],
            null!,
            state,
            state));
    }

    private static EnvironmentalState State(string key) => new(key, $"Authored {key} state.");
}
