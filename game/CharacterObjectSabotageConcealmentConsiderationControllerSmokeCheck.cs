using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Discovery;
using Allrpcs.Domain.Identifiers;
using Allrpcs.Domain.Perception;
using Allrpcs.Domain.WorldObjects;
using Godot;

namespace Allrpcs.Game;

/// <summary>Checks exact scene resolution of accepted concealment consideration results.</summary>
public partial class CharacterObjectSabotageConcealmentConsiderationControllerSmokeCheck : Node
{
    /// <inheritdoc />
    public override void _Ready()
    {
        ConsiderationInput input = CreateInput();

        if (!ResolvesSelection(input)
            || !ResolvesAbstention(input)
            || !RejectsStaleUninitializedAndNull(input))
        {
            GD.PushError("Concealment controller failed exact consideration-result resolution.");
            GetTree().Quit(1);
            return;
        }

        GD.Print(
            "Concealment consideration controller smoke passed: exact selection installs both "
            + "fresh snapshots atomically, abstention retains both exact snapshots, stale, null, "
            + "uninitialized, and repeated results fail closed, and surrounding state is unchanged.");
        GetTree().Quit();
    }

    private static bool ResolvesSelection(ConsiderationInput input)
    {
        CharacterObjectSabotageConcealmentController controller = new();
        CharacterObjectSabotageConcealmentConsiderationResult result = CreateResult(
            input,
            input.Concealment,
            input.Evidence,
            input.Candidate.Evidence.EvidenceId);

        bool initialized = controller.TryInitialize(input.Concealment, input.Evidence);
        bool resolved = controller.TryResolveConsideration(result);
        bool accepted = controller.Concealment is { } resolvedConcealment
            && controller.Evidence is { } resolvedEvidence
            && !ReferenceEquals(resolvedConcealment, input.Concealment)
            && !ReferenceEquals(resolvedEvidence, input.Evidence)
            && ReferenceEquals(
                resolvedConcealment.ConcealedObjectIds[0],
                input.RetainedObjectId)
            && ReferenceEquals(
                resolvedConcealment.ConcealedObjectIds[1],
                input.Target.Identity.Id)
            && ReferenceEquals(resolvedEvidence.Entries[0], input.RetainedEvidence)
            && ReferenceEquals(resolvedEvidence.Entries[1], input.Candidate.Evidence)
            && !controller.TryResolveConsideration(result)
            && ReferenceEquals(controller.Concealment, resolvedConcealment)
            && ReferenceEquals(controller.Evidence, resolvedEvidence)
            && HasUnchangedInputs(input, result.Request);

        controller.Free();
        return initialized && resolved && accepted;
    }

    private static bool ResolvesAbstention(ConsiderationInput input)
    {
        CharacterObjectSabotageConcealmentController controller = new();
        CharacterObjectSabotageConcealmentConsiderationResult result = CreateResult(
            input,
            input.Concealment,
            input.Evidence,
            null);

        bool accepted = controller.TryInitialize(input.Concealment, input.Evidence)
            && controller.TryResolveConsideration(result)
            && HasExactState(controller, input.Concealment, input.Evidence)
            && !controller.TryResolveConsideration(result)
            && HasExactState(controller, input.Concealment, input.Evidence)
            && HasUnchangedInputs(input, result.Request);

        controller.Free();
        return accepted;
    }

    private static bool RejectsStaleUninitializedAndNull(ConsiderationInput input)
    {
        CharacterObjectSabotageConcealmentController controller = new();
        CharacterObjectSabotageConcealmentConsiderationResult exact = CreateResult(
            input,
            input.Concealment,
            input.Evidence,
            input.Candidate.Evidence.EvidenceId);
        CharacterObjectSabotageConcealmentConsiderationResult staleConcealment = CreateResult(
            input,
            new WorldObjectConcealmentSnapshot(input.Concealment.ConcealedObjectIds),
            input.Evidence,
            input.Candidate.Evidence.EvidenceId);
        CharacterObjectSabotageConcealmentConsiderationResult staleEvidence = CreateResult(
            input,
            input.Concealment,
            new DiscoveryEvidenceSnapshot(input.Evidence.Entries),
            input.Candidate.Evidence.EvidenceId);

        bool rejectedUninitialized = !controller.TryResolveConsideration(exact)
            && controller.Concealment is null
            && controller.Evidence is null;
        bool rejected = controller.TryInitialize(input.Concealment, input.Evidence)
            && !controller.TryResolveConsideration(staleConcealment)
            && !controller.TryResolveConsideration(staleEvidence)
            && ThrowsArgumentNull(() => controller.TryResolveConsideration(null!))
            && HasExactState(controller, input.Concealment, input.Evidence)
            && HasUnchangedInputs(input, exact.Request)
            && HasUnchangedInputs(input, staleConcealment.Request)
            && HasUnchangedInputs(input, staleEvidence.Request);

        controller.Free();
        return rejectedUninitialized && rejected;
    }

    private static CharacterObjectSabotageConcealmentConsiderationResult CreateResult(
        ConsiderationInput input,
        WorldObjectConcealmentSnapshot concealment,
        DiscoveryEvidenceSnapshot evidence,
        EvidenceId? selectedEvidenceId) =>
        new(
            new CharacterObjectSabotageConcealmentConsiderationRequest(
                input.Character,
                input.PhysicalAccess,
                input.WorldObjects,
                concealment,
                evidence,
                [input.Candidate]),
            new CharacterObjectSabotageConcealmentDecision(
                input.Character.Identity.Id,
                selectedEvidenceId));

    private static bool HasUnchangedInputs(
        ConsiderationInput input,
        CharacterObjectSabotageConcealmentConsiderationRequest request) =>
        ReferenceEquals(request.Character, input.Character)
        && ReferenceEquals(request.PhysicalAccess, input.PhysicalAccess)
        && ReferenceEquals(request.WorldObjects, input.WorldObjects)
        && ReferenceEquals(request.Candidates.Single(), input.Candidate)
        && input.Concealment.ConcealedObjectIds.Count == 1
        && input.Evidence.Entries.Count == 1;

    private static bool HasExactState(
        CharacterObjectSabotageConcealmentController controller,
        WorldObjectConcealmentSnapshot concealment,
        DiscoveryEvidenceSnapshot evidence) =>
        ReferenceEquals(controller.Concealment, concealment)
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
        WorldObject target = new(
            new WorldObjectIdentity(new ObjectId("village-water-cask"), "Village water cask"),
            new WorldObjectDescription("A covered cask beside the village well."));
        CharacterObjectSabotageEvidence candidate = new(
            new CharacterObjectSabotageOccurrence(
                new CharacterObjectSabotageProposal(
                    character.Identity.Id,
                    new CharacterActionAccessRequirement(
                        target.Identity.Id,
                        "assess-local-influence",
                        character.Knowledge.KnowledgeReferences.Single()),
                    CharacterSabotageActionKind.Conceal),
                target),
            new DiscoveryEvidence(
                new EvidenceId("disturbed-cask-cover"),
                target.Identity.Id,
                VisibilityState.Noticed,
                false));
        ObjectId retainedObjectId = new("storehouse-ledger");
        DiscoveryEvidence retainedEvidence = new(
            new EvidenceId("retained-evidence"),
            retainedObjectId,
            VisibilityState.Identified,
            false);

        return new ConsiderationInput(
            character,
            target,
            new CharacterPhysicalAccessSnapshot(character.Identity.Id, [target.Identity.Id]),
            new WorldObjectSnapshot([target]),
            candidate,
            new WorldObjectConcealmentSnapshot([retainedObjectId]),
            new DiscoveryEvidenceSnapshot([retainedEvidence]),
            retainedObjectId,
            retainedEvidence);
    }

    private sealed record ConsiderationInput(
        Character Character,
        WorldObject Target,
        CharacterPhysicalAccessSnapshot PhysicalAccess,
        WorldObjectSnapshot WorldObjects,
        CharacterObjectSabotageEvidence Candidate,
        WorldObjectConcealmentSnapshot Concealment,
        DiscoveryEvidenceSnapshot Evidence,
        ObjectId RetainedObjectId,
        DiscoveryEvidence RetainedEvidence);
}
