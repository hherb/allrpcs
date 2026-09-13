using Allrpcs.Domain.Discovery;
using Allrpcs.Domain.Perception;
using Allrpcs.Domain.WorldObjects;
using Godot;

namespace Allrpcs.Game;

/// <summary>Checks explicit main-scene concealment and recovery presentation.</summary>
public partial class TemporaryCharacterObjectSabotagePresentationFlowSmokeCheck : Node
{
    private const string CheckEnvironmentVariable =
        "ALLRPCS_OBJECT_SABOTAGE_PRESENTATION_FLOW_CHECK";
    private const string EnabledEnvironmentValue = "1";

    /// <inheritdoc />
    public override void _Ready()
    {
        bool isEnabled = OS.GetEnvironment(CheckEnvironmentVariable) == EnabledEnvironmentValue;
        SetProcess(isEnabled);
    }

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
        CharacterObjectSabotageConcealmentPresentationController presentation =
            GetNode<CharacterObjectSabotageConcealmentPresentationController>(
                "../CharacterObjectSabotageConcealmentPresentationController");
        TemporaryCharacterObjectSabotagePresentationFlowController flow =
            GetNode<TemporaryCharacterObjectSabotagePresentationFlowController>(
                "../TemporaryCharacterObjectSabotagePresentationFlowController");
        Node2D body = GetNode<Node2D>("../InteractionCandidates/WaterCask");
        Vector2 expectedPosition = body.Position;
        WorldObjectSnapshot expectedWorldObjects = source.WorldObjects;
        WorldObjectConcealmentSnapshot initialConcealment = source.Concealment;

        if (source.Candidates.Count != 1
            || !ReferenceEquals(source.Candidates[0], source.Candidate)
            || !body.Visible
            || !ReferenceEquals(presentation.TargetObjectId, source.Candidate.Evidence.ObjectId)
            || !ReferenceEquals(presentation.LastPresentedConcealment, initialConcealment))
        {
            Fail("The main-scene presentation flow did not initialize exact visible state.");
            return;
        }

        if (!flow.TryApply())
        {
            Fail("Explicit main-scene sabotage rejected its exact eligible candidate.");
            return;
        }

        WorldObjectConcealmentSnapshot? appliedConcealment = state.Concealment;
        DiscoveryEvidenceSnapshot? appliedEvidence = state.Evidence;
        if (body.Visible
            || appliedConcealment is null
            || !ReferenceEquals(presentation.LastPresentedConcealment, appliedConcealment)
            || !ReferenceEquals(
                appliedConcealment.ConcealedObjectIds.Single(),
                source.Candidate.Evidence.ObjectId)
            || appliedEvidence is null
            || !ReferenceEquals(appliedEvidence.Entries.Single(), source.Candidate.Evidence)
            || flow.TryApply()
            || body.Visible)
        {
            Fail("Explicit main-scene sabotage did not conceal and hide the exact water cask once.");
            return;
        }

        CharacterPerceptionSnapshot unseen = new(source.Character.Identity.Id, []);
        if (flow.TryRecover(unseen)
            || !ThrowsArgumentNull(() => flow.TryRecover(null!))
            || body.Visible
            || !ReferenceEquals(state.Concealment, appliedConcealment)
            || !ReferenceEquals(state.Evidence, appliedEvidence)
            || !ReferenceEquals(presentation.LastPresentedConcealment, appliedConcealment))
        {
            Fail("Unseen or null recovery input changed concealment presentation state.");
            return;
        }

        CharacterPerceptionSnapshot noticed = new(
            source.Character.Identity.Id,
            [new WorldObjectVisibility(
                source.Candidate.Evidence.ObjectId,
                VisibilityState.Noticed)]);
        if (!flow.TryRecover(noticed))
        {
            Fail("Visible exact evidence did not recover the main-scene target.");
            return;
        }

        WorldObjectConcealmentSnapshot? recoveredConcealment = state.Concealment;
        if (!body.Visible
            || recoveredConcealment is null
            || recoveredConcealment.ConcealedObjectIds.Count != 0
            || !ReferenceEquals(presentation.LastPresentedConcealment, recoveredConcealment)
            || !ReferenceEquals(state.Evidence, appliedEvidence)
            || flow.TryRecover(noticed)
            || !body.Visible
            || body.Position != expectedPosition
            || !ReferenceEquals(source.WorldObjects, expectedWorldObjects)
            || !ReferenceEquals(source.Concealment, initialConcealment)
            || source.Concealment.ConcealedObjectIds.Count != 0
            || source.Evidence.Entries.Count != 0
            || !HasClosedFlowSurface())
        {
            Fail("Evidence-authorized main-scene recovery did not restore exact presentation.");
            return;
        }

        GD.Print(
            "Temporary object sabotage presentation flow smoke passed: one explicit request "
            + "concealed and hid the exact water cask, noticed evidence recovered and restored it, "
            + "and repeated operations, position, world objects, evidence, and source state were "
            + "preserved.");
        GetTree().Quit();
    }

    private static bool HasClosedFlowSurface()
    {
        Type type = typeof(TemporaryCharacterObjectSabotagePresentationFlowController);
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
        return methodNames.SequenceEqual(["TryApply", "TryRecover", "_Ready"])
            && properties.Length == 0;
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
