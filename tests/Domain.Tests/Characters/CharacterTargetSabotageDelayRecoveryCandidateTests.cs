using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Identifiers;
using Allrpcs.Domain.WorldEvents;

namespace Allrpcs.Domain.Tests.Characters;

public sealed class CharacterTargetSabotageDelayRecoveryCandidateTests
{
    [Fact]
    public void CandidateRetainsExactDelayResultAndRecoveryEvent()
    {
        RecoveryInput input = Input();

        Assert.Same(input.DelayResult, input.Candidate.DelayResult);
        Assert.Same(input.RecoveryEvent, input.Candidate.RecoveryEvent);
        Assert.Same(input.DelayEvent, input.Candidate.DelayResult.Candidate.WorldEvent);
        Assert.Same(input.DelayResult.EventLog, input.EventLog);
        Assert.Same(input.DelayResult.WorldTime, input.WorldTime);
    }

    [Fact]
    public void PolicyAllowsExactCurrentAuthoritativeUnrecordedAdvancingRecovery()
    {
        RecoveryInput input = Input();

        Assert.True(CharacterTargetSabotageDelayRecoveryPolicy.CanRecover(
            input.WorldEvents,
            input.EventLog,
            input.WorldTime,
            input.Candidate));
        Assert.Collection(
            input.EventLog.Entries,
            entry => Assert.Same(input.PriorEvent.EventId, entry),
            entry => Assert.Same(input.DelayEvent.EventId, entry));
        Assert.Same(input.DelayEvent.EventId, input.WorldTime.CurrentEventId);
        Assert.Same(input.RecoveryEvent, input.WorldEvents.Entries[^1]);
    }

    [Fact]
    public void PolicyRejectsStaleAndRebuiltCurrentState()
    {
        RecoveryInput input = Input();
        WorldEventLogSnapshot rebuiltLog = new(input.EventLog.Entries);
        WorldTimeSnapshot rebuiltTime = new(input.WorldTime.CurrentEventId);
        WorldEventLogSnapshot staleLog = new([input.PriorEvent.EventId]);
        WorldTimeSnapshot staleTime = new(input.PriorEvent.EventId);

        Assert.False(CanRecover(input, eventLog: rebuiltLog));
        Assert.False(CanRecover(input, worldTime: rebuiltTime));
        Assert.False(CanRecover(input, eventLog: staleLog, worldTime: staleTime));
        Assert.NotSame(input.EventLog, rebuiltLog);
        Assert.NotSame(input.WorldTime, rebuiltTime);
        Assert.Equal(input.EventLog.Entries, rebuiltLog.Entries);
        Assert.Equal(input.WorldTime, rebuiltTime);
        Assert.Same(input.DelayResult.EventLog, input.EventLog);
        Assert.Same(input.DelayResult.WorldTime, input.WorldTime);
    }

    [Fact]
    public void PolicyRejectsResultWhoseExactDelayEventIsNotCurrentAndAtLogTail()
    {
        RecoveryInput input = Input();
        WorldEventLogSnapshot wrongTailLog = new(
            [input.DelayEvent.EventId, input.PriorEvent.EventId]);
        CharacterTargetSabotageDelayResult wrongTailResult = new(
            input.DelayResult.Candidate,
            wrongTailLog,
            input.WorldTime);
        CharacterTargetSabotageDelayRecoveryCandidate wrongTailCandidate = new(
            wrongTailResult,
            input.RecoveryEvent);
        WorldTimeSnapshot wrongTime = new(input.PriorEvent.EventId);
        CharacterTargetSabotageDelayResult wrongTimeResult = new(
            input.DelayResult.Candidate,
            input.EventLog,
            wrongTime);
        CharacterTargetSabotageDelayRecoveryCandidate wrongTimeCandidate = new(
            wrongTimeResult,
            input.RecoveryEvent);

        Assert.False(CharacterTargetSabotageDelayRecoveryPolicy.CanRecover(
            input.WorldEvents,
            wrongTailLog,
            input.WorldTime,
            wrongTailCandidate));
        Assert.False(CharacterTargetSabotageDelayRecoveryPolicy.CanRecover(
            input.WorldEvents,
            input.EventLog,
            wrongTime,
            wrongTimeCandidate));
        Assert.Same(input.PriorEvent.EventId, wrongTailLog.Entries[^1]);
        Assert.Same(input.PriorEvent.EventId, wrongTime.CurrentEventId);
        Assert.Same(input.DelayEvent, input.DelayResult.Candidate.WorldEvent);
    }

    [Fact]
    public void PolicyRejectsInfluenceResultAndRebuiltOrMissingRecoveryEvent()
    {
        RecoveryInput input = Input();
        RecoveryInput influence = Input(CharacterSabotageActionKind.Influence);
        WorldEvent rebuiltRecovery = new(
            new WorldEventId(input.RecoveryEvent.EventId.Value),
            input.RecoveryEvent.TimeEffect);
        CharacterTargetSabotageDelayRecoveryCandidate rebuiltCandidate = new(
            input.DelayResult,
            rebuiltRecovery);
        WorldEventSnapshot missingRecovery = new([input.PriorEvent, input.DelayEvent]);

        Assert.False(CanRecover(influence));
        Assert.False(CharacterTargetSabotageDelayRecoveryPolicy.CanRecover(
            input.WorldEvents,
            input.EventLog,
            input.WorldTime,
            rebuiltCandidate));
        Assert.False(CanRecover(input, worldEvents: missingRecovery));
        Assert.NotSame(input.RecoveryEvent, rebuiltRecovery);
        Assert.Equal(input.RecoveryEvent, rebuiltRecovery);
        Assert.Same(rebuiltRecovery, rebuiltCandidate.RecoveryEvent);
        Assert.DoesNotContain(input.RecoveryEvent, missingRecovery.Entries);
    }

    [Fact]
    public void PolicyRejectsRebuiltOrNonAdvancingDelayDefinition()
    {
        RecoveryInput input = Input();
        WorldEvent rebuiltDelay = new(
            new WorldEventId(input.DelayEvent.EventId.Value),
            input.DelayEvent.TimeEffect);
        WorldEventSnapshot rebuiltDefinitions = new(
            [input.PriorEvent, rebuiltDelay, input.RecoveryEvent]);
        WorldEvent nonAdvancingDelay = new(
            new WorldEventId("enemy-agent-did-not-delay-sage"),
            WorldEventTimeEffect.NoAdvance);
        CharacterTargetSabotageDelayResult nonAdvancingResult = new(
            new CharacterTargetSabotageDelayCandidate(
                input.DelayResult.Candidate.Occurrence,
                nonAdvancingDelay),
            new WorldEventLogSnapshot([input.PriorEvent.EventId, nonAdvancingDelay.EventId]),
            new WorldTimeSnapshot(nonAdvancingDelay.EventId));
        CharacterTargetSabotageDelayRecoveryCandidate nonAdvancingCandidate = new(
            nonAdvancingResult,
            input.RecoveryEvent);
        WorldEventSnapshot nonAdvancingDefinitions = new(
            [input.PriorEvent, nonAdvancingDelay, input.RecoveryEvent]);

        Assert.False(CanRecover(input, worldEvents: rebuiltDefinitions));
        Assert.False(CharacterTargetSabotageDelayRecoveryPolicy.CanRecover(
            nonAdvancingDefinitions,
            nonAdvancingResult.EventLog,
            nonAdvancingResult.WorldTime,
            nonAdvancingCandidate));
        Assert.NotSame(input.DelayEvent, rebuiltDelay);
        Assert.Equal(input.DelayEvent, rebuiltDelay);
        Assert.Same(WorldEventTimeEffect.NoAdvance, nonAdvancingDelay.TimeEffect);
    }

    [Fact]
    public void PolicyRejectsRecordedOrNonAdvancingRecoveryEvent()
    {
        RecoveryInput recorded = Input(recoveryAlreadyRecorded: true);
        RecoveryInput noAdvance = Input(recoveryTimeEffect: WorldEventTimeEffect.NoAdvance);

        Assert.False(CanRecover(recorded));
        Assert.False(CanRecover(noAdvance));
        Assert.Same(recorded.RecoveryEvent.EventId, recorded.EventLog.Entries[0]);
        Assert.Same(recorded.DelayEvent.EventId, recorded.EventLog.Entries[^1]);
        Assert.Same(noAdvance.RecoveryEvent, noAdvance.WorldEvents.Entries[^1]);
        Assert.Same(WorldEventTimeEffect.NoAdvance, noAdvance.RecoveryEvent.TimeEffect);
    }

    [Fact]
    public void ContractsRejectNullsAndExposeOnlyTheBoundedPureSurface()
    {
        RecoveryInput input = Input();

        Assert.Throws<ArgumentNullException>(() =>
            new CharacterTargetSabotageDelayRecoveryCandidate(null!, input.RecoveryEvent));
        Assert.Throws<ArgumentNullException>(() =>
            new CharacterTargetSabotageDelayRecoveryCandidate(input.DelayResult, null!));
        Assert.Throws<ArgumentNullException>(() =>
            CharacterTargetSabotageDelayRecoveryPolicy.CanRecover(
                null!, input.EventLog, input.WorldTime, input.Candidate));
        Assert.Throws<ArgumentNullException>(() =>
            CharacterTargetSabotageDelayRecoveryPolicy.CanRecover(
                input.WorldEvents, null!, input.WorldTime, input.Candidate));
        Assert.Throws<ArgumentNullException>(() =>
            CharacterTargetSabotageDelayRecoveryPolicy.CanRecover(
                input.WorldEvents, input.EventLog, null!, input.Candidate));
        Assert.Throws<ArgumentNullException>(() =>
            CharacterTargetSabotageDelayRecoveryPolicy.CanRecover(
                input.WorldEvents, input.EventLog, input.WorldTime, null!));

        Type candidateType = typeof(CharacterTargetSabotageDelayRecoveryCandidate);
        (string Name, Type Type)[] expectedProperties =
        [
            ("DelayResult", typeof(CharacterTargetSabotageDelayResult)),
            ("RecoveryEvent", typeof(WorldEvent)),
        ];
        System.Reflection.PropertyInfo[] properties = candidateType.GetProperties()
            .OrderBy(property => property.Name, StringComparer.Ordinal)
            .ToArray();
        Assert.Equal(
            expectedProperties,
            properties.Select(property => (property.Name, property.PropertyType)).ToArray());
        Assert.All(properties, property => Assert.False(property.CanWrite));
        Assert.Empty(candidateType.GetMethods().Where(method =>
            method.DeclaringType == candidateType && !method.IsSpecialName));
        Assert.Empty(candidateType.GetEvents());
        Assert.True(candidateType.IsSealed);

        Type policyType = typeof(CharacterTargetSabotageDelayRecoveryPolicy);
        System.Reflection.MethodInfo method = Assert.Single(policyType.GetMethods().Where(
            candidateMethod => candidateMethod.DeclaringType == policyType));
        Assert.Equal("CanRecover", method.Name);
        Assert.Equal(typeof(bool), method.ReturnType);
        Assert.Equal(
            [
                typeof(WorldEventSnapshot),
                typeof(WorldEventLogSnapshot),
                typeof(WorldTimeSnapshot),
                typeof(CharacterTargetSabotageDelayRecoveryCandidate),
            ],
            method.GetParameters().Select(parameter => parameter.ParameterType));
        Assert.Empty(policyType.GetConstructors());
        Assert.True(policyType.IsAbstract && policyType.IsSealed);
    }

    private static bool CanRecover(
        RecoveryInput input,
        WorldEventSnapshot? worldEvents = null,
        WorldEventLogSnapshot? eventLog = null,
        WorldTimeSnapshot? worldTime = null) =>
        CharacterTargetSabotageDelayRecoveryPolicy.CanRecover(
            worldEvents ?? input.WorldEvents,
            eventLog ?? input.EventLog,
            worldTime ?? input.WorldTime,
            input.Candidate);

    private static RecoveryInput Input(
        CharacterSabotageActionKind? actionKind = null,
        WorldEventTimeEffect? recoveryTimeEffect = null,
        bool recoveryAlreadyRecorded = false)
    {
        Character actor = RepresentativeEnemyAgentCharacterSource.Create();
        Character target = RepresentativeSageCharacterSource.Create();
        CharacterTargetSabotageOccurrence occurrence = new(
            new CharacterTargetSabotageProposal(
                actor.Identity.Id,
                new CharacterTargetAccessRequirement(
                    target.Identity.Id,
                    "assess-local-influence",
                    actor.Knowledge.KnowledgeReferences.Single()),
                actionKind ?? CharacterSabotageActionKind.Delay),
            target);
        WorldEvent priorEvent = new(
            new WorldEventId("sage-began-round"),
            WorldEventTimeEffect.NoAdvance);
        WorldEvent delayEvent = new(
            new WorldEventId("enemy-agent-delayed-sage"),
            WorldEventTimeEffect.Advance);
        WorldEvent recoveryEvent = new(
            new WorldEventId("sage-delay-recovered"),
            recoveryTimeEffect ?? WorldEventTimeEffect.Advance);
        CharacterTargetSabotageDelayCandidate delayCandidate = new(occurrence, delayEvent);
        WorldEventLogSnapshot eventLog = new(
            recoveryAlreadyRecorded
                ? [recoveryEvent.EventId, priorEvent.EventId, delayEvent.EventId]
                : [priorEvent.EventId, delayEvent.EventId]);
        WorldTimeSnapshot worldTime = new(delayEvent.EventId);
        CharacterTargetSabotageDelayResult delayResult = new(
            delayCandidate,
            eventLog,
            worldTime);
        WorldEventSnapshot worldEvents = new([priorEvent, delayEvent, recoveryEvent]);

        return new RecoveryInput(
            priorEvent,
            delayEvent,
            recoveryEvent,
            worldEvents,
            eventLog,
            worldTime,
            delayResult,
            new CharacterTargetSabotageDelayRecoveryCandidate(delayResult, recoveryEvent));
    }

    private sealed record RecoveryInput(
        WorldEvent PriorEvent,
        WorldEvent DelayEvent,
        WorldEvent RecoveryEvent,
        WorldEventSnapshot WorldEvents,
        WorldEventLogSnapshot EventLog,
        WorldTimeSnapshot WorldTime,
        CharacterTargetSabotageDelayResult DelayResult,
        CharacterTargetSabotageDelayRecoveryCandidate Candidate);
}
