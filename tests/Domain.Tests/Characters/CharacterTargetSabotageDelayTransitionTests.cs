using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Identifiers;
using Allrpcs.Domain.WorldEvents;

namespace Allrpcs.Domain.Tests.Characters;

public sealed class CharacterTargetSabotageDelayTransitionTests
{
    [Fact]
    public void ApplyReturnsExactCandidateAndFreshWorldState()
    {
        DelayInput input = Input();
        WorldEventId priorEventId = input.EventLog.Entries.Single();

        CharacterTargetSabotageDelayResult result =
            CharacterTargetSabotageDelayTransition.Apply(
                input.Actor,
                input.InteractionAccess,
                input.WorldEvents,
                input.EventLog,
                input.WorldTime,
                input.Candidate);

        Assert.Same(input.Candidate, result.Candidate);
        Assert.NotSame(input.EventLog, result.EventLog);
        Assert.NotSame(input.WorldTime, result.WorldTime);
        Assert.Collection(
            result.EventLog.Entries,
            entry => Assert.Same(priorEventId, entry),
            entry => Assert.Same(input.Candidate.WorldEvent.EventId, entry));
        Assert.Same(input.Candidate.WorldEvent.EventId, result.WorldTime.CurrentEventId);
        Assert.Collection(input.EventLog.Entries, entry => Assert.Same(priorEventId, entry));
        Assert.Null(input.WorldTime.CurrentEventId);
        Assert.Same(input.Candidate.WorldEvent, input.WorldEvents.Entries[^1]);
    }

    [Fact]
    public void ApplyRejectsIneligibleCandidateWithoutChangingInputs()
    {
        DelayInput input = Input(CharacterSabotageActionKind.Influence);
        CharacterTargetSabotageOccurrence occurrence = input.Candidate.Occurrence;
        WorldEvent worldEvent = input.Candidate.WorldEvent;
        WorldEventId priorEventId = input.EventLog.Entries.Single();

        Assert.Throws<InvalidOperationException>(() =>
            CharacterTargetSabotageDelayTransition.Apply(
                input.Actor,
                input.InteractionAccess,
                input.WorldEvents,
                input.EventLog,
                input.WorldTime,
                input.Candidate));

        Assert.Collection(input.EventLog.Entries, entry => Assert.Same(priorEventId, entry));
        Assert.Null(input.WorldTime.CurrentEventId);
        Assert.Same(occurrence, input.Candidate.Occurrence);
        Assert.Same(worldEvent, input.Candidate.WorldEvent);
    }

    [Fact]
    public void ContractsRejectNullsAndExposeOnlyClosedReadOnlyOperations()
    {
        DelayInput input = Input();
        CharacterTargetSabotageDelayResult result =
            CharacterTargetSabotageDelayTransition.Apply(
                input.Actor,
                input.InteractionAccess,
                input.WorldEvents,
                input.EventLog,
                input.WorldTime,
                input.Candidate);

        Assert.Throws<ArgumentNullException>(() =>
            new CharacterTargetSabotageDelayResult(
                null!, result.EventLog, result.WorldTime));
        Assert.Throws<ArgumentNullException>(() =>
            new CharacterTargetSabotageDelayResult(
                input.Candidate, null!, result.WorldTime));
        Assert.Throws<ArgumentNullException>(() =>
            new CharacterTargetSabotageDelayResult(
                input.Candidate, result.EventLog, null!));
        Assert.Throws<ArgumentNullException>(() => Apply(input with { Actor = null! }));
        Assert.Throws<ArgumentNullException>(() =>
            Apply(input with { InteractionAccess = null! }));
        Assert.Throws<ArgumentNullException>(() => Apply(input with { WorldEvents = null! }));
        Assert.Throws<ArgumentNullException>(() => Apply(input with { EventLog = null! }));
        Assert.Throws<ArgumentNullException>(() => Apply(input with { WorldTime = null! }));
        Assert.Throws<ArgumentNullException>(() => Apply(input with { Candidate = null! }));

        Type resultType = typeof(CharacterTargetSabotageDelayResult);
        (string Name, Type Type)[] expectedProperties =
        [
            ("Candidate", typeof(CharacterTargetSabotageDelayCandidate)),
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

        Type transitionType = typeof(CharacterTargetSabotageDelayTransition);
        System.Reflection.MethodInfo method = Assert.Single(transitionType.GetMethods().Where(
            candidateMethod => candidateMethod.DeclaringType == transitionType));
        Assert.Equal("Apply", method.Name);
        Assert.Equal(typeof(CharacterTargetSabotageDelayResult), method.ReturnType);
        Assert.Equal(
            [
                typeof(Character),
                typeof(CharacterInteractionAccessSnapshot),
                typeof(WorldEventSnapshot),
                typeof(WorldEventLogSnapshot),
                typeof(WorldTimeSnapshot),
                typeof(CharacterTargetSabotageDelayCandidate),
            ],
            method.GetParameters().Select(parameter => parameter.ParameterType));
        Assert.Empty(transitionType.GetConstructors());
        Assert.True(transitionType.IsAbstract && transitionType.IsSealed);
    }

    private static CharacterTargetSabotageDelayResult Apply(DelayInput input) =>
        CharacterTargetSabotageDelayTransition.Apply(
            input.Actor,
            input.InteractionAccess,
            input.WorldEvents,
            input.EventLog,
            input.WorldTime,
            input.Candidate);

    private static DelayInput Input(CharacterSabotageActionKind? actionKind = null)
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
        CharacterTargetSabotageDelayCandidate candidate = new(occurrence, delayEvent);

        return new DelayInput(
            actor,
            new CharacterInteractionAccessSnapshot(actor.Identity.Id, [target.Identity.Id]),
            new WorldEventSnapshot([priorEvent, delayEvent]),
            new WorldEventLogSnapshot([priorEvent.EventId]),
            new WorldTimeSnapshot(null),
            candidate);
    }

    private sealed record DelayInput(
        Character Actor,
        CharacterInteractionAccessSnapshot InteractionAccess,
        WorldEventSnapshot WorldEvents,
        WorldEventLogSnapshot EventLog,
        WorldTimeSnapshot WorldTime,
        CharacterTargetSabotageDelayCandidate Candidate);
}
