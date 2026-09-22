using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Discovery;
using Allrpcs.Domain.Locations;
using Allrpcs.Domain.WorldObjects;
using Godot;

namespace Allrpcs.Game;

/// <summary>Checks one move-gated sabotage-action request from exact scene state.</summary>
public partial class CharacterSabotageActionMoveRequestControllerSmokeCheck : Node
{
    /// <inheritdoc />
    public override void _Ready()
    {
        SceneGraph unavailable = Graph("Unavailable");
        SceneGraph ineligible = Graph("Ineligible");
        SceneGraph eligible = Graph("Eligible");

        bool unavailableRejected = !TryCreate(unavailable)
            && unavailable.Requests.Request is null
            && unavailable.State.Placements is null
            && unavailable.State.Evidence is null;

        WorldObjectPlacementSnapshot alreadyMoved = new(
            [new WorldObjectPlacement(
                ineligible.Source.Candidate.Destination.Occurrence.Target,
                ineligible.Source.Destination)]);
        bool ineligibleRejected = ineligible.State.TryInitialize(
                alreadyMoved,
                ineligible.Source.Evidence)
            && !TryCreate(ineligible, alreadyMoved, ineligible.Source.Evidence)
            && ineligible.Requests.Request is null
            && ReferenceEquals(ineligible.State.Placements, alreadyMoved)
            && ReferenceEquals(ineligible.State.Evidence, ineligible.Source.Evidence);

        bool initialized = eligible.State.TryInitialize(
            eligible.Source.Placements,
            eligible.Source.Evidence);
        Location retainedDestination = eligible.Source.Destination;
        CharacterObjectSabotageMoveCandidate retainedCandidate = eligible.Source.Candidate;
        Character replacementActor = RepresentativeEnemyAgentCharacterSource.Create();
        CharacterPhysicalAccessSnapshot replacementAccess = new(
            eligible.Source.Character.Identity.Id,
            eligible.Source.PhysicalAccess.AccessibleObjectIds);
        WorldObjectSnapshot replacementWorldObjects = new(eligible.Source.WorldObjects.Entries);
        WorldObjectPlacementSnapshot replacementPlacements = new(
            eligible.Source.Placements.Entries);
        DiscoveryEvidenceSnapshot replacementEvidence = new([]);
        Location replacementDestination = new(
            eligible.Source.Destination.Identity,
            eligible.Source.Destination.Description);
        CharacterObjectSabotageMoveCandidate replacementCandidate = new(
            eligible.Source.Candidate.Destination,
            eligible.Source.Candidate.Evidence);
        bool staleAndReplacementRejected = initialized
            && !TryCreate(eligible, replacementPlacements, eligible.Source.Evidence)
            && !TryCreate(eligible, eligible.Source.Placements, replacementEvidence)
            && !TryCreate(eligible, actor: replacementActor)
            && !TryCreate(eligible, physicalAccess: replacementAccess)
            && !TryCreate(eligible, worldObjects: replacementWorldObjects)
            && !TryCreate(eligible, destination: replacementDestination)
            && !TryCreate(eligible, candidate: replacementCandidate)
            && eligible.Requests.Request is null;
        bool nullsRejected = RejectsNulls(eligible);
        bool created = TryCreate(eligible)
            && eligible.Requests.Request is { } request
            && ReferenceEquals(request.Actor, eligible.Source.Character)
            && request.ActionKinds.Count == 1
            && ReferenceEquals(
                request.ActionKinds.Single(),
                CharacterSabotageActionKind.Move)
            && eligible.Requests.CandidateBinding
                is CharacterSabotageActionCandidateBinding<CharacterObjectSabotageMoveCandidate>
                    binding
            && ReferenceEquals(binding.Request, request)
            && ReferenceEquals(binding.ActionKind, CharacterSabotageActionKind.Move)
            && ReferenceEquals(binding.Candidate, eligible.Source.Candidate);

        CharacterSabotageActionConsiderationRequest retained = eligible.Requests.Request!;
        bool repeatedIsInert = !TryCreate(eligible)
            && ReferenceEquals(eligible.Requests.Request, retained)
            && ReferenceEquals(eligible.State.Placements, eligible.Source.Placements)
            && ReferenceEquals(eligible.State.Evidence, eligible.Source.Evidence)
            && ReferenceEquals(eligible.Source.Destination, retainedDestination)
            && ReferenceEquals(eligible.Source.Candidate, retainedCandidate)
            && CharacterObjectSabotageMovePolicy.CanApply(
                eligible.Source.Character,
                eligible.Source.PhysicalAccess,
                eligible.Source.WorldObjects,
                eligible.Source.Placements,
                eligible.Source.Evidence,
                eligible.Source.Candidate);

        if (!unavailableRejected
            || !ineligibleRejected
            || !staleAndReplacementRejected
            || !nullsRejected
            || !created
            || !repeatedIsInert
            || !HasClosedSurface())
        {
            Fail("Move-gated sabotage-action request did not preserve exact scene state.");
            return;
        }

        GD.Print(
            "Sabotage-action move request smoke passed: unavailable, ineligible, stale, "
            + "replacement, null, and repeated inputs stayed inert; one request retained the "
            + "exact enemy actor and canonical move action without starting work.");
        GetTree().Quit();
    }

    private SceneGraph Graph(string parentName) =>
        new(
            GetNode<TemporaryCharacterObjectSabotageMoveSource>(
                $"../{parentName}/TemporaryCharacterObjectSabotageMoveSource"),
            GetNode<CharacterObjectSabotageMoveController>(
                $"../{parentName}/CharacterObjectSabotageMoveController"),
            GetNode<CharacterSabotageActionMoveRequestController>(
                $"../{parentName}/CharacterSabotageActionMoveRequestController"));

    private static bool TryCreate(
        SceneGraph graph,
        WorldObjectPlacementSnapshot? placements = null,
        DiscoveryEvidenceSnapshot? evidence = null,
        Character? actor = null,
        CharacterPhysicalAccessSnapshot? physicalAccess = null,
        WorldObjectSnapshot? worldObjects = null,
        Location? destination = null,
        CharacterObjectSabotageMoveCandidate? candidate = null) =>
        graph.Requests.TryCreate(
            actor ?? graph.Source.Character,
            physicalAccess ?? graph.Source.PhysicalAccess,
            worldObjects ?? graph.Source.WorldObjects,
            placements ?? graph.Source.Placements,
            evidence ?? graph.Source.Evidence,
            destination ?? graph.Source.Destination,
            candidate ?? graph.Source.Candidate);

    private static bool RejectsNulls(SceneGraph graph) =>
        ThrowsArgumentNull(() => graph.Requests.TryCreate(
            null!, graph.Source.PhysicalAccess, graph.Source.WorldObjects,
            graph.Source.Placements, graph.Source.Evidence, graph.Source.Destination,
            graph.Source.Candidate))
        && ThrowsArgumentNull(() => graph.Requests.TryCreate(
            graph.Source.Character, null!, graph.Source.WorldObjects,
            graph.Source.Placements, graph.Source.Evidence, graph.Source.Destination,
            graph.Source.Candidate))
        && ThrowsArgumentNull(() => graph.Requests.TryCreate(
            graph.Source.Character, graph.Source.PhysicalAccess, null!,
            graph.Source.Placements, graph.Source.Evidence, graph.Source.Destination,
            graph.Source.Candidate))
        && ThrowsArgumentNull(() => graph.Requests.TryCreate(
            graph.Source.Character, graph.Source.PhysicalAccess, graph.Source.WorldObjects,
            null!, graph.Source.Evidence, graph.Source.Destination, graph.Source.Candidate))
        && ThrowsArgumentNull(() => graph.Requests.TryCreate(
            graph.Source.Character, graph.Source.PhysicalAccess, graph.Source.WorldObjects,
            graph.Source.Placements, null!, graph.Source.Destination, graph.Source.Candidate))
        && ThrowsArgumentNull(() => graph.Requests.TryCreate(
            graph.Source.Character, graph.Source.PhysicalAccess, graph.Source.WorldObjects,
            graph.Source.Placements, graph.Source.Evidence, null!, graph.Source.Candidate))
        && ThrowsArgumentNull(() => graph.Requests.TryCreate(
            graph.Source.Character, graph.Source.PhysicalAccess, graph.Source.WorldObjects,
            graph.Source.Placements, graph.Source.Evidence, graph.Source.Destination, null!));

    private static bool HasClosedSurface()
    {
        Type type = typeof(CharacterSabotageActionMoveRequestController);
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
            && HasExpectedProperties(properties);
    }

    private static bool HasExpectedProperties(System.Reflection.PropertyInfo[] properties) =>
        properties.Length == 2
        && properties.Single(property => property.Name == "CandidateBinding").PropertyType
            == typeof(CharacterSabotageActionCandidateBinding)
        && properties.Single(property => property.Name == "Request").PropertyType
            == typeof(CharacterSabotageActionConsiderationRequest)
        && properties.All(property => property.SetMethod is { IsPrivate: true });

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

    private sealed record SceneGraph(
        TemporaryCharacterObjectSabotageMoveSource Source,
        CharacterObjectSabotageMoveController State,
        CharacterSabotageActionMoveRequestController Requests);
}
