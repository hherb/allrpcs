using Allrpcs.Domain.Discovery;
using Allrpcs.Domain.Perception;
using Allrpcs.Domain.WorldEvents;
using Allrpcs.Domain.WorldObjects;
using Godot;

namespace Allrpcs.Game;

/// <summary>Checks ordinary input-path focused-search world-event occurrence.</summary>
public partial class FocusedSearchWorldEventSmokeCheck : Node
{
    private const string CheckEnvironmentVariable =
        "ALLRPCS_FOCUSED_SEARCH_WORLD_EVENT_CHECK";
    private const string CompleteChangedText = "Focused search COMPLETE: CHANGED";
    private const string EnabledEnvironmentValue = "1";
    private const string FocusedSearchCompleteAction = "focused_search_complete";
    private const string FocusedSearchResolveAction = "focused_search_resolve";
    private const string FocusedSearchStartAction = "focused_search_start";
    private const string ResolveChangedText = "Focused search RESOLVE: CHANGED";
    private const string ResolveUnchangedText = "Focused search RESOLVE: UNCHANGED";
    private const string StartChangedText = "Focused search START: CHANGED";
    private Main main = null!;
    private InteractionFocusController interactionFocusController = null!;
    private FocusedSearchActivityController focusedSearchController = null!;
    private TemporaryResolvedPerceptionIndicator resolvedPerceptionIndicator = null!;
    private TemporaryWorldEventSource worldEventSource = null!;
    private TemporaryWorldObjectSource worldObjectSource = null!;
    private WorldEventController worldEventController = null!;
    private Label focusedSearchResult = null!;
    private CharacterBody2D seeker = null!;
    private SeekerWalkAnimationPresenter seekerAnimationPresenter = null!;

    /// <inheritdoc />
    public override void _Ready()
    {
        bool isEnabled = OS.GetEnvironment(CheckEnvironmentVariable) == EnabledEnvironmentValue;
        SetProcess(isEnabled);
        if (!isEnabled)
        {
            return;
        }

        main = GetNode<Main>("..");
        interactionFocusController = GetNode<InteractionFocusController>(
            "../InteractionFocusController");
        focusedSearchController = GetNode<FocusedSearchActivityController>(
            "../Seeker/FocusedSearchActivityController");
        resolvedPerceptionIndicator = GetNode<TemporaryResolvedPerceptionIndicator>(
            "../InteractionCandidates/HerbPatch/TemporaryResolvedPerceptionIndicator");
        worldEventSource = GetNode<TemporaryWorldEventSource>("../TemporaryWorldEventSource");
        worldObjectSource = GetNode<TemporaryWorldObjectSource>("../TemporaryWorldObjectSource");
        worldEventController = GetNode<WorldEventController>("../WorldEventController");
        focusedSearchResult = GetNode<Label>("../CommandOverlay/Panel/FocusedSearchResult");
        seeker = GetNode<CharacterBody2D>("../Seeker");
        seekerAnimationPresenter =
            seeker.GetNode<SeekerWalkAnimationPresenter>("WalkAnimation");
    }

    /// <inheritdoc />
    public override void _Process(double delta)
    {
        _ = delta;
        SetProcess(false);

        WorldObjectSnapshot worldObjects = worldObjectSource.WorldObjects;
        WorldObject herbPatch = worldObjects.Entries[0];
        WorldEventSnapshot worldEvents = worldEventSource.WorldEvents;
        WorldEvent initialEvent = worldEventSource.InitialWorldEvent;
        WorldEvent advancingEvent = worldEventSource.HerbPatchIdentifiedWorldEvent;
        WorldEventLogSnapshot startupLog = worldEventController.EventLog
            ?? throw new InvalidOperationException("World-event log was not initialized.");
        WorldTimeSnapshot startupTime = worldEventController.WorldTime
            ?? throw new InvalidOperationException("World time was not initialized.");
        FocusedSearchActivitySnapshot initialActivities = new([]);
        FocusedSearchOutcomeSnapshot initialOutcomes = new([]);
        WorldObjectVisibility herbVisibility = new(
            herbPatch.Identity.Id,
            VisibilityState.Noticed);
        CharacterPerceptionSnapshot initialPerception = new(
            focusedSearchController.SearcherCharacterId,
            [herbVisibility]);
        SetFocusedSearchState(initialActivities, initialOutcomes, initialPerception);

        seeker.GlobalPosition = interactionFocusController.GetTarget(0).GlobalPosition;
        interactionFocusController.RefreshFocus();
        if (interactionFocusController.FocusedTargetIndex != 0
            || !ReferenceEquals(
                interactionFocusController.FocusedTargetObjectId,
                herbPatch.Identity.Id)
            || !AssertWorldEventState(worldEvents, startupLog, startupTime)
            || resolvedPerceptionIndicator.IsIndicatorVisible)
        {
            Fail("Focused-search world-event smoke failed ordinary input-path setup.");
            return;
        }

        PressAction(FocusedSearchStartAction);
        FocusedSearchActivitySnapshot startedActivities = focusedSearchController.Activities;
        if (startedActivities.Entries.Count != 1
            || !ReferenceEquals(focusedSearchController.Outcomes, initialOutcomes)
            || !ReferenceEquals(focusedSearchController.Perception, initialPerception)
            || !ReferenceEquals(
                startedActivities.Entries[0].Request.SearcherCharacterId,
                focusedSearchController.SearcherCharacterId)
            || !ReferenceEquals(
                startedActivities.Entries[0].Request.TargetObjectId,
                herbPatch.Identity.Id)
            || !AssertWorldEventState(worldEvents, startupLog, startupTime)
            || !AssertResultText(StartChangedText)
            || resolvedPerceptionIndicator.IsIndicatorVisible)
        {
            Fail("Focused-search world-event smoke failed ordinary input-path start.");
            return;
        }

        FocusedSearchRequest request = startedActivities.Entries[0].Request;
        PressAction(FocusedSearchCompleteAction);
        FocusedSearchActivitySnapshot completedActivities = focusedSearchController.Activities;
        FocusedSearchOutcomeSnapshot completedOutcomes = focusedSearchController.Outcomes;
        if (completedActivities.Entries.Count != 0
            || completedOutcomes.Entries.Count != 1
            || !ReferenceEquals(focusedSearchController.Perception, initialPerception)
            || !ReferenceEquals(completedOutcomes.Entries[0].Request, request)
            || !ReferenceEquals(
                completedOutcomes.Entries[0].ResultingVisibilityState,
                VisibilityState.Identified)
            || !AssertWorldEventState(worldEvents, startupLog, startupTime)
            || !AssertResultText(CompleteChangedText)
            || resolvedPerceptionIndicator.IsIndicatorVisible)
        {
            Fail(
                "Focused-search world-event smoke recorded an event before successful input-path "
                + "resolution or failed exact completion retention.");
            return;
        }

        FocusedSearchOutcome retainedOutcome = completedOutcomes.Entries[0];
        PressAction(FocusedSearchResolveAction);
        FocusedSearchOutcomeSnapshot resolvedOutcomes = focusedSearchController.Outcomes;
        CharacterPerceptionSnapshot resolvedPerception = focusedSearchController.Perception
            ?? throw new InvalidOperationException("Resolved perception was not retained.");
        WorldEventLogSnapshot resolvedLog = worldEventController.EventLog
            ?? throw new InvalidOperationException("Resolved world-event log was not retained.");
        WorldTimeSnapshot resolvedTime = worldEventController.WorldTime
            ?? throw new InvalidOperationException("Resolved world time was not retained.");

        if (!ReferenceEquals(worldEventSource.WorldEvents, worldEvents)
            || !ReferenceEquals(worldObjectSource.WorldObjects, worldObjects)
            || !ReferenceEquals(worldEvents.Entries[0], initialEvent)
            || !ReferenceEquals(worldEvents.Entries[1], advancingEvent)
            || !ReferenceEquals(focusedSearchController.Activities, completedActivities)
            || resolvedOutcomes.Entries.Count != 0
            || resolvedPerception.Entries.Count != 1
            || !ReferenceEquals(resolvedPerception.Entries[0].ObjectId, herbPatch.Identity.Id)
            || !ReferenceEquals(
                resolvedPerception.Entries[0].VisibilityState,
                VisibilityState.Identified)
            || ReferenceEquals(resolvedLog, startupLog)
            || resolvedLog.Entries.Count != 2
            || !ReferenceEquals(resolvedLog.Entries[0], initialEvent.EventId)
            || !ReferenceEquals(resolvedLog.Entries[1], advancingEvent.EventId)
            || ReferenceEquals(resolvedTime, startupTime)
            || !ReferenceEquals(resolvedTime.CurrentEventId, advancingEvent.EventId)
            || !AssertResultText(ResolveChangedText)
            || seekerAnimationPresenter.Animation != "interact_south"
            || !seekerAnimationPresenter.IsPlaying()
            || !resolvedPerceptionIndicator.IsIndicatorVisible
            || !string.Equals(
                resolvedPerceptionIndicator.GetNode<Label>("Indicator").Text,
                herbPatch.Description.Text,
                StringComparison.Ordinal))
        {
            Fail("Focused-search world-event smoke failed exact atomic input-path resolution.");
            return;
        }

        seekerAnimationPresenter.Stop();
        seekerAnimationPresenter.PresentMovement(Vector2.Zero);
        PressAction(FocusedSearchResolveAction);
        if (!ReferenceEquals(focusedSearchController.Activities, completedActivities)
            || !ReferenceEquals(focusedSearchController.Outcomes, resolvedOutcomes)
            || !ReferenceEquals(focusedSearchController.Perception, resolvedPerception)
            || !ReferenceEquals(worldEventController.WorldEvents, worldEvents)
            || !ReferenceEquals(worldEventController.EventLog, resolvedLog)
            || !ReferenceEquals(worldEventController.WorldTime, resolvedTime)
            || !ReferenceEquals(completedOutcomes.Entries[0], retainedOutcome)
            || !ReferenceEquals(completedOutcomes.Entries[0].Request, request)
            || !ReferenceEquals(initialPerception.Entries[0], herbVisibility)
            || !ReferenceEquals(
                initialPerception.Entries[0].VisibilityState,
                VisibilityState.Noticed)
            || !AssertResultText(ResolveUnchangedText)
            || seekerAnimationPresenter.Animation != "idle_south"
            || !seekerAnimationPresenter.IsPlaying()
            || !resolvedPerceptionIndicator.IsIndicatorVisible
            || !string.Equals(
                resolvedPerceptionIndicator.GetNode<Label>("Indicator").Text,
                herbPatch.Description.Text,
                StringComparison.Ordinal))
        {
            Fail("Focused-search world-event smoke failed repeated input-path preservation.");
            return;
        }

        PressAction(FocusedSearchStartAction);
        FocusedSearchActivitySnapshot ineligibleStartedActivities =
            focusedSearchController.Activities;
        if (ineligibleStartedActivities.Entries.Count != 1
            || !ReferenceEquals(focusedSearchController.Outcomes, resolvedOutcomes)
            || !ReferenceEquals(focusedSearchController.Perception, resolvedPerception)
            || !ReferenceEquals(
                ineligibleStartedActivities.Entries[0].Request.TargetObjectId,
                herbPatch.Identity.Id)
            || !AssertWorldEventState(worldEvents, resolvedLog, resolvedTime)
            || !AssertResultText(StartChangedText)
            || !resolvedPerceptionIndicator.IsIndicatorVisible)
        {
            Fail("Focused-search world-event smoke failed ineligible-sequence start.");
            return;
        }

        FocusedSearchRequest ineligibleRequest = ineligibleStartedActivities.Entries[0].Request;
        PressAction(FocusedSearchCompleteAction);
        FocusedSearchActivitySnapshot ineligibleCompletedActivities =
            focusedSearchController.Activities;
        FocusedSearchOutcomeSnapshot ineligibleOutcomes = focusedSearchController.Outcomes;
        if (ineligibleCompletedActivities.Entries.Count != 0
            || ineligibleOutcomes.Entries.Count != 1
            || !ReferenceEquals(ineligibleOutcomes.Entries[0].Request, ineligibleRequest)
            || !ReferenceEquals(focusedSearchController.Perception, resolvedPerception)
            || !AssertWorldEventState(worldEvents, resolvedLog, resolvedTime)
            || !AssertResultText(CompleteChangedText)
            || !resolvedPerceptionIndicator.IsIndicatorVisible)
        {
            Fail("Focused-search world-event smoke failed ineligible-sequence completion.");
            return;
        }

        FocusedSearchOutcome ineligibleOutcome = ineligibleOutcomes.Entries[0];
        PressAction(FocusedSearchResolveAction);
        if (!ReferenceEquals(
                focusedSearchController.Activities,
                ineligibleCompletedActivities)
            || !ReferenceEquals(focusedSearchController.Outcomes, ineligibleOutcomes)
            || !ReferenceEquals(focusedSearchController.Perception, resolvedPerception)
            || !AssertWorldEventState(worldEvents, resolvedLog, resolvedTime)
            || !ReferenceEquals(ineligibleOutcomes.Entries[0], ineligibleOutcome)
            || !ReferenceEquals(ineligibleOutcome.Request, ineligibleRequest)
            || !AssertResultText(ResolveUnchangedText)
            || !resolvedPerceptionIndicator.IsIndicatorVisible
            || !string.Equals(
                resolvedPerceptionIndicator.GetNode<Label>("Indicator").Text,
                herbPatch.Description.Text,
                StringComparison.Ordinal))
        {
            Fail("Focused-search world-event smoke failed ineligible input-path preservation.");
            return;
        }

        GD.Print(
            "Focused-search world-event smoke passed: ordinary start, complete, and resolve input "
            + "preserved event and time state until exact herb-patch resolution, then consumed "
            + "the outcome, identified and presented the herb, recorded the exact advancing "
            + "event, advanced qualitative time, presented one success-only Seeker gesture, and "
            + "preserved all state and idle presentation on repeated or ineligible resolution.");
        GetTree().Quit();
    }

    private void PressAction(string action)
    {
        main._UnhandledInput(
            new InputEventAction
            {
                Action = new StringName(action),
                Pressed = true,
            });
    }

    private bool AssertWorldEventState(
        WorldEventSnapshot expectedEvents,
        WorldEventLogSnapshot expectedLog,
        WorldTimeSnapshot expectedTime)
    {
        return ReferenceEquals(worldEventController.WorldEvents, expectedEvents)
            && ReferenceEquals(worldEventController.EventLog, expectedLog)
            && ReferenceEquals(worldEventController.WorldTime, expectedTime);
    }

    private bool AssertResultText(string expectedText)
    {
        return string.Equals(focusedSearchResult.Text, expectedText, StringComparison.Ordinal);
    }

    private void SetFocusedSearchState(
        FocusedSearchActivitySnapshot activities,
        FocusedSearchOutcomeSnapshot outcomes,
        CharacterPerceptionSnapshot perception)
    {
        SetFocusedSearchProperty(nameof(FocusedSearchActivityController.Activities), activities);
        SetFocusedSearchProperty(nameof(FocusedSearchActivityController.Outcomes), outcomes);
        SetFocusedSearchProperty(nameof(FocusedSearchActivityController.Perception), perception);
    }

    private void SetFocusedSearchProperty(string propertyName, object value)
    {
        System.Reflection.PropertyInfo property = typeof(FocusedSearchActivityController)
            .GetProperty(propertyName)
            ?? throw new InvalidOperationException(
                $"Focused-search {propertyName} property was not found.");
        property.SetValue(focusedSearchController, value);
    }

    private void Fail(string message)
    {
        GD.PushError(message);
        GetTree().Quit(exitCode: 1);
    }
}
