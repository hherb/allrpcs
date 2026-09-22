using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Discovery;
using Allrpcs.Domain.WorldObjects;
using Godot;

namespace Allrpcs.Game;

/// <summary>Checks one concealment-gated sabotage-action request from exact scene state.</summary>
public partial class CharacterSabotageActionConcealmentRequestControllerSmokeCheck : Node
{
    /// <inheritdoc />
    public override void _Ready()
    {
        SceneGraph unavailable = Graph("Unavailable");
        SceneGraph ineligible = Graph("Ineligible");
        SceneGraph eligible = Graph("Eligible");

        bool unavailableRejected = !TryCreate(unavailable)
            && unavailable.Requests.Request is null
            && unavailable.State.Concealment is null
            && unavailable.State.Evidence is null;

        WorldObjectConcealmentSnapshot alreadyConcealed = new(
            [ineligible.Source.Candidate.Occurrence.Target.Identity.Id]);
        bool ineligibleRejected = ineligible.State.TryInitialize(
                alreadyConcealed,
                ineligible.Source.Evidence)
            && !TryCreate(ineligible, alreadyConcealed, ineligible.Source.Evidence)
            && ineligible.Requests.Request is null
            && ReferenceEquals(ineligible.State.Concealment, alreadyConcealed)
            && ReferenceEquals(ineligible.State.Evidence, ineligible.Source.Evidence);

        bool initialized = eligible.State.TryInitialize(
            eligible.Source.Concealment,
            eligible.Source.Evidence);
        Character replacementActor = RepresentativeEnemyAgentCharacterSource.Create();
        CharacterPhysicalAccessSnapshot replacementAccess = new(
            eligible.Source.Character.Identity.Id,
            eligible.Source.PhysicalAccess.AccessibleObjectIds);
        WorldObjectSnapshot replacementWorldObjects = new(eligible.Source.WorldObjects.Entries);
        WorldObjectConcealmentSnapshot replacementConcealment = new([]);
        DiscoveryEvidenceSnapshot replacementEvidence = new([]);
        CharacterObjectSabotageEvidence replacementCandidate = new(
            eligible.Source.Candidate.Occurrence,
            eligible.Source.Candidate.Evidence);
        bool staleAndReplacementRejected = initialized
            && !TryCreate(eligible, replacementConcealment, eligible.Source.Evidence)
            && !TryCreate(eligible, eligible.Source.Concealment, replacementEvidence)
            && !TryCreate(eligible, actor: replacementActor)
            && !TryCreate(eligible, physicalAccess: replacementAccess)
            && !TryCreate(eligible, worldObjects: replacementWorldObjects)
            && !TryCreate(eligible, candidate: replacementCandidate)
            && eligible.Requests.Request is null;
        bool nullsRejected = ThrowsArgumentNull(() => eligible.Requests.TryCreate(
                null!,
                eligible.Source.PhysicalAccess,
                eligible.Source.WorldObjects,
                eligible.Source.Concealment,
                eligible.Source.Evidence,
                eligible.Source.Candidate))
            && ThrowsArgumentNull(() => eligible.Requests.TryCreate(
                eligible.Source.Character,
                null!,
                eligible.Source.WorldObjects,
                eligible.Source.Concealment,
                eligible.Source.Evidence,
                eligible.Source.Candidate))
            && ThrowsArgumentNull(() => eligible.Requests.TryCreate(
                eligible.Source.Character,
                eligible.Source.PhysicalAccess,
                null!,
                eligible.Source.Concealment,
                eligible.Source.Evidence,
                eligible.Source.Candidate))
            && ThrowsArgumentNull(() => eligible.Requests.TryCreate(
                eligible.Source.Character,
                eligible.Source.PhysicalAccess,
                eligible.Source.WorldObjects,
                null!,
                eligible.Source.Evidence,
                eligible.Source.Candidate))
            && ThrowsArgumentNull(() => eligible.Requests.TryCreate(
                eligible.Source.Character,
                eligible.Source.PhysicalAccess,
                eligible.Source.WorldObjects,
                eligible.Source.Concealment,
                null!,
                eligible.Source.Candidate))
            && ThrowsArgumentNull(() => eligible.Requests.TryCreate(
                eligible.Source.Character,
                eligible.Source.PhysicalAccess,
                eligible.Source.WorldObjects,
                eligible.Source.Concealment,
                eligible.Source.Evidence,
                null!));
        bool created = TryCreate(eligible)
            && eligible.Requests.Request is { } request
            && ReferenceEquals(request.Actor, eligible.Source.Character)
            && request.ActionKinds.Count == 1
            && ReferenceEquals(
                request.ActionKinds.Single(),
                CharacterSabotageActionKind.Conceal);

        CharacterSabotageActionConsiderationRequest retained = eligible.Requests.Request!;
        bool repeatedIsInert = !TryCreate(eligible)
            && ReferenceEquals(eligible.Requests.Request, retained)
            && ReferenceEquals(eligible.State.Concealment, eligible.Source.Concealment)
            && ReferenceEquals(eligible.State.Evidence, eligible.Source.Evidence)
            && ReferenceEquals(
                eligible.Source.Candidates.Single(),
                eligible.Source.Candidate)
            && CharacterObjectSabotageConcealmentPolicy.CanApply(
                eligible.Source.Character,
                eligible.Source.PhysicalAccess,
                eligible.Source.WorldObjects,
                eligible.Source.Concealment,
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
            Fail("Concealment-gated sabotage-action request did not preserve exact scene state.");
            return;
        }

        GD.Print(
            "Sabotage-action concealment request smoke passed: unavailable, ineligible, stale, "
            + "replacement, null, and repeated inputs stayed inert; one request retained the "
            + "exact enemy actor and canonical conceal action without starting work.");
        GetTree().Quit();
    }

    private SceneGraph Graph(string parentName) =>
        new(
            GetNode<TemporaryCharacterObjectSabotageSource>(
                $"../{parentName}/TemporaryCharacterObjectSabotageSource"),
            GetNode<CharacterObjectSabotageConcealmentController>(
                $"../{parentName}/CharacterObjectSabotageConcealmentController"),
            GetNode<CharacterSabotageActionConcealmentRequestController>(
                $"../{parentName}/CharacterSabotageActionConcealmentRequestController"));

    private static bool TryCreate(
        SceneGraph graph,
        WorldObjectConcealmentSnapshot? concealment = null,
        DiscoveryEvidenceSnapshot? evidence = null,
        Character? actor = null,
        CharacterPhysicalAccessSnapshot? physicalAccess = null,
        WorldObjectSnapshot? worldObjects = null,
        CharacterObjectSabotageEvidence? candidate = null) =>
        graph.Requests.TryCreate(
            actor ?? graph.Source.Character,
            physicalAccess ?? graph.Source.PhysicalAccess,
            worldObjects ?? graph.Source.WorldObjects,
            concealment ?? graph.Source.Concealment,
            evidence ?? graph.Source.Evidence,
            candidate ?? graph.Source.Candidate);

    private static bool HasClosedSurface()
    {
        Type type = typeof(CharacterSabotageActionConcealmentRequestController);
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
        TemporaryCharacterObjectSabotageSource Source,
        CharacterObjectSabotageConcealmentController State,
        CharacterSabotageActionConcealmentRequestController Requests);
}
