using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Identifiers;
using Allrpcs.Domain.WorldEvents;
using Godot;

namespace Allrpcs.Game;

/// <summary>Checks exact scene ownership and resolution of accepted target delays.</summary>
public partial class CharacterTargetSabotageDelayControllerSmokeCheck : Node
{
    /// <inheritdoc />
    public override void _Ready()
    {
        DelayInput input = CreateInput();

        if (!ResolvesSelection(input)
            || !ConsumesReferentiallyInertAbstention(input)
            || !RejectsStaleUninitializedAndNull(input)
            || !HasClosedSurface())
        {
            GD.PushError("Target-delay controller failed exact authoritative world-state resolution.");
            GetTree().Quit(1);
            return;
        }

        GD.Print(
            "Target-delay controller smoke passed: exact selection installs fresh event-log and "
            + "world-time state atomically, abstention is inert but consumed, stale, value-equal, "
            + "null, uninitialized, and repeated inputs fail closed, and sources stay unchanged.");
        GetTree().Quit();
    }

    private static bool ResolvesSelection(DelayInput input)
    {
        CharacterTargetSabotageDelayController controller = new();
        CharacterTargetSabotageDelayConsiderationResult selection = Result(
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
            && ReferenceEquals(changedEventLog.Entries[^1], input.FirstCandidate.WorldEvent.EventId)
            && ReferenceEquals(changedWorldTime.CurrentEventId, input.FirstCandidate.WorldEvent.EventId)
            && !controller.TryResolveConsideration(selection)
            && ReferenceEquals(controller.EventLog, changedEventLog)
            && ReferenceEquals(controller.WorldTime, changedWorldTime)
            && ReferenceEquals(controller.LatestAppliedResult, applied)
            && HasUnchangedSources(input, selection.Request);

        controller.Free();
        return accepted;
    }

    private static bool ConsumesReferentiallyInertAbstention(DelayInput input)
    {
        CharacterTargetSabotageDelayController controller = new();
        CharacterTargetSabotageDelayConsiderationResult abstention = Result(
            input,
            input.EventLog,
            input.WorldTime,
            null);
        CharacterTargetSabotageDelayConsiderationResult laterAbstention = Result(
            input,
            input.EventLog,
            input.WorldTime,
            null);

        bool accepted = controller.TryInitialize(input.EventLog, input.WorldTime)
            && controller.TryResolveConsideration(abstention)
            && ReferenceEquals(controller.EventLog, input.EventLog)
            && ReferenceEquals(controller.WorldTime, input.WorldTime)
            && controller.LatestAppliedResult is null
            && !controller.TryResolveConsideration(abstention)
            && controller.TryResolveConsideration(laterAbstention)
            && !controller.TryResolveConsideration(abstention)
            && ReferenceEquals(controller.EventLog, input.EventLog)
            && ReferenceEquals(controller.WorldTime, input.WorldTime)
            && controller.LatestAppliedResult is null
            && HasUnchangedSources(input, abstention.Request)
            && HasUnchangedSources(input, laterAbstention.Request);

        controller.Free();
        return accepted;
    }

    private static bool RejectsStaleUninitializedAndNull(DelayInput input)
    {
        CharacterTargetSabotageDelayController controller = new();
        CharacterTargetSabotageDelayController nullInitializationController = new();
        CharacterTargetSabotageDelayConsiderationResult exact = Result(
            input,
            input.EventLog,
            input.WorldTime,
            input.FirstCandidate);
        WorldEventLogSnapshot valueEqualEventLog = new(input.EventLog.Entries);
        WorldTimeSnapshot valueEqualWorldTime = new(input.WorldTime.CurrentEventId);
        CharacterTargetSabotageDelayConsiderationResult staleLog = Result(
            input,
            valueEqualEventLog,
            input.WorldTime,
            input.FirstCandidate);
        CharacterTargetSabotageDelayConsiderationResult staleTime = Result(
            input,
            input.EventLog,
            valueEqualWorldTime,
            input.FirstCandidate);

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
            && ReferenceEquals(valueEqualEventLog.Entries.Single(), input.PriorEvent.EventId)
            && valueEqualWorldTime.CurrentEventId is null
            && HasUnchangedSources(input, exact.Request)
            && HasUnchangedSources(input, staleLog.Request)
            && HasUnchangedSources(input, staleTime.Request);

        controller.Free();
        nullInitializationController.Free();
        return rejectedUninitialized && rejectedNullInitialization && rejected;
    }

    private static CharacterTargetSabotageDelayConsiderationResult Result(
        DelayInput input,
        WorldEventLogSnapshot eventLog,
        WorldTimeSnapshot worldTime,
        CharacterTargetSabotageDelayCandidate? selectedCandidate) =>
        new(
            new CharacterTargetSabotageDelayConsiderationRequest(
                input.Actor,
                input.InteractionAccess,
                input.WorldEvents,
                eventLog,
                worldTime,
                [input.FirstCandidate, input.SecondCandidate]),
            new CharacterTargetSabotageDelayDecision(
                input.Actor.Identity.Id,
                selectedCandidate));

    private static bool HasUnchangedSources(
        DelayInput input,
        CharacterTargetSabotageDelayConsiderationRequest request) =>
        ReferenceEquals(request.Actor, input.Actor)
        && ReferenceEquals(request.InteractionAccess, input.InteractionAccess)
        && ReferenceEquals(request.WorldEvents, input.WorldEvents)
        && ReferenceEquals(request.Candidates[0], input.FirstCandidate)
        && ReferenceEquals(request.Candidates[1], input.SecondCandidate)
        && ReferenceEquals(input.EventLog.Entries.Single(), input.PriorEvent.EventId)
        && input.WorldTime.CurrentEventId is null;

    private static bool HasClosedSurface()
    {
        Type type = typeof(CharacterTargetSabotageDelayController);
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
        WorldEvent firstDelayEvent = new(
            new WorldEventId("enemy-agent-delayed-sage-once"),
            WorldEventTimeEffect.Advance);
        WorldEvent secondDelayEvent = new(
            new WorldEventId("enemy-agent-delayed-sage-twice"),
            WorldEventTimeEffect.Advance);
        CharacterTargetSabotageDelayCandidate firstCandidate = new(occurrence, firstDelayEvent);
        CharacterTargetSabotageDelayCandidate secondCandidate = new(occurrence, secondDelayEvent);

        return new DelayInput(
            actor,
            new CharacterInteractionAccessSnapshot(actor.Identity.Id, [target.Identity.Id]),
            new WorldEventSnapshot([priorEvent, firstDelayEvent, secondDelayEvent]),
            new WorldEventLogSnapshot([priorEvent.EventId]),
            new WorldTimeSnapshot(null),
            priorEvent,
            firstCandidate,
            secondCandidate);
    }

    private sealed record DelayInput(
        Character Actor,
        CharacterInteractionAccessSnapshot InteractionAccess,
        WorldEventSnapshot WorldEvents,
        WorldEventLogSnapshot EventLog,
        WorldTimeSnapshot WorldTime,
        WorldEvent PriorEvent,
        CharacterTargetSabotageDelayCandidate FirstCandidate,
        CharacterTargetSabotageDelayCandidate SecondCandidate);
}
