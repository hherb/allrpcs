using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Discovery;
using Allrpcs.Domain.Identifiers;
using Allrpcs.Domain.Perception;
using Allrpcs.Domain.WorldObjects;
using Godot;

namespace Allrpcs.Game;

/// <summary>Runs credential-free scene-owned recoverable substitution checks.</summary>
public partial class CharacterObjectSabotageSubstitutionControllerSmokeCheck : Node
{
    private static readonly string[] ExpectedMethodNames =
        ["TryApply", "TryInitialize", "TryRecover", "TryResolveConsideration"];

    /// <inheritdoc />
    public override void _Ready()
    {
        CharacterObjectSabotageSubstitutionController controller = new();
        SubstitutionInput input = CreateInput();
        CharacterObjectSabotageSubstitution retained = IndependentSubstitution(
            "storehouse-ledger",
            "false-ledger");
        CharacterObjectSabotageSubstitutionSnapshot substitutions = new([retained]);
        DiscoveryEvidence retainedEvidence = new(
            new EvidenceId("retained-evidence"),
            retained.Occurrence.Target.Identity.Id,
            VisibilityState.Identified,
            false);
        DiscoveryEvidenceSnapshot evidence = new([retainedEvidence]);

        if (!RejectsNullInitialization(controller, substitutions, evidence)
            || !controller.TryInitialize(substitutions, evidence)
            || !HasExactState(controller, substitutions, evidence)
            || controller.TryInitialize(
                new CharacterObjectSabotageSubstitutionSnapshot([]),
                evidence))
        {
            Fail("Substitution controller rejected exact one-time atomic initialization.");
            return;
        }

        if (!RejectsStaleApply(controller, input, substitutions, evidence)
            || !RejectsIneligibleApply(controller, input, substitutions, evidence)
            || !RejectsNullApply(controller, input, substitutions, evidence))
        {
            Fail("Substitution controller changed state after rejected apply input.");
            return;
        }

        if (!controller.TryApply(
                input.Character,
                input.PhysicalAccess,
                input.WorldObjects,
                substitutions,
                evidence,
                input.Candidate)
            || controller.Substitutions is not { } appliedSubstitutions
            || controller.Evidence is not { } appliedEvidence
            || ReferenceEquals(appliedSubstitutions, substitutions)
            || ReferenceEquals(appliedEvidence, evidence)
            || !ReferenceEquals(appliedSubstitutions.Entries[0], retained)
            || !ReferenceEquals(appliedSubstitutions.Entries[1], input.Candidate.Substitution)
            || !ReferenceEquals(appliedEvidence.Entries[0], retainedEvidence)
            || !ReferenceEquals(appliedEvidence.Entries[1], input.Candidate.Evidence.Evidence))
        {
            Fail("Substitution controller failed atomic stable substitution and evidence install.");
            return;
        }

        CharacterEvidenceViewSnapshot evidenceView = new(
            new CharacterId("seeker"),
            [input.Candidate.Evidence.Evidence.EvidenceId]);
        if (!RejectsStaleRecovery(
                controller,
                appliedSubstitutions,
                appliedEvidence,
                evidenceView,
                input.Candidate)
            || !RejectsIneligibleRecovery(
                controller,
                appliedSubstitutions,
                appliedEvidence,
                input.Candidate)
            || !RejectsNullRecovery(
                controller,
                appliedSubstitutions,
                appliedEvidence,
                evidenceView,
                input.Candidate))
        {
            Fail("Substitution controller changed state after rejected recovery input.");
            return;
        }

        if (!controller.TryRecover(
                appliedSubstitutions,
                appliedEvidence,
                evidenceView,
                input.Candidate)
            || controller.Substitutions is not { } recoveredSubstitutions
            || !ReferenceEquals(controller.Evidence, appliedEvidence)
            || ReferenceEquals(recoveredSubstitutions, appliedSubstitutions)
            || !ReferenceEquals(AssertSingle(recoveredSubstitutions), retained)
            || !HasExactState(controller, recoveredSubstitutions, appliedEvidence)
            || !HasMinimalSurface())
        {
            Fail("Substitution controller failed exact recovery or exposed extra state.");
            return;
        }

        GD.Print(
            "Substitution controller smoke passed: one-time ownership, exact CAS apply and "
            + "recovery, stable unrelated entries, stale rejection, and atomic failure.");
        controller.Free();
        GetTree().Quit();
    }

    private static bool RejectsStaleApply(
        CharacterObjectSabotageSubstitutionController controller,
        SubstitutionInput input,
        CharacterObjectSabotageSubstitutionSnapshot substitutions,
        DiscoveryEvidenceSnapshot evidence)
    {
        CharacterObjectSabotageSubstitutionSnapshot staleSubstitutions = new(
            substitutions.Entries);
        DiscoveryEvidenceSnapshot staleEvidence = new(evidence.Entries);

        return !controller.TryApply(
                input.Character,
                input.PhysicalAccess,
                input.WorldObjects,
                staleSubstitutions,
                evidence,
                input.Candidate)
            && !controller.TryApply(
                input.Character,
                input.PhysicalAccess,
                input.WorldObjects,
                substitutions,
                staleEvidence,
                input.Candidate)
            && HasExactState(controller, substitutions, evidence);
    }

    private static bool RejectsIneligibleApply(
        CharacterObjectSabotageSubstitutionController controller,
        SubstitutionInput input,
        CharacterObjectSabotageSubstitutionSnapshot substitutions,
        DiscoveryEvidenceSnapshot evidence) =>
        !controller.TryApply(
            input.Character,
            new CharacterPhysicalAccessSnapshot(input.Character.Identity.Id, []),
            input.WorldObjects,
            substitutions,
            evidence,
            input.Candidate)
        && HasExactState(controller, substitutions, evidence);

    private static bool RejectsNullApply(
        CharacterObjectSabotageSubstitutionController controller,
        SubstitutionInput input,
        CharacterObjectSabotageSubstitutionSnapshot substitutions,
        DiscoveryEvidenceSnapshot evidence) =>
        ThrowsArgumentNull(() => controller.TryApply(
            null!, input.PhysicalAccess, input.WorldObjects, substitutions, evidence,
            input.Candidate))
        && ThrowsArgumentNull(() => controller.TryApply(
            input.Character, null!, input.WorldObjects, substitutions, evidence,
            input.Candidate))
        && ThrowsArgumentNull(() => controller.TryApply(
            input.Character, input.PhysicalAccess, null!, substitutions, evidence,
            input.Candidate))
        && ThrowsArgumentNull(() => controller.TryApply(
            input.Character, input.PhysicalAccess, input.WorldObjects, null!, evidence,
            input.Candidate))
        && ThrowsArgumentNull(() => controller.TryApply(
            input.Character, input.PhysicalAccess, input.WorldObjects, substitutions, null!,
            input.Candidate))
        && ThrowsArgumentNull(() => controller.TryApply(
            input.Character, input.PhysicalAccess, input.WorldObjects, substitutions, evidence,
            null!))
        && HasExactState(controller, substitutions, evidence);

    private static bool RejectsStaleRecovery(
        CharacterObjectSabotageSubstitutionController controller,
        CharacterObjectSabotageSubstitutionSnapshot substitutions,
        DiscoveryEvidenceSnapshot evidence,
        CharacterEvidenceViewSnapshot evidenceView,
        CharacterObjectSabotageSubstitutionCandidate candidate) =>
        !controller.TryRecover(
            new CharacterObjectSabotageSubstitutionSnapshot(substitutions.Entries),
            evidence,
            evidenceView,
            candidate)
        && !controller.TryRecover(
            substitutions,
            new DiscoveryEvidenceSnapshot(evidence.Entries),
            evidenceView,
            candidate)
        && HasExactState(controller, substitutions, evidence);

    private static bool RejectsIneligibleRecovery(
        CharacterObjectSabotageSubstitutionController controller,
        CharacterObjectSabotageSubstitutionSnapshot substitutions,
        DiscoveryEvidenceSnapshot evidence,
        CharacterObjectSabotageSubstitutionCandidate candidate) =>
        !controller.TryRecover(
            substitutions,
            evidence,
            new CharacterEvidenceViewSnapshot(new CharacterId("seeker"), []),
            candidate)
        && HasExactState(controller, substitutions, evidence);

    private static bool RejectsNullRecovery(
        CharacterObjectSabotageSubstitutionController controller,
        CharacterObjectSabotageSubstitutionSnapshot substitutions,
        DiscoveryEvidenceSnapshot evidence,
        CharacterEvidenceViewSnapshot evidenceView,
        CharacterObjectSabotageSubstitutionCandidate candidate) =>
        ThrowsArgumentNull(() => controller.TryRecover(null!, evidence, evidenceView, candidate))
        && ThrowsArgumentNull(() => controller.TryRecover(
            substitutions, null!, evidenceView, candidate))
        && ThrowsArgumentNull(() => controller.TryRecover(
            substitutions, evidence, null!, candidate))
        && ThrowsArgumentNull(() => controller.TryRecover(
            substitutions, evidence, evidenceView, null!))
        && HasExactState(controller, substitutions, evidence);

    private static bool RejectsNullInitialization(
        CharacterObjectSabotageSubstitutionController controller,
        CharacterObjectSabotageSubstitutionSnapshot substitutions,
        DiscoveryEvidenceSnapshot evidence) =>
        ThrowsArgumentNull(() => controller.TryInitialize(null!, evidence))
        && ThrowsArgumentNull(() => controller.TryInitialize(substitutions, null!))
        && controller.Substitutions is null
        && controller.Evidence is null;

    private static bool HasExactState(
        CharacterObjectSabotageSubstitutionController controller,
        CharacterObjectSabotageSubstitutionSnapshot substitutions,
        DiscoveryEvidenceSnapshot evidence) =>
        ReferenceEquals(controller.Substitutions, substitutions)
        && ReferenceEquals(controller.Evidence, evidence);

    private static bool HasMinimalSurface()
    {
        Type type = typeof(CharacterObjectSabotageSubstitutionController);
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

    private static SubstitutionInput CreateInput()
    {
        Character character = RepresentativeEnemyAgentCharacterSource.Create();
        WorldObject target = WorldObject("village-water-cask");
        CharacterObjectSabotageOccurrence occurrence = Occurrence(character, target);
        DiscoveryEvidence evidence = new(
            new EvidenceId("water-cask-substitution-signs"),
            target.Identity.Id,
            VisibilityState.Noticed,
            false);

        return new SubstitutionInput(
            character,
            new CharacterPhysicalAccessSnapshot(character.Identity.Id, [target.Identity.Id]),
            new WorldObjectSnapshot([target]),
            new CharacterObjectSabotageSubstitutionCandidate(
                new CharacterObjectSabotageSubstitution(
                    occurrence,
                    WorldObject("tainted-water-cask")),
                new CharacterObjectSabotageEvidence(occurrence, evidence)));
    }

    private static CharacterObjectSabotageSubstitution IndependentSubstitution(
        string targetId,
        string replacementId)
    {
        Character character = RepresentativeEnemyAgentCharacterSource.Create();
        WorldObject target = WorldObject(targetId);
        return new CharacterObjectSabotageSubstitution(
            Occurrence(character, target),
            WorldObject(replacementId));
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

    private static CharacterObjectSabotageSubstitution AssertSingle(
        CharacterObjectSabotageSubstitutionSnapshot substitutions) =>
        substitutions.Entries.Single();

    private sealed record SubstitutionInput(
        Character Character,
        CharacterPhysicalAccessSnapshot PhysicalAccess,
        WorldObjectSnapshot WorldObjects,
        CharacterObjectSabotageSubstitutionCandidate Candidate);

    private void Fail(string message)
    {
        GD.PushError(message);
        GetTree().Quit(1);
    }
}
