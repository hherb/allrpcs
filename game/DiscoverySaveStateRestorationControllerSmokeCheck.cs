using Allrpcs.Domain.Discovery;
using Allrpcs.Domain.Identifiers;
using Allrpcs.Domain.Perception;
using Allrpcs.Domain.Persistence;
using Allrpcs.Domain.WorldEvents;
using Allrpcs.Domain.WorldObjects;
using Godot;

namespace Allrpcs.Game;

/// <summary>
/// Runs credential-free discovery save-state controller restoration checks when enabled.
/// </summary>
public partial class DiscoverySaveStateRestorationControllerSmokeCheck : Node
{
    private const string CheckEnvironmentVariable =
        "ALLRPCS_DISCOVERY_SAVE_STATE_RESTORATION_CONTROLLER_CHECK";
    private const string EnabledEnvironmentValue = "1";

    private FocusedSearchActivityController controller = null!;
    private WorldObjectController worldObjectController = null!;
    private WorldEventController worldEventController = null!;
    private Line2D activityIndicator = null!;
    private CharacterBody2D seeker = null!;
    private Label focusStatus = null!;
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

        controller = GetNode<FocusedSearchActivityController>(
            "../Seeker/FocusedSearchActivityController");
        worldObjectController = GetNode<WorldObjectController>("../WorldObjectController");
        worldEventController = GetNode<WorldEventController>("../WorldEventController");
        activityIndicator = GetNode<Line2D>(
            "../Seeker/FocusedSearchActivityIndicator/Indicator");
        seeker = GetNode<CharacterBody2D>("../Seeker");
        focusStatus = GetNode<Label>("../CommandOverlay/Panel/FocusStatus");
        focusedSearchResult = GetNode<Label>("../CommandOverlay/Panel/FocusedSearchResult");
    }

    /// <inheritdoc />
    public override void _Process(double delta)
    {
        _ = delta;
        SetProcess(false);

        WorldObjectSnapshot worldObjects = worldObjectController.WorldObjects
            ?? throw new InvalidOperationException("World objects were not initialized.");
        WorldEventSnapshot? expectedWorldEvents = worldEventController.WorldEvents;
        WorldEventLogSnapshot? expectedEventLog = worldEventController.EventLog;
        WorldTimeSnapshot? expectedWorldTime = worldEventController.WorldTime;
        Vector2 expectedPosition = seeker.Position;
        Vector2 expectedVelocity = seeker.Velocity;
        string expectedFocusStatus = focusStatus.Text;
        string expectedFocusedSearchResult = focusedSearchResult.Text;

        CharacterId savedSearcherId = new(controller.SearcherCharacterId.Value);
        ObjectId firstTargetId = new(worldObjects.Entries[0].Identity.Id.Value);
        ObjectId secondTargetId = new(worldObjects.Entries[1].Identity.Id.Value);
        DiscoverySaveState state = CreateState(savedSearcherId, firstTargetId, secondTargetId);

        if (ReferenceEquals(savedSearcherId, controller.SearcherCharacterId)
            || savedSearcherId != controller.SearcherCharacterId
            || !controller.TryRestoreDiscoveryState(state, worldObjects)
            || !HasExactState(state)
            || !activityIndicator.Visible)
        {
            Fail("Discovery save-state controller smoke failed exact eligible restoration.");
            return;
        }

        if (!controller.TryRestoreDiscoveryState(state, worldObjects)
            || !HasExactState(state)
            || !activityIndicator.Visible)
        {
            Fail("Discovery save-state controller smoke failed repeated exact restoration.");
            return;
        }

        DiscoverySaveState wrongOwner = CreateState(
            new CharacterId("travelling-healer"),
            firstTargetId,
            secondTargetId);
        DiscoverySaveState missingTarget = CreateState(
            savedSearcherId,
            firstTargetId,
            new ObjectId("missing-object"));
        WorldObjectSnapshot incompleteWorldObjects = new([worldObjects.Entries[0]]);

        if (controller.TryRestoreDiscoveryState(wrongOwner, worldObjects)
            || !HasExactState(state)
            || controller.TryRestoreDiscoveryState(missingTarget, worldObjects)
            || !HasExactState(state)
            || controller.TryRestoreDiscoveryState(state, incompleteWorldObjects)
            || !HasExactState(state)
            || !RejectsNullInputs(state, worldObjects)
            || !HasExactState(state)
            || !activityIndicator.Visible)
        {
            Fail("Discovery save-state controller smoke failed rejection or state preservation.");
            return;
        }

        if (!ReferenceEquals(worldObjectController.WorldObjects, worldObjects)
            || !ReferenceEquals(worldEventController.WorldEvents, expectedWorldEvents)
            || !ReferenceEquals(worldEventController.EventLog, expectedEventLog)
            || !ReferenceEquals(worldEventController.WorldTime, expectedWorldTime)
            || seeker.Position != expectedPosition
            || seeker.Velocity != expectedVelocity
            || !string.Equals(focusStatus.Text, expectedFocusStatus, StringComparison.Ordinal)
            || !string.Equals(
                focusedSearchResult.Text,
                expectedFocusedSearchResult,
                StringComparison.Ordinal))
        {
            Fail("Discovery restoration changed world, event, movement, input, or other presentation state.");
            return;
        }

        GD.Print(
            "Discovery save-state controller smoke passed: exact atomic discovery snapshot "
            + "installation, activity presentation, value-equal ownership and targets, repeated "
            + "restoration, fail-closed unsupported owner, missing target, incomplete world, and "
            + "null handling, and unchanged world, event, movement, input, and other presentation "
            + "state were correct.");
        GetTree().Quit();
    }

    private bool HasExactState(DiscoverySaveState state) =>
        ReferenceEquals(controller.Perception, state.Perception)
        && ReferenceEquals(controller.Activities, state.FocusedSearchActivities)
        && ReferenceEquals(controller.Outcomes, state.FocusedSearchOutcomes)
        && ReferenceEquals(
            controller.PassiveNoticingCandidates,
            state.PassiveNoticingCandidates)
        && ReferenceEquals(
            controller.ObservationReinterpretationCandidates,
            state.ObservationReinterpretationCandidates);

    private bool RejectsNullInputs(
        DiscoverySaveState state,
        WorldObjectSnapshot worldObjects) =>
        ThrowsArgumentNull(() => controller.TryRestoreDiscoveryState(null!, worldObjects))
        && ThrowsArgumentNull(() => controller.TryRestoreDiscoveryState(state, null!));

    private static bool ThrowsArgumentNull(Action action)
    {
        try
        {
            action();
        }
        catch (ArgumentNullException)
        {
            return true;
        }

        return false;
    }

    private static DiscoverySaveState CreateState(
        CharacterId owner,
        ObjectId firstTarget,
        ObjectId secondTarget)
    {
        FocusedSearchRequest activityRequest = new(owner, firstTarget);
        FocusedSearchRequest outcomeRequest = new(owner, secondTarget);
        return new DiscoverySaveState(
            new CharacterPerceptionSnapshot(
                owner,
                [
                    new WorldObjectVisibility(firstTarget, VisibilityState.Noticed),
                    new WorldObjectVisibility(secondTarget, VisibilityState.Identified),
                ]),
            new FocusedSearchActivitySnapshot([new FocusedSearchActivity(activityRequest)]),
            new FocusedSearchOutcomeSnapshot(
                [new FocusedSearchOutcome(outcomeRequest, VisibilityState.Understood)]),
            new PassiveNoticingCandidateSnapshot(
                [new PassiveNoticingCandidate(owner, firstTarget)]),
            new ObservationReinterpretationCandidateSnapshot(
                [
                    new ObservationReinterpretationCandidate(
                        owner,
                        secondTarget,
                        new KnowledgeId("stone-marker-meaning")),
                ]));
    }

    private void Fail(string message)
    {
        GD.PushError(message);
        GetTree().Quit(exitCode: 1);
    }
}
