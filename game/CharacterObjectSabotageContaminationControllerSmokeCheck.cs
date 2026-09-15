using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Discovery;
using Allrpcs.Domain.Identifiers;
using Allrpcs.Domain.Perception;
using Allrpcs.Domain.WorldObjects;
using Godot;

namespace Allrpcs.Game;

/// <summary>Checks exact scene ownership and consideration of recoverable contamination.</summary>
public partial class CharacterObjectSabotageContaminationControllerSmokeCheck : Node
{
    /// <inheritdoc />
    public override void _Ready()
    {
        ContaminationInput input = CreateInput();

        if (!ResolvesSelection(input)
            || !ResolvesAbstention(input)
            || !AppliesAndRecovers(input)
            || !RejectsStaleUninitializedAndNull(input))
        {
            GD.PushError("Contamination controller failed exact scene-state ownership.");
            GetTree().Quit(1);
            return;
        }

        GD.Print(
            "Contamination controller smoke passed: exact selection installs both fresh snapshots "
            + "atomically, abstention retains exact state, direct application and visible-evidence "
            + "recovery are stale-safe, and invalid inputs leave scene state unchanged.");
        GetTree().Quit();
    }

    private static bool ResolvesSelection(ContaminationInput input)
    {
        CharacterObjectSabotageContaminationController controller = new();
        CharacterObjectSabotageContaminationConsiderationResult result = CreateResult(
            input,
            input.Contamination,
            input.Evidence,
            input.Candidate.Evidence.EvidenceId);

        bool initialized = controller.TryInitialize(input.Contamination, input.Evidence);
        bool resolved = controller.TryResolveConsideration(result);
        bool accepted = controller.Contamination is { } resolvedContamination
            && controller.Evidence is { } resolvedEvidence
            && !ReferenceEquals(resolvedContamination, input.Contamination)
            && !ReferenceEquals(resolvedEvidence, input.Evidence)
            && ReferenceEquals(resolvedContamination.ContaminatedObjectIds[0], input.RetainedTargetId)
            && ReferenceEquals(resolvedContamination.ContaminatedObjectIds[1], input.Target.Identity.Id)
            && ReferenceEquals(resolvedEvidence.Entries[0], input.RetainedEvidence)
            && ReferenceEquals(resolvedEvidence.Entries[1], input.Candidate.Evidence)
            && !controller.TryResolveConsideration(result)
            && ReferenceEquals(controller.Contamination, resolvedContamination)
            && ReferenceEquals(controller.Evidence, resolvedEvidence)
            && HasUnchangedInputs(input, result.Request);

        controller.Free();
        return initialized && resolved && accepted;
    }

    private static bool ResolvesAbstention(ContaminationInput input)
    {
        CharacterObjectSabotageContaminationController controller = new();
        CharacterObjectSabotageContaminationConsiderationResult result = CreateResult(
            input,
            input.Contamination,
            input.Evidence,
            null);

        bool accepted = controller.TryInitialize(input.Contamination, input.Evidence)
            && controller.TryResolveConsideration(result)
            && HasExactState(controller, input.Contamination, input.Evidence)
            && !controller.TryResolveConsideration(result)
            && HasExactState(controller, input.Contamination, input.Evidence)
            && HasUnchangedInputs(input, result.Request);

        controller.Free();
        return accepted;
    }

    private static bool AppliesAndRecovers(ContaminationInput input)
    {
        CharacterObjectSabotageContaminationController controller = new();
        bool initialized = controller.TryInitialize(input.Contamination, input.Evidence);
        bool applied = controller.TryApply(
            input.Character,
            input.PhysicalAccess,
            input.WorldObjects,
            input.Contamination,
            input.Evidence,
            input.Candidate);
        WorldObjectContaminationSnapshot? appliedContamination = controller.Contamination;
        DiscoveryEvidenceSnapshot? appliedEvidence = controller.Evidence;
        CharacterEvidenceViewSnapshot evidenceView = new(
            new CharacterId("investigator"),
            [input.Candidate.Evidence.EvidenceId]);
        bool recovered = appliedContamination is not null
            && appliedEvidence is not null
            && controller.TryRecover(
                appliedContamination,
                appliedEvidence,
                evidenceView,
                input.Candidate);
        bool accepted = appliedEvidence is { } exactAppliedEvidence
            && controller.Contamination is { } recoveredContamination
            && ReferenceEquals(controller.Evidence, exactAppliedEvidence)
            && ReferenceEquals(recoveredContamination.ContaminatedObjectIds.Single(), input.RetainedTargetId)
            && ReferenceEquals(exactAppliedEvidence.Entries[1], input.Candidate.Evidence)
            && !controller.TryRecover(
                appliedContamination!,
                appliedEvidence!,
                evidenceView,
                input.Candidate);

        controller.Free();
        return initialized && applied && recovered && accepted;
    }

    private static bool RejectsStaleUninitializedAndNull(ContaminationInput input)
    {
        CharacterObjectSabotageContaminationController controller = new();
        CharacterObjectSabotageContaminationConsiderationResult exact = CreateResult(
            input,
            input.Contamination,
            input.Evidence,
            input.Candidate.Evidence.EvidenceId);
        CharacterObjectSabotageContaminationConsiderationResult staleContamination = CreateResult(
            input,
            new WorldObjectContaminationSnapshot(input.Contamination.ContaminatedObjectIds),
            input.Evidence,
            input.Candidate.Evidence.EvidenceId);
        CharacterObjectSabotageContaminationConsiderationResult staleEvidence = CreateResult(
            input,
            input.Contamination,
            new DiscoveryEvidenceSnapshot(input.Evidence.Entries),
            input.Candidate.Evidence.EvidenceId);

        bool rejectedUninitialized = !controller.TryResolveConsideration(exact)
            && controller.Contamination is null
            && controller.Evidence is null;
        bool rejected = controller.TryInitialize(input.Contamination, input.Evidence)
            && !controller.TryResolveConsideration(staleContamination)
            && !controller.TryResolveConsideration(staleEvidence)
            && ThrowsArgumentNull(() => controller.TryResolveConsideration(null!))
            && HasExactState(controller, input.Contamination, input.Evidence)
            && HasUnchangedInputs(input, exact.Request)
            && HasUnchangedInputs(input, staleContamination.Request)
            && HasUnchangedInputs(input, staleEvidence.Request);

        controller.Free();
        return rejectedUninitialized && rejected;
    }

    private static CharacterObjectSabotageContaminationConsiderationResult CreateResult(
        ContaminationInput input,
        WorldObjectContaminationSnapshot contamination,
        DiscoveryEvidenceSnapshot evidence,
        EvidenceId? selectedEvidenceId) =>
        new(
            new CharacterObjectSabotageContaminationConsiderationRequest(
                input.Character,
                input.PhysicalAccess,
                input.WorldObjects,
                contamination,
                evidence,
                [input.Candidate]),
            new CharacterObjectSabotageContaminationDecision(
                input.Character.Identity.Id,
                selectedEvidenceId));

    private static bool HasUnchangedInputs(
        ContaminationInput input,
        CharacterObjectSabotageContaminationConsiderationRequest request) =>
        ReferenceEquals(request.Character, input.Character)
        && ReferenceEquals(request.PhysicalAccess, input.PhysicalAccess)
        && ReferenceEquals(request.WorldObjects, input.WorldObjects)
        && ReferenceEquals(request.Candidates.Single(), input.Candidate)
        && ReferenceEquals(input.Contamination.ContaminatedObjectIds.Single(), input.RetainedTargetId)
        && ReferenceEquals(input.Evidence.Entries.Single(), input.RetainedEvidence);

    private static bool HasExactState(
        CharacterObjectSabotageContaminationController controller,
        WorldObjectContaminationSnapshot contamination,
        DiscoveryEvidenceSnapshot evidence) =>
        ReferenceEquals(controller.Contamination, contamination)
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

    private static ContaminationInput CreateInput()
    {
        Character character = RepresentativeEnemyAgentCharacterSource.Create();
        WorldObject target = WorldObject("village-water-cask");
        CharacterObjectSabotageOccurrence occurrence = new(
            new CharacterObjectSabotageProposal(
                character.Identity.Id,
                new CharacterActionAccessRequirement(
                    target.Identity.Id,
                    "assess-local-influence",
                    character.Knowledge.KnowledgeReferences.Single()),
                CharacterSabotageActionKind.Contaminate),
            target);
        CharacterObjectSabotageEvidence candidate = new(
            occurrence,
            new DiscoveryEvidence(
                new EvidenceId("bitter-cask-residue"),
                target.Identity.Id,
                VisibilityState.Noticed,
                false));
        ObjectId retainedTargetId = new("healers-mortar");
        DiscoveryEvidence retainedEvidence = new(
            new EvidenceId("retained-evidence"),
            retainedTargetId,
            VisibilityState.Identified,
            false);

        return new(
            character,
            target,
            new CharacterPhysicalAccessSnapshot(character.Identity.Id, [target.Identity.Id]),
            new WorldObjectSnapshot([target]),
            candidate,
            new WorldObjectContaminationSnapshot([retainedTargetId]),
            new DiscoveryEvidenceSnapshot([retainedEvidence]),
            retainedTargetId,
            retainedEvidence);
    }

    private static WorldObject WorldObject(string id) =>
        new(
            new WorldObjectIdentity(new ObjectId(id), $"Object {id}"),
            new WorldObjectDescription($"Description for {id}."));

    private sealed record ContaminationInput(
        Character Character,
        WorldObject Target,
        CharacterPhysicalAccessSnapshot PhysicalAccess,
        WorldObjectSnapshot WorldObjects,
        CharacterObjectSabotageEvidence Candidate,
        WorldObjectContaminationSnapshot Contamination,
        DiscoveryEvidenceSnapshot Evidence,
        ObjectId RetainedTargetId,
        DiscoveryEvidence RetainedEvidence);
}
