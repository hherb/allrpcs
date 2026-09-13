using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Perception;
using Allrpcs.Domain.WorldObjects;
using Godot;

namespace Allrpcs.Game;

/// <summary>Checks exact inert sabotage initialization against the main scene world state.</summary>
public partial class TemporaryCharacterObjectSabotageMainSceneSmokeCheck : Node
{
    private const string CheckEnvironmentVariable = "ALLRPCS_OBJECT_SABOTAGE_MAIN_SCENE_CHECK";
    private const string EnabledEnvironmentValue = "1";
    private const string HerbPatchObjectId = "herb-patch";
    private const string StoneMarkerObjectId = "stone-marker";
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
        TemporaryCharacterObjectSabotageSource sabotageSource =
            GetNode<TemporaryCharacterObjectSabotageSource>(
                "../TemporaryCharacterObjectSabotageSource");
        CharacterObjectSabotageConcealmentController state =
            GetNode<CharacterObjectSabotageConcealmentController>(
                "../CharacterObjectSabotageConcealmentController");
        TemporaryCharacterObjectSabotageFlowController flow =
            GetNode<TemporaryCharacterObjectSabotageFlowController>(
                "../TemporaryCharacterObjectSabotageFlowController");
        CharacterObjectSabotageConcealmentPresentationController presentation =
            GetNode<CharacterObjectSabotageConcealmentPresentationController>(
                "../CharacterObjectSabotageConcealmentPresentationController");
        Node2D waterCaskBody = GetNode<Node2D>("../InteractionCandidates/WaterCask");
        WorldObjectSnapshot worldObjects = worldSource.WorldObjects;
        WorldObject waterCask = worldObjects.Entries[^1];

        if (worldObjects.Entries.Count != 3
            || !string.Equals(
                worldObjects.Entries[0].Identity.Id.Value,
                HerbPatchObjectId,
                StringComparison.Ordinal)
            || !string.Equals(
                worldObjects.Entries[1].Identity.Id.Value,
                StoneMarkerObjectId,
                StringComparison.Ordinal)
            || !string.Equals(
                waterCask.Identity.Id.Value,
                WaterCaskObjectId,
                StringComparison.Ordinal)
            || !ReferenceEquals(sabotageSource.WorldObjects, worldObjects)
            || !ReferenceEquals(sabotageSource.Candidate.Occurrence.Target, waterCask)
            || ReferenceEquals(sabotageSource.Candidate.Occurrence.Target, worldObjects.Entries[0])
            || ReferenceEquals(sabotageSource.Candidate.Occurrence.Target, worldObjects.Entries[1])
            || !ReferenceEquals(
                sabotageSource.PhysicalAccess.AccessibleObjectIds.Single(),
                waterCask.Identity.Id)
            || !ReferenceEquals(state.Concealment, sabotageSource.Concealment)
            || !ReferenceEquals(state.Evidence, sabotageSource.Evidence)
            || !ReferenceEquals(
                presentation.TargetObjectId,
                sabotageSource.Candidate.Evidence.ObjectId)
            || !ReferenceEquals(
                presentation.LastPresentedConcealment,
                sabotageSource.Concealment)
            || state.Concealment.ConcealedObjectIds.Count != 0
            || state.Evidence.Entries.Count != 0
            || !waterCaskBody.Visible
            || !CharacterObjectSabotageConcealmentPolicy.CanApply(
                sabotageSource.Character,
                sabotageSource.PhysicalAccess,
                worldObjects,
                state.Concealment,
                state.Evidence,
                sabotageSource.Candidate)
            || !HasExpectedFlowSurface(flow))
        {
            Fail("The main scene did not retain one exact inert eligible concealment candidate.");
            return;
        }

        GD.Print(
            "Temporary object sabotage main-scene smoke passed: existing objects retained stable "
            + "identity and order, the exact tail water cask is eligible and visible, exact empty "
            + "source state was initialized, and no effect occurred automatically.");
        GetTree().Quit();
    }

    private static bool HasExpectedFlowSurface(
        TemporaryCharacterObjectSabotageFlowController flow)
    {
        return flow.GetType().GetMethods(
                System.Reflection.BindingFlags.Public
                | System.Reflection.BindingFlags.Instance
                | System.Reflection.BindingFlags.DeclaredOnly)
            .Where(method => !method.IsSpecialName)
            .Select(method => method.Name)
            .Order(StringComparer.Ordinal)
            .SequenceEqual(["TryApply", "TryRecover", "_Ready"]);
    }

    private void Fail(string message)
    {
        GD.PushError(message);
        GetTree().Quit(1);
    }
}
