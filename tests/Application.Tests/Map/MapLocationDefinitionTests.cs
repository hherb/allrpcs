using Allrpcs.Application.Map;
using Allrpcs.Domain.Identifiers;

namespace Allrpcs.Application.Tests.Map;

public sealed class MapLocationDefinitionTests
{
    [Fact]
    public void RetainsExactIdentifierAndBoundedPlayerFacingLabel()
    {
        LocationId id = new("old-shrine");

        MapLocationDefinition definition = new(id, " Old Shrine ");

        Assert.Same(id, definition.Id);
        Assert.Equal(" Old Shrine ", definition.DisplayLabel);
    }

    [Fact]
    public void AcceptsMaximumLengthLabel()
    {
        string label = new('l', MapLocationDefinition.MaximumDisplayLabelLength);

        MapLocationDefinition definition = new(new LocationId("stream"), label);

        Assert.Equal(label, definition.DisplayLabel);
    }

    [Fact]
    public void RejectsNullBlankAndOversizedInput()
    {
        Assert.Throws<ArgumentNullException>(() => new MapLocationDefinition(null!, "Stream"));
        Assert.Throws<ArgumentNullException>(() =>
            new MapLocationDefinition(new LocationId("stream"), null!));
        Assert.Throws<ArgumentException>(() =>
            new MapLocationDefinition(new LocationId("stream"), " \t\r\n"));
        Assert.Throws<ArgumentOutOfRangeException>(() => new MapLocationDefinition(
            new LocationId("stream"),
            new string('l', MapLocationDefinition.MaximumDisplayLabelLength + 1)));
    }

    [Fact]
    public void ExposesOnlySafeAuthoredMapLocationState()
    {
        Assert.Equal(
            ["DisplayLabel", "Id"],
            typeof(MapLocationDefinition)
                .GetProperties()
                .Select(property => property.Name)
                .Order());
    }
}
