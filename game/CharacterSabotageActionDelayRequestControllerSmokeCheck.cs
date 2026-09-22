using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Identifiers;
using Allrpcs.Domain.WorldEvents;
using Godot;

namespace Allrpcs.Game;

/// <summary>Checks one delay-gated sabotage-action request from exact current world state.</summary>
public partial class CharacterSabotageActionDelayRequestControllerSmokeCheck : Node
{
    /// <inheritdoc />
    public override void _Ready()
    {
        SceneGraph unavailable = Graph("Unavailable", "unavailable", false);
        SceneGraph ineligible = Graph("Ineligible", "ineligible", true);
        SceneGraph eligible = Graph("Eligible", "eligible", false);

        bool unavailableRejected = !TryCreate(unavailable)
            && unavailable.Requests.Request is null
            && unavailable.State.EventLog is null
            && unavailable.State.WorldTime is null;
        bool ineligibleRejected = ineligible.State.TryInitialize(
                ineligible.Input.Request.EventLog,
                ineligible.Input.Request.WorldTime)
            && !TryCreate(ineligible)
            && ineligible.Requests.Request is null
            && ReferenceEquals(ineligible.State.EventLog, ineligible.Input.Request.EventLog)
            && ReferenceEquals(ineligible.State.WorldTime, ineligible.Input.Request.WorldTime);

        bool initialized = eligible.State.TryInitialize(
            eligible.Input.Request.EventLog,
            eligible.Input.Request.WorldTime);
        Character replacementActor = RepresentativeEnemyAgentCharacterSource.Create();
        CharacterInteractionAccessSnapshot replacementAccess = new(
            eligible.Input.Request.Actor.Identity.Id,
            eligible.Input.Request.InteractionAccess.ReachableTargetCharacterIds);
        WorldEventSnapshot replacementWorldEvents = new(
            eligible.Input.Request.WorldEvents.Entries);
        WorldEventLogSnapshot replacementEventLog = new(
            eligible.Input.Request.EventLog.Entries);
        WorldTimeSnapshot replacementWorldTime = new(
            eligible.Input.Request.WorldTime.CurrentEventId);
        CharacterTargetSabotageDelayCandidate replacementCandidate = new(
            eligible.Input.Candidate.Occurrence,
            eligible.Input.Candidate.WorldEvent);
        bool staleAndReplacementRejected = initialized
            && !TryCreate(eligible, actor: replacementActor)
            && !TryCreate(eligible, interactionAccess: replacementAccess)
            && !TryCreate(eligible, worldEvents: replacementWorldEvents)
            && !TryCreate(eligible, eventLog: replacementEventLog)
            && !TryCreate(eligible, worldTime: replacementWorldTime)
            && !TryCreate(eligible, candidate: replacementCandidate)
            && eligible.Requests.Request is null;
        bool nullsRejected = RejectsNulls(eligible);
        bool created = TryCreate(eligible)
            && eligible.Requests.Request is { } request
            && ReferenceEquals(request.Actor, eligible.Input.Request.Actor)
            && request.ActionKinds.Count == 1
            && ReferenceEquals(
                request.ActionKinds.Single(),
                CharacterSabotageActionKind.Delay);

        CharacterSabotageActionConsiderationRequest retained = eligible.Requests.Request!;
        bool repeatedIsInert = !TryCreate(eligible)
            && ReferenceEquals(eligible.Requests.Request, retained)
            && ReferenceEquals(eligible.State.EventLog, eligible.Input.Request.EventLog)
            && ReferenceEquals(eligible.State.WorldTime, eligible.Input.Request.WorldTime)
            && eligible.State.LatestAppliedResult is null
            && CharacterTargetSabotageDelayPolicy.CanApply(
                eligible.Input.Request.Actor,
                eligible.Input.Request.InteractionAccess,
                eligible.Input.Request.WorldEvents,
                eligible.Input.Request.EventLog,
                eligible.Input.Request.WorldTime,
                eligible.Input.Candidate);

        if (!unavailableRejected
            || !ineligibleRejected
            || !staleAndReplacementRejected
            || !nullsRejected
            || !created
            || !repeatedIsInert
            || !HasClosedSurface())
        {
            Fail("Delay-gated sabotage-action request did not preserve exact world state.");
            return;
        }

        GD.Print(
            "Sabotage-action delay request smoke passed: unavailable, ineligible, stale, "
            + "replacement, null, and repeated inputs stayed inert; one request retained the "
            + "exact enemy actor and canonical delay action without advancing world time.");
        GetTree().Quit();
    }

    private SceneGraph Graph(string parentName, string suffix, bool alreadyOccurred)
    {
        DelayInput input = CreateInput(suffix, alreadyOccurred);
        return new SceneGraph(
            input,
            GetNode<CharacterTargetSabotageDelayController>(
                $"../{parentName}/CharacterTargetSabotageDelayController"),
            GetNode<CharacterSabotageActionDelayRequestController>(
                $"../{parentName}/CharacterSabotageActionDelayRequestController"));
    }

    private static bool TryCreate(
        SceneGraph graph,
        Character? actor = null,
        CharacterInteractionAccessSnapshot? interactionAccess = null,
        WorldEventSnapshot? worldEvents = null,
        WorldEventLogSnapshot? eventLog = null,
        WorldTimeSnapshot? worldTime = null,
        CharacterTargetSabotageDelayCandidate? candidate = null) =>
        graph.Requests.TryCreate(
            graph.Input.Request,
            actor ?? graph.Input.Request.Actor,
            interactionAccess ?? graph.Input.Request.InteractionAccess,
            worldEvents ?? graph.Input.Request.WorldEvents,
            eventLog ?? graph.Input.Request.EventLog,
            worldTime ?? graph.Input.Request.WorldTime,
            candidate ?? graph.Input.Candidate);

    private static bool RejectsNulls(SceneGraph graph) =>
        ThrowsArgumentNull(() => graph.Requests.TryCreate(
            null!, graph.Input.Request.Actor, graph.Input.Request.InteractionAccess,
            graph.Input.Request.WorldEvents, graph.Input.Request.EventLog,
            graph.Input.Request.WorldTime, graph.Input.Candidate))
        && ThrowsArgumentNull(() => graph.Requests.TryCreate(
            graph.Input.Request, null!, graph.Input.Request.InteractionAccess,
            graph.Input.Request.WorldEvents, graph.Input.Request.EventLog,
            graph.Input.Request.WorldTime, graph.Input.Candidate))
        && ThrowsArgumentNull(() => graph.Requests.TryCreate(
            graph.Input.Request, graph.Input.Request.Actor, null!,
            graph.Input.Request.WorldEvents, graph.Input.Request.EventLog,
            graph.Input.Request.WorldTime, graph.Input.Candidate))
        && ThrowsArgumentNull(() => graph.Requests.TryCreate(
            graph.Input.Request, graph.Input.Request.Actor,
            graph.Input.Request.InteractionAccess, null!, graph.Input.Request.EventLog,
            graph.Input.Request.WorldTime, graph.Input.Candidate))
        && ThrowsArgumentNull(() => graph.Requests.TryCreate(
            graph.Input.Request, graph.Input.Request.Actor,
            graph.Input.Request.InteractionAccess, graph.Input.Request.WorldEvents, null!,
            graph.Input.Request.WorldTime, graph.Input.Candidate))
        && ThrowsArgumentNull(() => graph.Requests.TryCreate(
            graph.Input.Request, graph.Input.Request.Actor,
            graph.Input.Request.InteractionAccess, graph.Input.Request.WorldEvents,
            graph.Input.Request.EventLog, null!, graph.Input.Candidate))
        && ThrowsArgumentNull(() => graph.Requests.TryCreate(
            graph.Input.Request, graph.Input.Request.Actor,
            graph.Input.Request.InteractionAccess, graph.Input.Request.WorldEvents,
            graph.Input.Request.EventLog, graph.Input.Request.WorldTime, null!));

    private static DelayInput CreateInput(string suffix, bool alreadyOccurred)
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
        WorldEvent worldEvent = new(
            new WorldEventId($"enemy-agent-delayed-sage-{suffix}"),
            WorldEventTimeEffect.Advance);
        CharacterTargetSabotageDelayCandidate candidate = new(occurrence, worldEvent);
        WorldEventLogSnapshot eventLog = new(
            alreadyOccurred ? [worldEvent.EventId] : []);
        CharacterTargetSabotageDelayConsiderationRequest request = new(
            actor,
            new CharacterInteractionAccessSnapshot(actor.Identity.Id, [target.Identity.Id]),
            new WorldEventSnapshot([worldEvent]),
            eventLog,
            new WorldTimeSnapshot(null),
            [candidate]);
        return new DelayInput(request, candidate);
    }

    private static bool HasClosedSurface()
    {
        Type type = typeof(CharacterSabotageActionDelayRequestController);
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
                == typeof(CharacterSabotageActionConsiderationRequest)
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

    private void Fail(string message)
    {
        GD.PushError(message);
        GetTree().Quit(1);
    }

    private sealed record DelayInput(
        CharacterTargetSabotageDelayConsiderationRequest Request,
        CharacterTargetSabotageDelayCandidate Candidate);

    private sealed record SceneGraph(
        DelayInput Input,
        CharacterTargetSabotageDelayController State,
        CharacterSabotageActionDelayRequestController Requests);
}
