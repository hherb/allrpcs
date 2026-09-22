using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Identifiers;
using Allrpcs.Domain.WorldEvents;

namespace Allrpcs.Infrastructure.Tests.Characters;

internal static class CharacterTargetSabotageDelayRecoveryDecisionJsonTestInput
{
    internal static Input Create(int candidateCount = 1)
    {
        Character enemy = RepresentativeEnemyAgentCharacterSource.Create();
        Character actor = RepresentativeSageCharacterSource.Create();
        CharacterTargetSabotageOccurrence occurrence = new(
            new CharacterTargetSabotageProposal(
                enemy.Identity.Id,
                new CharacterTargetAccessRequirement(
                    actor.Identity.Id,
                    "assess-local-influence",
                    enemy.Knowledge.KnowledgeReferences.Single()),
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
        CharacterTargetSabotageDelayRecoveryCandidate[] candidates = Enumerable
            .Range(0, candidateCount)
            .Select(_ => new CharacterTargetSabotageDelayRecoveryCandidate(
                delayResult,
                recoveryEvent))
            .ToArray();
        WorldEventSnapshot worldEvents = new([priorEvent, delayEvent, recoveryEvent]);
        CharacterTargetSabotageDelayRecoveryConsiderationRequest request = new(
            actor,
            worldEvents,
            eventLog,
            worldTime,
            candidates);

        return new(
            actor,
            delayEvent,
            recoveryEvent,
            worldEvents,
            eventLog,
            worldTime,
            delayResult,
            candidates,
            request);
    }

    internal sealed record Input(
        Character Actor,
        WorldEvent DelayEvent,
        WorldEvent RecoveryEvent,
        WorldEventSnapshot WorldEvents,
        WorldEventLogSnapshot EventLog,
        WorldTimeSnapshot WorldTime,
        CharacterTargetSabotageDelayResult DelayResult,
        IReadOnlyList<CharacterTargetSabotageDelayRecoveryCandidate> Candidates,
        CharacterTargetSabotageDelayRecoveryConsiderationRequest Request);
}
