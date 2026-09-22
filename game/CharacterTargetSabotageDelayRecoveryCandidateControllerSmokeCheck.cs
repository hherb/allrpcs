using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Identifiers;
using Allrpcs.Domain.WorldEvents;
using Godot;

namespace Allrpcs.Game;

/// <summary>Checks one-shot production of an exact-current target-delay recovery candidate.</summary>
public partial class CharacterTargetSabotageDelayRecoveryCandidateControllerSmokeCheck : Node
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
        DelayInput input = CreateInput();
        CharacterTargetSabotageDelayResult fabricatedResult =
            CharacterTargetSabotageDelayTransition.Apply(
                input.Actor,
                input.InteractionAccess,
                input.WorldEvents,
                input.EventLog,
                input.WorldTime,
                input.DelayCandidate);
        bool uninitializedRejected = !candidates.TryCreate(
                input.WorldEvents,
                fabricatedResult,
                input.RecoveryEvent)
            && candidates.Candidate is null;
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
        if (!applied || currentResult is null)
        {
            GD.PushError("Target-delay recovery candidate setup did not apply the delay.");
            GetTree().Quit(1);
            return;
        }

        CharacterTargetSabotageDelayResult staleResult = new(
            input.DelayCandidate,
            currentResult.EventLog,
            currentResult.WorldTime);
        CharacterTargetSabotageDelayResult rebuiltResult = new(
            input.DelayCandidate,
            new WorldEventLogSnapshot(currentResult.EventLog.Entries),
            new WorldTimeSnapshot(currentResult.WorldTime.CurrentEventId));
        WorldEventSnapshot missingRecovery = new(
            [input.PriorEvent, input.DelayEvent, input.NoAdvanceRecoveryEvent]);

        bool rejectedBeforeCreation = uninitializedRejected
            && candidates.Candidate is null
            && !candidates.TryCreate(input.WorldEvents, staleResult, input.RecoveryEvent)
            && !candidates.TryCreate(input.WorldEvents, rebuiltResult, input.RecoveryEvent)
            && !candidates.TryCreate(
                input.WorldEvents,
                currentResult,
                input.NoAdvanceRecoveryEvent)
            && !candidates.TryCreate(input.WorldEvents, currentResult, input.DelayEvent)
            && !candidates.TryCreate(missingRecovery, currentResult, input.RecoveryEvent)
            && candidates.Candidate is null;
        bool created = candidates.TryCreate(
                input.WorldEvents,
                currentResult,
                input.RecoveryEvent)
            && candidates.Candidate is { } candidate
            && ReferenceEquals(candidate.DelayResult, currentResult)
            && ReferenceEquals(candidate.RecoveryEvent, input.RecoveryEvent)
            && CharacterTargetSabotageDelayRecoveryPolicy.CanRecover(
                input.WorldEvents,
                currentResult.EventLog,
                currentResult.WorldTime,
                candidate);

        CharacterTargetSabotageDelayRecoveryCandidate retained = candidates.Candidate!;
        bool repeatedAndNullAreInert = !candidates.TryCreate(
                input.WorldEvents,
                currentResult,
                input.RecoveryEvent)
            && ThrowsArgumentNull(
                () => candidates.TryCreate(null!, currentResult, input.RecoveryEvent))
            && ThrowsArgumentNull(
                () => candidates.TryCreate(input.WorldEvents, null!, input.RecoveryEvent))
            && ThrowsArgumentNull(
                () => candidates.TryCreate(input.WorldEvents, currentResult, null!))
            && ReferenceEquals(candidates.Candidate, retained);
        bool sourcesUnchanged = ReferenceEquals(delay.LatestAppliedResult, currentResult)
            && ReferenceEquals(delay.EventLog, currentResult.EventLog)
            && ReferenceEquals(delay.WorldTime, currentResult.WorldTime)
            && ReferenceEquals(selection.Request.EventLog, input.EventLog)
            && ReferenceEquals(selection.Request.WorldTime, input.WorldTime)
            && ReferenceEquals(selection.Request.Candidates.Single(), input.DelayCandidate)
            && ReferenceEquals(currentResult.Candidate, input.DelayCandidate)
            && currentResult.EventLog.Entries.Count == input.EventLog.Entries.Count + 1
            && ReferenceEquals(currentResult.EventLog.Entries[^1], input.DelayEvent.EventId)
            && ReferenceEquals(currentResult.WorldTime.CurrentEventId, input.DelayEvent.EventId);

        if (!rejectedBeforeCreation
            || !created
            || !repeatedAndNullAreInert
            || !sourcesUnchanged
            || !HasClosedSurface())
        {
            GD.PushError("Target-delay recovery candidate source failed exact-current ownership.");
            GetTree().Quit(1);
            return;
        }

        GD.Print(
            "Target-delay recovery candidate source smoke passed: stale, rebuilt, missing, "
            + "non-advancing, recorded, null, and repeated input stayed inert; one exact current "
            + "delay result retained the caller-authored recovery event.");
        GetTree().Quit();
    }

    private static bool HasClosedSurface()
    {
        Type type = typeof(CharacterTargetSabotageDelayRecoveryCandidateController);
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
            && properties[0].Name == "Candidate"
            && properties[0].PropertyType
                == typeof(CharacterTargetSabotageDelayRecoveryCandidate)
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
            new WorldEventId("sage-delay-recovery-paused"),
            WorldEventTimeEffect.NoAdvance);

        return new DelayInput(
            actor,
            new CharacterInteractionAccessSnapshot(actor.Identity.Id, [target.Identity.Id]),
            new WorldEventSnapshot(
                [priorEvent, delayEvent, recoveryEvent, noAdvanceRecoveryEvent]),
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
