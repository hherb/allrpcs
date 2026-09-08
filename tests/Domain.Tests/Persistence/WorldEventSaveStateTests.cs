using Allrpcs.Domain.Identifiers;
using Allrpcs.Domain.Persistence;
using Allrpcs.Domain.WorldEvents;

namespace Allrpcs.Domain.Tests.Persistence;

public sealed class WorldEventSaveStateTests
{
    [Fact]
    public void RetainsTheExactEventLogAndQualitativeTimeSnapshots()
    {
        WorldEventId eventId = new("patient-stabilized");
        WorldEventLogSnapshot eventLog = new([eventId]);
        WorldTimeSnapshot worldTime = new(eventId);

        WorldEventSaveState state = new(eventLog, worldTime);

        Assert.Same(eventLog, state.EventLog);
        Assert.Same(worldTime, state.WorldTime);
    }

    [Fact]
    public void EqualityUsesTheRetainedSnapshotInstances()
    {
        WorldEventLogSnapshot eventLog = new([]);
        WorldTimeSnapshot worldTime = new(null);
        WorldEventSaveState first = new(eventLog, worldTime);
        WorldEventSaveState same = new(eventLog, worldTime);
        WorldEventSaveState distinctEmptySnapshots = new(
            new WorldEventLogSnapshot([]),
            new WorldTimeSnapshot(null));

        Assert.Equal(first, same);
        Assert.True(first == same);
        Assert.NotEqual(first, distinctEmptySnapshots);
    }

    [Fact]
    public void RejectsEitherNullSnapshot()
    {
        WorldEventLogSnapshot eventLog = new([]);
        WorldTimeSnapshot worldTime = new(null);

        Assert.Throws<ArgumentNullException>(() => new WorldEventSaveState(null!, worldTime));
        Assert.Throws<ArgumentNullException>(() => new WorldEventSaveState(eventLog, null!));
    }

    [Fact]
    public void ExposesOnlyTheImmutableWorldEventStateSurface()
    {
        Type stateType = typeof(WorldEventSaveState);
        (string Name, Type Type)[] expectedProperties =
        [
            ("EventLog", typeof(WorldEventLogSnapshot)),
            ("WorldTime", typeof(WorldTimeSnapshot)),
        ];

        System.Reflection.PropertyInfo[] properties = stateType
            .GetProperties()
            .OrderBy(property => property.Name, StringComparer.Ordinal)
            .ToArray();

        Assert.Equal(expectedProperties, properties.Select(property =>
            (property.Name, property.PropertyType)).ToArray());
        Assert.All(properties, property => Assert.False(property.CanWrite));
        Assert.Empty(stateType.GetEvents());
        Assert.True(stateType.IsSealed);
    }
}
