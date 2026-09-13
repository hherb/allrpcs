using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Discovery;
using Allrpcs.Domain.Identifiers;
using Allrpcs.Domain.Perception;
using Allrpcs.Domain.WorldObjects;
using Godot;

namespace Allrpcs.Game;

/// <summary>Runs credential-free scene-owned recoverable concealment checks.</summary>
public partial class CharacterObjectSabotageConcealmentControllerSmokeCheck : Node
{
    private static readonly string[] ExpectedMethodNames =
        ["TryApply", "TryInitialize", "TryRecover"];

    /// <inheritdoc />
    public override void _Ready()
    {
        CharacterObjectSabotageConcealmentController controller = new();
        ConcealmentInput input = CreateInput();
        WorldObjectConcealmentSnapshot concealment = new([input.RetainedObjectId]);
        DiscoveryEvidenceSnapshot evidence = new([input.RetainedEvidence]);

        if (!RejectsNullInitialization(controller, concealment, evidence)
            || !controller.TryInitialize(concealment, evidence)
            || !HasExactState(controller, concealment, evidence)
            || controller.TryInitialize(new WorldObjectConcealmentSnapshot([]), evidence))
        {
            Fail("Concealment controller rejected exact one-time atomic initialization.");
            return;
        }

        if (!RejectsStaleApply(controller, input, concealment, evidence)
            || !RejectsIneligibleApply(controller, input, concealment, evidence)
            || !RejectsNullApply(controller, input, concealment, evidence))
        {
            Fail("Concealment controller changed state after rejected apply input.");
            return;
        }

        if (!controller.TryApply(
                input.Character,
                input.PhysicalAccess,
                input.WorldObjects,
                concealment,
                evidence,
                input.Candidate)
            || controller.Concealment is not { } appliedConcealment
            || controller.Evidence is not { } appliedEvidence
            || ReferenceEquals(appliedConcealment, concealment)
            || ReferenceEquals(appliedEvidence, evidence)
            || !ReferenceEquals(appliedConcealment.ConcealedObjectIds[0], input.RetainedObjectId)
            || !ReferenceEquals(
                appliedConcealment.ConcealedObjectIds[1],
                input.Target.Identity.Id)
            || !ReferenceEquals(appliedEvidence.Entries[0], input.RetainedEvidence)
            || !ReferenceEquals(appliedEvidence.Entries[1], input.Candidate.Evidence)
            || !HasExactState(controller, appliedConcealment, appliedEvidence))
        {
            Fail("Concealment controller failed atomic stable concealment and evidence install.");
            return;
        }

        if (!RejectsStaleRecovery(controller, input, appliedConcealment, appliedEvidence)
            || !RejectsIneligibleRecovery(controller, input, appliedConcealment, appliedEvidence)
            || !RejectsNullRecovery(controller, input, appliedConcealment, appliedEvidence))
        {
            Fail("Concealment controller changed state after rejected recovery input.");
            return;
        }

        if (!controller.TryRecover(
                appliedConcealment,
                appliedEvidence,
                input.EvidenceView,
                input.Candidate)
            || controller.Concealment is not { } recoveredConcealment
            || !ReferenceEquals(controller.Evidence, appliedEvidence)
            || ReferenceEquals(recoveredConcealment, appliedConcealment)
            || !ReferenceEquals(
                AssertSingle(recoveredConcealment.ConcealedObjectIds),
                input.RetainedObjectId)
            || !ReferenceEquals(appliedEvidence.Entries[0], input.RetainedEvidence)
            || !ReferenceEquals(appliedEvidence.Entries[1], input.Candidate.Evidence)
            || !HasExactState(controller, recoveredConcealment, appliedEvidence)
            || !HasMinimalSurface())
        {
            Fail("Concealment controller failed exact recovery or exposed extra state.");
            return;
        }

        GD.Print(
            "Concealment controller smoke passed: one-time scene ownership, exact CAS apply and "
            + "recovery, stable unrelated entries, stale rejection, minimal surface, and atomic "
            + "failure.");
        controller.Free();
        GetTree().Quit();
    }

    private static bool RejectsStaleApply(
        CharacterObjectSabotageConcealmentController controller,
        ConcealmentInput input,
        WorldObjectConcealmentSnapshot concealment,
        DiscoveryEvidenceSnapshot evidence)
    {
        WorldObjectConcealmentSnapshot staleConcealment = new(
            concealment.ConcealedObjectIds.Select(id => new ObjectId(id.Value)));
        DiscoveryEvidenceSnapshot staleEvidence = new(evidence.Entries);

        return !controller.TryApply(
                input.Character,
                input.PhysicalAccess,
                input.WorldObjects,
                staleConcealment,
                evidence,
                input.Candidate)
            && !controller.TryApply(
                input.Character,
                input.PhysicalAccess,
                input.WorldObjects,
                concealment,
                staleEvidence,
                input.Candidate)
            && HasExactState(controller, concealment, evidence);
    }

    private static bool RejectsIneligibleApply(
        CharacterObjectSabotageConcealmentController controller,
        ConcealmentInput input,
        WorldObjectConcealmentSnapshot concealment,
        DiscoveryEvidenceSnapshot evidence)
    {
        CharacterPhysicalAccessSnapshot missingAccess = new(input.Character.Identity.Id, []);

        return !controller.TryApply(
                input.Character,
                missingAccess,
                input.WorldObjects,
                concealment,
                evidence,
                input.Candidate)
            && HasExactState(controller, concealment, evidence);
    }

    private static bool RejectsNullApply(
        CharacterObjectSabotageConcealmentController controller,
        ConcealmentInput input,
        WorldObjectConcealmentSnapshot concealment,
        DiscoveryEvidenceSnapshot evidence) =>
        ThrowsArgumentNull(() => controller.TryApply(
            null!, input.PhysicalAccess, input.WorldObjects, concealment, evidence, input.Candidate))
        && ThrowsArgumentNull(() => controller.TryApply(
            input.Character, null!, input.WorldObjects, concealment, evidence, input.Candidate))
        && ThrowsArgumentNull(() => controller.TryApply(
            input.Character, input.PhysicalAccess, null!, concealment, evidence, input.Candidate))
        && ThrowsArgumentNull(() => controller.TryApply(
            input.Character, input.PhysicalAccess, input.WorldObjects, null!, evidence,
            input.Candidate))
        && ThrowsArgumentNull(() => controller.TryApply(
            input.Character, input.PhysicalAccess, input.WorldObjects, concealment, null!,
            input.Candidate))
        && ThrowsArgumentNull(() => controller.TryApply(
            input.Character, input.PhysicalAccess, input.WorldObjects, concealment, evidence, null!))
        && HasExactState(controller, concealment, evidence);

    private static bool RejectsStaleRecovery(
        CharacterObjectSabotageConcealmentController controller,
        ConcealmentInput input,
        WorldObjectConcealmentSnapshot concealment,
        DiscoveryEvidenceSnapshot evidence)
    {
        WorldObjectConcealmentSnapshot staleConcealment = new(
            concealment.ConcealedObjectIds.Select(id => new ObjectId(id.Value)));
        DiscoveryEvidenceSnapshot staleEvidence = new(evidence.Entries);

        return !controller.TryRecover(
                staleConcealment,
                evidence,
                input.EvidenceView,
                input.Candidate)
            && !controller.TryRecover(
                concealment,
                staleEvidence,
                input.EvidenceView,
                input.Candidate)
            && HasExactState(controller, concealment, evidence);
    }

    private static bool RejectsIneligibleRecovery(
        CharacterObjectSabotageConcealmentController controller,
        ConcealmentInput input,
        WorldObjectConcealmentSnapshot concealment,
        DiscoveryEvidenceSnapshot evidence)
    {
        CharacterEvidenceViewSnapshot unseen = new(input.EvidenceView.CharacterId, []);

        return !controller.TryRecover(concealment, evidence, unseen, input.Candidate)
            && HasExactState(controller, concealment, evidence);
    }

    private static bool RejectsNullRecovery(
        CharacterObjectSabotageConcealmentController controller,
        ConcealmentInput input,
        WorldObjectConcealmentSnapshot concealment,
        DiscoveryEvidenceSnapshot evidence) =>
        ThrowsArgumentNull(() => controller.TryRecover(
            null!, evidence, input.EvidenceView, input.Candidate))
        && ThrowsArgumentNull(() => controller.TryRecover(
            concealment, null!, input.EvidenceView, input.Candidate))
        && ThrowsArgumentNull(() => controller.TryRecover(
            concealment, evidence, null!, input.Candidate))
        && ThrowsArgumentNull(() => controller.TryRecover(
            concealment, evidence, input.EvidenceView, null!))
        && HasExactState(controller, concealment, evidence);

    private static bool RejectsNullInitialization(
        CharacterObjectSabotageConcealmentController controller,
        WorldObjectConcealmentSnapshot concealment,
        DiscoveryEvidenceSnapshot evidence) =>
        ThrowsArgumentNull(() => controller.TryInitialize(null!, evidence))
        && ThrowsArgumentNull(() => controller.TryInitialize(concealment, null!))
        && controller.Concealment is null
        && controller.Evidence is null;

    private static bool HasExactState(
        CharacterObjectSabotageConcealmentController controller,
        WorldObjectConcealmentSnapshot concealment,
        DiscoveryEvidenceSnapshot evidence) =>
        ReferenceEquals(controller.Concealment, concealment)
        && ReferenceEquals(controller.Evidence, evidence);

    private static bool HasMinimalSurface()
    {
        Type type = typeof(CharacterObjectSabotageConcealmentController);
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

    private static T AssertSingle<T>(IReadOnlyList<T> values) =>
        values.Count == 1 ? values[0] : default!;

    private static ConcealmentInput CreateInput()
    {
        Character character = RepresentativeEnemyAgentCharacterSource.Create();
        WorldObject target = new(
            new WorldObjectIdentity(new ObjectId("village-water-cask"), "Village water cask"),
            new WorldObjectDescription("A covered cask beside the village well."));
        CharacterObjectSabotageOccurrence occurrence = new(
            new CharacterObjectSabotageProposal(
                character.Identity.Id,
                new CharacterActionAccessRequirement(
                    target.Identity.Id,
                    "assess-local-influence",
                    character.Knowledge.KnowledgeReferences.Single()),
                CharacterSabotageActionKind.Conceal),
            target);
        DiscoveryEvidence sabotageEvidence = new(
            new EvidenceId("disturbed-cask-cover"),
            target.Identity.Id,
            VisibilityState.Noticed,
            false);
        ObjectId retainedObjectId = new("storehouse-ledger");
        DiscoveryEvidence retainedEvidence = new(
            new EvidenceId("retained-evidence"),
            retainedObjectId,
            VisibilityState.Identified,
            false);

        return new ConcealmentInput(
            character,
            target,
            new CharacterPhysicalAccessSnapshot(character.Identity.Id, [target.Identity.Id]),
            new WorldObjectSnapshot([target]),
            new CharacterObjectSabotageEvidence(occurrence, sabotageEvidence),
            new CharacterEvidenceViewSnapshot(
                new CharacterId("seeker"),
                [sabotageEvidence.EvidenceId]),
            retainedObjectId,
            retainedEvidence);
    }

    private void Fail(string message)
    {
        GD.PushError(message);
        GetTree().Quit(1);
    }

    private sealed record ConcealmentInput(
        Character Character,
        WorldObject Target,
        CharacterPhysicalAccessSnapshot PhysicalAccess,
        WorldObjectSnapshot WorldObjects,
        CharacterObjectSabotageEvidence Candidate,
        CharacterEvidenceViewSnapshot EvidenceView,
        ObjectId RetainedObjectId,
        DiscoveryEvidence RetainedEvidence);
}
