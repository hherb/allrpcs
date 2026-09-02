using Allrpcs.Domain.Identifiers;
using Allrpcs.Domain.Inventory;

namespace Allrpcs.Domain.Tests.Inventory;

public sealed class InventoryCapacityTests
{
    [Theory]
    [InlineData(0)]
    [InlineData(12)]
    public void RetainsCharacterAndNonNegativeMaximumObjectCount(int maximumObjectCount)
    {
        CharacterId characterId = new("travelling-healer");

        InventoryCapacity capacity = new(characterId, maximumObjectCount);

        Assert.Same(characterId, capacity.CharacterId);
        Assert.Equal(maximumObjectCount, capacity.MaximumObjectCount);
    }

    [Fact]
    public void EqualityUsesCharacterAndMaximumObjectCountValues()
    {
        InventoryCapacity first = new(new CharacterId("travelling-healer"), 12);
        InventoryCapacity same = new(new CharacterId("travelling-healer"), 12);
        InventoryCapacity differentCharacter = new(new CharacterId("seeker"), 12);
        InventoryCapacity differentMaximum = new(new CharacterId("travelling-healer"), 6);

        Assert.Equal(first, same);
        Assert.True(first == same);
        Assert.NotEqual(first, differentCharacter);
        Assert.NotEqual(first, differentMaximum);
    }

    [Fact]
    public void RejectsNullCharacterAndNegativeMaximumObjectCount()
    {
        Assert.Throws<ArgumentNullException>(() => new InventoryCapacity(null!, 12));
        Assert.Throws<ArgumentOutOfRangeException>(
            () => new InventoryCapacity(new CharacterId("travelling-healer"), -1));
    }

    [Fact]
    public void ExposesOnlyTheImmutableCapacitySurface()
    {
        Type capacityType = typeof(InventoryCapacity);
        (string Name, Type Type)[] expectedProperties =
        [
            ("CharacterId", typeof(CharacterId)),
            ("MaximumObjectCount", typeof(int)),
        ];

        (string Name, Type Type, bool CanWrite)[] actualProperties = capacityType
            .GetProperties()
            .OrderBy(property => property.Name, StringComparer.Ordinal)
            .Select(property => (property.Name, property.PropertyType, property.CanWrite))
            .ToArray();

        Assert.Equal(
            expectedProperties,
            actualProperties.Select(property => (property.Name, property.Type)).ToArray());
        Assert.All(actualProperties, property => Assert.False(property.CanWrite));
        Assert.Empty(capacityType.GetEvents());
        Assert.True(capacityType.IsSealed);
    }
}
