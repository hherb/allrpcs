using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Identifiers;
using Allrpcs.Domain.WorldEvents;
using Godot;

namespace Allrpcs.Game;

/// <summary>Checks exact scene ownership and resolution of accepted target-delay recovery.</summary>
public partial class CharacterTargetSabotageDelayRecoveryControllerSmokeCheck : Node
{
    /// <inheritdoc />
    public override void _Ready()
    {
        RecoveryInput input = CreateInput();

        if (!ResolvesSelection(input)
            || !ConsumesAbstentionWhilePreservingLatestResult(input)
            || !RejectsStaleUninitializedAndNull(input)
            || !HasClosedSurface())
        {
            GD.PushError(
                "Target-delay recovery controller failed exact world-state resolution.");
            GetTree().Quit(1);
            return;
        }

        GD.Print(
            "Target-delay recovery controller smoke passed: exact selection installs fresh "
            + "event-log and world-time state atomically, later abstention is inert but consumed "
            + "without clearing the latest result, stale, value-equal, null, uninitialized, and "
            + "repeated inputs fail closed, and sources stay unchanged.");
        GetTree().Quit();
    }

    private static bool ResolvesSelection(RecoveryInput input)
    {
        CharacterTargetSabotageDelayRecoveryController controller = new();
        CharacterTargetSabotageDelayRecoveryConsiderationResult selection = Result(
            input,
            input.EventLog,
            input.WorldTime,
            input.FirstCandidate);

        bool accepted = controller.TryInitialize(input.EventLog, input.WorldTime)
            && controller.TryResolveConsideration(selection)
            && controller.EventLog is { } changedEventLog
            && controller.WorldTime is { } changedWorldTime
            && controller.LatestAppliedResult is { } applied
            && ReferenceEquals(applied.Candidate, input.FirstCandidate)
            && ReferenceEquals(applied.EventLog, changedEventLog)
            && ReferenceEquals(applied.WorldTime, changedWorldTime)
            && !ReferenceEquals(changedEventLog, input.EventLog)
            && !ReferenceEquals(changedWorldTime, input.WorldTime)
            && changedEventLog.Entries.Count == input.EventLog.Entries.Count + 1
            && ReferenceEquals(
                changedEventLog.Entries[^1],
                input.FirstCandidate.RecoveryEvent.EventId)
            && ReferenceEquals(
                changedWorldTime.CurrentEventId,
                input.FirstCandidate.RecoveryEvent.EventId)
            && !controller.TryResolveConsideration(selection)
            && ReferenceEquals(controller.EventLog, changedEventLog)
            && ReferenceEquals(controller.WorldTime, changedWorldTime)
            && ReferenceEquals(controller.LatestAppliedResult, applied)
            && HasUnchangedSources(input, selection.Request);

        controller.Free();
        return accepted;
    }

    private static bool ConsumesAbstentionWhilePreservingLatestResult(RecoveryInput input)
    {
        CharacterTargetSabotageDelayRecoveryController controller = new();
        CharacterTargetSabotageDelayRecoveryConsiderationResult selection = Result(
            input,
            input.EventLog,
            input.WorldTime,
            input.FirstCandidate);
        if (!controller.TryInitialize(input.EventLog, input.WorldTime)
            || !controller.TryResolveConsideration(selection)
            || controller.EventLog is not { } changedEventLog
            || controller.WorldTime is not { } changedWorldTime
            || controller.LatestAppliedResult is not { } applied)
        {
            controller.Free();
            return false;
        }

        CharacterTargetSabotageDelayRecoveryConsiderationResult abstention = Result(
            input,
            changedEventLog,
            changedWorldTime,
            null);
        CharacterTargetSabotageDelayRecoveryConsiderationResult laterAbstention = Result(
            input,
            changedEventLog,
            changedWorldTime,
            null);

        bool accepted = controller.TryResolveConsideration(abstention)
            && ReferenceEquals(controller.EventLog, changedEventLog)
            && ReferenceEquals(controller.WorldTime, changedWorldTime)
            && ReferenceEquals(controller.LatestAppliedResult, applied)
            && !controller.TryResolveConsideration(abstention)
            && controller.TryResolveConsideration(laterAbstention)
            && !controller.TryResolveConsideration(abstention)
            && ReferenceEquals(controller.EventLog, changedEventLog)
            && ReferenceEquals(controller.WorldTime, changedWorldTime)
            && ReferenceEquals(controller.LatestAppliedResult, applied)
            && HasUnchangedSources(input, abstention.Request)
            && HasUnchangedSources(input, laterAbstention.Request);

        controller.Free();
        return accepted;
    }

    private static bool RejectsStaleUninitializedAndNull(RecoveryInput input)
    {
        CharacterTargetSabotageDelayRecoveryController controller = new();
        CharacterTargetSabotageDelayRecoveryController nullInitializationController = new();
        CharacterTargetSabotageDelayRecoveryConsiderationResult exact = Result(
            input,
            input.EventLog,
            input.WorldTime,
            input.FirstCandidate);
        WorldEventLogSnapshot valueEqualEventLog = new(input.EventLog.Entries);
        WorldTimeSnapshot valueEqualWorldTime = new(input.WorldTime.CurrentEventId);
        CharacterTargetSabotageDelayRecoveryConsiderationResult staleLog = Result(
            input,
            valueEqualEventLog,
            input.WorldTime,
            null);
        CharacterTargetSabotageDelayRecoveryConsiderationResult staleTime = Result(
            input,
            input.EventLog,
            valueEqualWorldTime,
            null);

        bool rejectedUninitialized = !controller.TryResolveConsideration(exact)
            && controller.EventLog is null
            && controller.WorldTime is null
            && controller.LatestAppliedResult is null;
        bool rejectedNullInitialization = ThrowsArgumentNull(
                () => nullInitializationController.TryInitialize(null!, input.WorldTime))
            && ThrowsArgumentNull(
                () => nullInitializationController.TryInitialize(input.EventLog, null!))
            && nullInitializationController.EventLog is null
            && nullInitializationController.WorldTime is null
            && nullInitializationController.LatestAppliedResult is null;
        bool rejected = controller.TryInitialize(input.EventLog, input.WorldTime)
            && !controller.TryInitialize(valueEqualEventLog, valueEqualWorldTime)
            && !controller.TryResolveConsideration(staleLog)
            && !controller.TryResolveConsideration(staleTime)
            && ThrowsArgumentNull(() => controller.TryInitialize(null!, input.WorldTime))
            && ThrowsArgumentNull(() => controller.TryInitialize(input.EventLog, null!))
            && ThrowsArgumentNull(() => controller.TryResolveConsideration(null!))
            && ReferenceEquals(controller.EventLog, input.EventLog)
            && ReferenceEquals(controller.WorldTime, input.WorldTime)
            && controller.LatestAppliedResult is null
            && HasUnchangedSources(input, exact.Request)
            && HasUnchangedSources(input, staleLog.Request)
            && HasUnchangedSources(input, staleTime.Request);

        controller.Free();
        nullInitializationController.Free();
        return rejectedUninitialized && rejectedNullInitialization && rejected;
    }

    private static CharacterTargetSabotageDelayRecoveryConsiderationResult Result(
        RecoveryInput input,
        WorldEventLogSnapshot eventLog,
        WorldTimeSnapshot worldTime,
        CharacterTargetSabotageDelayRecoveryCandidate? selectedCandidate) =>
        new(
            new CharacterTargetSabotageDelayRecoveryConsiderationRequest(
                input.Actor,
                input.WorldEvents,
                eventLog,
                worldTime,
                [input.FirstCandidate, input.SecondCandidate]),
            new CharacterTargetSabotageDelayRecoveryDecision(
                input.Actor.Identity.Id,
                selectedCandidate));

    private static bool HasUnchangedSources(
        RecoveryInput input,
        CharacterTargetSabotageDelayRecoveryConsiderationRequest request) =>
        ReferenceEquals(request.Actor, input.Actor)
        && ReferenceEquals(request.WorldEvents, input.WorldEvents)
        && ReferenceEquals(request.Candidates[0], input.FirstCandidate)
        && ReferenceEquals(request.Candidates[1], input.SecondCandidate)
        && ReferenceEquals(input.EventLog, input.DelayResult.EventLog)
        && ReferenceEquals(input.WorldTime, input.DelayResult.WorldTime)
        && input.EventLog.Entries.Count == 2
        && ReferenceEquals(input.EventLog.Entries[0], input.PriorEvent.EventId)
        && ReferenceEquals(input.EventLog.Entries[1], input.DelayEvent.EventId)
        && ReferenceEquals(input.WorldTime.CurrentEventId, input.DelayEvent.EventId);

    private static bool HasClosedSurface()
    {
        Type type = typeof(CharacterTargetSabotageDelayRecoveryController);
        string[] methods = type.GetMethods(
                System.Reflection.BindingFlags.Public
                | System.Reflection.BindingFlags.Instance
                | System.Reflection.BindingFlags.DeclaredOnly)
            .Where(method => !method.IsSpecialName)
            .Select(method => method.Name)
            .Order(StringComparer.Ordinal)
            .ToArray();
        string[] properties = type.GetProperties(
                System.Reflection.BindingFlags.Public
                | System.Reflection.BindingFlags.Instance
                | System.Reflection.BindingFlags.DeclaredOnly)
            .Select(property => property.Name)
            .Order(StringComparer.Ordinal)
            .ToArray();

        return methods.SequenceEqual(["TryInitialize", "TryResolveConsideration"])
            && properties.SequenceEqual(["EventLog", "LatestAppliedResult", "WorldTime"]);
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

    private static RecoveryInput CreateInput()
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
        WorldEvent firstRecoveryEvent = new(
            new WorldEventId("sage-delay-recovered-once"),
            WorldEventTimeEffect.Advance);
        WorldEvent secondRecoveryEvent = new(
            new WorldEventId("sage-delay-recovered-twice"),
            WorldEventTimeEffect.Advance);
        CharacterTargetSabotageDelayResult delayResult = new(
            new CharacterTargetSabotageDelayCandidate(occurrence, delayEvent),
            new WorldEventLogSnapshot([priorEvent.EventId, delayEvent.EventId]),
            new WorldTimeSnapshot(delayEvent.EventId));

        return new RecoveryInput(
            actor,
            new WorldEventSnapshot(
                [priorEvent, delayEvent, firstRecoveryEvent, secondRecoveryEvent]),
            delayResult.EventLog,
            delayResult.WorldTime,
            priorEvent,
            delayEvent,
            delayResult,
            new CharacterTargetSabotageDelayRecoveryCandidate(
                delayResult,
                firstRecoveryEvent),
            new CharacterTargetSabotageDelayRecoveryCandidate(
                delayResult,
                secondRecoveryEvent));
    }

    private sealed record RecoveryInput(
        Character Actor,
        WorldEventSnapshot WorldEvents,
        WorldEventLogSnapshot EventLog,
        WorldTimeSnapshot WorldTime,
        WorldEvent PriorEvent,
        WorldEvent DelayEvent,
        CharacterTargetSabotageDelayResult DelayResult,
        CharacterTargetSabotageDelayRecoveryCandidate FirstCandidate,
        CharacterTargetSabotageDelayRecoveryCandidate SecondCandidate);
}
