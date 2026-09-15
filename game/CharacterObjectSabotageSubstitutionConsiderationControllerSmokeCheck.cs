using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Discovery;
using Allrpcs.Domain.Identifiers;
using Allrpcs.Domain.Perception;
using Allrpcs.Domain.WorldObjects;
using Godot;

namespace Allrpcs.Game;

/// <summary>Checks exact scene resolution of accepted substitution consideration results.</summary>
public partial class CharacterObjectSabotageSubstitutionConsiderationControllerSmokeCheck : Node
{
    /// <inheritdoc />
    public override void _Ready()
    {
        ConsiderationInput input = CreateInput();

        if (!ResolvesSelection(input)
            || !ResolvesAbstention(input)
            || !RejectsStaleUninitializedAndNull(input))
        {
            GD.PushError("Substitution controller failed exact consideration-result resolution.");
            GetTree().Quit(1);
            return;
        }

        GD.Print(
            "Substitution consideration controller smoke passed: exact selection installs both "
            + "fresh snapshots atomically, abstention retains both exact snapshots, stale, null, "
            + "uninitialized, and repeated results fail closed, and inputs remain unchanged.");
        GetTree().Quit();
    }

    private static bool ResolvesSelection(ConsiderationInput input)
    {
        CharacterObjectSabotageSubstitutionController controller = new();
        CharacterObjectSabotageSubstitutionConsiderationResult result = CreateResult(
            input,
            input.Substitutions,
            input.Evidence,
            input.Candidate.Evidence.Evidence.EvidenceId);

        bool initialized = controller.TryInitialize(input.Substitutions, input.Evidence);
        bool resolved = controller.TryResolveConsideration(result);
        bool accepted = controller.Substitutions is { } resolvedSubstitutions
            && controller.Evidence is { } resolvedEvidence
            && !ReferenceEquals(resolvedSubstitutions, input.Substitutions)
            && !ReferenceEquals(resolvedEvidence, input.Evidence)
            && ReferenceEquals(resolvedSubstitutions.Entries[0], input.RetainedSubstitution)
            && ReferenceEquals(resolvedSubstitutions.Entries[1], input.Candidate.Substitution)
            && ReferenceEquals(resolvedEvidence.Entries[0], input.RetainedEvidence)
            && ReferenceEquals(resolvedEvidence.Entries[1], input.Candidate.Evidence.Evidence)
            && !controller.TryResolveConsideration(result)
            && ReferenceEquals(controller.Substitutions, resolvedSubstitutions)
            && ReferenceEquals(controller.Evidence, resolvedEvidence)
            && HasUnchangedInputs(input, result.Request);

        controller.Free();
        return initialized && resolved && accepted;
    }

    private static bool ResolvesAbstention(ConsiderationInput input)
    {
        CharacterObjectSabotageSubstitutionController controller = new();
        CharacterObjectSabotageSubstitutionConsiderationResult result = CreateResult(
            input,
            input.Substitutions,
            input.Evidence,
            null);

        bool accepted = controller.TryInitialize(input.Substitutions, input.Evidence)
            && controller.TryResolveConsideration(result)
            && HasExactState(controller, input.Substitutions, input.Evidence)
            && !controller.TryResolveConsideration(result)
            && HasExactState(controller, input.Substitutions, input.Evidence)
            && HasUnchangedInputs(input, result.Request);

        controller.Free();
        return accepted;
    }

    private static bool RejectsStaleUninitializedAndNull(ConsiderationInput input)
    {
        CharacterObjectSabotageSubstitutionController controller = new();
        CharacterObjectSabotageSubstitutionConsiderationResult exact = CreateResult(
            input,
            input.Substitutions,
            input.Evidence,
            input.Candidate.Evidence.Evidence.EvidenceId);
        CharacterObjectSabotageSubstitutionConsiderationResult staleSubstitutions = CreateResult(
            input,
            new CharacterObjectSabotageSubstitutionSnapshot(input.Substitutions.Entries),
            input.Evidence,
            input.Candidate.Evidence.Evidence.EvidenceId);
        CharacterObjectSabotageSubstitutionConsiderationResult staleEvidence = CreateResult(
            input,
            input.Substitutions,
            new DiscoveryEvidenceSnapshot(input.Evidence.Entries),
            input.Candidate.Evidence.Evidence.EvidenceId);

        bool rejectedUninitialized = !controller.TryResolveConsideration(exact)
            && controller.Substitutions is null
            && controller.Evidence is null;
        bool rejected = controller.TryInitialize(input.Substitutions, input.Evidence)
            && !controller.TryResolveConsideration(staleSubstitutions)
            && !controller.TryResolveConsideration(staleEvidence)
            && ThrowsArgumentNull(() => controller.TryResolveConsideration(null!))
            && HasExactState(controller, input.Substitutions, input.Evidence)
            && HasUnchangedInputs(input, exact.Request)
            && HasUnchangedInputs(input, staleSubstitutions.Request)
            && HasUnchangedInputs(input, staleEvidence.Request);

        controller.Free();
        return rejectedUninitialized && rejected;
    }

    private static CharacterObjectSabotageSubstitutionConsiderationResult CreateResult(
        ConsiderationInput input,
        CharacterObjectSabotageSubstitutionSnapshot substitutions,
        DiscoveryEvidenceSnapshot evidence,
        EvidenceId? selectedEvidenceId) =>
        new(
            new CharacterObjectSabotageSubstitutionConsiderationRequest(
                input.Character,
                input.PhysicalAccess,
                input.WorldObjects,
                substitutions,
                evidence,
                [input.Candidate]),
            new CharacterObjectSabotageSubstitutionDecision(
                input.Character.Identity.Id,
                selectedEvidenceId));

    private static bool HasUnchangedInputs(
        ConsiderationInput input,
        CharacterObjectSabotageSubstitutionConsiderationRequest request) =>
        ReferenceEquals(request.Character, input.Character)
        && ReferenceEquals(request.PhysicalAccess, input.PhysicalAccess)
        && ReferenceEquals(request.WorldObjects, input.WorldObjects)
        && ReferenceEquals(request.Candidates.Single(), input.Candidate)
        && ReferenceEquals(input.Substitutions.Entries.Single(), input.RetainedSubstitution)
        && ReferenceEquals(input.Evidence.Entries.Single(), input.RetainedEvidence);

    private static bool HasExactState(
        CharacterObjectSabotageSubstitutionController controller,
        CharacterObjectSabotageSubstitutionSnapshot substitutions,
        DiscoveryEvidenceSnapshot evidence) =>
        ReferenceEquals(controller.Substitutions, substitutions)
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
        WorldObject replacement = WorldObject("tainted-water-cask");
        CharacterObjectSabotageOccurrence occurrence = Occurrence(character, target);
        CharacterObjectSabotageSubstitutionCandidate candidate = new(
            new CharacterObjectSabotageSubstitution(occurrence, replacement),
            new CharacterObjectSabotageEvidence(
                occurrence,
                new DiscoveryEvidence(
                    new EvidenceId("water-cask-substitution-signs"),
                    target.Identity.Id,
                    VisibilityState.Noticed,
                    false)));
        WorldObject retainedTarget = WorldObject("storehouse-ledger");
        CharacterObjectSabotageSubstitution retainedSubstitution = new(
            Occurrence(character, retainedTarget),
            WorldObject("false-ledger"));
        DiscoveryEvidence retainedEvidence = new(
            new EvidenceId("retained-evidence"),
            retainedTarget.Identity.Id,
            VisibilityState.Identified,
            false);

        return new(
            character,
            new CharacterPhysicalAccessSnapshot(character.Identity.Id, [target.Identity.Id]),
            new WorldObjectSnapshot([target]),
            candidate,
            new CharacterObjectSabotageSubstitutionSnapshot([retainedSubstitution]),
            new DiscoveryEvidenceSnapshot([retainedEvidence]),
            retainedSubstitution,
            retainedEvidence);
    }

    private static CharacterObjectSabotageOccurrence Occurrence(
        Character character,
        WorldObject target) =>
        new(
            new CharacterObjectSabotageProposal(
                character.Identity.Id,
                new CharacterActionAccessRequirement(
                    target.Identity.Id,
                    "assess-local-influence",
                    character.Knowledge.KnowledgeReferences.Single()),
                CharacterSabotageActionKind.Substitute),
            target);

    private static WorldObject WorldObject(string id) =>
        new(
            new WorldObjectIdentity(new ObjectId(id), $"Object {id}"),
            new WorldObjectDescription($"Description for {id}."));

    private sealed record ConsiderationInput(
        Character Character,
        CharacterPhysicalAccessSnapshot PhysicalAccess,
        WorldObjectSnapshot WorldObjects,
        CharacterObjectSabotageSubstitutionCandidate Candidate,
        CharacterObjectSabotageSubstitutionSnapshot Substitutions,
        DiscoveryEvidenceSnapshot Evidence,
        CharacterObjectSabotageSubstitution RetainedSubstitution,
        DiscoveryEvidence RetainedEvidence);
}
