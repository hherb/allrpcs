using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Discovery;
using Allrpcs.Domain.WorldObjects;
using Godot;

namespace Allrpcs.Game;

/// <summary>Checks one contamination-gated sabotage-action request from exact scene state.</summary>
public partial class CharacterSabotageActionContaminationRequestControllerSmokeCheck : Node
{
    /// <inheritdoc />
    public override void _Ready()
    {
        SceneGraph unavailable = Graph("Unavailable");
        SceneGraph ineligible = Graph("Ineligible");
        SceneGraph eligible = Graph("Eligible");

        bool unavailableRejected = !TryCreate(unavailable)
            && unavailable.Requests.Request is null
            && unavailable.State.Contamination is null
            && unavailable.State.Evidence is null;

        WorldObjectContaminationSnapshot alreadyContaminated = new(
            [ineligible.Source.Candidate.Occurrence.Target.Identity.Id]);
        bool ineligibleRejected = ineligible.State.TryInitialize(
                alreadyContaminated,
                ineligible.Source.Evidence)
            && !TryCreate(ineligible, alreadyContaminated, ineligible.Source.Evidence)
            && ineligible.Requests.Request is null
            && ReferenceEquals(ineligible.State.Contamination, alreadyContaminated)
            && ReferenceEquals(ineligible.State.Evidence, ineligible.Source.Evidence);

        bool initialized = eligible.State.TryInitialize(
            eligible.Source.Contamination,
            eligible.Source.Evidence);
        CharacterObjectSabotageEvidence retainedCandidate = eligible.Source.Candidate;
        Character replacementActor = RepresentativeEnemyAgentCharacterSource.Create();
        CharacterPhysicalAccessSnapshot replacementAccess = new(
            eligible.Source.Character.Identity.Id,
            eligible.Source.PhysicalAccess.AccessibleObjectIds);
        WorldObjectSnapshot replacementWorldObjects = new(eligible.Source.WorldObjects.Entries);
        WorldObjectContaminationSnapshot replacementContamination = new([]);
        DiscoveryEvidenceSnapshot replacementEvidence = new([]);
        CharacterObjectSabotageEvidence replacementCandidate = new(
            eligible.Source.Candidate.Occurrence,
            eligible.Source.Candidate.Evidence);
        bool staleAndReplacementRejected = initialized
            && !TryCreate(eligible, replacementContamination, eligible.Source.Evidence)
            && !TryCreate(eligible, eligible.Source.Contamination, replacementEvidence)
            && !TryCreate(eligible, actor: replacementActor)
            && !TryCreate(eligible, physicalAccess: replacementAccess)
            && !TryCreate(eligible, worldObjects: replacementWorldObjects)
            && !TryCreate(eligible, candidate: replacementCandidate)
            && eligible.Requests.Request is null;
        bool nullsRejected = RejectsNulls(eligible);
        bool created = TryCreate(eligible)
            && eligible.Requests.Request is { } request
            && ReferenceEquals(request.Actor, eligible.Source.Character)
            && request.ActionKinds.Count == 1
            && ReferenceEquals(
                request.ActionKinds.Single(),
                CharacterSabotageActionKind.Contaminate)
            && eligible.Requests.CandidateBinding
                is CharacterSabotageActionCandidateBinding<CharacterObjectSabotageEvidence> binding
            && ReferenceEquals(binding.Request, request)
            && ReferenceEquals(binding.ActionKind, CharacterSabotageActionKind.Contaminate)
            && ReferenceEquals(binding.Candidate, eligible.Source.Candidate);

        CharacterSabotageActionConsiderationRequest retained = eligible.Requests.Request!;
        bool repeatedIsInert = !TryCreate(eligible)
            && ReferenceEquals(eligible.Requests.Request, retained)
            && ReferenceEquals(eligible.State.Contamination, eligible.Source.Contamination)
            && ReferenceEquals(eligible.State.Evidence, eligible.Source.Evidence)
            && ReferenceEquals(eligible.Source.Candidate, retainedCandidate)
            && CharacterObjectSabotageContaminationPolicy.CanApply(
                eligible.Source.Character,
                eligible.Source.PhysicalAccess,
                eligible.Source.WorldObjects,
                eligible.Source.Contamination,
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
            Fail("Contamination-gated sabotage-action request did not preserve exact scene state.");
            return;
        }

        GD.Print(
            "Sabotage-action contamination request smoke passed: unavailable, ineligible, stale, "
            + "replacement, null, and repeated inputs stayed inert; one request retained the "
            + "exact enemy actor and canonical contaminate action without starting work.");
        GetTree().Quit();
    }

    private SceneGraph Graph(string parentName) =>
        new(
            GetNode<TemporaryCharacterObjectSabotageContaminationSource>(
                $"../{parentName}/TemporaryCharacterObjectSabotageContaminationSource"),
            GetNode<CharacterObjectSabotageContaminationController>(
                $"../{parentName}/CharacterObjectSabotageContaminationController"),
            GetNode<CharacterSabotageActionContaminationRequestController>(
                $"../{parentName}/CharacterSabotageActionContaminationRequestController"));

    private static bool TryCreate(
        SceneGraph graph,
        WorldObjectContaminationSnapshot? contamination = null,
        DiscoveryEvidenceSnapshot? evidence = null,
        Character? actor = null,
        CharacterPhysicalAccessSnapshot? physicalAccess = null,
        WorldObjectSnapshot? worldObjects = null,
        CharacterObjectSabotageEvidence? candidate = null) =>
        graph.Requests.TryCreate(
            actor ?? graph.Source.Character,
            physicalAccess ?? graph.Source.PhysicalAccess,
            worldObjects ?? graph.Source.WorldObjects,
            contamination ?? graph.Source.Contamination,
            evidence ?? graph.Source.Evidence,
            candidate ?? graph.Source.Candidate);

    private static bool RejectsNulls(SceneGraph graph) =>
        ThrowsArgumentNull(() => graph.Requests.TryCreate(
            null!, graph.Source.PhysicalAccess, graph.Source.WorldObjects,
            graph.Source.Contamination, graph.Source.Evidence, graph.Source.Candidate))
        && ThrowsArgumentNull(() => graph.Requests.TryCreate(
            graph.Source.Character, null!, graph.Source.WorldObjects,
            graph.Source.Contamination, graph.Source.Evidence, graph.Source.Candidate))
        && ThrowsArgumentNull(() => graph.Requests.TryCreate(
            graph.Source.Character, graph.Source.PhysicalAccess, null!,
            graph.Source.Contamination, graph.Source.Evidence, graph.Source.Candidate))
        && ThrowsArgumentNull(() => graph.Requests.TryCreate(
            graph.Source.Character, graph.Source.PhysicalAccess, graph.Source.WorldObjects,
            null!, graph.Source.Evidence, graph.Source.Candidate))
        && ThrowsArgumentNull(() => graph.Requests.TryCreate(
            graph.Source.Character, graph.Source.PhysicalAccess, graph.Source.WorldObjects,
            graph.Source.Contamination, null!, graph.Source.Candidate))
        && ThrowsArgumentNull(() => graph.Requests.TryCreate(
            graph.Source.Character, graph.Source.PhysicalAccess, graph.Source.WorldObjects,
            graph.Source.Contamination, graph.Source.Evidence, null!));

    private static bool HasClosedSurface()
    {
        Type type = typeof(CharacterSabotageActionContaminationRequestController);
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
        TemporaryCharacterObjectSabotageContaminationSource Source,
        CharacterObjectSabotageContaminationController State,
        CharacterSabotageActionContaminationRequestController Requests);
}
