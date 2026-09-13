using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Dialogue;
using Allrpcs.Domain.Identifiers;
using Allrpcs.Domain.WorldEvents;

namespace Allrpcs.Domain.Tests.Characters;

public sealed class CharacterIndependentTaskWorldEventOccurrencePolicyTests
{
    [Fact]
    public void AllowsExactActiveAssignmentAndAuthoritativeEligibleOccurrence()
    {
        CharacterIndependentTaskActivity activity =
            CharacterIndependentTaskWorldEventOccurrenceTests.ActiveActivity();
        WorldEvent worldEvent = CharacterIndependentTaskWorldEventOccurrenceTests.Event();
        CharacterIndependentTaskWorldEventOccurrence candidate = new(
            activity,
            CharacterIndependentTaskWorldEventOccurrenceTests.Occurrence(activity, worldEvent));

        Assert.True(CanOccur(candidate, worldEvent));
    }

    [Theory]
    [InlineData("assigned")]
    [InlineData("completed")]
    [InlineData("cancelled")]
    public void RejectsNonActiveTaskStates(string stateValue)
    {
        CharacterIndependentTaskActivity active =
            CharacterIndependentTaskWorldEventOccurrenceTests.ActiveActivity();
        CharacterIndependentTaskActivity activity = new(
            active.Assignment,
            CharacterIndependentTaskActivityState.Parse(stateValue));
        WorldEvent worldEvent = CharacterIndependentTaskWorldEventOccurrenceTests.Event();
        CharacterIndependentTaskWorldEventOccurrence candidate = new(
            activity,
            CharacterIndependentTaskWorldEventOccurrenceTests.Occurrence(activity, worldEvent));

        Assert.False(CanOccur(candidate, worldEvent));
    }

    [Fact]
    public void RejectsDistinctValueEqualCharacterAndIntentionInstances()
    {
        CharacterIndependentTaskActivity activity =
            CharacterIndependentTaskWorldEventOccurrenceTests.ActiveActivity();
        WorldEvent worldEvent = CharacterIndependentTaskWorldEventOccurrenceTests.Event();
        CharacterId equalCharacterId = new(activity.Assignment.CharacterId.Value);
        CharacterIntention equalIntention = new(
            activity.Assignment.Intention.Type,
            new ObjectId(activity.Assignment.Intention.TargetObjectId!.Value));

        Assert.Equal(activity.Assignment.CharacterId, equalCharacterId);
        Assert.Equal(activity.Assignment.Intention, equalIntention);
        Assert.False(CanOccur(
            new CharacterIndependentTaskWorldEventOccurrence(
                activity,
                CharacterIndependentTaskWorldEventOccurrenceTests.Occurrence(
                    activity,
                    worldEvent,
                    equalCharacterId)),
            worldEvent));
        Assert.False(CanOccur(
            new CharacterIndependentTaskWorldEventOccurrence(
                activity,
                CharacterIndependentTaskWorldEventOccurrenceTests.Occurrence(
                    activity,
                    worldEvent,
                    intention: equalIntention)),
            worldEvent));
    }

    [Fact]
    public void RejectsNonPhysicalOrNonAuthoritativeWorldOccurrence()
    {
        CharacterIndependentTaskActivity activity =
            CharacterIndependentTaskWorldEventOccurrenceTests.ActiveActivity();
        WorldEvent authoritative = CharacterIndependentTaskWorldEventOccurrenceTests.Event();
        WorldEvent equal = CharacterIndependentTaskWorldEventOccurrenceTests.Event();
        CharacterTurnWorldEventOccurrence spoken = new(
            authoritative,
            new CharacterTurnTranscriptEntry(
                new DialogueDelivery(
                    new DialogueUtterance(activity.Assignment.CharacterId, "I found it."),
                    DialogueDeliveryMode.OrdinarySpeech)));

        Assert.NotSame(authoritative, equal);
        Assert.False(CanOccur(
            new CharacterIndependentTaskWorldEventOccurrence(activity, spoken),
            authoritative));
        Assert.False(CanOccur(
            new CharacterIndependentTaskWorldEventOccurrence(
                activity,
                CharacterIndependentTaskWorldEventOccurrenceTests.Occurrence(activity, equal)),
            authoritative));
    }

    [Fact]
    public void RejectsNullInputsAndExposesOnlyThePureDecision()
    {
        CharacterIndependentTaskActivity activity =
            CharacterIndependentTaskWorldEventOccurrenceTests.ActiveActivity();
        WorldEvent worldEvent = CharacterIndependentTaskWorldEventOccurrenceTests.Event();
        CharacterIndependentTaskWorldEventOccurrence candidate = new(
            activity,
            CharacterIndependentTaskWorldEventOccurrenceTests.Occurrence(activity, worldEvent));
        WorldEventSnapshot worldEvents = new([worldEvent]);
        WorldEventLogSnapshot eventLog = new([]);
        WorldTimeSnapshot worldTime = new(null);

        Assert.Throws<ArgumentNullException>(() =>
            CharacterIndependentTaskWorldEventOccurrencePolicy.CanOccur(
                null!, worldEvents, eventLog, worldTime));
        Assert.Throws<ArgumentNullException>(() =>
            CharacterIndependentTaskWorldEventOccurrencePolicy.CanOccur(
                candidate, null!, eventLog, worldTime));
        Assert.Throws<ArgumentNullException>(() =>
            CharacterIndependentTaskWorldEventOccurrencePolicy.CanOccur(
                candidate, worldEvents, null!, worldTime));
        Assert.Throws<ArgumentNullException>(() =>
            CharacterIndependentTaskWorldEventOccurrencePolicy.CanOccur(
                candidate, worldEvents, eventLog, null!));

        Type policyType = typeof(CharacterIndependentTaskWorldEventOccurrencePolicy);
        System.Reflection.MethodInfo method = Assert.Single(
            policyType.GetMethods().Where(item => item.DeclaringType == policyType));
        Assert.Equal("CanOccur", method.Name);
        Assert.Equal(typeof(bool), method.ReturnType);
        Assert.Empty(policyType.GetConstructors());
        Assert.Empty(policyType.GetProperties());
        Assert.Empty(policyType.GetEvents());
        Assert.True(policyType.IsAbstract && policyType.IsSealed);
    }

    private static bool CanOccur(
        CharacterIndependentTaskWorldEventOccurrence candidate,
        WorldEvent authoritative) =>
        CharacterIndependentTaskWorldEventOccurrencePolicy.CanOccur(
            candidate,
            new WorldEventSnapshot([authoritative]),
            new WorldEventLogSnapshot([]),
            new WorldTimeSnapshot(null));
}
