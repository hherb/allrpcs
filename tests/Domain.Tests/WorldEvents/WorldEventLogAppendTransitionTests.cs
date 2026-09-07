using Allrpcs.Domain.Identifiers;
using Allrpcs.Domain.WorldEvents;

namespace Allrpcs.Domain.Tests.WorldEvents;

public sealed class WorldEventLogAppendTransitionTests
{
    [Fact]
    public void AppendsExactIdentifierAfterStableRetainedEntries()
    {
        WorldEventId first = new("healer-arrived");
        WorldEventId second = new("patient-examined");
        WorldEventId appended = new("patient-stabilized");
        WorldEventLogSnapshot original = Log(first, second);

        WorldEventLogSnapshot result = WorldEventLogAppendTransition.Append(appended, original);

        Assert.NotSame(original, result);
        Assert.Collection(
            original.Entries,
            entry => Assert.Same(first, entry),
            entry => Assert.Same(second, entry));
        Assert.Collection(
            result.Entries,
            entry => Assert.Same(first, entry),
            entry => Assert.Same(second, entry),
            entry => Assert.Same(appended, entry));
    }

    [Fact]
    public void AppendsToAnEmptyLog()
    {
        WorldEventId appended = new("healer-arrived");

        WorldEventLogSnapshot result = WorldEventLogAppendTransition.Append(appended, Log());

        Assert.Collection(result.Entries, entry => Assert.Same(appended, entry));
    }

    [Fact]
    public void RejectsExactAndValueEqualDuplicatesWithoutMutation()
    {
        WorldEventId retained = new("patient-stabilized");
        WorldEventId duplicate = new("patient-stabilized");
        WorldEventLogSnapshot log = Log(retained);

        Assert.Throws<InvalidOperationException>(
            () => WorldEventLogAppendTransition.Append(retained, log));
        Assert.Throws<InvalidOperationException>(
            () => WorldEventLogAppendTransition.Append(duplicate, log));
        Assert.Collection(log.Entries, entry => Assert.Same(retained, entry));
    }

    [Fact]
    public void RejectsNullInputsThroughAppendPolicy()
    {
        WorldEventId eventId = new("patient-stabilized");
        WorldEventLogSnapshot log = Log();

        Assert.Throws<ArgumentNullException>(
            () => WorldEventLogAppendTransition.Append(null!, log));
        Assert.Throws<ArgumentNullException>(
            () => WorldEventLogAppendTransition.Append(eventId, null!));
    }

    [Fact]
    public void ExposesOnlyTheSinglePureAppendOperation()
    {
        Type transitionType = typeof(WorldEventLogAppendTransition);
        System.Reflection.MethodInfo method = Assert.Single(
            transitionType.GetMethods().Where(candidate => candidate.DeclaringType == transitionType));

        Assert.Equal("Append", method.Name);
        Assert.Equal(typeof(WorldEventLogSnapshot), method.ReturnType);
        Assert.True(method.IsStatic);
        Assert.Collection(
            method.GetParameters(),
            parameter => Assert.Equal(typeof(WorldEventId), parameter.ParameterType),
            parameter => Assert.Equal(typeof(WorldEventLogSnapshot), parameter.ParameterType));
        Assert.Empty(transitionType.GetConstructors());
        Assert.Empty(transitionType.GetProperties());
        Assert.Empty(transitionType.GetEvents());
        Assert.True(transitionType.IsAbstract);
        Assert.True(transitionType.IsSealed);
    }

    private static WorldEventLogSnapshot Log(params WorldEventId[] entries) => new(entries);
}
