using Allrpcs.Domain.Discovery;
using Allrpcs.Domain.Perception;
using Allrpcs.Domain.WorldEvents;
using Allrpcs.Domain.WorldObjects;
using Godot;

namespace Allrpcs.Game;

/// <summary>Checks atomic focused-search resolution and authored world-event occurrence.</summary>
public partial class FocusedSearchWorldEventSmokeCheck : Node
{
    private const string CheckEnvironmentVariable =
        "ALLRPCS_FOCUSED_SEARCH_WORLD_EVENT_CHECK";
    private const string EnabledEnvironmentValue = "1";

    private Main main = null!;
    private FocusedSearchActivityController focusedSearchController = null!;
    private TemporaryWorldEventSource worldEventSource = null!;
    private TemporaryWorldObjectSource worldObjectSource = null!;
    private WorldEventController worldEventController = null!;
    private Label focusedSearchResult = null!;

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
        focusedSearchController = GetNode<FocusedSearchActivityController>(
            "../Seeker/FocusedSearchActivityController");
        worldEventSource = GetNode<TemporaryWorldEventSource>("../TemporaryWorldEventSource");
        worldObjectSource = GetNode<TemporaryWorldObjectSource>("../TemporaryWorldObjectSource");
        worldEventController = GetNode<WorldEventController>("../WorldEventController");
        focusedSearchResult = GetNode<Label>("../CommandOverlay/Panel/FocusedSearchResult");
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
        FocusedSearchActivitySnapshot activities = focusedSearchController.Activities;
        string expectedResultText = focusedSearchResult.Text;

        FocusedSearchOutcome retainedOutcome = Outcome(herbPatch);
        FocusedSearchOutcomeSnapshot outcomes = new([retainedOutcome]);
        WorldObjectVisibility herbVisibility = new(
            herbPatch.Identity.Id,
            VisibilityState.Noticed);
        CharacterPerceptionSnapshot perception = new(
            focusedSearchController.SearcherCharacterId,
            [herbVisibility]);
        SetFocusedSearchState(outcomes, perception);

        if (!InvokeResolution(retainedOutcome, herbPatch))
        {
            Fail("Focused-search world-event smoke rejected an eligible atomic resolution.");
            return;
        }

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
            || !ReferenceEquals(focusedSearchController.Activities, activities)
            || resolvedOutcomes.Entries.Count != 0
            || resolvedPerception.Entries.Count != 1
            || !ReferenceEquals(
                resolvedPerception.Entries[0].ObjectId,
                herbPatch.Identity.Id)
            || !ReferenceEquals(
                resolvedPerception.Entries[0].VisibilityState,
                VisibilityState.Identified)
            || ReferenceEquals(resolvedLog, startupLog)
            || resolvedLog.Entries.Count != 2
            || !ReferenceEquals(resolvedLog.Entries[0], initialEvent.EventId)
            || !ReferenceEquals(resolvedLog.Entries[1], advancingEvent.EventId)
            || ReferenceEquals(resolvedTime, startupTime)
            || !ReferenceEquals(resolvedTime.CurrentEventId, advancingEvent.EventId)
            || !string.Equals(focusedSearchResult.Text, expectedResultText, StringComparison.Ordinal))
        {
            Fail("Focused-search world-event smoke failed exact atomic result retention.");
            return;
        }

        FocusedSearchOutcome blockedOutcome = Outcome(herbPatch);
        FocusedSearchOutcomeSnapshot blockedOutcomes = new([blockedOutcome]);
        CharacterPerceptionSnapshot blockedPerception = new(
            focusedSearchController.SearcherCharacterId,
            [new WorldObjectVisibility(herbPatch.Identity.Id, VisibilityState.Noticed)]);
        SetFocusedSearchState(blockedOutcomes, blockedPerception);

        if (InvokeResolution(blockedOutcome, herbPatch)
            || !ReferenceEquals(focusedSearchController.Outcomes, blockedOutcomes)
            || !ReferenceEquals(focusedSearchController.Perception, blockedPerception)
            || !ReferenceEquals(worldEventController.EventLog, resolvedLog)
            || !ReferenceEquals(worldEventController.WorldTime, resolvedTime)
            || !ReferenceEquals(worldEventController.WorldEvents, worldEvents)
            || !string.Equals(focusedSearchResult.Text, expectedResultText, StringComparison.Ordinal))
        {
            Fail("Focused-search world-event smoke failed all-or-nothing rejection.");
            return;
        }

        GD.Print(
            "Focused-search world-event smoke passed: exact herb-patch identification resolved "
            + "perception and consumed its outcome while atomically recording the exact authored "
            + "advancing event and advancing qualitative time; duplicate-event rejection preserved "
            + "all focused-search and world-event snapshots and presentation.");
        GetTree().Quit();
    }

    private FocusedSearchOutcome Outcome(WorldObject worldObject) =>
        new(
            new FocusedSearchRequest(
                focusedSearchController.SearcherCharacterId,
                worldObject.Identity.Id),
            VisibilityState.Identified);

    private bool InvokeResolution(FocusedSearchOutcome outcome, WorldObject worldObject)
    {
        System.Reflection.MethodInfo method = typeof(Main).GetMethod(
            "TryResolveFocusedSearchOutcome",
            System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)
            ?? throw new InvalidOperationException(
                "Focused-search world-event orchestration was not found.");

        return (bool)(method.Invoke(main, [outcome, worldObject])
            ?? throw new InvalidOperationException(
                "Focused-search world-event orchestration returned no result."));
    }

    private void SetFocusedSearchState(
        FocusedSearchOutcomeSnapshot outcomes,
        CharacterPerceptionSnapshot perception)
    {
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
