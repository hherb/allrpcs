using Allrpcs.Domain.Identifiers;
using Allrpcs.Domain.WorldEvents;

namespace Allrpcs.Domain.Tests.WorldEvents;

public sealed class WorldEventLogLookupTests
{
    [Fact]
    public void ReturnsNullForEmptyOrAbsentIdentifiers()
    {
        WorldEventId requested = new("patient-stabilized");

        Assert.Null(WorldEventLogLookup.Find(requested, Log()));
        Assert.Null(WorldEventLogLookup.Find(
            requested,
            Log(new WorldEventId("healer-arrived"))));
    }

    [Fact]
    public void ReturnsExactRetainedValueEqualIdentifier()
    {
        WorldEventId retained = new("patient-stabilized");
        WorldEventId requested = new("patient-stabilized");

        WorldEventId? result = WorldEventLogLookup.Find(requested, Log(retained));

        Assert.NotSame(requested, retained);
        Assert.Same(retained, result);
    }

    [Fact]
    public void LeavesInputsUnchanged()
    {
        WorldEventId before = new("healer-arrived");
        WorldEventId retained = new("patient-stabilized");
        WorldEventId after = new("herb-collected");
        WorldEventLogSnapshot log = Log(before, retained, after);

        WorldEventId? result = WorldEventLogLookup.Find(retained, log);

        Assert.Same(retained, result);
        Assert.Collection(
            log.Entries,
            entry => Assert.Same(before, entry),
            entry => Assert.Same(retained, entry),
            entry => Assert.Same(after, entry));
    }

    [Fact]
    public void RejectsNullInputs()
    {
        WorldEventId eventId = new("patient-stabilized");
        WorldEventLogSnapshot log = Log();

        Assert.Throws<ArgumentNullException>(() => WorldEventLogLookup.Find(null!, log));
        Assert.Throws<ArgumentNullException>(() => WorldEventLogLookup.Find(eventId, null!));
    }

    [Fact]
    public void ExposesOnlyTheSinglePureLookupOperation()
    {
        Type lookupType = typeof(WorldEventLogLookup);
        System.Reflection.MethodInfo method = Assert.Single(
            lookupType.GetMethods().Where(candidate => candidate.DeclaringType == lookupType));

        Assert.Equal("Find", method.Name);
        Assert.Equal(typeof(WorldEventId), method.ReturnType);
        Assert.True(method.IsStatic);
        Assert.Collection(
            method.GetParameters(),
            parameter => Assert.Equal(typeof(WorldEventId), parameter.ParameterType),
            parameter => Assert.Equal(typeof(WorldEventLogSnapshot), parameter.ParameterType));
        Assert.Empty(lookupType.GetConstructors());
        Assert.Empty(lookupType.GetProperties());
        Assert.Empty(lookupType.GetEvents());
        Assert.True(lookupType.IsAbstract);
        Assert.True(lookupType.IsSealed);
    }

    private static WorldEventLogSnapshot Log(params WorldEventId[] entries) => new(entries);
}
