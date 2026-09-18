using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Identifiers;
using Allrpcs.Domain.WorldEvents;

namespace Allrpcs.Domain.Tests.Characters;

public sealed class CharacterTargetSabotageDelayRecoveryTransitionTests
{
    [Fact]
    public void RecoverReturnsExactCandidateAndFreshWorldStateWithCompleteHistory()
    {
        RecoveryInput input = Input();

        CharacterTargetSabotageDelayRecoveryResult result =
            CharacterTargetSabotageDelayRecoveryTransition.Recover(
                input.WorldEvents,
                input.DelayResult.EventLog,
                input.DelayResult.WorldTime,
                input.Candidate);

        Assert.Same(input.Candidate, result.Candidate);
        Assert.NotSame(input.DelayResult.EventLog, result.EventLog);
        Assert.NotSame(input.DelayResult.WorldTime, result.WorldTime);
        Assert.Collection(
            result.EventLog.Entries,
            entry => Assert.Same(input.PriorEvent.EventId, entry),
            entry => Assert.Same(input.DelayEvent.EventId, entry),
            entry => Assert.Same(input.RecoveryEvent.EventId, entry));
        Assert.Same(input.RecoveryEvent.EventId, result.WorldTime.CurrentEventId);
        Assert.Collection(
            input.DelayResult.EventLog.Entries,
            entry => Assert.Same(input.PriorEvent.EventId, entry),
            entry => Assert.Same(input.DelayEvent.EventId, entry));
        Assert.Same(input.DelayEvent.EventId, input.DelayResult.WorldTime.CurrentEventId);
    }

    [Fact]
    public void RecoverRejectsRebuiltCurrentStateWithoutChangingInputs()
    {
        RecoveryInput input = Input();
        WorldEventLogSnapshot rebuiltLog = new(input.DelayResult.EventLog.Entries);
        WorldTimeSnapshot rebuiltTime = new(input.DelayResult.WorldTime.CurrentEventId);

        Assert.Throws<InvalidOperationException>(() =>
            CharacterTargetSabotageDelayRecoveryTransition.Recover(
                input.WorldEvents,
                rebuiltLog,
                rebuiltTime,
                input.Candidate));

        Assert.Equal(input.DelayResult.EventLog.Entries, rebuiltLog.Entries);
        Assert.Equal(input.DelayResult.WorldTime, rebuiltTime);
        Assert.Same(input.DelayResult, input.Candidate.DelayResult);
        Assert.Same(input.RecoveryEvent, input.Candidate.RecoveryEvent);
        Assert.Same(input.DelayEvent.EventId, input.DelayResult.EventLog.Entries[^1]);
        Assert.Same(input.DelayEvent.EventId, input.DelayResult.WorldTime.CurrentEventId);
    }

    [Fact]
    public void ContractsRejectNullsAndExposeOnlyClosedReadOnlyOperations()
    {
        RecoveryInput input = Input();
        CharacterTargetSabotageDelayRecoveryResult result =
            CharacterTargetSabotageDelayRecoveryTransition.Recover(
                input.WorldEvents,
                input.DelayResult.EventLog,
                input.DelayResult.WorldTime,
                input.Candidate);

        Assert.Throws<ArgumentNullException>(() =>
            new CharacterTargetSabotageDelayRecoveryResult(
                null!, result.EventLog, result.WorldTime));
        Assert.Throws<ArgumentNullException>(() =>
            new CharacterTargetSabotageDelayRecoveryResult(
                input.Candidate, null!, result.WorldTime));
        Assert.Throws<ArgumentNullException>(() =>
            new CharacterTargetSabotageDelayRecoveryResult(
                input.Candidate, result.EventLog, null!));
        Assert.Throws<ArgumentNullException>(() => Recover(input with { WorldEvents = null! }));
        Assert.Throws<ArgumentNullException>(() =>
            CharacterTargetSabotageDelayRecoveryTransition.Recover(
                input.WorldEvents,
                null!,
                input.DelayResult.WorldTime,
                input.Candidate));
        Assert.Throws<ArgumentNullException>(() =>
            CharacterTargetSabotageDelayRecoveryTransition.Recover(
                input.WorldEvents,
                input.DelayResult.EventLog,
                null!,
                input.Candidate));
        Assert.Throws<ArgumentNullException>(() => Recover(input with { Candidate = null! }));

        Type resultType = typeof(CharacterTargetSabotageDelayRecoveryResult);
        (string Name, Type Type)[] expectedProperties =
        [
            ("Candidate", typeof(CharacterTargetSabotageDelayRecoveryCandidate)),
            ("EventLog", typeof(WorldEventLogSnapshot)),
            ("WorldTime", typeof(WorldTimeSnapshot)),
        ];
        System.Reflection.PropertyInfo[] properties = resultType.GetProperties()
            .OrderBy(property => property.Name, StringComparer.Ordinal)
            .ToArray();
        Assert.Equal(
            expectedProperties,
            properties.Select(property => (property.Name, property.PropertyType)).ToArray());
        Assert.All(properties, property => Assert.False(property.CanWrite));
        Assert.True(resultType.IsSealed);

        Type transitionType = typeof(CharacterTargetSabotageDelayRecoveryTransition);
        System.Reflection.MethodInfo method = Assert.Single(transitionType.GetMethods().Where(
            candidateMethod => candidateMethod.DeclaringType == transitionType));
        Assert.Equal("Recover", method.Name);
        Assert.Equal(typeof(CharacterTargetSabotageDelayRecoveryResult), method.ReturnType);
        Assert.Equal(
            [
                typeof(WorldEventSnapshot),
                typeof(WorldEventLogSnapshot),
                typeof(WorldTimeSnapshot),
                typeof(CharacterTargetSabotageDelayRecoveryCandidate),
            ],
            method.GetParameters().Select(parameter => parameter.ParameterType));
        Assert.Empty(transitionType.GetConstructors());
        Assert.True(transitionType.IsAbstract && transitionType.IsSealed);
    }

    private static CharacterTargetSabotageDelayRecoveryResult Recover(
        RecoveryInput input,
        WorldEventLogSnapshot? eventLog = null,
        WorldTimeSnapshot? worldTime = null) =>
        CharacterTargetSabotageDelayRecoveryTransition.Recover(
            input.WorldEvents,
            eventLog ?? input.DelayResult.EventLog,
            worldTime ?? input.DelayResult.WorldTime,
            input.Candidate);

    private static RecoveryInput Input()
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
                CharacterSabotageActionKind.Delay),
            target);
        WorldEvent priorEvent = new(
            new WorldEventId("sage-began-round"),
            WorldEventTimeEffect.NoAdvance);
        WorldEvent delayEvent = new(
            new WorldEventId("enemy-agent-delayed-sage"),
            WorldEventTimeEffect.Advance);
        WorldEvent recoveryEvent = new(
            new WorldEventId("sage-delay-recovered"),
            WorldEventTimeEffect.Advance);
        CharacterTargetSabotageDelayCandidate delayCandidate = new(occurrence, delayEvent);
        CharacterTargetSabotageDelayResult delayResult = new(
            delayCandidate,
            new WorldEventLogSnapshot([priorEvent.EventId, delayEvent.EventId]),
            new WorldTimeSnapshot(delayEvent.EventId));
        CharacterTargetSabotageDelayRecoveryCandidate candidate = new(
            delayResult,
            recoveryEvent);

        return new RecoveryInput(
            priorEvent,
            delayEvent,
            recoveryEvent,
            new WorldEventSnapshot([priorEvent, delayEvent, recoveryEvent]),
            delayResult,
            candidate);
    }

    private sealed record RecoveryInput(
        WorldEvent PriorEvent,
        WorldEvent DelayEvent,
        WorldEvent RecoveryEvent,
        WorldEventSnapshot WorldEvents,
        CharacterTargetSabotageDelayResult DelayResult,
        CharacterTargetSabotageDelayRecoveryCandidate Candidate);
}
