using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Discovery;
using Allrpcs.Domain.Identifiers;
using Allrpcs.Domain.Perception;
using Allrpcs.Domain.WorldObjects;
using Godot;

namespace Allrpcs.Game;

/// <summary>Checks one explicit authored enemy concealment through exact scene-owned state.</summary>
public partial class TemporaryCharacterObjectSabotageFlowSmokeCheck : Node
{
    private static readonly string[] ExpectedMethodNames = ["TryApply", "TryRecover", "_Ready"];

    /// <inheritdoc />
    public override void _Ready() => SetProcess(true);

    /// <inheritdoc />
    public override void _Process(double delta)
    {
        _ = delta;
        SetProcess(false);

        TemporaryCharacterObjectSabotageSource source =
            GetNode<TemporaryCharacterObjectSabotageSource>(
                "../TemporaryCharacterObjectSabotageSource");
        CharacterObjectSabotageConcealmentController state =
            GetNode<CharacterObjectSabotageConcealmentController>(
                "../CharacterObjectSabotageConcealmentController");
        TemporaryCharacterObjectSabotageFlowController flow =
            GetNode<TemporaryCharacterObjectSabotageFlowController>(
                "../TemporaryCharacterObjectSabotageFlowController");
        WorldObjectConcealmentSnapshot initialConcealment = source.Concealment;
        DiscoveryEvidenceSnapshot initialEvidence = source.Evidence;

        if (!ReferenceEquals(state.Concealment, initialConcealment)
            || !ReferenceEquals(state.Evidence, initialEvidence)
            || initialConcealment.ConcealedObjectIds.Count != 0
            || initialEvidence.Entries.Count != 0)
        {
            Fail("The temporary flow did not initialize exact source state without applying it.");
            return;
        }

        if (!flow.TryApply()
            || state.Concealment is not { } appliedConcealment
            || state.Evidence is not { } appliedEvidence)
        {
            Fail("The temporary flow rejected its first exact eligible application.");
            return;
        }

        if (!ReferenceEquals(
                appliedConcealment.ConcealedObjectIds.Single(),
                source.Candidate.Occurrence.Target.Identity.Id)
            || !ReferenceEquals(appliedEvidence.Entries.Single(), source.Candidate.Evidence)
            || !ReferenceEquals(state.Concealment, appliedConcealment)
            || !ReferenceEquals(state.Evidence, appliedEvidence))
        {
            Fail("The temporary flow did not install the exact concealment and evidence result.");
            return;
        }

        if (source.Concealment.ConcealedObjectIds.Count != 0
            || source.Evidence.Entries.Count != 0)
        {
            Fail("The temporary flow mutated its authored source snapshots.");
            return;
        }

        if (flow.TryApply()
            || !ReferenceEquals(state.Concealment, appliedConcealment)
            || !ReferenceEquals(state.Evidence, appliedEvidence))
        {
            Fail("The temporary flow did not reject repeat application atomically.");
            return;
        }

        CharacterPerceptionSnapshot unseen = new(new CharacterId("seeker"), []);
        if (flow.TryRecover(unseen)
            || !ThrowsArgumentNull(() => flow.TryRecover(null!))
            || !ReferenceEquals(state.Concealment, appliedConcealment)
            || !ReferenceEquals(state.Evidence, appliedEvidence))
        {
            Fail("The temporary flow changed state without visible authoritative evidence.");
            return;
        }

        CharacterPerceptionSnapshot noticed = new(
            unseen.CharacterId,
            [new WorldObjectVisibility(
                source.Candidate.Occurrence.Target.Identity.Id,
                VisibilityState.Noticed)]);
        if (!flow.TryRecover(noticed)
            || state.Concealment is not { } recoveredConcealment
            || recoveredConcealment.ConcealedObjectIds.Count != 0
            || !ReferenceEquals(state.Evidence, appliedEvidence)
            || flow.TryRecover(noticed)
            || !ReferenceEquals(state.Concealment, recoveredConcealment)
            || !ReferenceEquals(state.Evidence, appliedEvidence)
            || source.Concealment.ConcealedObjectIds.Count != 0
            || source.Evidence.Entries.Count != 0)
        {
            Fail("The temporary flow did not recover exactly through projected perception.");
            return;
        }

        if (!HasClosedFlowSurface())
        {
            Fail("The temporary flow exposed behavior beyond initialization and exact apply.");
            return;
        }

        GD.Print(
            "Temporary object sabotage flow smoke passed: exact source state initializes without "
            + "effect, one explicit request atomically conceals and leaves evidence, and repeat "
            + "application fails closed; exact noticed perception recovers without consuming "
            + "evidence.");
        GetTree().Quit();
    }

    private static bool HasClosedFlowSurface()
    {
        Type type = typeof(TemporaryCharacterObjectSabotageFlowController);
        string[] methodNames = type.GetMethods(
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

        return methodNames.SequenceEqual(ExpectedMethodNames) && properties.Length == 0;
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
}
