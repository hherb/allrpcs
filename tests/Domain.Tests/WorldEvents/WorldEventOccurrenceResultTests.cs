using Allrpcs.Domain.Identifiers;
using Allrpcs.Domain.WorldEvents;

namespace Allrpcs.Domain.Tests.WorldEvents;

public sealed class WorldEventOccurrenceResultTests
{
    [Fact]
    public void RetainsExactLogAndWorldTimeSnapshots()
    {
        WorldEventLogSnapshot eventLog = new([new WorldEventId("patient-stabilized")]);
        WorldTimeSnapshot worldTime = new(new WorldEventId("patient-stabilized"));

        WorldEventOccurrenceResult result = new(eventLog, worldTime);

        Assert.Same(eventLog, result.EventLog);
        Assert.Same(worldTime, result.WorldTime);
    }

    [Fact]
    public void RejectsNullInputs()
    {
        WorldEventLogSnapshot eventLog = new([]);
        WorldTimeSnapshot worldTime = new(null);

        Assert.Throws<ArgumentNullException>(() => new WorldEventOccurrenceResult(null!, worldTime));
        Assert.Throws<ArgumentNullException>(() => new WorldEventOccurrenceResult(eventLog, null!));
    }

    [Fact]
    public void ExposesOnlyItsTwoGetOnlySnapshots()
    {
        Type resultType = typeof(WorldEventOccurrenceResult);
        System.Reflection.PropertyInfo[] properties = resultType.GetProperties();

        Assert.True(resultType.IsSealed);
        Assert.Collection(
            properties.OrderBy(property => property.Name),
            property =>
            {
                Assert.Equal("EventLog", property.Name);
                Assert.Equal(typeof(WorldEventLogSnapshot), property.PropertyType);
                Assert.False(property.CanWrite);
            },
            property =>
            {
                Assert.Equal("WorldTime", property.Name);
                Assert.Equal(typeof(WorldTimeSnapshot), property.PropertyType);
                Assert.False(property.CanWrite);
            });
        Assert.Single(resultType.GetConstructors());
        Assert.Empty(
            resultType.GetMethods()
                .Where(method => method.DeclaringType == resultType && !method.IsSpecialName));
        Assert.Empty(resultType.GetEvents());
    }
}
