using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Identifiers;
using Allrpcs.Domain.WorldEvents;

namespace Allrpcs.Domain.Tests.Characters;

public sealed class CharacterTargetSabotageDelayCandidateTests
{
    [Fact]
    public void CandidateRetainsExactOccurrenceAndWorldEvent()
    {
        CharacterTargetSabotageOccurrence occurrence = Occurrence();
        WorldEvent worldEvent = Event();

        CharacterTargetSabotageDelayCandidate candidate = new(occurrence, worldEvent);

        Assert.Same(occurrence, candidate.Occurrence);
        Assert.Same(worldEvent, candidate.WorldEvent);
        Assert.Same(occurrence.Target, candidate.Occurrence.Target);
        Assert.Same(worldEvent.EventId, candidate.WorldEvent.EventId);
        Assert.Same(worldEvent.TimeEffect, candidate.WorldEvent.TimeEffect);
    }

    [Fact]
    public void PolicyAllowsEligibleAdvancingDelayWithoutMutation()
    {
        Character actor = RepresentativeEnemyAgentCharacterSource.Create();
        Character target = RepresentativeSageCharacterSource.Create();
        CharacterTargetSabotageOccurrence occurrence = Occurrence(actor, target);
        WorldEvent worldEvent = Event();
        CharacterTargetSabotageDelayCandidate candidate = new(occurrence, worldEvent);
        CharacterInteractionAccessSnapshot interactionAccess = Access(actor, target);
        WorldEventSnapshot worldEvents = new([worldEvent]);
        WorldEventLogSnapshot eventLog = new([]);
        WorldTimeSnapshot worldTime = new(null);

        Assert.True(CharacterTargetSabotageDelayPolicy.CanApply(
            actor,
            interactionAccess,
            worldEvents,
            eventLog,
            worldTime,
            candidate));
        Assert.Null(worldTime.CurrentEventId);
        Assert.Collection(worldEvents.Entries, entry => Assert.Same(worldEvent, entry));
        Assert.Empty(eventLog.Entries);
        Assert.Same(occurrence, candidate.Occurrence);
        Assert.Same(worldEvent, candidate.WorldEvent);
    }

    [Fact]
    public void PolicyRejectsInfluenceAndMismatchedActorOrTarget()
    {
        Character actor = RepresentativeEnemyAgentCharacterSource.Create();
        Character target = RepresentativeSageCharacterSource.Create();
        Character otherTarget = RepresentativeHermitCharacterSource.Create()[0];
        CharacterInteractionAccessSnapshot interactionAccess = new(
            actor.Identity.Id,
            [target.Identity.Id, otherTarget.Identity.Id]);
        WorldTimeSnapshot worldTime = new(null);

        Assert.False(CanApply(
            actor,
            interactionAccess,
            worldTime,
            Candidate(Occurrence(actor, target, CharacterSabotageActionKind.Influence))));
        Assert.False(CanApply(
            otherTarget,
            new CharacterInteractionAccessSnapshot(otherTarget.Identity.Id, [target.Identity.Id]),
            worldTime,
            Candidate(Occurrence(actor, target))));
        Assert.False(CanApply(
            actor,
            interactionAccess,
            worldTime,
            Candidate(Occurrence(actor, target, requirementTargetId: otherTarget.Identity.Id))));
    }

    [Fact]
    public void PolicyRejectsInaccessibleOccurrence()
    {
        Character actor = RepresentativeEnemyAgentCharacterSource.Create();
        Character target = RepresentativeSageCharacterSource.Create();
        CharacterTargetSabotageDelayCandidate candidate = Candidate(Occurrence(actor, target));

        Assert.False(CanApply(
            actor,
            new CharacterInteractionAccessSnapshot(actor.Identity.Id, []),
            new WorldTimeSnapshot(null),
            candidate));
        Assert.Same(target, candidate.Occurrence.Target);
    }

    [Fact]
    public void PolicyRejectsNonAdvancingAndAlreadyCurrentEvents()
    {
        Character actor = RepresentativeEnemyAgentCharacterSource.Create();
        Character target = RepresentativeSageCharacterSource.Create();
        CharacterTargetSabotageOccurrence occurrence = Occurrence(actor, target);
        CharacterInteractionAccessSnapshot interactionAccess = Access(actor, target);
        WorldEvent noAdvance = Event("enemy-agent-delayed-sage", WorldEventTimeEffect.NoAdvance);
        WorldEvent current = Event();
        WorldTimeSnapshot rebuiltCurrentTime = new(
            new WorldEventId(current.EventId.Value));

        Assert.False(CanApply(
            actor,
            interactionAccess,
            new WorldTimeSnapshot(null),
            Candidate(occurrence, noAdvance)));
        Assert.False(CanApply(
            actor,
            interactionAccess,
            rebuiltCurrentTime,
            Candidate(occurrence, current)));
        Assert.Equal("enemy-agent-delayed-sage", rebuiltCurrentTime.CurrentEventId?.Value);
        Assert.Same(current, Candidate(occurrence, current).WorldEvent);
    }

    [Fact]
    public void PolicyRejectsRebuiltOrAlreadyRecordedWorldEventInput()
    {
        Character actor = RepresentativeEnemyAgentCharacterSource.Create();
        Character target = RepresentativeSageCharacterSource.Create();
        CharacterTargetSabotageOccurrence occurrence = Occurrence(actor, target);
        CharacterInteractionAccessSnapshot interactionAccess = Access(actor, target);
        WorldEvent authoritative = Event();
        WorldEvent rebuilt = Event();
        CharacterTargetSabotageDelayCandidate rebuiltCandidate = Candidate(occurrence, rebuilt);
        CharacterTargetSabotageDelayCandidate authoritativeCandidate =
            Candidate(occurrence, authoritative);

        Assert.Equal(authoritative, rebuilt);
        Assert.NotSame(authoritative, rebuilt);
        Assert.False(CanApply(
            actor,
            interactionAccess,
            new WorldTimeSnapshot(null),
            rebuiltCandidate,
            authoritative));
        Assert.False(CanApply(
            actor,
            interactionAccess,
            new WorldTimeSnapshot(null),
            authoritativeCandidate,
            eventLog: new WorldEventLogSnapshot([authoritative.EventId])));
        Assert.Same(rebuilt, rebuiltCandidate.WorldEvent);
        Assert.Same(authoritative, authoritativeCandidate.WorldEvent);
    }

    [Fact]
    public void ContractsRejectNullsAndExposeOnlyTheBoundedPureSurface()
    {
        Character actor = RepresentativeEnemyAgentCharacterSource.Create();
        Character target = RepresentativeSageCharacterSource.Create();
        CharacterTargetSabotageOccurrence occurrence = Occurrence(actor, target);
        WorldEvent worldEvent = Event();
        CharacterTargetSabotageDelayCandidate candidate = new(occurrence, worldEvent);
        CharacterInteractionAccessSnapshot interactionAccess = Access(actor, target);
        WorldEventSnapshot worldEvents = new([worldEvent]);
        WorldEventLogSnapshot eventLog = new([]);
        WorldTimeSnapshot worldTime = new(null);

        Assert.Throws<ArgumentNullException>(() =>
            new CharacterTargetSabotageDelayCandidate(null!, worldEvent));
        Assert.Throws<ArgumentNullException>(() =>
            new CharacterTargetSabotageDelayCandidate(occurrence, null!));
        Assert.Throws<ArgumentNullException>(() =>
            CharacterTargetSabotageDelayPolicy.CanApply(
                null!, interactionAccess, worldEvents, eventLog, worldTime, candidate));
        Assert.Throws<ArgumentNullException>(() =>
            CharacterTargetSabotageDelayPolicy.CanApply(
                actor, null!, worldEvents, eventLog, worldTime, candidate));
        Assert.Throws<ArgumentNullException>(() =>
            CharacterTargetSabotageDelayPolicy.CanApply(
                actor, interactionAccess, null!, eventLog, worldTime, candidate));
        Assert.Throws<ArgumentNullException>(() =>
            CharacterTargetSabotageDelayPolicy.CanApply(
                actor, interactionAccess, worldEvents, null!, worldTime, candidate));
        Assert.Throws<ArgumentNullException>(() =>
            CharacterTargetSabotageDelayPolicy.CanApply(
                actor, interactionAccess, worldEvents, eventLog, null!, candidate));
        Assert.Throws<ArgumentNullException>(() =>
            CharacterTargetSabotageDelayPolicy.CanApply(
                actor, interactionAccess, worldEvents, eventLog, worldTime, null!));

        Type candidateType = typeof(CharacterTargetSabotageDelayCandidate);
        (string Name, Type Type)[] expectedProperties =
        [
            ("Occurrence", typeof(CharacterTargetSabotageOccurrence)),
            ("WorldEvent", typeof(WorldEvent)),
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

        Type policyType = typeof(CharacterTargetSabotageDelayPolicy);
        System.Reflection.MethodInfo method = Assert.Single(policyType.GetMethods().Where(
            candidateMethod => candidateMethod.DeclaringType == policyType));
        Assert.Equal("CanApply", method.Name);
        Assert.Equal(typeof(bool), method.ReturnType);
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
        Assert.Empty(policyType.GetConstructors());
        Assert.True(policyType.IsAbstract && policyType.IsSealed);
    }

    private static bool CanApply(
        Character actor,
        CharacterInteractionAccessSnapshot interactionAccess,
        WorldTimeSnapshot worldTime,
        CharacterTargetSabotageDelayCandidate candidate,
        WorldEvent? authoritativeEvent = null,
        WorldEventLogSnapshot? eventLog = null) =>
        CharacterTargetSabotageDelayPolicy.CanApply(
            actor,
            interactionAccess,
            new WorldEventSnapshot([authoritativeEvent ?? candidate.WorldEvent]),
            eventLog ?? new WorldEventLogSnapshot([]),
            worldTime,
            candidate);

    private static CharacterTargetSabotageDelayCandidate Candidate(
        CharacterTargetSabotageOccurrence occurrence,
        WorldEvent? worldEvent = null) =>
        new(occurrence, worldEvent ?? Event());

    private static CharacterTargetSabotageOccurrence Occurrence() =>
        Occurrence(
            RepresentativeEnemyAgentCharacterSource.Create(),
            RepresentativeSageCharacterSource.Create());

    private static CharacterTargetSabotageOccurrence Occurrence(
        Character actor,
        Character target,
        CharacterSabotageActionKind? actionKind = null,
        CharacterId? requirementTargetId = null) =>
        new(
            new CharacterTargetSabotageProposal(
                actor.Identity.Id,
                new CharacterTargetAccessRequirement(
                    requirementTargetId ?? target.Identity.Id,
                    "assess-local-influence",
                    actor.Knowledge.KnowledgeReferences.Single()),
                actionKind ?? CharacterSabotageActionKind.Delay),
            target);

    private static WorldEvent Event(
        string eventId = "enemy-agent-delayed-sage",
        WorldEventTimeEffect? timeEffect = null) =>
        new(new WorldEventId(eventId), timeEffect ?? WorldEventTimeEffect.Advance);

    private static CharacterInteractionAccessSnapshot Access(Character actor, Character target) =>
        new(actor.Identity.Id, [target.Identity.Id]);
}
