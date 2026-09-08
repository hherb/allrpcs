using Allrpcs.Domain.Identifiers;
using Allrpcs.Domain.Persistence;
using Allrpcs.Domain.WorldEvents;

namespace Allrpcs.Domain.Tests.Persistence;

public sealed class WorldEventSaveStateRestorationPolicyTests
{
    [Fact]
    public void AcceptsEmptyInitialState()
    {
        WorldEventSaveState state = new(new WorldEventLogSnapshot([]), new WorldTimeSnapshot(null));

        Assert.True(WorldEventSaveStateRestorationPolicy.CanRestore(
            state,
            new WorldEventSnapshot([])));
    }

    [Fact]
    public void AcceptsACompleteLogWhoseTimeMatchesTheLatestAdvancingEvent()
    {
        WorldEvent firstAdvance = Event("healer-arrived", WorldEventTimeEffect.Advance);
        WorldEvent noAdvance = Event("herb-examined", WorldEventTimeEffect.NoAdvance);
        WorldEvent latestAdvance = Event("patient-stabilized", WorldEventTimeEffect.Advance);
        WorldEvent unrecorded = Event("journal-opened", WorldEventTimeEffect.NoAdvance);
        WorldEventLogSnapshot eventLog = new(
            [firstAdvance.EventId, noAdvance.EventId, latestAdvance.EventId]);
        WorldTimeSnapshot worldTime = new(new WorldEventId("patient-stabilized"));
        WorldEventSaveState state = new(eventLog, worldTime);
        WorldEventSnapshot definitions = new(
            [unrecorded, latestAdvance, firstAdvance, noAdvance]);

        bool canRestore = WorldEventSaveStateRestorationPolicy.CanRestore(state, definitions);

        Assert.True(canRestore);
        Assert.Same(eventLog, state.EventLog);
        Assert.Same(worldTime, state.WorldTime);
        Assert.Collection(
            definitions.Entries,
            entry => Assert.Same(unrecorded, entry),
            entry => Assert.Same(latestAdvance, entry),
            entry => Assert.Same(firstAdvance, entry),
            entry => Assert.Same(noAdvance, entry));
    }

    [Fact]
    public void RejectsAnUndefinedLoggedEventWithoutChangingInputs()
    {
        WorldEvent retained = Event("healer-arrived", WorldEventTimeEffect.NoAdvance);
        WorldEventId missing = new("patient-stabilized");
        WorldEventSaveState state = new(
            new WorldEventLogSnapshot([retained.EventId, missing]),
            new WorldTimeSnapshot(null));
        WorldEventSnapshot definitions = new([retained]);

        Assert.False(WorldEventSaveStateRestorationPolicy.CanRestore(state, definitions));
        Assert.Collection(
            state.EventLog.Entries,
            entry => Assert.Same(retained.EventId, entry),
            entry => Assert.Same(missing, entry));
        Assert.Collection(definitions.Entries, entry => Assert.Same(retained, entry));
    }

    [Fact]
    public void RejectsEveryTimeStateThatDoesNotMatchTheResolvedLog()
    {
        WorldEvent first = Event("healer-arrived", WorldEventTimeEffect.Advance);
        WorldEvent latest = Event("patient-stabilized", WorldEventTimeEffect.Advance);
        WorldEvent noAdvance = Event("journal-opened", WorldEventTimeEffect.NoAdvance);
        WorldEventSnapshot definitions = new([first, latest, noAdvance]);
        WorldEventLogSnapshot log = new([first.EventId, latest.EventId, noAdvance.EventId]);

        Assert.False(CanRestore(log, new WorldTimeSnapshot(null), definitions));
        Assert.False(CanRestore(log, new WorldTimeSnapshot(first.EventId), definitions));
        Assert.False(CanRestore(log, new WorldTimeSnapshot(noAdvance.EventId), definitions));
        Assert.False(CanRestore(
            log,
            new WorldTimeSnapshot(new WorldEventId("missing-event")),
            definitions));
    }

    [Fact]
    public void RejectsNullInputs()
    {
        WorldEventSaveState state = new(new WorldEventLogSnapshot([]), new WorldTimeSnapshot(null));
        WorldEventSnapshot definitions = new([]);

        Assert.Throws<ArgumentNullException>(
            () => WorldEventSaveStateRestorationPolicy.CanRestore(null!, definitions));
        Assert.Throws<ArgumentNullException>(
            () => WorldEventSaveStateRestorationPolicy.CanRestore(state, null!));
    }

    [Fact]
    public void ExposesOnlyTheStaticRestorationDecision()
    {
        Type policyType = typeof(WorldEventSaveStateRestorationPolicy);
        System.Reflection.MethodInfo method = Assert.Single(
            policyType.GetMethods(
                System.Reflection.BindingFlags.Public
                | System.Reflection.BindingFlags.Static
                | System.Reflection.BindingFlags.DeclaredOnly));

        Assert.Equal(nameof(WorldEventSaveStateRestorationPolicy.CanRestore), method.Name);
        Assert.Equal(typeof(bool), method.ReturnType);
        Assert.Collection(
            method.GetParameters(),
            parameter => Assert.Equal(typeof(WorldEventSaveState), parameter.ParameterType),
            parameter => Assert.Equal(typeof(WorldEventSnapshot), parameter.ParameterType));
        Assert.Empty(policyType.GetProperties());
        Assert.Empty(policyType.GetEvents());
    }

    private static bool CanRestore(
        WorldEventLogSnapshot eventLog,
        WorldTimeSnapshot worldTime,
        WorldEventSnapshot definitions) =>
        WorldEventSaveStateRestorationPolicy.CanRestore(
            new WorldEventSaveState(eventLog, worldTime),
            definitions);

    private static WorldEvent Event(string id, WorldEventTimeEffect timeEffect) =>
        new(new WorldEventId(id), timeEffect);
}
