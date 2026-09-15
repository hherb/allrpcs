using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Discovery;
using Allrpcs.Domain.Perception;
using Allrpcs.Domain.WorldObjects;
using Godot;
using NumericsVector2 = System.Numerics.Vector2;

namespace Allrpcs.Game;

/// <summary>Checks exact focused-search recovery for the contaminated main-scene water cask.</summary>
public partial class WaterCaskContaminationSabotageInvestigationFlowSmokeCheck : Node
{
    private const string CheckEnvironmentVariable =
        "ALLRPCS_WATER_CASK_CONTAMINATION_SABOTAGE_INVESTIGATION_CHECK";
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
        TemporaryCharacterObjectSabotageContaminationSource sabotageSource =
            GetNode<TemporaryCharacterObjectSabotageContaminationSource>(
                "../TemporaryCharacterObjectSabotageContaminationSource");
        CharacterObjectSabotageContaminationController sabotageState =
            GetNode<CharacterObjectSabotageContaminationController>(
                "../CharacterObjectSabotageContaminationController");
        CharacterObjectSabotageContaminationPresentationController sabotagePresentation =
            GetNode<CharacterObjectSabotageContaminationPresentationController>(
                "../CharacterObjectSabotageContaminationPresentationController");
        TemporaryCharacterObjectSabotageContaminationPresentationFlowController sabotageFlow =
            GetNode<TemporaryCharacterObjectSabotageContaminationPresentationFlowController>(
                "../TemporaryCharacterObjectSabotageContaminationPresentationFlowController");
        CharacterBody2D seeker = GetNode<CharacterBody2D>("../Seeker");
        CharacterBody2D healer = GetNode<CharacterBody2D>("../Healer");
        CanvasItem marker = GetNode<CanvasItem>(
            "../InteractionCandidates/WaterCask/ContaminationMarker");

        WorldObjectSnapshot expectedWorldObjects = worldSource.WorldObjects;
        WorldObject herbPatch = expectedWorldObjects.Entries[0];
        WorldObject waterCask = expectedWorldObjects.Entries[^1];
        GreyboxInteractionTarget herbTarget = focus.GetTarget(HerbPatchTargetIndex);
        GreyboxInteractionTarget waterCaskTarget = focus.GetTarget(WaterCaskTargetIndex);
        Vector2 expectedWaterCaskPosition = waterCaskTarget.Position;
        Vector2 expectedSeekerPosition = seeker.GlobalPosition;
        Vector2 expectedHealerPosition = healer.GlobalPosition;
        DiscoveryEvidenceSnapshot expectedSourceEvidence = sabotageSource.Evidence;
        WorldObjectContaminationSnapshot expectedSourceContamination = sabotageSource.Contamination;
        CharacterPerceptionSnapshot initialPerception = search.Perception
            ?? throw new InvalidOperationException(
                "The contaminated-cask investigation smoke requires initialized perception.");

        if (!sabotageState.TryApply(
                sabotageSource.Character,
                sabotageSource.PhysicalAccess,
                sabotageSource.WorldObjects,
                expectedSourceContamination,
                expectedSourceEvidence,
                sabotageSource.Candidate)
            || sabotageState.Contamination is not { } appliedContamination
            || sabotageState.Evidence is not { } appliedEvidence
            || !sabotagePresentation.TryPresent(
                expectedSourceContamination,
                appliedContamination)
            || !ReferenceEquals(
                appliedContamination.ContaminatedObjectIds.Single(),
                waterCask.Identity.Id)
            || !ReferenceEquals(appliedEvidence.Entries.Single(), sabotageSource.Candidate.Evidence)
            || !marker.Visible
            || !ReferenceEquals(
                sabotagePresentation.LastPresentedContamination,
                appliedContamination))
        {
            Fail("The investigation smoke could not explicitly contaminate the exact water cask.");
            return;
        }

        if (!sabotagePresentation.TryPresent(
                appliedContamination,
                expectedSourceContamination)
            || marker.Visible
            || sabotageFlow.TryRecover(initialPerception)
            || !ThrowsArgumentNull(() => sabotageFlow.TryRecover(null!))
            || !ReferenceEquals(sabotageState.Contamination, appliedContamination)
            || !ReferenceEquals(sabotageState.Evidence, appliedEvidence)
            || !sabotagePresentation.TryPresent(
                expectedSourceContamination,
                appliedContamination)
            || !marker.Visible)
        {
            Fail("Stale or null recovery input changed contamination or presentation state.");
            return;
        }

        focus.RefreshFocusFrom(ToNumerics(herbTarget.GlobalPosition));
        if (!RunFocusedSearch(main, search, herbPatch)
            || !ReferenceEquals(sabotageState.Contamination, appliedContamination)
            || !ReferenceEquals(sabotageState.Evidence, appliedEvidence)
            || !marker.Visible)
        {
            Fail("Resolving another target changed the contaminated water cask.");
            return;
        }

        CharacterPerceptionSnapshot herbResolvedPerception = search.Perception
            ?? throw new InvalidOperationException(
                "The contaminated-cask investigation smoke lost perception after the herb search.");
        focus.RefreshFocusFrom(ToNumerics(waterCaskTarget.GlobalPosition));
        if (!RunFocusedSearch(main, search, waterCask)
            || sabotageState.Contamination is not { } recoveredContamination
            || ReferenceEquals(recoveredContamination, appliedContamination)
            || recoveredContamination.ContaminatedObjectIds.Count != 0
            || !ReferenceEquals(sabotageState.Evidence, appliedEvidence)
            || marker.Visible
            || !ReferenceEquals(
                sabotagePresentation.LastPresentedContamination,
                recoveredContamination)
            || search.Perception is not { } recoveredPerception
            || ReferenceEquals(recoveredPerception, herbResolvedPerception)
            || recoveredPerception.Entries.Single(
                    entry => entry.ObjectId == waterCask.Identity.Id).VisibilityState
                != VisibilityState.Identified
            || !AssertUnchangedAuthority())
        {
            Fail("The exact water-cask investigation did not recover its contamination.");
            return;
        }

        GD.Print(
            "Water-cask contamination investigation smoke passed: explicit sabotage presented "
            + "the exact contamination, another resolved search left it active, the cask's exact "
            + "outcome hid its marker through retained evidence, and no actor, source, position, "
            + "or world-object truth changed.");
        GetTree().Quit();

        bool AssertUnchangedAuthority() =>
            ReferenceEquals(worldSource.WorldObjects, expectedWorldObjects)
            && ReferenceEquals(worldState.WorldObjects, expectedWorldObjects)
            && ReferenceEquals(sabotageSource.WorldObjects, expectedWorldObjects)
            && ReferenceEquals(sabotageSource.Evidence, expectedSourceEvidence)
            && ReferenceEquals(sabotageSource.Contamination, expectedSourceContamination)
            && ReferenceEquals(sabotageSource.Candidate.Occurrence.Target, waterCask)
            && waterCaskTarget.Position == expectedWaterCaskPosition
            && seeker.GlobalPosition == expectedSeekerPosition
            && healer.GlobalPosition == expectedHealerPosition
            && HasClosedFlowSurface()
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

    private static bool HasClosedFlowSurface()
    {
        Type type = typeof(TemporaryCharacterObjectSabotageContaminationPresentationFlowController);
        string[] methodNames = type.GetMethods(
                System.Reflection.BindingFlags.Public
                | System.Reflection.BindingFlags.Instance
                | System.Reflection.BindingFlags.DeclaredOnly)
            .Where(method => !method.IsSpecialName)
            .Select(method => method.Name)
            .Order(StringComparer.Ordinal)
            .ToArray();

        return methodNames.SequenceEqual(["TryRecover", "_Ready"])
            && type.GetProperties(
                System.Reflection.BindingFlags.Public
                | System.Reflection.BindingFlags.Instance
                | System.Reflection.BindingFlags.DeclaredOnly).Length == 0;
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
