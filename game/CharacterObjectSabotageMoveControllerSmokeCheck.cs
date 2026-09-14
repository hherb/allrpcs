using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Discovery;
using Allrpcs.Domain.Identifiers;
using Allrpcs.Domain.Locations;
using Allrpcs.Domain.Perception;
using Allrpcs.Domain.WorldObjects;
using Godot;

namespace Allrpcs.Game;

/// <summary>Runs credential-free scene-owned recoverable move-sabotage checks.</summary>
public partial class CharacterObjectSabotageMoveControllerSmokeCheck : Node
{
    private static readonly string[] ExpectedMethodNames =
        ["TryApply", "TryInitialize", "TryRecover", "TryResolveConsideration"];

    /// <inheritdoc />
    public override void _Ready()
    {
        CharacterObjectSabotageMoveController controller = new();
        MoveInput input = CreateInput();
        WorldObjectPlacementSnapshot placements = new(
            [input.RetainedPlacement, input.SourcePlacement]);
        DiscoveryEvidenceSnapshot evidence = new([input.RetainedEvidence]);

        if (!RejectsNullInitialization(controller, placements, evidence)
            || !controller.TryInitialize(placements, evidence)
            || !HasExactState(controller, placements, evidence)
            || controller.TryInitialize(new WorldObjectPlacementSnapshot([]), evidence))
        {
            Fail("Move controller rejected exact one-time atomic initialization.");
            return;
        }

        if (!RejectsStaleApply(controller, input, placements, evidence)
            || !RejectsIneligibleApply(controller, input, placements, evidence)
            || !RejectsNullApply(controller, input, placements, evidence))
        {
            Fail("Move controller changed state after rejected apply input.");
            return;
        }

        if (!controller.TryApply(
                input.Character,
                input.PhysicalAccess,
                input.WorldObjects,
                placements,
                evidence,
                input.MoveCandidate)
            || controller.Placements is not { } movedPlacements
            || controller.Evidence is not { } movedEvidence
            || ReferenceEquals(movedPlacements, placements)
            || ReferenceEquals(movedEvidence, evidence)
            || !ReferenceEquals(movedPlacements.Entries[0], input.RetainedPlacement)
            || !ReferenceEquals(movedPlacements.Entries[1].WorldObject, input.Target)
            || !ReferenceEquals(movedPlacements.Entries[1].Location, input.Destination)
            || !ReferenceEquals(movedEvidence.Entries[0], input.RetainedEvidence)
            || !ReferenceEquals(movedEvidence.Entries[1], input.EvidenceDefinition))
        {
            Fail("Move controller failed atomic stable placement and evidence install.");
            return;
        }

        CharacterEvidenceViewSnapshot evidenceView = new(
            new CharacterId("seeker"),
            [input.EvidenceDefinition.EvidenceId]);
        CharacterObjectSabotageMoveRecoveryCandidate recoveryCandidate = new(
            input.MoveCandidate,
            input.SourcePlacement);
        if (!RejectsStaleRecovery(
                controller,
                movedPlacements,
                movedEvidence,
                evidenceView,
                recoveryCandidate)
            || !RejectsIneligibleRecovery(
                controller,
                movedPlacements,
                movedEvidence,
                recoveryCandidate)
            || !RejectsNullRecovery(
                controller,
                movedPlacements,
                movedEvidence,
                evidenceView,
                recoveryCandidate))
        {
            Fail("Move controller changed state after rejected recovery input.");
            return;
        }

        if (!controller.TryRecover(
                movedPlacements,
                movedEvidence,
                evidenceView,
                recoveryCandidate)
            || controller.Placements is not { } recoveredPlacements
            || !ReferenceEquals(controller.Evidence, movedEvidence)
            || ReferenceEquals(recoveredPlacements, movedPlacements)
            || !ReferenceEquals(recoveredPlacements.Entries[0], input.RetainedPlacement)
            || !ReferenceEquals(recoveredPlacements.Entries[1], input.SourcePlacement)
            || !HasExactState(controller, recoveredPlacements, movedEvidence)
            || !HasMinimalSurface())
        {
            Fail("Move controller failed exact recovery or exposed extra state.");
            return;
        }

        GD.Print(
            "Move controller smoke passed: one-time ownership, exact CAS apply and recovery, "
            + "stable unrelated entries, stale rejection, and atomic failure.");
        controller.Free();
        GetTree().Quit();
    }

    private static bool RejectsStaleApply(
        CharacterObjectSabotageMoveController controller,
        MoveInput input,
        WorldObjectPlacementSnapshot placements,
        DiscoveryEvidenceSnapshot evidence)
    {
        WorldObjectPlacementSnapshot stalePlacements = new(placements.Entries);
        DiscoveryEvidenceSnapshot staleEvidence = new(evidence.Entries);

        return !controller.TryApply(
                input.Character,
                input.PhysicalAccess,
                input.WorldObjects,
                stalePlacements,
                evidence,
                input.MoveCandidate)
            && !controller.TryApply(
                input.Character,
                input.PhysicalAccess,
                input.WorldObjects,
                placements,
                staleEvidence,
                input.MoveCandidate)
            && HasExactState(controller, placements, evidence);
    }

    private static bool RejectsIneligibleApply(
        CharacterObjectSabotageMoveController controller,
        MoveInput input,
        WorldObjectPlacementSnapshot placements,
        DiscoveryEvidenceSnapshot evidence)
    {
        CharacterPhysicalAccessSnapshot inaccessible = new(input.Character.Identity.Id, []);

        return !controller.TryApply(
                input.Character,
                inaccessible,
                input.WorldObjects,
                placements,
                evidence,
                input.MoveCandidate)
            && HasExactState(controller, placements, evidence);
    }

    private static bool RejectsNullApply(
        CharacterObjectSabotageMoveController controller,
        MoveInput input,
        WorldObjectPlacementSnapshot placements,
        DiscoveryEvidenceSnapshot evidence) =>
        ThrowsArgumentNull(() => controller.TryApply(
            null!, input.PhysicalAccess, input.WorldObjects, placements, evidence,
            input.MoveCandidate))
        && ThrowsArgumentNull(() => controller.TryApply(
            input.Character, null!, input.WorldObjects, placements, evidence,
            input.MoveCandidate))
        && ThrowsArgumentNull(() => controller.TryApply(
            input.Character, input.PhysicalAccess, null!, placements, evidence,
            input.MoveCandidate))
        && ThrowsArgumentNull(() => controller.TryApply(
            input.Character, input.PhysicalAccess, input.WorldObjects, null!, evidence,
            input.MoveCandidate))
        && ThrowsArgumentNull(() => controller.TryApply(
            input.Character, input.PhysicalAccess, input.WorldObjects, placements, null!,
            input.MoveCandidate))
        && ThrowsArgumentNull(() => controller.TryApply(
            input.Character, input.PhysicalAccess, input.WorldObjects, placements, evidence,
            null!))
        && HasExactState(controller, placements, evidence);

    private static bool RejectsStaleRecovery(
        CharacterObjectSabotageMoveController controller,
        WorldObjectPlacementSnapshot placements,
        DiscoveryEvidenceSnapshot evidence,
        CharacterEvidenceViewSnapshot evidenceView,
        CharacterObjectSabotageMoveRecoveryCandidate candidate)
    {
        WorldObjectPlacementSnapshot stalePlacements = new(placements.Entries);
        DiscoveryEvidenceSnapshot staleEvidence = new(evidence.Entries);

        return !controller.TryRecover(stalePlacements, evidence, evidenceView, candidate)
            && !controller.TryRecover(placements, staleEvidence, evidenceView, candidate)
            && HasExactState(controller, placements, evidence);
    }

    private static bool RejectsIneligibleRecovery(
        CharacterObjectSabotageMoveController controller,
        WorldObjectPlacementSnapshot placements,
        DiscoveryEvidenceSnapshot evidence,
        CharacterObjectSabotageMoveRecoveryCandidate candidate)
    {
        CharacterEvidenceViewSnapshot unseen = new(new CharacterId("seeker"), []);

        return !controller.TryRecover(placements, evidence, unseen, candidate)
            && HasExactState(controller, placements, evidence);
    }

    private static bool RejectsNullRecovery(
        CharacterObjectSabotageMoveController controller,
        WorldObjectPlacementSnapshot placements,
        DiscoveryEvidenceSnapshot evidence,
        CharacterEvidenceViewSnapshot evidenceView,
        CharacterObjectSabotageMoveRecoveryCandidate candidate) =>
        ThrowsArgumentNull(() => controller.TryRecover(null!, evidence, evidenceView, candidate))
        && ThrowsArgumentNull(() => controller.TryRecover(placements, null!, evidenceView, candidate))
        && ThrowsArgumentNull(() => controller.TryRecover(placements, evidence, null!, candidate))
        && ThrowsArgumentNull(() => controller.TryRecover(placements, evidence, evidenceView, null!))
        && HasExactState(controller, placements, evidence);

    private static bool RejectsNullInitialization(
        CharacterObjectSabotageMoveController controller,
        WorldObjectPlacementSnapshot placements,
        DiscoveryEvidenceSnapshot evidence) =>
        ThrowsArgumentNull(() => controller.TryInitialize(null!, evidence))
        && ThrowsArgumentNull(() => controller.TryInitialize(placements, null!))
        && controller.Placements is null
        && controller.Evidence is null;

    private static bool HasExactState(
        CharacterObjectSabotageMoveController controller,
        WorldObjectPlacementSnapshot placements,
        DiscoveryEvidenceSnapshot evidence) =>
        ReferenceEquals(controller.Placements, placements)
        && ReferenceEquals(controller.Evidence, evidence);

    private static bool HasMinimalSurface()
    {
        Type type = typeof(CharacterObjectSabotageMoveController);
        string[] methodNames = type.GetMethods(
                System.Reflection.BindingFlags.Public
                | System.Reflection.BindingFlags.Instance
                | System.Reflection.BindingFlags.DeclaredOnly)
            .Where(method => !method.IsSpecialName)
            .Select(method => method.Name)
            .Order()
            .ToArray();
        System.Reflection.PropertyInfo[] properties = type.GetProperties(
            System.Reflection.BindingFlags.Public
            | System.Reflection.BindingFlags.Instance
            | System.Reflection.BindingFlags.DeclaredOnly);

        return methodNames.SequenceEqual(ExpectedMethodNames)
            && properties.Length == 2
            && properties.All(property => property.SetMethod is not { IsPublic: true });
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

    private static MoveInput CreateInput()
    {
        Character character = RepresentativeEnemyAgentCharacterSource.Create();
        WorldObject target = WorldObject("village-water-cask");
        Location sourceLocation = Location("village-square");
        Location destination = Location("abandoned-wagon");
        CharacterObjectSabotageOccurrence occurrence = new(
            new CharacterObjectSabotageProposal(
                character.Identity.Id,
                new CharacterActionAccessRequirement(
                    target.Identity.Id,
                    "assess-local-influence",
                    character.Knowledge.KnowledgeReferences.Single()),
                CharacterSabotageActionKind.Move),
            target);
        DiscoveryEvidence evidenceDefinition = new(
            new EvidenceId("cask-drag-marks"),
            target.Identity.Id,
            VisibilityState.Noticed,
            false);
        CharacterObjectSabotageMoveCandidate moveCandidate = new(
            new CharacterObjectSabotageMoveDestination(occurrence, destination),
            new CharacterObjectSabotageEvidence(occurrence, evidenceDefinition));
        WorldObject retainedObject = WorldObject("storehouse-ledger");

        return new MoveInput(
            character,
            target,
            destination,
            new WorldObjectPlacement(target, sourceLocation),
            new WorldObjectPlacement(retainedObject, Location("storehouse")),
            new DiscoveryEvidence(
                new EvidenceId("retained-evidence"),
                retainedObject.Identity.Id,
                VisibilityState.Identified,
                false),
            evidenceDefinition,
            new CharacterPhysicalAccessSnapshot(
                character.Identity.Id,
                [target.Identity.Id]),
            new WorldObjectSnapshot([target]),
            moveCandidate);
    }

    private static WorldObject WorldObject(string id) =>
        new(
            new WorldObjectIdentity(new ObjectId(id), $"Object {id}"),
            new WorldObjectDescription($"Description for {id}."));

    private static Location Location(string id) =>
        new(
            new LocationIdentity(new LocationId(id), $"Location {id}"),
            new LocationDescription($"Description for {id}."));

    private sealed record MoveInput(
        Character Character,
        WorldObject Target,
        Location Destination,
        WorldObjectPlacement SourcePlacement,
        WorldObjectPlacement RetainedPlacement,
        DiscoveryEvidence RetainedEvidence,
        DiscoveryEvidence EvidenceDefinition,
        CharacterPhysicalAccessSnapshot PhysicalAccess,
        WorldObjectSnapshot WorldObjects,
        CharacterObjectSabotageMoveCandidate MoveCandidate);

    private void Fail(string message)
    {
        GD.PushError(message);
        GetTree().Quit(1);
    }
}
