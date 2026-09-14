using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Discovery;
using Allrpcs.Domain.Identifiers;
using Allrpcs.Domain.Locations;
using Allrpcs.Domain.Perception;
using Allrpcs.Domain.WorldObjects;
using Godot;

namespace Allrpcs.Game;

/// <summary>Checks exact scene resolution of accepted move consideration results.</summary>
public partial class CharacterObjectSabotageMoveConsiderationControllerSmokeCheck : Node
{
    /// <inheritdoc />
    public override void _Ready()
    {
        ConsiderationInput input = CreateInput();

        if (!ResolvesSelection(input)
            || !ResolvesAbstention(input)
            || !RejectsStaleUninitializedAndNull(input))
        {
            GD.PushError("Move controller failed exact consideration-result resolution.");
            GetTree().Quit(1);
            return;
        }

        GD.Print(
            "Move consideration controller smoke passed: exact selection installs both fresh "
            + "snapshots atomically, abstention retains both exact snapshots, stale, null, "
            + "uninitialized, and repeated results fail closed, and surrounding state is unchanged.");
        GetTree().Quit();
    }

    private static bool ResolvesSelection(ConsiderationInput input)
    {
        CharacterObjectSabotageMoveController controller = new();
        CharacterObjectSabotageMoveConsiderationResult result = CreateResult(
            input,
            input.Placements,
            input.Evidence,
            input.Candidate.Evidence.Evidence.EvidenceId);

        bool initialized = controller.TryInitialize(input.Placements, input.Evidence);
        bool resolved = controller.TryResolveConsideration(result);
        bool accepted = controller.Placements is { } resolvedPlacements
            && controller.Evidence is { } resolvedEvidence
            && !ReferenceEquals(resolvedPlacements, input.Placements)
            && !ReferenceEquals(resolvedEvidence, input.Evidence)
            && ReferenceEquals(resolvedPlacements.Entries[0], input.RetainedPlacement)
            && ReferenceEquals(resolvedPlacements.Entries[1].WorldObject, input.Target)
            && ReferenceEquals(resolvedPlacements.Entries[1].Location, input.Destination)
            && ReferenceEquals(resolvedEvidence.Entries[0], input.RetainedEvidence)
            && ReferenceEquals(resolvedEvidence.Entries[1], input.Candidate.Evidence.Evidence)
            && !controller.TryResolveConsideration(result)
            && ReferenceEquals(controller.Placements, resolvedPlacements)
            && ReferenceEquals(controller.Evidence, resolvedEvidence)
            && HasUnchangedInputs(input, result.Request);

        controller.Free();
        return initialized && resolved && accepted;
    }

    private static bool ResolvesAbstention(ConsiderationInput input)
    {
        CharacterObjectSabotageMoveController controller = new();
        CharacterObjectSabotageMoveConsiderationResult result = CreateResult(
            input,
            input.Placements,
            input.Evidence,
            null);

        bool accepted = controller.TryInitialize(input.Placements, input.Evidence)
            && controller.TryResolveConsideration(result)
            && HasExactState(controller, input.Placements, input.Evidence)
            && !controller.TryResolveConsideration(result)
            && HasExactState(controller, input.Placements, input.Evidence)
            && HasUnchangedInputs(input, result.Request);

        controller.Free();
        return accepted;
    }

    private static bool RejectsStaleUninitializedAndNull(ConsiderationInput input)
    {
        CharacterObjectSabotageMoveController controller = new();
        CharacterObjectSabotageMoveConsiderationResult exact = CreateResult(
            input,
            input.Placements,
            input.Evidence,
            input.Candidate.Evidence.Evidence.EvidenceId);
        CharacterObjectSabotageMoveConsiderationResult stalePlacements = CreateResult(
            input,
            new WorldObjectPlacementSnapshot(input.Placements.Entries),
            input.Evidence,
            input.Candidate.Evidence.Evidence.EvidenceId);
        CharacterObjectSabotageMoveConsiderationResult staleEvidence = CreateResult(
            input,
            input.Placements,
            new DiscoveryEvidenceSnapshot(input.Evidence.Entries),
            input.Candidate.Evidence.Evidence.EvidenceId);

        bool rejectedUninitialized = !controller.TryResolveConsideration(exact)
            && controller.Placements is null
            && controller.Evidence is null;
        bool rejected = controller.TryInitialize(input.Placements, input.Evidence)
            && !controller.TryResolveConsideration(stalePlacements)
            && !controller.TryResolveConsideration(staleEvidence)
            && ThrowsArgumentNull(() => controller.TryResolveConsideration(null!))
            && HasExactState(controller, input.Placements, input.Evidence)
            && HasUnchangedInputs(input, exact.Request)
            && HasUnchangedInputs(input, stalePlacements.Request)
            && HasUnchangedInputs(input, staleEvidence.Request);

        controller.Free();
        return rejectedUninitialized && rejected;
    }

    private static CharacterObjectSabotageMoveConsiderationResult CreateResult(
        ConsiderationInput input,
        WorldObjectPlacementSnapshot placements,
        DiscoveryEvidenceSnapshot evidence,
        EvidenceId? selectedEvidenceId) =>
        new(
            new CharacterObjectSabotageMoveConsiderationRequest(
                input.Character,
                input.PhysicalAccess,
                input.WorldObjects,
                placements,
                evidence,
                [input.Candidate]),
            new CharacterObjectSabotageMoveDecision(
                input.Character.Identity.Id,
                selectedEvidenceId));

    private static bool HasUnchangedInputs(
        ConsiderationInput input,
        CharacterObjectSabotageMoveConsiderationRequest request) =>
        ReferenceEquals(request.Character, input.Character)
        && ReferenceEquals(request.PhysicalAccess, input.PhysicalAccess)
        && ReferenceEquals(request.WorldObjects, input.WorldObjects)
        && ReferenceEquals(request.Candidates.Single(), input.Candidate)
        && ReferenceEquals(input.Placements.Entries[0], input.RetainedPlacement)
        && ReferenceEquals(input.Placements.Entries[1], input.SourcePlacement)
        && ReferenceEquals(input.Evidence.Entries.Single(), input.RetainedEvidence);

    private static bool HasExactState(
        CharacterObjectSabotageMoveController controller,
        WorldObjectPlacementSnapshot placements,
        DiscoveryEvidenceSnapshot evidence) =>
        ReferenceEquals(controller.Placements, placements)
        && ReferenceEquals(controller.Evidence, evidence);

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

    private static ConsiderationInput CreateInput()
    {
        Character character = RepresentativeEnemyAgentCharacterSource.Create();
        WorldObject target = WorldObject("village-water-cask");
        Location source = Location("village-square");
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
        CharacterObjectSabotageMoveCandidate candidate = new(
            new CharacterObjectSabotageMoveDestination(occurrence, destination),
            new CharacterObjectSabotageEvidence(
                occurrence,
                new DiscoveryEvidence(
                    new EvidenceId("cask-drag-marks"),
                    target.Identity.Id,
                    VisibilityState.Noticed,
                    false)));
        WorldObject retainedObject = WorldObject("storehouse-ledger");
        WorldObjectPlacement sourcePlacement = new(target, source);
        WorldObjectPlacement retainedPlacement = new(retainedObject, Location("storehouse"));
        DiscoveryEvidence retainedEvidence = new(
            new EvidenceId("retained-evidence"),
            retainedObject.Identity.Id,
            VisibilityState.Identified,
            false);

        return new ConsiderationInput(
            character,
            target,
            destination,
            new CharacterPhysicalAccessSnapshot(character.Identity.Id, [target.Identity.Id]),
            new WorldObjectSnapshot([target]),
            candidate,
            new WorldObjectPlacementSnapshot([retainedPlacement, sourcePlacement]),
            new DiscoveryEvidenceSnapshot([retainedEvidence]),
            sourcePlacement,
            retainedPlacement,
            retainedEvidence);
    }

    private static WorldObject WorldObject(string id) =>
        new(
            new WorldObjectIdentity(new ObjectId(id), $"Object {id}"),
            new WorldObjectDescription($"Description for {id}."));

    private static Location Location(string id) =>
        new(
            new LocationIdentity(new LocationId(id), $"Location {id}"),
            new LocationDescription($"Description for {id}."));

    private sealed record ConsiderationInput(
        Character Character,
        WorldObject Target,
        Location Destination,
        CharacterPhysicalAccessSnapshot PhysicalAccess,
        WorldObjectSnapshot WorldObjects,
        CharacterObjectSabotageMoveCandidate Candidate,
        WorldObjectPlacementSnapshot Placements,
        DiscoveryEvidenceSnapshot Evidence,
        WorldObjectPlacement SourcePlacement,
        WorldObjectPlacement RetainedPlacement,
        DiscoveryEvidence RetainedEvidence);
}
