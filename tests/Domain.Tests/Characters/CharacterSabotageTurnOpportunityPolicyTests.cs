using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Identifiers;
using Allrpcs.Domain.WorldEvents;

namespace Allrpcs.Domain.Tests.Characters;

public sealed class CharacterSabotageTurnOpportunityPolicyTests
{
    [Fact]
    public void AcceptsExactActorAndDueAuthoritativeMeaningfulEventSignal()
    {
        Input input = CreateInput();

        Assert.True(CanCreate(input));
    }

    [Fact]
    public void RejectsRebuiltActorAndCadence()
    {
        Input input = CreateInput();
        Character replacementActor = RepresentativeEnemyAgentCharacterSource.Create();
        CharacterSabotageTurnOpportunityCadence replacementCadence = new(
            input.Cadence.MeaningfulEventInterval);
        CharacterSabotageTurnOpportunityCadenceSignal replacementSignal = new(
            replacementCadence,
            input.CurrentEvent,
            input.Signal.OccurredMeaningfulEventCount);

        Assert.False(CanCreate(input with { Actor = replacementActor }));
        Assert.False(CanCreate(input with { Signal = replacementSignal }));
    }

    [Fact]
    public void RejectsForeignNonAdvancingUnrecordedAndStaleEvents()
    {
        Input input = CreateInput();
        WorldEvent foreignEvent = new(
            new WorldEventId(input.CurrentEvent.EventId.Value),
            WorldEventTimeEffect.Advance);
        CharacterSabotageTurnOpportunityCadenceSignal foreignSignal = new(
            input.Cadence,
            foreignEvent,
            input.Signal.OccurredMeaningfulEventCount);
        WorldEvent noAdvance = new(new WorldEventId("inspection"), WorldEventTimeEffect.NoAdvance);
        CharacterSabotageTurnOpportunityCadenceSignal noAdvanceSignal = new(
            input.Cadence,
            noAdvance,
            input.Signal.OccurredMeaningfulEventCount);

        Assert.False(CanCreate(input with { Signal = foreignSignal }));
        Assert.False(CanCreate(input with
        {
            Signal = noAdvanceSignal,
            WorldEvents = new WorldEventSnapshot(input.WorldEvents.Entries.Append(noAdvance)),
            EventLog = new WorldEventLogSnapshot([noAdvance.EventId]),
            WorldTime = new WorldTimeSnapshot(noAdvance.EventId),
        }));
        Assert.False(CanCreate(input with { EventLog = new WorldEventLogSnapshot([]) }));
        Assert.False(CanCreate(input with
        {
            WorldTime = new WorldTimeSnapshot(input.FirstEvent.EventId),
        }));
    }

    [Fact]
    public void RejectsStaleEarlyAndRepeatedSignals()
    {
        Input input = CreateInput();
        CharacterSabotageTurnOpportunityCadenceSignal staleCount = new(
            input.Cadence,
            input.CurrentEvent,
            1);
        CharacterSabotageTurnOpportunityCadenceSignal earlySignal = new(
            new CharacterSabotageTurnOpportunityCadence(3),
            input.CurrentEvent,
            2);
        CharacterSabotageTurnOpportunity existing = new(input.Actor, input.Signal);

        Assert.False(CanCreate(input with { Signal = staleCount }));
        Assert.False(CanCreate(input with
        {
            Cadence = earlySignal.Cadence,
            Signal = earlySignal,
        }));
        Assert.False(CanCreate(input with { CurrentOpportunity = existing }));
    }

    [Fact]
    public void AcceptsValueEqualRestoredLogIdentifiers()
    {
        Input input = CreateInput();
        WorldEventLogSnapshot rebuiltLog = new(
            [
                new WorldEventId(input.FirstEvent.EventId.Value),
                new WorldEventId(input.CurrentEvent.EventId.Value),
            ]);

        Assert.True(CanCreate(input with { EventLog = rebuiltLog }));
    }

    [Fact]
    public void RejectsNullInputsAndExposesOnlyOnePureOperation()
    {
        Input input = CreateInput();

        Assert.Throws<ArgumentNullException>(() => CharacterSabotageTurnOpportunityPolicy.CanCreate(
            null!,
            input.CurrentActor,
            input.Signal,
            input.Cadence,
            input.WorldEvents,
            input.EventLog,
            input.WorldTime,
            input.CurrentOpportunity));
        Assert.Throws<ArgumentNullException>(() => CharacterSabotageTurnOpportunityPolicy.CanCreate(
            input.Actor,
            null!,
            input.Signal,
            input.Cadence,
            input.WorldEvents,
            input.EventLog,
            input.WorldTime,
            input.CurrentOpportunity));
        Assert.Throws<ArgumentNullException>(() => CharacterSabotageTurnOpportunityPolicy.CanCreate(
            input.Actor,
            input.CurrentActor,
            null!,
            input.Cadence,
            input.WorldEvents,
            input.EventLog,
            input.WorldTime,
            input.CurrentOpportunity));
        Assert.Throws<ArgumentNullException>(() => CharacterSabotageTurnOpportunityPolicy.CanCreate(
            input.Actor,
            input.CurrentActor,
            input.Signal,
            null!,
            input.WorldEvents,
            input.EventLog,
            input.WorldTime,
            input.CurrentOpportunity));
        Assert.Throws<ArgumentNullException>(() => CharacterSabotageTurnOpportunityPolicy.CanCreate(
            input.Actor,
            input.CurrentActor,
            input.Signal,
            input.Cadence,
            null!,
            input.EventLog,
            input.WorldTime,
            input.CurrentOpportunity));
        Assert.Throws<ArgumentNullException>(() => CharacterSabotageTurnOpportunityPolicy.CanCreate(
            input.Actor,
            input.CurrentActor,
            input.Signal,
            input.Cadence,
            input.WorldEvents,
            null!,
            input.WorldTime,
            input.CurrentOpportunity));
        Assert.Throws<ArgumentNullException>(() => CharacterSabotageTurnOpportunityPolicy.CanCreate(
            input.Actor,
            input.CurrentActor,
            input.Signal,
            input.Cadence,
            input.WorldEvents,
            input.EventLog,
            null!,
            input.CurrentOpportunity));

        Type policyType = typeof(CharacterSabotageTurnOpportunityPolicy);
        System.Reflection.MethodInfo method = Assert.Single(
            policyType.GetMethods().Where(candidate => candidate.DeclaringType == policyType));

        Assert.Equal("CanCreate", method.Name);
        Assert.Equal(typeof(bool), method.ReturnType);
        Assert.Empty(policyType.GetConstructors());
        Assert.Empty(policyType.GetProperties());
        Assert.Empty(policyType.GetEvents());
        Assert.True(policyType.IsAbstract);
        Assert.True(policyType.IsSealed);
    }

    internal static Input CreateInput()
    {
        Character actor = RepresentativeEnemyAgentCharacterSource.Create();
        CharacterSabotageTurnOpportunityCadence cadence = new(2);
        WorldEvent firstEvent = new(new WorldEventId("first-event"), WorldEventTimeEffect.Advance);
        WorldEvent currentEvent = new(new WorldEventId("second-event"), WorldEventTimeEffect.Advance);
        WorldEventSnapshot worldEvents = new([firstEvent, currentEvent]);
        WorldEventLogSnapshot eventLog = new([firstEvent.EventId, currentEvent.EventId]);
        WorldTimeSnapshot worldTime = new(currentEvent.EventId);
        CharacterSabotageTurnOpportunityCadenceSignal signal = new(cadence, currentEvent, 2);

        return new Input(
            actor,
            actor,
            cadence,
            signal,
            firstEvent,
            currentEvent,
            worldEvents,
            eventLog,
            worldTime,
            null);
    }

    internal static bool CanCreate(Input input) =>
        CharacterSabotageTurnOpportunityPolicy.CanCreate(
            input.Actor,
            input.CurrentActor,
            input.Signal,
            input.Cadence,
            input.WorldEvents,
            input.EventLog,
            input.WorldTime,
            input.CurrentOpportunity);

    internal sealed record Input(
        Character Actor,
        Character CurrentActor,
        CharacterSabotageTurnOpportunityCadence Cadence,
        CharacterSabotageTurnOpportunityCadenceSignal Signal,
        WorldEvent FirstEvent,
        WorldEvent CurrentEvent,
        WorldEventSnapshot WorldEvents,
        WorldEventLogSnapshot EventLog,
        WorldTimeSnapshot WorldTime,
        CharacterSabotageTurnOpportunity? CurrentOpportunity);
}
