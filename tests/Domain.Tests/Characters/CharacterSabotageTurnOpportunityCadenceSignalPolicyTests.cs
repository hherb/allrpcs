using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Identifiers;
using Allrpcs.Domain.WorldEvents;

namespace Allrpcs.Domain.Tests.Characters;

public sealed class CharacterSabotageTurnOpportunityCadenceSignalPolicyTests
{
    [Fact]
    public void AcceptsTheExactDueAuthoritativeMeaningfulEvent()
    {
        Input input = CreateInput();

        Assert.True(CanCreate(input));
    }

    [Fact]
    public void RejectsEarlyForeignNonAdvancingUnrecordedAndStaleEvents()
    {
        Input input = CreateInput();
        CharacterSabotageTurnOpportunityCadence laterCadence = new(3);
        WorldEvent foreign = new(
            new WorldEventId(input.CurrentEvent.EventId.Value),
            WorldEventTimeEffect.Advance);
        WorldEvent noAdvance = new(new WorldEventId("inspection"), WorldEventTimeEffect.NoAdvance);

        Assert.False(CanCreate(input with { Cadence = laterCadence }));
        Assert.False(CanCreate(input with { CurrentEvent = foreign }));
        Assert.False(CanCreate(input with
        {
            CurrentEvent = noAdvance,
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
    public void RejectsRebuiltValueEqualLogIdentifiers()
    {
        Input input = CreateInput();
        WorldEventLogSnapshot rebuiltLog = new(
            [
                new WorldEventId(input.FirstEvent.EventId.Value),
                new WorldEventId(input.CurrentEvent.EventId.Value),
            ]);

        Assert.False(CanCreate(input with { EventLog = rebuiltLog }));
    }

    [Fact]
    public void RejectsNullsAndExposesOnlyOnePureOperation()
    {
        Input input = CreateInput();

        Assert.Throws<ArgumentNullException>(() =>
            CharacterSabotageTurnOpportunityCadenceSignalPolicy.CanCreate(
                null!,
                input.CurrentEvent,
                input.WorldEvents,
                input.EventLog,
                input.WorldTime));
        Assert.Throws<ArgumentNullException>(() =>
            CharacterSabotageTurnOpportunityCadenceSignalPolicy.CanCreate(
                input.Cadence,
                null!,
                input.WorldEvents,
                input.EventLog,
                input.WorldTime));
        Assert.Throws<ArgumentNullException>(() =>
            CharacterSabotageTurnOpportunityCadenceSignalPolicy.CanCreate(
                input.Cadence,
                input.CurrentEvent,
                null!,
                input.EventLog,
                input.WorldTime));
        Assert.Throws<ArgumentNullException>(() =>
            CharacterSabotageTurnOpportunityCadenceSignalPolicy.CanCreate(
                input.Cadence,
                input.CurrentEvent,
                input.WorldEvents,
                null!,
                input.WorldTime));
        Assert.Throws<ArgumentNullException>(() =>
            CharacterSabotageTurnOpportunityCadenceSignalPolicy.CanCreate(
                input.Cadence,
                input.CurrentEvent,
                input.WorldEvents,
                input.EventLog,
                null!));

        Type policyType = typeof(CharacterSabotageTurnOpportunityCadenceSignalPolicy);
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
        CharacterSabotageTurnOpportunityCadence cadence = new(2);
        WorldEvent firstEvent = new(new WorldEventId("first-event"), WorldEventTimeEffect.Advance);
        WorldEvent currentEvent = new(new WorldEventId("second-event"), WorldEventTimeEffect.Advance);
        WorldEventSnapshot worldEvents = new([firstEvent, currentEvent]);
        return new Input(
            cadence,
            firstEvent,
            currentEvent,
            worldEvents,
            new WorldEventLogSnapshot([firstEvent.EventId, currentEvent.EventId]),
            new WorldTimeSnapshot(currentEvent.EventId));
    }

    internal static bool CanCreate(Input input) =>
        CharacterSabotageTurnOpportunityCadenceSignalPolicy.CanCreate(
            input.Cadence,
            input.CurrentEvent,
            input.WorldEvents,
            input.EventLog,
            input.WorldTime);

    internal sealed record Input(
        CharacterSabotageTurnOpportunityCadence Cadence,
        WorldEvent FirstEvent,
        WorldEvent CurrentEvent,
        WorldEventSnapshot WorldEvents,
        WorldEventLogSnapshot EventLog,
        WorldTimeSnapshot WorldTime);
}
