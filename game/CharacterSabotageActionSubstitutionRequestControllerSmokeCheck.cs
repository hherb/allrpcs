using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Discovery;
using Allrpcs.Domain.WorldObjects;
using Godot;

namespace Allrpcs.Game;

/// <summary>Checks one substitution-gated sabotage-action request from exact scene state.</summary>
public partial class CharacterSabotageActionSubstitutionRequestControllerSmokeCheck : Node
{
    /// <inheritdoc />
    public override void _Ready()
    {
        SceneGraph unavailable = Graph("Unavailable");
        SceneGraph ineligible = Graph("Ineligible");
        SceneGraph eligible = Graph("Eligible");

        bool unavailableRejected = !TryCreate(unavailable)
            && unavailable.Requests.Request is null
            && unavailable.State.Substitutions is null
            && unavailable.State.Evidence is null;

        CharacterObjectSabotageSubstitutionSnapshot alreadySubstituted = new(
            [ineligible.Source.Candidate.Substitution]);
        bool ineligibleRejected = ineligible.State.TryInitialize(
                alreadySubstituted,
                ineligible.Source.Evidence)
            && !TryCreate(ineligible, alreadySubstituted, ineligible.Source.Evidence)
            && ineligible.Requests.Request is null
            && ReferenceEquals(ineligible.State.Substitutions, alreadySubstituted)
            && ReferenceEquals(ineligible.State.Evidence, ineligible.Source.Evidence);

        bool initialized = eligible.State.TryInitialize(
            eligible.Source.Substitutions,
            eligible.Source.Evidence);
        CharacterObjectSabotageSubstitutionCandidate retainedCandidate =
            eligible.Source.Candidate;
        Character replacementActor = RepresentativeEnemyAgentCharacterSource.Create();
        CharacterPhysicalAccessSnapshot replacementAccess = new(
            eligible.Source.Character.Identity.Id,
            eligible.Source.PhysicalAccess.AccessibleObjectIds);
        WorldObjectSnapshot replacementWorldObjects = new(eligible.Source.WorldObjects.Entries);
        CharacterObjectSabotageSubstitutionSnapshot replacementSubstitutions = new([]);
        DiscoveryEvidenceSnapshot replacementEvidence = new([]);
        CharacterObjectSabotageSubstitutionCandidate replacementCandidate = new(
            eligible.Source.Candidate.Substitution,
            eligible.Source.Candidate.Evidence);
        bool staleAndReplacementRejected = initialized
            && !TryCreate(eligible, replacementSubstitutions, eligible.Source.Evidence)
            && !TryCreate(eligible, eligible.Source.Substitutions, replacementEvidence)
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
                CharacterSabotageActionKind.Substitute);

        CharacterSabotageActionConsiderationRequest retained = eligible.Requests.Request!;
        bool repeatedIsInert = !TryCreate(eligible)
            && ReferenceEquals(eligible.Requests.Request, retained)
            && ReferenceEquals(eligible.State.Substitutions, eligible.Source.Substitutions)
            && ReferenceEquals(eligible.State.Evidence, eligible.Source.Evidence)
            && ReferenceEquals(eligible.Source.Candidate, retainedCandidate)
            && CharacterObjectSabotageSubstitutionPolicy.CanApply(
                eligible.Source.Character,
                eligible.Source.PhysicalAccess,
                eligible.Source.WorldObjects,
                eligible.Source.Substitutions,
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
            Fail("Substitution-gated sabotage-action request did not preserve exact scene state.");
            return;
        }

        GD.Print(
            "Sabotage-action substitution request smoke passed: unavailable, ineligible, stale, "
            + "replacement, null, and repeated inputs stayed inert; one request retained the "
            + "exact enemy actor and canonical substitute action without starting work.");
        GetTree().Quit();
    }

    private SceneGraph Graph(string parentName) =>
        new(
            GetNode<TemporaryCharacterObjectSabotageSubstitutionSource>(
                $"../{parentName}/TemporaryCharacterObjectSabotageSubstitutionSource"),
            GetNode<CharacterObjectSabotageSubstitutionController>(
                $"../{parentName}/CharacterObjectSabotageSubstitutionController"),
            GetNode<CharacterSabotageActionSubstitutionRequestController>(
                $"../{parentName}/CharacterSabotageActionSubstitutionRequestController"));

    private static bool TryCreate(
        SceneGraph graph,
        CharacterObjectSabotageSubstitutionSnapshot? substitutions = null,
        DiscoveryEvidenceSnapshot? evidence = null,
        Character? actor = null,
        CharacterPhysicalAccessSnapshot? physicalAccess = null,
        WorldObjectSnapshot? worldObjects = null,
        CharacterObjectSabotageSubstitutionCandidate? candidate = null) =>
        graph.Requests.TryCreate(
            actor ?? graph.Source.Character,
            physicalAccess ?? graph.Source.PhysicalAccess,
            worldObjects ?? graph.Source.WorldObjects,
            substitutions ?? graph.Source.Substitutions,
            evidence ?? graph.Source.Evidence,
            candidate ?? graph.Source.Candidate);

    private static bool RejectsNulls(SceneGraph graph) =>
        ThrowsArgumentNull(() => graph.Requests.TryCreate(
            null!, graph.Source.PhysicalAccess, graph.Source.WorldObjects,
            graph.Source.Substitutions, graph.Source.Evidence, graph.Source.Candidate))
        && ThrowsArgumentNull(() => graph.Requests.TryCreate(
            graph.Source.Character, null!, graph.Source.WorldObjects,
            graph.Source.Substitutions, graph.Source.Evidence, graph.Source.Candidate))
        && ThrowsArgumentNull(() => graph.Requests.TryCreate(
            graph.Source.Character, graph.Source.PhysicalAccess, null!,
            graph.Source.Substitutions, graph.Source.Evidence, graph.Source.Candidate))
        && ThrowsArgumentNull(() => graph.Requests.TryCreate(
            graph.Source.Character, graph.Source.PhysicalAccess, graph.Source.WorldObjects,
            null!, graph.Source.Evidence, graph.Source.Candidate))
        && ThrowsArgumentNull(() => graph.Requests.TryCreate(
            graph.Source.Character, graph.Source.PhysicalAccess, graph.Source.WorldObjects,
            graph.Source.Substitutions, null!, graph.Source.Candidate))
        && ThrowsArgumentNull(() => graph.Requests.TryCreate(
            graph.Source.Character, graph.Source.PhysicalAccess, graph.Source.WorldObjects,
            graph.Source.Substitutions, graph.Source.Evidence, null!));

    private static bool HasClosedSurface()
    {
        Type type = typeof(CharacterSabotageActionSubstitutionRequestController);
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

    private sealed record SceneGraph(
        TemporaryCharacterObjectSabotageSubstitutionSource Source,
        CharacterObjectSabotageSubstitutionController State,
        CharacterSabotageActionSubstitutionRequestController Requests);
}
