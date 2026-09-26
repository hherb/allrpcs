using Allrpcs.Domain.EnvironmentalStates;
using Allrpcs.Domain.Locations;

namespace Allrpcs.Domain.Tests.EnvironmentalStates;

public sealed class OpeningRegionEnvironmentalRegionSetTests
{
    [Fact]
    public void RetainsExactCompleteNonOverlappingRegionAssignment()
    {
        OpeningRegionSnapshot topology = RepresentativeOpeningRegionSource.Create();
        EnvironmentalState weather = State("steady-rain");
        EnvironmentalState lighting = State("cool-overcast-light");
        EnvironmentalState sound = State("rain-on-stone");
        EnvironmentalStateSet catalog = new([weather, lighting, sound]);
        OpeningRegionEnvironmentalRegion region = Region(
            "opening-region",
            topology.Locations,
            weather,
            lighting,
            sound);

        OpeningRegionEnvironmentalRegionSet regions = new(topology, catalog, [region]);

        Assert.Same(topology, regions.Topology);
        Assert.Same(catalog, regions.EnvironmentalStates);
        Assert.Same(region, Assert.Single(regions.Regions));
        Assert.Same(region, OpeningRegionEnvironmentalRegionLookup.FindByLocation(
            regions,
            topology.Locations[4]));
        Assert.Null(OpeningRegionEnvironmentalRegionLookup.FindByLocation(
            regions,
            new Location(
                topology.Locations[4].Identity,
                topology.Locations[4].Description)));
    }

    [Fact]
    public void RejectsMissingOverlappingForeignAndRebuiltLocations()
    {
        OpeningRegionSnapshot topology = RepresentativeOpeningRegionSource.Create();
        (EnvironmentalStateSet catalog, EnvironmentalState[] states) = Catalog();
        OpeningRegionEnvironmentalRegion complete = Region(
            "complete",
            topology.Locations,
            states[0],
            states[1],
            states[2]);
        OpeningRegionEnvironmentalRegion overlap = Region(
            "overlap",
            [topology.Locations[0]],
            states[0],
            states[1],
            states[2]);
        OpeningRegionSnapshot foreign = RepresentativeOpeningRegionSource.Create();
        OpeningRegionEnvironmentalRegion foreignRegion = Region(
            "foreign",
            foreign.Locations,
            states[0],
            states[1],
            states[2]);

        Assert.Throws<ArgumentException>(() => new OpeningRegionEnvironmentalRegionSet(
            topology,
            catalog,
            [Region(
                "missing",
                topology.Locations.Take(topology.Locations.Count - 1),
                states[0],
                states[1],
                states[2])]));
        Assert.Throws<ArgumentException>(() => new OpeningRegionEnvironmentalRegionSet(
            topology,
            catalog,
            [complete, overlap]));
        Assert.Throws<ArgumentException>(() => new OpeningRegionEnvironmentalRegionSet(
            topology,
            catalog,
            [foreignRegion]));
        Assert.Throws<ArgumentException>(() => new OpeningRegionEnvironmentalRegionSet(
            topology,
            catalog,
            [Region(
                "rebuilt",
                topology.Locations.Select(location => new Location(
                    location.Identity,
                    location.Description)),
                states[0],
                states[1],
                states[2])]));
    }

    [Fact]
    public void RejectsDuplicateRegionKeysAndUnretainedStates()
    {
        OpeningRegionSnapshot topology = RepresentativeOpeningRegionSource.Create();
        (EnvironmentalStateSet catalog, EnvironmentalState[] states) = Catalog();
        OpeningRegionEnvironmentalRegion first = Region(
            "same",
            topology.Locations.Take(5),
            states[0],
            states[1],
            states[2]);
        OpeningRegionEnvironmentalRegion duplicate = Region(
            "same",
            topology.Locations.Skip(5),
            states[0],
            states[1],
            states[2]);
        OpeningRegionEnvironmentalRegion rebuiltState = Region(
            "rebuilt-state",
            topology.Locations,
            new EnvironmentalState(states[0].Key, states[0].Description),
            states[1],
            states[2]);
        Assert.Throws<ArgumentException>(() => new OpeningRegionEnvironmentalRegionSet(
            topology,
            catalog,
            [first, duplicate]));
        Assert.Throws<ArgumentException>(() => new OpeningRegionEnvironmentalRegionSet(
            topology,
            catalog,
            [rebuiltState]));
    }

    private static OpeningRegionEnvironmentalRegion Region(
        string key,
        IEnumerable<Location> locations,
        EnvironmentalState weather,
        EnvironmentalState lighting,
        EnvironmentalState sound) => new(key, locations, weather, lighting, sound);

    private static (EnvironmentalStateSet, EnvironmentalState[]) Catalog()
    {
        EnvironmentalState[] states =
        [
            State("steady-rain"),
            State("cool-overcast-light"),
            State("rain-on-stone"),
        ];
        return (new EnvironmentalStateSet(states), states);
    }

    private static EnvironmentalState State(string key) => new(key, $"Authored {key} state.");
}
