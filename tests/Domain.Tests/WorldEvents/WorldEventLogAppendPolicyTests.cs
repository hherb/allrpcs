using Allrpcs.Domain.Identifiers;
using Allrpcs.Domain.WorldEvents;

namespace Allrpcs.Domain.Tests.WorldEvents;

public sealed class WorldEventLogAppendPolicyTests
{
    [Fact]
    public void AllowsOnlyAbsentIdentifiers()
    {
        WorldEventId eventId = new("patient-stabilized");

        Assert.True(WorldEventLogAppendPolicy.CanAppend(eventId, Log()));
        Assert.True(WorldEventLogAppendPolicy.CanAppend(
            eventId,
            Log(new WorldEventId("healer-arrived"))));
        Assert.False(WorldEventLogAppendPolicy.CanAppend(eventId, Log(eventId)));
        Assert.False(WorldEventLogAppendPolicy.CanAppend(
            eventId,
            Log(new WorldEventId("patient-stabilized"))));
    }

    [Fact]
    public void LeavesInputsUnchanged()
    {
        WorldEventId eventId = new("patient-stabilized");
        WorldEventId retained = new("healer-arrived");
        WorldEventLogSnapshot log = Log(retained);

        bool result = WorldEventLogAppendPolicy.CanAppend(eventId, log);

        Assert.True(result);
        Assert.Equal("patient-stabilized", eventId.Value);
        Assert.Collection(log.Entries, entry => Assert.Same(retained, entry));
    }

    [Fact]
    public void RejectsNullInputsThroughLookup()
    {
        WorldEventId eventId = new("patient-stabilized");
        WorldEventLogSnapshot log = Log();

        Assert.Throws<ArgumentNullException>(
            () => WorldEventLogAppendPolicy.CanAppend(null!, log));
        Assert.Throws<ArgumentNullException>(
            () => WorldEventLogAppendPolicy.CanAppend(eventId, null!));
    }

    [Fact]
    public void ExposesOnlyTheSinglePureAppendEligibilityOperation()
    {
        Type policyType = typeof(WorldEventLogAppendPolicy);
        System.Reflection.MethodInfo method = Assert.Single(
            policyType.GetMethods().Where(candidate => candidate.DeclaringType == policyType));

        Assert.Equal("CanAppend", method.Name);
        Assert.Equal(typeof(bool), method.ReturnType);
        Assert.True(method.IsStatic);
        Assert.Collection(
            method.GetParameters(),
            parameter => Assert.Equal(typeof(WorldEventId), parameter.ParameterType),
            parameter => Assert.Equal(typeof(WorldEventLogSnapshot), parameter.ParameterType));
        Assert.Empty(policyType.GetConstructors());
        Assert.Empty(policyType.GetProperties());
        Assert.Empty(policyType.GetEvents());
        Assert.True(policyType.IsAbstract);
        Assert.True(policyType.IsSealed);
    }

    private static WorldEventLogSnapshot Log(params WorldEventId[] entries) => new(entries);
}
