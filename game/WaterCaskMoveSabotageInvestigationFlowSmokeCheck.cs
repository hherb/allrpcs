using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Discovery;
using Allrpcs.Domain.Perception;
using Allrpcs.Domain.WorldObjects;
using Godot;
using NumericsVector2 = System.Numerics.Vector2;

namespace Allrpcs.Game;

/// <summary>Checks exact focused-search recovery for the moved main-scene water cask.</summary>
public partial class WaterCaskMoveSabotageInvestigationFlowSmokeCheck : Node
{
    private const string CheckEnvironmentVariable =
        "ALLRPCS_WATER_CASK_MOVE_SABOTAGE_INVESTIGATION_CHECK";
    private const string EnabledEnvironmentValue = "1";
    private const string FocusedSearchStartAction = "focused_search_start";
    private const string FocusedSearchCompleteAction = "focused_search_complete";
    private const string FocusedSearchResolveAction = "focused_search_resolve";
    private const int HerbPatchTargetIndex = 0;
    private const int WaterCaskTargetIndex = 2;

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

        Main main = GetNode<Main>("..");
        InteractionFocusController focus =
            GetNode<InteractionFocusController>("../InteractionFocusController");
        FocusedSearchActivityController search =
            GetNode<FocusedSearchActivityController>("../Seeker/FocusedSearchActivityController");
        TemporaryWorldObjectSource worldSource =
            GetNode<TemporaryWorldObjectSource>("../TemporaryWorldObjectSource");
        WorldObjectController worldState =
            GetNode<WorldObjectController>("../WorldObjectController");
        TemporaryCharacterObjectSabotageMoveSource sabotageSource =
            GetNode<TemporaryCharacterObjectSabotageMoveSource>(
                "../TemporaryCharacterObjectSabotageMoveSource");
        CharacterObjectSabotageMoveController sabotageState =
            GetNode<CharacterObjectSabotageMoveController>(
                "../CharacterObjectSabotageMoveController");
        CharacterObjectSabotageMovePresentationController sabotagePresentation =
            GetNode<CharacterObjectSabotageMovePresentationController>(
                "../CharacterObjectSabotageMovePresentationController");
        TemporaryCharacterObjectSabotageMovePresentationFlowController sabotageFlow =
            GetNode<TemporaryCharacterObjectSabotageMovePresentationFlowController>(
                "../TemporaryCharacterObjectSabotageMovePresentationFlowController");
        CharacterBody2D seeker = GetNode<CharacterBody2D>("../Seeker");
        CharacterBody2D healer = GetNode<CharacterBody2D>("../Healer");

        WorldObjectSnapshot expectedWorldObjects = worldSource.WorldObjects;
        WorldObject herbPatch = expectedWorldObjects.Entries[0];
        WorldObject waterCask = expectedWorldObjects.Entries[^1];
        GreyboxInteractionTarget herbTarget = focus.GetTarget(HerbPatchTargetIndex);
        GreyboxInteractionTarget waterCaskTarget = focus.GetTarget(WaterCaskTargetIndex);
        Vector2 expectedSourcePosition = waterCaskTarget.Position;
        Vector2 expectedSeekerPosition = seeker.GlobalPosition;
        Vector2 expectedHealerPosition = healer.GlobalPosition;
        DiscoveryEvidenceSnapshot expectedSourceEvidence = sabotageSource.Evidence;
        WorldObjectPlacementSnapshot expectedSourcePlacements = sabotageSource.Placements;
        CharacterPerceptionSnapshot initialPerception = search.Perception
            ?? throw new InvalidOperationException(
                "The moved-cask investigation smoke requires initialized perception.");

        if (!sabotageFlow.TryApply()
            || sabotageState.Placements is not { } movedPlacements
            || sabotageState.Evidence is not { } movedEvidence
            || !ReferenceEquals(movedPlacements.Entries.Single().Location, sabotageSource.Destination)
            || movedEvidence.Entries.Count != 1
            || !ReferenceEquals(
                movedEvidence.Entries[0],
                sabotageSource.Candidate.Evidence.Evidence)
            || waterCaskTarget.Position == expectedSourcePosition
            || !ReferenceEquals(
                sabotagePresentation.LastPresentedPlacements,
                movedPlacements))
        {
            Fail("The investigation smoke could not explicitly move the exact water cask.");
            return;
        }

        Vector2 expectedDestinationPosition = waterCaskTarget.Position;
        focus.RefreshFocusFrom(ToNumerics(herbTarget.GlobalPosition));
        if (!RunFocusedSearch(main, search, herbPatch)
            || !ReferenceEquals(sabotageState.Placements, movedPlacements)
            || !ReferenceEquals(sabotageState.Evidence, movedEvidence)
            || waterCaskTarget.Position != expectedDestinationPosition)
        {
            Fail("Resolving another target changed the moved water-cask placement.");
            return;
        }

        CharacterPerceptionSnapshot herbResolvedPerception = search.Perception
            ?? throw new InvalidOperationException(
                "The moved-cask investigation smoke lost perception after the herb search.");
        focus.RefreshFocusFrom(ToNumerics(waterCaskTarget.GlobalPosition));
        if (!RunFocusedSearch(main, search, waterCask)
            || sabotageState.Placements is not { } recoveredPlacements
            || ReferenceEquals(recoveredPlacements, movedPlacements)
            || !ReferenceEquals(
                recoveredPlacements.Entries.Single(),
                sabotageSource.RecoveryCandidate.SourcePlacement)
            || !ReferenceEquals(sabotageState.Evidence, movedEvidence)
            || waterCaskTarget.Position != expectedSourcePosition
            || !ReferenceEquals(
                sabotagePresentation.LastPresentedPlacements,
                recoveredPlacements)
            || search.Perception is not { } recoveredPerception
            || ReferenceEquals(recoveredPerception, herbResolvedPerception)
            || !ReferenceEquals(
                recoveredPerception.Entries.Single(
                    entry => entry.ObjectId == waterCask.Identity.Id).VisibilityState,
                VisibilityState.Identified)
            || !AssertUnchangedAuthority())
        {
            Fail("The exact water-cask investigation did not recover its moved placement.");
            return;
        }

        GD.Print(
            "Water-cask move sabotage investigation smoke passed: explicit sabotage moved the "
            + "exact target, another resolved search left it moved, the cask's exact outcome "
            + "restored its source placement through retained evidence, and no actor, source, or "
            + "world-object truth changed.");
        GetTree().Quit();

        bool AssertUnchangedAuthority() =>
            ReferenceEquals(worldSource.WorldObjects, expectedWorldObjects)
            && ReferenceEquals(worldState.WorldObjects, expectedWorldObjects)
            && ReferenceEquals(sabotageSource.WorldObjects, expectedWorldObjects)
            && ReferenceEquals(sabotageSource.Evidence, expectedSourceEvidence)
            && ReferenceEquals(sabotageSource.Placements, expectedSourcePlacements)
            && ReferenceEquals(
                sabotageSource.Candidate.Destination.Occurrence.Target,
                waterCask)
            && seeker.GlobalPosition == expectedSeekerPosition
            && healer.GlobalPosition == expectedHealerPosition
            && ReferenceEquals(
                initialPerception.Entries.Single(
                    entry => entry.ObjectId == waterCask.Identity.Id).VisibilityState,
                VisibilityState.Noticed);
    }

    private static bool RunFocusedSearch(
        Main main,
        FocusedSearchActivityController search,
        WorldObject expectedTarget)
    {
        Press(main, FocusedSearchStartAction);
        FocusedSearchActivity? activity = search.Activities.Entries.SingleOrDefault();
        if (activity is null
            || !ReferenceEquals(activity.Request.TargetObjectId, expectedTarget.Identity.Id))
        {
            return false;
        }

        Press(main, FocusedSearchCompleteAction);
        FocusedSearchOutcome? outcome = search.Outcomes.Entries.SingleOrDefault();
        if (outcome is null
            || !ReferenceEquals(outcome.Request, activity.Request)
            || !ReferenceEquals(outcome.ResultingVisibilityState, VisibilityState.Identified))
        {
            return false;
        }

        Press(main, FocusedSearchResolveAction);
        return search.Activities.Entries.Count == 0
            && search.Outcomes.Entries.Count == 0;
    }

    private static void Press(Main main, string action)
    {
        main._UnhandledInput(
            new InputEventAction
            {
                Action = new StringName(action),
                Pressed = true,
            });
    }

    private static NumericsVector2 ToNumerics(Vector2 position) =>
        new(position.X, position.Y);

    private void Fail(string message)
    {
        GD.PushError(message);
        GetTree().Quit(1);
    }
}
