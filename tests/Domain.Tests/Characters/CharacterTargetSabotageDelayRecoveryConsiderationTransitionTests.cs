using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Identifiers;
using Allrpcs.Domain.WorldEvents;

namespace Allrpcs.Domain.Tests.Characters;

public sealed class CharacterTargetSabotageDelayRecoveryConsiderationTransitionTests
{
    [Fact]
    public void ApplyReturnsFreshRecoveryResultForExactAcceptedSelection()
    {
        TransitionInput input = Input();
        CharacterTargetSabotageDelayRecoveryConsiderationResult consideration =
            Result(input, input.Candidate);

        CharacterTargetSabotageDelayRecoveryResult? result =
            CharacterTargetSabotageDelayRecoveryConsiderationTransition.Apply(
                consideration);

        CharacterTargetSabotageDelayRecoveryResult recovered = Assert.IsType<
            CharacterTargetSabotageDelayRecoveryResult>(result);
        Assert.Same(input.Candidate, recovered.Candidate);
        Assert.NotSame(input.EventLog, recovered.EventLog);
        Assert.NotSame(input.WorldTime, recovered.WorldTime);
        Assert.Collection(
            recovered.EventLog.Entries,
            entry => Assert.Same(input.PriorEvent.EventId, entry),
            entry => Assert.Same(input.DelayEvent.EventId, entry),
            entry => Assert.Same(input.RecoveryEvent.EventId, entry));
        Assert.Same(input.RecoveryEvent.EventId, recovered.WorldTime.CurrentEventId);
        Assert.Equal(2, input.EventLog.Entries.Count);
        Assert.Same(input.DelayEvent.EventId, input.WorldTime.CurrentEventId);
    }

    [Fact]
    public void ApplyReturnsNullForAcceptedAbstentionWithoutChangingInputs()
    {
        TransitionInput input = Input();
        CharacterTargetSabotageDelayRecoveryConsiderationResult consideration =
            Result(input, null);

        CharacterTargetSabotageDelayRecoveryResult? result =
            CharacterTargetSabotageDelayRecoveryConsiderationTransition.Apply(
                consideration);

        Assert.Null(result);
        Assert.Same(input.Request, consideration.Request);
        Assert.Null(consideration.SelectedCandidate);
        Assert.Same(input.Candidate, Assert.Single(input.Request.Candidates));
        Assert.Equal(2, input.EventLog.Entries.Count);
        Assert.Same(input.DelayEvent.EventId, input.EventLog.Entries[^1]);
        Assert.Same(input.DelayEvent.EventId, input.WorldTime.CurrentEventId);
    }

    [Fact]
    public void RejectsNullAndExposesOnlyOneClosedPureOperation()
    {
        Assert.Throws<ArgumentNullException>(() =>
            CharacterTargetSabotageDelayRecoveryConsiderationTransition.Apply(null!));

        Type transitionType = typeof(
            CharacterTargetSabotageDelayRecoveryConsiderationTransition);
        System.Reflection.MethodInfo method = Assert.Single(transitionType.GetMethods().Where(
            candidate => candidate.DeclaringType == transitionType));
        Assert.Equal("Apply", method.Name);
        Assert.Equal(typeof(CharacterTargetSabotageDelayRecoveryResult), method.ReturnType);
        Assert.Equal(
            [typeof(CharacterTargetSabotageDelayRecoveryConsiderationResult)],
            method.GetParameters().Select(parameter => parameter.ParameterType));
        Assert.Empty(transitionType.GetConstructors());
        Assert.True(transitionType.IsAbstract && transitionType.IsSealed);
    }

    private static CharacterTargetSabotageDelayRecoveryConsiderationResult Result(
        TransitionInput input,
        CharacterTargetSabotageDelayRecoveryCandidate? selectedCandidate) =>
        new(
            input.Request,
            new CharacterTargetSabotageDelayRecoveryDecision(
                input.Actor.Identity.Id,
                selectedCandidate));

    private static TransitionInput Input()
    {
        Character enemyAgent = RepresentativeEnemyAgentCharacterSource.Create();
        Character actor = RepresentativeSageCharacterSource.Create();
        CharacterTargetSabotageOccurrence occurrence = new(
            new CharacterTargetSabotageProposal(
                enemyAgent.Identity.Id,
                new CharacterTargetAccessRequirement(
                    actor.Identity.Id,
                    "assess-local-influence",
                    enemyAgent.Knowledge.KnowledgeReferences.Single()),
                CharacterSabotageActionKind.Delay),
            actor);
        WorldEvent priorEvent = new(
            new WorldEventId("sage-began-round"),
            WorldEventTimeEffect.NoAdvance);
        WorldEvent delayEvent = new(
            new WorldEventId("enemy-agent-delayed-sage"),
            WorldEventTimeEffect.Advance);
        WorldEvent recoveryEvent = new(
            new WorldEventId("sage-delay-recovered"),
            WorldEventTimeEffect.Advance);
        WorldEventLogSnapshot eventLog = new([priorEvent.EventId, delayEvent.EventId]);
        WorldTimeSnapshot worldTime = new(delayEvent.EventId);
        CharacterTargetSabotageDelayResult delayResult = new(
            new CharacterTargetSabotageDelayCandidate(occurrence, delayEvent),
            eventLog,
            worldTime);
        CharacterTargetSabotageDelayRecoveryCandidate candidate = new(
            delayResult,
            recoveryEvent);
        WorldEventSnapshot worldEvents = new([priorEvent, delayEvent, recoveryEvent]);
        CharacterTargetSabotageDelayRecoveryConsiderationRequest request = new(
            actor,
            worldEvents,
            eventLog,
            worldTime,
            [candidate]);

        return new TransitionInput(
            actor,
            priorEvent,
            delayEvent,
            recoveryEvent,
            eventLog,
            worldTime,
            candidate,
            request);
    }

    private sealed record TransitionInput(
        Character Actor,
        WorldEvent PriorEvent,
        WorldEvent DelayEvent,
        WorldEvent RecoveryEvent,
        WorldEventLogSnapshot EventLog,
        WorldTimeSnapshot WorldTime,
        CharacterTargetSabotageDelayRecoveryCandidate Candidate,
        CharacterTargetSabotageDelayRecoveryConsiderationRequest Request);
}
