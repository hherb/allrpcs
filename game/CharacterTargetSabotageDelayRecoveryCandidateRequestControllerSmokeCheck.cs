using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Identifiers;
using Allrpcs.Domain.WorldEvents;
using Godot;

namespace Allrpcs.Game;

/// <summary>Checks one consideration request for the exact current delay-recovery candidate.</summary>
public partial class CharacterTargetSabotageDelayRecoveryCandidateRequestControllerSmokeCheck
    : Node
{
    /// <inheritdoc />
    public override void _Ready()
    {
        CharacterTargetSabotageDelayController delay =
            GetNode<CharacterTargetSabotageDelayController>(
                "../CharacterTargetSabotageDelayController");
        CharacterTargetSabotageDelayRecoveryCandidateController candidates =
            GetNode<CharacterTargetSabotageDelayRecoveryCandidateController>(
                "../CharacterTargetSabotageDelayRecoveryCandidateController");
        CharacterTargetSabotageDelayRecoveryCandidateRequestController requests =
            GetNode<CharacterTargetSabotageDelayRecoveryCandidateRequestController>(
                "../CharacterTargetSabotageDelayRecoveryCandidateRequestController");
        DelayInput input = CreateInput();
        CharacterTargetSabotageDelayResult staleResult =
            CharacterTargetSabotageDelayTransition.Apply(
                input.Actor,
                input.InteractionAccess,
                input.WorldEvents,
                input.EventLog,
                input.WorldTime,
                input.DelayCandidate);
        CharacterTargetSabotageDelayRecoveryCandidate staleCandidate = new(
            staleResult,
            input.RecoveryEvent);

        bool unavailableRejected = !requests.TryCreate(input.WorldEvents, staleCandidate)
            && requests.Request is null;
        CharacterTargetSabotageDelayConsiderationResult selection = new(
            new CharacterTargetSabotageDelayConsiderationRequest(
                input.Actor,
                input.InteractionAccess,
                input.WorldEvents,
                input.EventLog,
                input.WorldTime,
                [input.DelayCandidate]),
            new CharacterTargetSabotageDelayDecision(
                input.Actor.Identity.Id,
                input.DelayCandidate));
        bool applied = delay.TryInitialize(input.EventLog, input.WorldTime)
            && delay.TryResolveConsideration(selection);
        CharacterTargetSabotageDelayResult? currentResult = delay.LatestAppliedResult;
        bool candidateCreated = currentResult is not null
            && candidates.TryCreate(input.WorldEvents, currentResult, input.RecoveryEvent)
            && candidates.Candidate is not null;
        CharacterTargetSabotageDelayResult retainedResult = currentResult!;
        CharacterTargetSabotageDelayRecoveryCandidate retainedCandidate =
            candidates.Candidate!;
        CharacterTargetSabotageDelayRecoveryCandidate rebuiltCandidate = new(
            retainedResult,
            input.RecoveryEvent);
        CharacterTargetSabotageDelayRecoveryCandidate foreignCandidate = ForeignCandidate();
        WorldEventSnapshot missingRecovery = new([input.PriorEvent, input.DelayEvent]);
        WorldEventSnapshot inapplicableRecovery = new(
            [input.PriorEvent, input.DelayEvent, input.NoAdvanceRecoveryEvent]);

        bool invalidAndNullRejected = !requests.TryCreate(input.WorldEvents, staleCandidate)
            && !requests.TryCreate(input.WorldEvents, rebuiltCandidate)
            && !requests.TryCreate(input.WorldEvents, foreignCandidate)
            && !requests.TryCreate(missingRecovery, retainedCandidate)
            && !requests.TryCreate(inapplicableRecovery, retainedCandidate)
            && ThrowsArgumentNull(() => requests.TryCreate(null!, retainedCandidate))
            && ThrowsArgumentNull(() => requests.TryCreate(input.WorldEvents, null!))
            && requests.Request is null;
        bool created = requests.TryCreate(input.WorldEvents, retainedCandidate)
            && requests.Request is { } request
            && ReferenceEquals(request.Actor, input.Target)
            && ReferenceEquals(request.WorldEvents, input.WorldEvents)
            && ReferenceEquals(request.EventLog, retainedResult.EventLog)
            && ReferenceEquals(request.WorldTime, retainedResult.WorldTime)
            && request.Candidates.Count == 1
            && ReferenceEquals(request.Candidates.Single(), retainedCandidate)
            && CharacterTargetSabotageDelayRecoveryPolicy.CanRecover(
                request.WorldEvents,
                request.EventLog,
                request.WorldTime,
                retainedCandidate)
            && CharacterTargetSabotageDelayRecoveryDecisionPolicy.CanAccept(
                request,
                new CharacterTargetSabotageDelayRecoveryDecision(
                    input.Target.Identity.Id,
                    null));

        CharacterTargetSabotageDelayRecoveryConsiderationRequest retained = requests.Request!;
        bool repeatedIsInert = !requests.TryCreate(input.WorldEvents, retainedCandidate)
            && !requests.TryCreate(input.WorldEvents, rebuiltCandidate)
            && ReferenceEquals(requests.Request, retained)
            && ReferenceEquals(candidates.Candidate, retainedCandidate)
            && ReferenceEquals(delay.LatestAppliedResult, retainedResult)
            && ReferenceEquals(delay.EventLog, retainedResult.EventLog)
            && ReferenceEquals(delay.WorldTime, retainedResult.WorldTime)
            && ReferenceEquals(retainedResult.Candidate, input.DelayCandidate)
            && ReferenceEquals(retainedCandidate.RecoveryEvent, input.RecoveryEvent)
            && retainedResult.EventLog.Entries.Count == input.EventLog.Entries.Count + 1
            && ReferenceEquals(retainedResult.EventLog.Entries[^1], input.DelayEvent.EventId)
            && ReferenceEquals(retainedResult.WorldTime.CurrentEventId, input.DelayEvent.EventId);

        if (!unavailableRejected
            || !applied
            || !candidateCreated
            || !invalidAndNullRejected
            || !created
            || !repeatedIsInert
            || !HasClosedSurface())
        {
            GD.PushError("Delay-recovery request did not retain the exact current candidate.");
            GetTree().Quit(1);
            return;
        }

        GD.Print(
            "Delay-recovery candidate request smoke passed: unavailable, stale, rebuilt, foreign, "
            + "missing-event, inapplicable, null, and repeated inputs stayed inert; one request "
            + "retained the exact target, world state, and current candidate without starting work.");
        GetTree().Quit();
    }

    private static bool HasClosedSurface()
    {
        Type type = typeof(CharacterTargetSabotageDelayRecoveryCandidateRequestController);
        string[] methods = type.GetMethods(
                System.Reflection.BindingFlags.Public
                | System.Reflection.BindingFlags.Instance
                | System.Reflection.BindingFlags.DeclaredOnly)
            .Where(method => !method.IsSpecialName)
            .Select(method => method.Name)
            .Order(StringComparer.Ordinal)
            .ToArray();
        System.Reflection.PropertyInfo[] properties = type.GetProperties(
            System.Reflection.BindingFlags.Public
            | System.Reflection.BindingFlags.Instance
            | System.Reflection.BindingFlags.DeclaredOnly);

        return methods.SequenceEqual(["TryCreate", "_Ready"])
            && properties.Length == 1
            && properties[0].Name == "Request"
            && properties[0].PropertyType
                == typeof(CharacterTargetSabotageDelayRecoveryConsiderationRequest)
            && properties[0].SetMethod is not null
            && properties[0].SetMethod!.IsPrivate;
    }

    private static bool ThrowsArgumentNull(Action action)
    {
        try
        {
            action();
            return false;
        }
        catch (ArgumentNullException)
        {
            return true;
        }
    }

    private static CharacterTargetSabotageDelayRecoveryCandidate ForeignCandidate()
    {
        DelayInput input = CreateInput();
        CharacterTargetSabotageDelayResult result = CharacterTargetSabotageDelayTransition.Apply(
            input.Actor,
            input.InteractionAccess,
            input.WorldEvents,
            input.EventLog,
            input.WorldTime,
            input.DelayCandidate);
        return new CharacterTargetSabotageDelayRecoveryCandidate(result, input.RecoveryEvent);
    }

    private static DelayInput CreateInput()
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
        WorldEvent noAdvanceRecoveryEvent = new(
            recoveryEvent.EventId,
            WorldEventTimeEffect.NoAdvance);

        return new DelayInput(
            actor,
            target,
            new CharacterInteractionAccessSnapshot(actor.Identity.Id, [target.Identity.Id]),
            new WorldEventSnapshot([priorEvent, delayEvent, recoveryEvent]),
            new WorldEventLogSnapshot([priorEvent.EventId]),
            new WorldTimeSnapshot(null),
            priorEvent,
            delayEvent,
            recoveryEvent,
            noAdvanceRecoveryEvent,
            new CharacterTargetSabotageDelayCandidate(occurrence, delayEvent));
    }

    private sealed record DelayInput(
        Character Actor,
        Character Target,
        CharacterInteractionAccessSnapshot InteractionAccess,
        WorldEventSnapshot WorldEvents,
        WorldEventLogSnapshot EventLog,
        WorldTimeSnapshot WorldTime,
        WorldEvent PriorEvent,
        WorldEvent DelayEvent,
        WorldEvent RecoveryEvent,
        WorldEvent NoAdvanceRecoveryEvent,
        CharacterTargetSabotageDelayCandidate DelayCandidate);
}
