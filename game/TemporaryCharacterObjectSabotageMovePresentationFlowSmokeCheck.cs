using Allrpcs.Domain.Discovery;
using Allrpcs.Domain.Perception;
using Allrpcs.Domain.WorldObjects;
using Godot;

namespace Allrpcs.Game;

/// <summary>Checks explicit main-scene move and recovery presentation.</summary>
public partial class TemporaryCharacterObjectSabotageMovePresentationFlowSmokeCheck : Node
{
    private const string CheckEnvironmentVariable =
        "ALLRPCS_OBJECT_SABOTAGE_MOVE_PRESENTATION_FLOW_CHECK";
    private const string EnabledEnvironmentValue = "1";
    private static readonly Vector2 SourcePosition = new(2200.0f, 900.0f);
    private static readonly Vector2 DestinationPosition = new(1900.0f, 1640.0f);

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

        TemporaryCharacterObjectSabotageMoveSource source =
            GetNode<TemporaryCharacterObjectSabotageMoveSource>(
                "../TemporaryCharacterObjectSabotageMoveSource");
        CharacterObjectSabotageMoveController state =
            GetNode<CharacterObjectSabotageMoveController>(
                "../CharacterObjectSabotageMoveController");
        CharacterObjectSabotageMovePresentationController presentation =
            GetNode<CharacterObjectSabotageMovePresentationController>(
                "../CharacterObjectSabotageMovePresentationController");
        TemporaryCharacterObjectSabotageMovePresentationFlowController flow =
            GetNode<TemporaryCharacterObjectSabotageMovePresentationFlowController>(
                "../TemporaryCharacterObjectSabotageMovePresentationFlowController");
        Node2D body = GetNode<Node2D>("../InteractionCandidates/WaterCask");
        WorldObjectSnapshot expectedWorldObjects = source.WorldObjects;
        WorldObjectPlacementSnapshot initialPlacements = source.Placements;
        DiscoveryEvidenceSnapshot initialEvidence = source.Evidence;

        if (body.Position != SourcePosition
            || !ReferenceEquals(presentation.Target, initialPlacements.Entries.Single().WorldObject)
            || !ReferenceEquals(presentation.LastPresentedPlacements, initialPlacements)
            || !ReferenceEquals(state.Placements, initialPlacements)
            || !ReferenceEquals(state.Evidence, initialEvidence))
        {
            Fail("The main-scene move presentation flow did not initialize exact source state.");
            return;
        }

        if (!flow.TryApply())
        {
            Fail("Explicit main-scene move rejected its exact eligible candidate.");
            return;
        }

        WorldObjectPlacementSnapshot? movedPlacements = state.Placements;
        DiscoveryEvidenceSnapshot? movedEvidence = state.Evidence;
        if (movedPlacements is null
            || movedEvidence is null
            || body.Position != DestinationPosition
            || !ReferenceEquals(presentation.LastPresentedPlacements, movedPlacements)
            || !ReferenceEquals(movedPlacements.Entries.Single().Location, source.Destination)
            || !ReferenceEquals(movedEvidence.Entries.Single(), source.Candidate.Evidence.Evidence)
            || flow.TryApply()
            || body.Position != DestinationPosition)
        {
            Fail("Explicit main-scene move did not present the exact authored destination once.");
            return;
        }

        CharacterPerceptionSnapshot unseen = new(source.Character.Identity.Id, []);
        if (flow.TryRecover(unseen)
            || !ThrowsArgumentNull(() => flow.TryRecover(null!))
            || body.Position != DestinationPosition
            || !ReferenceEquals(state.Placements, movedPlacements)
            || !ReferenceEquals(state.Evidence, movedEvidence)
            || !ReferenceEquals(presentation.LastPresentedPlacements, movedPlacements))
        {
            Fail("Unseen or null recovery input changed move presentation state.");
            return;
        }

        CharacterPerceptionSnapshot noticed = new(
            source.Character.Identity.Id,
            [new WorldObjectVisibility(
                source.Candidate.Evidence.Evidence.ObjectId,
                VisibilityState.Noticed)]);
        if (!flow.TryRecover(noticed))
        {
            Fail("Visible exact evidence did not recover the main-scene target.");
            return;
        }

        WorldObjectPlacementSnapshot? recoveredPlacements = state.Placements;
        if (recoveredPlacements is null
            || body.Position != SourcePosition
            || !ReferenceEquals(presentation.LastPresentedPlacements, recoveredPlacements)
            || !ReferenceEquals(recoveredPlacements.Entries.Single(), source.RecoveryCandidate.SourcePlacement)
            || !ReferenceEquals(state.Evidence, movedEvidence)
            || flow.TryRecover(noticed)
            || body.Position != SourcePosition
            || !ReferenceEquals(source.WorldObjects, expectedWorldObjects)
            || !ReferenceEquals(source.Placements, initialPlacements)
            || !ReferenceEquals(source.Evidence, initialEvidence)
            || source.Evidence.Entries.Count != 0
            || !HasClosedFlowSurface())
        {
            Fail("Evidence-authorized recovery did not restore exact move presentation state.");
            return;
        }

        GD.Print(
            "Temporary move sabotage presentation flow smoke passed: inert exact source "
            + "presentation, one explicit destination move, repeat and unseen rejection, exact "
            + "noticed-evidence recovery, retained evidence and source truth, and closed behavior.");
        GetTree().Quit();
    }

    private static bool HasClosedFlowSurface()
    {
        Type type = typeof(TemporaryCharacterObjectSabotageMovePresentationFlowController);
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
