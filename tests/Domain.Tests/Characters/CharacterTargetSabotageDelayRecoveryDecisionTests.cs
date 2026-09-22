using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Identifiers;
using Allrpcs.Domain.WorldEvents;

namespace Allrpcs.Domain.Tests.Characters;

public sealed class CharacterTargetSabotageDelayRecoveryDecisionTests
{
    [Fact]
    public void DecisionRetainsExactSelectionOrAbstention()
    {
        DecisionInput input = Input();

        CharacterTargetSabotageDelayRecoveryDecision selection = new(
            input.Actor.Identity.Id,
            input.Candidate);
        CharacterTargetSabotageDelayRecoveryDecision abstention = new(
            input.Actor.Identity.Id,
            null);

        Assert.Same(input.Actor.Identity.Id, selection.ActorId);
        Assert.Same(input.Candidate, selection.SelectedCandidate);
        Assert.Same(input.Actor.Identity.Id, abstention.ActorId);
        Assert.Null(abstention.SelectedCandidate);
        Assert.Same(input.DelayResult, selection.SelectedCandidate!.DelayResult);
        Assert.Same(input.RecoveryEvent, selection.SelectedCandidate.RecoveryEvent);
    }

    [Fact]
    public void PolicyAcceptsExactCurrentEligibleSelectionAndAbstention()
    {
        DecisionInput input = Input();
        CharacterTargetSabotageDelayRecoveryDecision selection = new(
            input.Actor.Identity.Id,
            input.Candidate);
        CharacterTargetSabotageDelayRecoveryDecision abstention = new(
            input.Actor.Identity.Id,
            null);

        Assert.True(CharacterTargetSabotageDelayRecoveryDecisionPolicy.CanAccept(
            input.Request,
            selection));
        Assert.True(CharacterTargetSabotageDelayRecoveryDecisionPolicy.CanAccept(
            input.Request,
            abstention));
        Assert.Same(input.Candidate, Assert.Single(input.Request.Candidates));
        Assert.Same(input.DelayEvent.EventId, input.EventLog.Entries[^1]);
        Assert.Same(input.DelayEvent.EventId, input.WorldTime.CurrentEventId);
    }

    [Fact]
    public void PolicyRejectsWrongActorOwnerForeignEquivalentAndIneligibleSelections()
    {
        DecisionInput input = Input();
        Character wrongOwner = RepresentativeHermitCharacterSource.Create()[0];
        CharacterTargetSabotageDelayRecoveryCandidate equivalent = new(
            input.DelayResult,
            input.RecoveryEvent);
        CharacterTargetSabotageDelayRecoveryConsiderationRequest staleRequest = new(
            input.Actor,
            input.WorldEvents,
            new WorldEventLogSnapshot(input.EventLog.Entries),
            input.WorldTime,
            [input.Candidate]);
        CharacterTargetSabotageDelayRecoveryConsiderationRequest wrongOwnerRequest = new(
            wrongOwner,
            input.WorldEvents,
            input.EventLog,
            input.WorldTime,
            [input.Candidate]);

        Assert.False(CharacterTargetSabotageDelayRecoveryDecisionPolicy.CanAccept(
            input.Request,
            new CharacterTargetSabotageDelayRecoveryDecision(
                new CharacterId(input.Actor.Identity.Id.Value),
                input.Candidate)));
        Assert.False(CharacterTargetSabotageDelayRecoveryDecisionPolicy.CanAccept(
            wrongOwnerRequest,
            new CharacterTargetSabotageDelayRecoveryDecision(
                wrongOwner.Identity.Id,
                input.Candidate)));
        Assert.False(CharacterTargetSabotageDelayRecoveryDecisionPolicy.CanAccept(
            input.Request,
            new CharacterTargetSabotageDelayRecoveryDecision(
                input.Actor.Identity.Id,
                equivalent)));
        Assert.False(CharacterTargetSabotageDelayRecoveryDecisionPolicy.CanAccept(
            staleRequest,
            new CharacterTargetSabotageDelayRecoveryDecision(
                input.Actor.Identity.Id,
                input.Candidate)));
        Assert.NotSame(input.Candidate, equivalent);
        Assert.NotSame(input.Actor.Identity.Id, wrongOwner.Identity.Id);
        Assert.NotSame(input.EventLog, staleRequest.EventLog);
        Assert.Equal(input.EventLog.Entries, staleRequest.EventLog.Entries);
    }

    [Fact]
    public void ResultRetainsExactAcceptedRequestDecisionAndSelection()
    {
        DecisionInput input = Input();
        CharacterTargetSabotageDelayRecoveryDecision decision = new(
            input.Actor.Identity.Id,
            input.Candidate);

        CharacterTargetSabotageDelayRecoveryConsiderationResult result = new(
            input.Request,
            decision);

        Assert.Same(input.Request, result.Request);
        Assert.Same(decision, result.Decision);
        Assert.Same(input.Candidate, result.SelectedCandidate);
        Assert.Same(input.WorldEvents, result.Request.WorldEvents);
        Assert.Same(input.EventLog, result.Request.EventLog);
        Assert.Same(input.WorldTime, result.Request.WorldTime);
    }

    [Fact]
    public void ResultRetainsAcceptedAbstentionWithoutSelection()
    {
        DecisionInput input = Input();
        CharacterTargetSabotageDelayRecoveryDecision decision = new(
            input.Actor.Identity.Id,
            null);

        CharacterTargetSabotageDelayRecoveryConsiderationResult result = new(
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
        CharacterTargetSabotageDelayRecoveryCandidate equivalent = new(
            input.DelayResult,
            input.RecoveryEvent);
        CharacterTargetSabotageDelayRecoveryDecision invalid = new(
            input.Actor.Identity.Id,
            equivalent);

        Assert.Throws<ArgumentException>(() =>
            new CharacterTargetSabotageDelayRecoveryConsiderationResult(
                input.Request,
                invalid));
        Assert.Same(input.Candidate, Assert.Single(input.Request.Candidates));
        Assert.Same(input.EventLog, input.Request.EventLog);
        Assert.Same(input.WorldTime, input.Request.WorldTime);
        Assert.Same(input.DelayEvent.EventId, input.EventLog.Entries[^1]);
    }

    [Fact]
    public void ContractsRejectNullsAndExposeOnlyClosedReadOnlySurfaces()
    {
        DecisionInput input = Input();
        CharacterTargetSabotageDelayRecoveryDecision decision = new(
            input.Actor.Identity.Id,
            input.Candidate);

        Assert.Throws<ArgumentNullException>(() =>
            new CharacterTargetSabotageDelayRecoveryDecision(null!, input.Candidate));
        Assert.Throws<ArgumentNullException>(() =>
            CharacterTargetSabotageDelayRecoveryDecisionPolicy.CanAccept(null!, decision));
        Assert.Throws<ArgumentNullException>(() =>
            CharacterTargetSabotageDelayRecoveryDecisionPolicy.CanAccept(
                input.Request,
                null!));
        Assert.Throws<ArgumentNullException>(() =>
            new CharacterTargetSabotageDelayRecoveryConsiderationResult(null!, decision));
        Assert.Throws<ArgumentNullException>(() =>
            new CharacterTargetSabotageDelayRecoveryConsiderationResult(
                input.Request,
                null!));

        AssertReadOnlySurface(
            typeof(CharacterTargetSabotageDelayRecoveryDecision),
            [
                ("ActorId", typeof(CharacterId)),
                ("SelectedCandidate", typeof(CharacterTargetSabotageDelayRecoveryCandidate)),
            ]);
        AssertReadOnlySurface(
            typeof(CharacterTargetSabotageDelayRecoveryConsiderationResult),
            [
                ("Decision", typeof(CharacterTargetSabotageDelayRecoveryDecision)),
                ("Request", typeof(CharacterTargetSabotageDelayRecoveryConsiderationRequest)),
                ("SelectedCandidate", typeof(CharacterTargetSabotageDelayRecoveryCandidate)),
            ]);

        Type policyType = typeof(CharacterTargetSabotageDelayRecoveryDecisionPolicy);
        System.Reflection.MethodInfo method = Assert.Single(policyType.GetMethods().Where(
            candidate => candidate.DeclaringType == policyType));
        Assert.Equal("CanAccept", method.Name);
        Assert.Equal(typeof(bool), method.ReturnType);
        Assert.Equal(
            [
                typeof(CharacterTargetSabotageDelayRecoveryConsiderationRequest),
                typeof(CharacterTargetSabotageDelayRecoveryDecision),
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
        CharacterTargetSabotageDelayResult delayResult = new(
            new CharacterTargetSabotageDelayCandidate(occurrence, delayEvent),
            new WorldEventLogSnapshot([priorEvent.EventId, delayEvent.EventId]),
            new WorldTimeSnapshot(delayEvent.EventId));
        WorldEventSnapshot worldEvents = new([priorEvent, delayEvent, recoveryEvent]);
        CharacterTargetSabotageDelayRecoveryCandidate candidate = new(
            delayResult,
            recoveryEvent);
        CharacterTargetSabotageDelayRecoveryConsiderationRequest request = new(
            actor,
            worldEvents,
            delayResult.EventLog,
            delayResult.WorldTime,
            [candidate]);

        return new DecisionInput(
            actor,
            delayEvent,
            recoveryEvent,
            worldEvents,
            delayResult.EventLog,
            delayResult.WorldTime,
            delayResult,
            candidate,
            request);
    }

    private sealed record DecisionInput(
        Character Actor,
        WorldEvent DelayEvent,
        WorldEvent RecoveryEvent,
        WorldEventSnapshot WorldEvents,
        WorldEventLogSnapshot EventLog,
        WorldTimeSnapshot WorldTime,
        CharacterTargetSabotageDelayResult DelayResult,
        CharacterTargetSabotageDelayRecoveryCandidate Candidate,
        CharacterTargetSabotageDelayRecoveryConsiderationRequest Request);
}
