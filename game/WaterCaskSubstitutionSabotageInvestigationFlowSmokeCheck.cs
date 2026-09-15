using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Discovery;
using Allrpcs.Domain.Perception;
using Allrpcs.Domain.WorldObjects;
using Godot;
using NumericsVector2 = System.Numerics.Vector2;

namespace Allrpcs.Game;

/// <summary>Checks exact focused-search recovery for the substituted main-scene water cask.</summary>
public partial class WaterCaskSubstitutionSabotageInvestigationFlowSmokeCheck : Node
{
    private const string CheckEnvironmentVariable =
        "ALLRPCS_WATER_CASK_SUBSTITUTION_SABOTAGE_INVESTIGATION_CHECK";
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
        TemporaryCharacterObjectSabotageSubstitutionSource sabotageSource =
            GetNode<TemporaryCharacterObjectSabotageSubstitutionSource>(
                "../TemporaryCharacterObjectSabotageSubstitutionSource");
        CharacterObjectSabotageSubstitutionController sabotageState =
            GetNode<CharacterObjectSabotageSubstitutionController>(
                "../CharacterObjectSabotageSubstitutionController");
        CharacterObjectSabotageSubstitutionPresentationController sabotagePresentation =
            GetNode<CharacterObjectSabotageSubstitutionPresentationController>(
                "../CharacterObjectSabotageSubstitutionPresentationController");
        TemporaryCharacterObjectSabotageSubstitutionPresentationFlowController sabotageFlow =
            GetNode<TemporaryCharacterObjectSabotageSubstitutionPresentationFlowController>(
                "../TemporaryCharacterObjectSabotageSubstitutionPresentationFlowController");
        CharacterBody2D seeker = GetNode<CharacterBody2D>("../Seeker");
        CharacterBody2D healer = GetNode<CharacterBody2D>("../Healer");
        CanvasItem originalBody = GetNode<CanvasItem>("../InteractionCandidates/WaterCask/Body");
        CanvasItem replacementBody =
            GetNode<CanvasItem>("../InteractionCandidates/WaterCask/SubstituteBody");

        WorldObjectSnapshot expectedWorldObjects = worldSource.WorldObjects;
        WorldObject herbPatch = expectedWorldObjects.Entries[0];
        WorldObject waterCask = expectedWorldObjects.Entries[^1];
        GreyboxInteractionTarget herbTarget = focus.GetTarget(HerbPatchTargetIndex);
        GreyboxInteractionTarget waterCaskTarget = focus.GetTarget(WaterCaskTargetIndex);
        Vector2 expectedWaterCaskPosition = waterCaskTarget.Position;
        Vector2 expectedSeekerPosition = seeker.GlobalPosition;
        Vector2 expectedHealerPosition = healer.GlobalPosition;
        DiscoveryEvidenceSnapshot expectedSourceEvidence = sabotageSource.Evidence;
        CharacterObjectSabotageSubstitutionSnapshot expectedSourceSubstitutions =
            sabotageSource.Substitutions;
        CharacterPerceptionSnapshot initialPerception = search.Perception
            ?? throw new InvalidOperationException(
                "The substituted-cask investigation smoke requires initialized perception.");

        if (!sabotageFlow.TryApply()
            || sabotageState.Substitutions is not { } appliedSubstitutions
            || sabotageState.Evidence is not { } appliedEvidence
            || !ReferenceEquals(
                appliedSubstitutions.Entries.Single(),
                sabotageSource.Candidate.Substitution)
            || !ReferenceEquals(
                appliedEvidence.Entries.Single(),
                sabotageSource.Candidate.Evidence.Evidence)
            || originalBody.Visible
            || !replacementBody.Visible
            || !ReferenceEquals(
                sabotagePresentation.LastPresentedSubstitutions,
                appliedSubstitutions))
        {
            Fail("The investigation smoke could not explicitly substitute the exact water cask.");
            return;
        }

        focus.RefreshFocusFrom(ToNumerics(herbTarget.GlobalPosition));
        if (!RunFocusedSearch(main, search, herbPatch)
            || !ReferenceEquals(sabotageState.Substitutions, appliedSubstitutions)
            || !ReferenceEquals(sabotageState.Evidence, appliedEvidence)
            || originalBody.Visible
            || !replacementBody.Visible)
        {
            Fail("Resolving another target changed the substituted water cask.");
            return;
        }

        CharacterPerceptionSnapshot herbResolvedPerception = search.Perception
            ?? throw new InvalidOperationException(
                "The substituted-cask investigation smoke lost perception after the herb search.");
        focus.RefreshFocusFrom(ToNumerics(waterCaskTarget.GlobalPosition));
        if (!RunFocusedSearch(main, search, waterCask)
            || sabotageState.Substitutions is not { } recoveredSubstitutions
            || ReferenceEquals(recoveredSubstitutions, appliedSubstitutions)
            || recoveredSubstitutions.Entries.Count != 0
            || !ReferenceEquals(sabotageState.Evidence, appliedEvidence)
            || !originalBody.Visible
            || replacementBody.Visible
            || !ReferenceEquals(
                sabotagePresentation.LastPresentedSubstitutions,
                recoveredSubstitutions)
            || search.Perception is not { } recoveredPerception
            || ReferenceEquals(recoveredPerception, herbResolvedPerception)
            || recoveredPerception.Entries.Single(
                    entry => entry.ObjectId == waterCask.Identity.Id).VisibilityState
                != VisibilityState.Identified
            || !AssertUnchangedAuthority())
        {
            Fail("The exact water-cask investigation did not recover its substitution.");
            return;
        }

        GD.Print(
            "Water-cask substitution investigation smoke passed: explicit sabotage presented the "
            + "exact replacement, another resolved search left it active, the cask's exact outcome "
            + "restored its original body through retained evidence, and no actor, source, position, "
            + "or world-object truth changed.");
        GetTree().Quit();

        bool AssertUnchangedAuthority() =>
            ReferenceEquals(worldSource.WorldObjects, expectedWorldObjects)
            && ReferenceEquals(worldState.WorldObjects, expectedWorldObjects)
            && ReferenceEquals(sabotageSource.WorldObjects, expectedWorldObjects)
            && ReferenceEquals(sabotageSource.Evidence, expectedSourceEvidence)
            && ReferenceEquals(sabotageSource.Substitutions, expectedSourceSubstitutions)
            && ReferenceEquals(
                sabotageSource.Candidate.Substitution.Occurrence.Target,
                waterCask)
            && waterCaskTarget.Position == expectedWaterCaskPosition
            && seeker.GlobalPosition == expectedSeekerPosition
            && healer.GlobalPosition == expectedHealerPosition
            && initialPerception.Entries.Single(
                    entry => entry.ObjectId == waterCask.Identity.Id).VisibilityState
                == VisibilityState.Noticed;
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
            || outcome.ResultingVisibilityState != VisibilityState.Identified)
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
