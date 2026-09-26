using Allrpcs.Application.Map;

namespace Allrpcs.Application.Tests.Map;

public sealed class OpeningRegionMapLocationCatalogueTests
{
    private static readonly string[] CanonicalIds =
    [
        "sickhouse",
        "healer-garden",
        "village-square",
        "village-edge",
        "stream",
        "abandoned-laundry",
        "woodland-clearing",
        "old-shrine",
        "southern-road",
        "abandoned-wagon",
        "closed-deeper-forest-path",
    ];

    [Fact]
    public void AuthorsEveryOpeningLandmarkInStableOrder()
    {
        OpeningRegionMapLocationCatalogue catalogue =
            RepresentativeOpeningRegionMapLocationSource.Create();

        Assert.Equal(CanonicalIds, catalogue.Locations.Select(location => location.Id.Value));
        Assert.Equal(
            [
                "Sickhouse",
                "Healer's Garden",
                "Village Square",
                "Village Edge",
                "Stream",
                "Abandoned Laundry",
                "Woodland Clearing",
                "Old Shrine",
                "Southern Road",
                "Abandoned Wagon",
                "Closed Forest Path",
            ],
            catalogue.Locations.Select(location => location.DisplayLabel));
    }

    [Fact]
    public void ReturnsIndependentCataloguesAndReadOnlyCollections()
    {
        OpeningRegionMapLocationCatalogue first =
            RepresentativeOpeningRegionMapLocationSource.Create();
        OpeningRegionMapLocationCatalogue second =
            RepresentativeOpeningRegionMapLocationSource.Create();

        Assert.NotSame(first, second);
        Assert.NotSame(first.Locations, second.Locations);
        for (int index = 0; index < first.Locations.Count; index++)
        {
            Assert.NotSame(first.Locations[index], second.Locations[index]);
            Assert.NotSame(first.Locations[index].Id, second.Locations[index].Id);
        }

        Assert.Throws<NotSupportedException>(() =>
            ((IList<MapLocationDefinition>)first.Locations).Clear());
    }

    [Fact]
    public void CatalogueRejectsNullIncompleteDuplicateReorderedAndUnknownDefinitions()
    {
        OpeningRegionMapLocationCatalogue source =
            RepresentativeOpeningRegionMapLocationSource.Create();
        MapLocationDefinition[] locations = source.Locations.ToArray();

        Assert.Throws<ArgumentNullException>(() => new OpeningRegionMapLocationCatalogue(null!));
        Assert.Throws<ArgumentException>(() => new OpeningRegionMapLocationCatalogue([]));
        Assert.Throws<ArgumentException>(() =>
            new OpeningRegionMapLocationCatalogue(locations[..^1]));
        Assert.Throws<ArgumentException>(() => new OpeningRegionMapLocationCatalogue(
            [locations[0], locations[0], .. locations[2..]]));
        Assert.Throws<ArgumentException>(() => new OpeningRegionMapLocationCatalogue(
            [locations[1], locations[0], .. locations[2..]]));
        Assert.Throws<ArgumentException>(() => new OpeningRegionMapLocationCatalogue(
            [.. locations[..^1], new(new("unknown"), "Unknown")]));
        Assert.Throws<ArgumentException>(() => new OpeningRegionMapLocationCatalogue(
            [locations[0], null!, .. locations[2..]]));
    }

    [Fact]
    public void ExposesOnlyTheSafeCatalogueDefinitions()
    {
        Assert.Equal(
            ["Locations"],
            typeof(OpeningRegionMapLocationCatalogue)
                .GetProperties()
                .Select(property => property.Name));
    }
}
