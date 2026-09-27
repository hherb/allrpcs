using Allrpcs.Application.Characters;
using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Discovery;
using Allrpcs.Domain.WorldObjects;
using Godot;

namespace Allrpcs.Game;

/// <summary>Checks exact inert move-sabotage composition in the main scene.</summary>
public partial class TemporaryCharacterObjectSabotageMoveMainSceneSmokeCheck : Node
{
    private const string CheckEnvironmentVariable =
        "ALLRPCS_OBJECT_SABOTAGE_MOVE_MAIN_SCENE_CHECK";
    private const string EnabledEnvironmentValue = "1";
    private const string WaterCaskObjectId = "village-water-cask";

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

        TemporaryWorldObjectSource worldSource =
            GetNode<TemporaryWorldObjectSource>("../TemporaryWorldObjectSource");
        TemporaryCharacterObjectSabotageMoveSource source =
            GetNode<TemporaryCharacterObjectSabotageMoveSource>(
                "../TemporaryCharacterObjectSabotageMoveSource");
        CharacterObjectSabotageMoveController state =
            GetNode<CharacterObjectSabotageMoveController>(
                "../CharacterObjectSabotageMoveController");
        TemporaryCharacterObjectSabotageMoveFlowController flow =
            GetNode<TemporaryCharacterObjectSabotageMoveFlowController>(
                "../TemporaryCharacterObjectSabotageMoveFlowController");
        TemporaryCharacterObjectSabotageMoveModelSource modelSource =
            GetNode<TemporaryCharacterObjectSabotageMoveModelSource>(
                "../TemporaryCharacterObjectSabotageMoveModelSource");
        CharacterObjectSabotageMoveTaskController task =
            GetNode<CharacterObjectSabotageMoveTaskController>(
                "../CharacterObjectSabotageMoveTaskController");
        TemporaryCharacterObjectSabotageMoveConsiderationFlowController considerationFlow =
            GetNode<TemporaryCharacterObjectSabotageMoveConsiderationFlowController>(
                "../TemporaryCharacterObjectSabotageMoveConsiderationFlowController");
        CharacterObjectSabotageMovePresentationCoordinator presentationCoordinator =
            GetNode<CharacterObjectSabotageMovePresentationCoordinator>(
                "../CharacterObjectSabotageMovePresentationCoordinator");
        WorldObject target = worldSource.WorldObjects.Entries[2];
        WorldObjectPlacement placement = source.Placements.Entries.Single();
        ICharacterObjectSabotageMoveDecisionModel offline = modelSource.TakeModel();

        if (!ReferenceEquals(source.WorldObjects, worldSource.WorldObjects)
            || worldSource.WorldObjects.Entries.Count != 4
            || !ReferenceEquals(source.Candidate.Destination.Occurrence.Target, target)
            || target.Identity.Id.Value != WaterCaskObjectId
            || !ReferenceEquals(placement.WorldObject, target)
            || !ReferenceEquals(placement.Location, source.SourceLocation)
            || !ReferenceEquals(source.Candidate.Destination.Destination, source.Destination)
            || !ReferenceEquals(source.RecoveryCandidate.Candidate, source.Candidate)
            || !ReferenceEquals(source.RecoveryCandidate.SourcePlacement, placement)
            || !ReferenceEquals(state.Placements, source.Placements)
            || !ReferenceEquals(state.Evidence, source.Evidence)
            || state.Evidence.Entries.Count != 0
            || !CharacterObjectSabotageMovePolicy.CanApply(
                source.Character,
                source.PhysicalAccess,
                source.WorldObjects,
                state.Placements,
                state.Evidence,
                source.Candidate)
            || task.Status != CharacterObjectSabotageMoveTaskStatus.Idle
            || task.Result is not null
            || !ReferenceEquals(modelSource.TakeModel(), offline)
            || presentationCoordinator.TryPresentCompletedResult()
            || !HasExpectedFlowSurface(flow)
            || !HasExpectedConsiderationFlowSurface(considerationFlow)
            || !HasExpectedPresentationCoordinatorSurface(presentationCoordinator))
        {
            Fail("The main scene did not retain one exact inert eligible move candidate.");
            return;
        }

        GD.Print(
            "Temporary move sabotage main-scene smoke passed: exact water-cask source, "
            + "eligible move and aligned recovery, exact empty scene-owned state, stable offline "
            + "decision source, idle task, explicit consideration and presentation flow, and no "
            + "automatic effect.");
        GetTree().Quit();
    }

    private static bool HasExpectedFlowSurface(
        TemporaryCharacterObjectSabotageMoveFlowController flow) =>
        flow.GetType().GetMethods(
                System.Reflection.BindingFlags.Public
                | System.Reflection.BindingFlags.Instance
                | System.Reflection.BindingFlags.DeclaredOnly)
            .Where(method => !method.IsSpecialName)
            .Select(method => method.Name)
            .Order(StringComparer.Ordinal)
            .SequenceEqual(["TryApply", "TryRecover", "_Ready"]);

    private static bool HasExpectedConsiderationFlowSurface(
        TemporaryCharacterObjectSabotageMoveConsiderationFlowController flow) =>
        flow.GetType().GetMethods(
                System.Reflection.BindingFlags.Public
                | System.Reflection.BindingFlags.Instance
                | System.Reflection.BindingFlags.DeclaredOnly)
            .Where(method => !method.IsSpecialName)
            .Select(method => method.Name)
            .Order(StringComparer.Ordinal)
            .SequenceEqual(["TryStart", "_Ready"]);

    private static bool HasExpectedPresentationCoordinatorSurface(
        CharacterObjectSabotageMovePresentationCoordinator coordinator) =>
        coordinator.GetType().GetMethods(
                System.Reflection.BindingFlags.Public
                | System.Reflection.BindingFlags.Instance
                | System.Reflection.BindingFlags.DeclaredOnly)
            .Where(method => !method.IsSpecialName)
            .Select(method => method.Name)
            .Order(StringComparer.Ordinal)
            .SequenceEqual(
                ["TryAcknowledgeCompletedResult", "TryPresentCompletedResult", "_Process", "_Ready"]);

    private void Fail(string message)
    {
        GD.PushError(message);
        GetTree().Quit(1);
    }
}
