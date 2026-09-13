using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Discovery;
using Allrpcs.Domain.Identifiers;
using Allrpcs.Domain.Perception;
using Allrpcs.Domain.WorldObjects;
using Godot;
using NumericsVector2 = System.Numerics.Vector2;

namespace Allrpcs.Game;

/// <summary>Checks exact player investigation recovery for the main-scene water cask.</summary>
public partial class WaterCaskSabotageInvestigationFlowSmokeCheck : Node
{
    private const string CheckEnvironmentVariable =
        "ALLRPCS_WATER_CASK_SABOTAGE_INVESTIGATION_CHECK";
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
        TemporaryCharacterObjectSabotageSource sabotageSource =
            GetNode<TemporaryCharacterObjectSabotageSource>(
                "../TemporaryCharacterObjectSabotageSource");
        CharacterObjectSabotageConcealmentController sabotageState =
            GetNode<CharacterObjectSabotageConcealmentController>(
                "../CharacterObjectSabotageConcealmentController");
        CharacterObjectSabotageConcealmentPresentationController sabotagePresentation =
            GetNode<CharacterObjectSabotageConcealmentPresentationController>(
                "../CharacterObjectSabotageConcealmentPresentationController");
        TemporaryCharacterObjectSabotagePresentationFlowController sabotageFlow =
            GetNode<TemporaryCharacterObjectSabotagePresentationFlowController>(
                "../TemporaryCharacterObjectSabotagePresentationFlowController");
        CharacterBody2D seeker = GetNode<CharacterBody2D>("../Seeker");
        CharacterBody2D healer = GetNode<CharacterBody2D>("../Healer");

        WorldObjectSnapshot expectedWorldObjects = worldSource.WorldObjects;
        WorldObject herbPatch = expectedWorldObjects.Entries[0];
        WorldObject waterCask = expectedWorldObjects.Entries[^1];
        GreyboxInteractionTarget herbTarget = focus.GetTarget(HerbPatchTargetIndex);
        GreyboxInteractionTarget waterCaskTarget = focus.GetTarget(WaterCaskTargetIndex);
        Vector2 expectedSeekerPosition = seeker.GlobalPosition;
        Vector2 expectedHealerPosition = healer.GlobalPosition;
        DiscoveryEvidenceSnapshot expectedSourceEvidence = sabotageSource.Evidence;
        WorldObjectConcealmentSnapshot expectedSourceConcealment = sabotageSource.Concealment;
        CharacterPerceptionSnapshot initialPerception = search.Perception
            ?? throw new InvalidOperationException(
                "The water-cask investigation smoke requires initialized perception.");

        if (!sabotageFlow.TryApply()
            || sabotageState.Concealment is not { } appliedConcealment
            || sabotageState.Evidence is not { } appliedEvidence
            || appliedConcealment.ConcealedObjectIds.Count != 1
            || !ReferenceEquals(
                appliedConcealment.ConcealedObjectIds[0],
                waterCask.Identity.Id)
            || appliedEvidence.Entries.Count != 1
            || !ReferenceEquals(appliedEvidence.Entries[0], sabotageSource.Candidate.Evidence)
            || waterCaskTarget.Visible
            || !ReferenceEquals(
                sabotagePresentation.LastPresentedConcealment,
                appliedConcealment))
        {
            Fail("The investigation smoke could not explicitly conceal the exact water cask.");
            return;
        }

        focus.RefreshFocusFrom(ToNumerics(herbTarget.GlobalPosition));
        if (!RunFocusedSearch(main, search, herbPatch)
            || !ReferenceEquals(sabotageState.Concealment, appliedConcealment)
            || !ReferenceEquals(sabotageState.Evidence, appliedEvidence)
            || waterCaskTarget.Visible)
        {
            Fail("Resolving another target changed the water-cask concealment.");
            return;
        }

        CharacterPerceptionSnapshot herbResolvedPerception = search.Perception
            ?? throw new InvalidOperationException(
                "The investigation smoke lost perception after the herb search.");
        focus.RefreshFocusFrom(ToNumerics(waterCaskTarget.GlobalPosition));
        if (!RunFocusedSearch(main, search, waterCask)
            || sabotageState.Concealment is not { } recoveredConcealment
            || ReferenceEquals(recoveredConcealment, appliedConcealment)
            || recoveredConcealment.ConcealedObjectIds.Count != 0
            || !ReferenceEquals(sabotageState.Evidence, appliedEvidence)
            || !waterCaskTarget.Visible
            || !ReferenceEquals(
                sabotagePresentation.LastPresentedConcealment,
                recoveredConcealment)
            || search.Perception is not { } recoveredPerception
            || ReferenceEquals(recoveredPerception, herbResolvedPerception)
            || !ReferenceEquals(
                recoveredPerception.Entries.Single(
                    entry => entry.ObjectId == waterCask.Identity.Id).VisibilityState,
                VisibilityState.Identified)
            || !AssertUnchangedAuthority())
        {
            Fail("The exact water-cask investigation did not recover and reveal its concealment.");
            return;
        }

        GD.Print(
            "Water-cask sabotage investigation smoke passed: explicit sabotage concealed the exact "
            + "target, another resolved search left it concealed, the cask's exact F/E/R outcome "
            + "recovered and revealed it through authoritative evidence, and no actor, source, or "
            + "world-object state changed.");
        GetTree().Quit();

        bool AssertUnchangedAuthority() =>
            ReferenceEquals(worldSource.WorldObjects, expectedWorldObjects)
            && ReferenceEquals(worldState.WorldObjects, expectedWorldObjects)
            && ReferenceEquals(sabotageSource.WorldObjects, expectedWorldObjects)
            && ReferenceEquals(sabotageSource.Evidence, expectedSourceEvidence)
            && ReferenceEquals(sabotageSource.Concealment, expectedSourceConcealment)
            && ReferenceEquals(sabotageSource.Candidate.Occurrence.Target, waterCask)
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
