using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Identifiers;
using Allrpcs.Domain.WorldEvents;

namespace Allrpcs.Domain.Tests.Characters;

public sealed class CharacterTargetSabotageDelayDecisionTests
{
    [Fact]
    public void DecisionRetainsExactSelectionOrAbstention()
    {
        DecisionInput input = Input();

        CharacterTargetSabotageDelayDecision selection = new(
            input.Actor.Identity.Id,
            input.Candidate);
        CharacterTargetSabotageDelayDecision abstention = new(
            input.Actor.Identity.Id,
            null);

        Assert.Same(input.Actor.Identity.Id, selection.ActorId);
        Assert.Same(input.Candidate, selection.SelectedCandidate);
        Assert.Same(input.Actor.Identity.Id, abstention.ActorId);
        Assert.Null(abstention.SelectedCandidate);
    }

    [Fact]
    public void PolicyAcceptsExactCurrentEligibleSelectionAndAbstention()
    {
        DecisionInput input = Input();
        CharacterTargetSabotageDelayDecision selection = new(
            input.Actor.Identity.Id,
            input.Candidate);
        CharacterTargetSabotageDelayDecision abstention = new(
            input.Actor.Identity.Id,
            null);

        Assert.True(CharacterTargetSabotageDelayDecisionPolicy.CanAccept(
            input.Request,
            selection));
        Assert.True(CharacterTargetSabotageDelayDecisionPolicy.CanAccept(
            input.Request,
            abstention));
        Assert.Same(input.Candidate, Assert.Single(input.Request.Candidates));
        Assert.Empty(input.EventLog.Entries);
        Assert.Null(input.WorldTime.CurrentEventId);
    }

    [Fact]
    public void PolicyRejectsWrongActorForeignEquivalentAndIneligibleSelections()
    {
        DecisionInput input = Input();
        CharacterTargetSabotageDelayCandidate equivalent = new(
            input.Occurrence,
            input.WorldEvent);
        CharacterTargetSabotageOccurrence influenceOccurrence = Occurrence(
            input.Actor,
            input.Target,
            CharacterSabotageActionKind.Influence);
        WorldEvent ineligibleEvent = Event("enemy-agent-influenced-sage");
        CharacterTargetSabotageDelayCandidate ineligible = new(
            influenceOccurrence,
            ineligibleEvent);
        CharacterTargetSabotageDelayConsiderationRequest requestWithIneligible = Request(
            input,
            new WorldEventSnapshot([ineligibleEvent]),
            [ineligible]);
        CharacterTargetSabotageDelayConsiderationRequest staleRequest = new(
            input.Actor,
            input.InteractionAccess,
            input.WorldEvents,
            new WorldEventLogSnapshot([input.WorldEvent.EventId]),
            input.WorldTime,
            [input.Candidate]);

        Assert.False(CharacterTargetSabotageDelayDecisionPolicy.CanAccept(
            input.Request,
            new CharacterTargetSabotageDelayDecision(
                new CharacterId(input.Actor.Identity.Id.Value),
                input.Candidate)));
        Assert.False(CharacterTargetSabotageDelayDecisionPolicy.CanAccept(
            input.Request,
            new CharacterTargetSabotageDelayDecision(
                input.Actor.Identity.Id,
                equivalent)));
        Assert.False(CharacterTargetSabotageDelayDecisionPolicy.CanAccept(
            requestWithIneligible,
            new CharacterTargetSabotageDelayDecision(
                input.Actor.Identity.Id,
                ineligible)));
        Assert.False(CharacterTargetSabotageDelayDecisionPolicy.CanAccept(
            staleRequest,
            new CharacterTargetSabotageDelayDecision(
                input.Actor.Identity.Id,
                input.Candidate)));
        Assert.NotSame(input.Candidate, equivalent);
        Assert.Same(input.Occurrence, equivalent.Occurrence);
        Assert.Same(input.WorldEvent, equivalent.WorldEvent);
        Assert.Empty(input.EventLog.Entries);
        Assert.Null(input.WorldTime.CurrentEventId);
    }

    [Fact]
    public void ResultRetainsExactAcceptedRequestDecisionAndSelection()
    {
        DecisionInput input = Input();
        CharacterTargetSabotageDelayDecision decision = new(
            input.Actor.Identity.Id,
            input.Candidate);

        CharacterTargetSabotageDelayConsiderationResult result = new(
            input.Request,
            decision);

        Assert.Same(input.Request, result.Request);
        Assert.Same(decision, result.Decision);
        Assert.Same(input.Candidate, result.SelectedCandidate);
        Assert.Same(input.EventLog, result.Request.EventLog);
        Assert.Same(input.WorldTime, result.Request.WorldTime);
    }

    [Fact]
    public void ResultRetainsAcceptedAbstentionWithoutSelection()
    {
        DecisionInput input = Input();
        CharacterTargetSabotageDelayDecision decision = new(
            input.Actor.Identity.Id,
            null);

        CharacterTargetSabotageDelayConsiderationResult result = new(
            input.Request,
            decision);

        Assert.Same(input.Request, result.Request);
        Assert.Same(decision, result.Decision);
        Assert.Null(result.SelectedCandidate);
        Assert.Same(input.Candidate, Assert.Single(result.Request.Candidates));
    }

    [Fact]
    public void ResultRejectsInvalidDecisionWithoutChangingRequestState()
    {
        DecisionInput input = Input();
        CharacterTargetSabotageDelayCandidate equivalent = new(
            input.Occurrence,
            input.WorldEvent);
        CharacterTargetSabotageDelayDecision invalid = new(
            input.Actor.Identity.Id,
            equivalent);

        Assert.Throws<ArgumentException>(() =>
            new CharacterTargetSabotageDelayConsiderationResult(input.Request, invalid));
        Assert.Same(input.Candidate, Assert.Single(input.Request.Candidates));
        Assert.Empty(input.EventLog.Entries);
        Assert.Null(input.WorldTime.CurrentEventId);
    }

    [Fact]
    public void ContractsRejectNullsAndExposeOnlyClosedReadOnlySurfaces()
    {
        DecisionInput input = Input();
        CharacterTargetSabotageDelayDecision decision = new(
            input.Actor.Identity.Id,
            input.Candidate);

        Assert.Throws<ArgumentNullException>(() =>
            new CharacterTargetSabotageDelayDecision(null!, input.Candidate));
        Assert.Throws<ArgumentNullException>(() =>
            CharacterTargetSabotageDelayDecisionPolicy.CanAccept(null!, decision));
        Assert.Throws<ArgumentNullException>(() =>
            CharacterTargetSabotageDelayDecisionPolicy.CanAccept(input.Request, null!));
        Assert.Throws<ArgumentNullException>(() =>
            new CharacterTargetSabotageDelayConsiderationResult(null!, decision));
        Assert.Throws<ArgumentNullException>(() =>
            new CharacterTargetSabotageDelayConsiderationResult(input.Request, null!));

        AssertReadOnlySurface(
            typeof(CharacterTargetSabotageDelayDecision),
            [
                ("ActorId", typeof(CharacterId)),
                ("SelectedCandidate", typeof(CharacterTargetSabotageDelayCandidate)),
            ]);
        AssertReadOnlySurface(
            typeof(CharacterTargetSabotageDelayConsiderationResult),
            [
                ("Decision", typeof(CharacterTargetSabotageDelayDecision)),
                ("Request", typeof(CharacterTargetSabotageDelayConsiderationRequest)),
                ("SelectedCandidate", typeof(CharacterTargetSabotageDelayCandidate)),
            ]);

        Type policyType = typeof(CharacterTargetSabotageDelayDecisionPolicy);
        System.Reflection.MethodInfo method = Assert.Single(policyType.GetMethods().Where(
            candidate => candidate.DeclaringType == policyType));
        Assert.Equal("CanAccept", method.Name);
        Assert.Equal(typeof(bool), method.ReturnType);
        Assert.Equal(
            [
                typeof(CharacterTargetSabotageDelayConsiderationRequest),
                typeof(CharacterTargetSabotageDelayDecision),
            ],
            method.GetParameters().Select(parameter => parameter.ParameterType));
        Assert.Empty(policyType.GetConstructors());
        Assert.True(policyType.IsAbstract && policyType.IsSealed);
    }

    private static void AssertReadOnlySurface(
        Type type,
        IReadOnlyList<(string Name, Type Type)> expectedProperties)
    {
        System.Reflection.PropertyInfo[] properties = type.GetProperties()
            .OrderBy(property => property.Name, StringComparer.Ordinal)
            .ToArray();

        Assert.Equal(
            expectedProperties,
            properties.Select(property => (property.Name, property.PropertyType)).ToArray());
        Assert.All(properties, property => Assert.False(property.CanWrite));
        Assert.Single(type.GetConstructors());
        Assert.Empty(type.GetEvents());
        Assert.True(type.IsSealed);
    }

    private static DecisionInput Input()
    {
        Character actor = RepresentativeEnemyAgentCharacterSource.Create();
        Character target = RepresentativeSageCharacterSource.Create();
        CharacterTargetSabotageOccurrence occurrence = Occurrence(actor, target);
        WorldEvent worldEvent = Event("enemy-agent-delayed-sage");
        CharacterTargetSabotageDelayCandidate candidate = new(occurrence, worldEvent);
        WorldEventSnapshot worldEvents = new([worldEvent]);
        WorldEventLogSnapshot eventLog = new([]);
        WorldTimeSnapshot worldTime = new(null);
        CharacterInteractionAccessSnapshot interactionAccess = new(
            actor.Identity.Id,
            [target.Identity.Id]);
        CharacterTargetSabotageDelayConsiderationRequest request = new(
            actor,
            interactionAccess,
            worldEvents,
            eventLog,
            worldTime,
            [candidate]);

        return new(
            actor,
            target,
            interactionAccess,
            worldEvents,
            eventLog,
            worldTime,
            occurrence,
            worldEvent,
            candidate,
            request);
    }

    private static CharacterTargetSabotageDelayConsiderationRequest Request(
        DecisionInput input,
        WorldEventSnapshot worldEvents,
        IEnumerable<CharacterTargetSabotageDelayCandidate> candidates) =>
        new(
            input.Actor,
            input.InteractionAccess,
            worldEvents,
            input.EventLog,
            input.WorldTime,
            candidates);

    private static CharacterTargetSabotageOccurrence Occurrence(
        Character actor,
        Character target,
        CharacterSabotageActionKind? actionKind = null) =>
        new(
            new CharacterTargetSabotageProposal(
                actor.Identity.Id,
                new CharacterTargetAccessRequirement(
                    target.Identity.Id,
                    "assess-local-influence",
                    actor.Knowledge.KnowledgeReferences.Single()),
                actionKind ?? CharacterSabotageActionKind.Delay),
            target);

    private static WorldEvent Event(string value) =>
        new(new WorldEventId(value), WorldEventTimeEffect.Advance);

    private sealed record DecisionInput(
        Character Actor,
        Character Target,
        CharacterInteractionAccessSnapshot InteractionAccess,
        WorldEventSnapshot WorldEvents,
        WorldEventLogSnapshot EventLog,
        WorldTimeSnapshot WorldTime,
        CharacterTargetSabotageOccurrence Occurrence,
        WorldEvent WorldEvent,
        CharacterTargetSabotageDelayCandidate Candidate,
        CharacterTargetSabotageDelayConsiderationRequest Request);
}
