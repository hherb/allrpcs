using Allrpcs.Domain.Dialogue;
using Allrpcs.Domain.Discovery;
using Allrpcs.Domain.Perception;
using Allrpcs.Domain.WorldEvents;
using Allrpcs.Domain.WorldObjects;
using Godot;

namespace Allrpcs.Game;

/// <summary>Runs focused player character-turn cancellation input checks when enabled.</summary>
public partial class CharacterTurnCancellationInputSmokeCheck : Node
{
    private const string CheckEnvironmentVariable = "ALLRPCS_CHARACTER_TURN_INPUT_CHECK";
    private const string AwaitingMode = "awaiting";
    private const string SpeakingMode = "speaking";
    private static readonly StringName CharacterTurnCancelAction = new("character_turn_cancel");

    private string mode = string.Empty;
    private Main main = null!;
    private CharacterTurnActivityController controller = null!;
    private TemporaryCharacterTurnSource source = null!;

    /// <inheritdoc />
    public override void _Ready()
    {
        mode = OS.GetEnvironment(CheckEnvironmentVariable);
        bool isEnabled = mode is AwaitingMode or SpeakingMode;
        SetProcess(isEnabled);
        if (!isEnabled)
        {
            return;
        }

        main = GetNode<Main>("..");
        controller = GetNode<CharacterTurnActivityController>("../CharacterTurnActivityController");
        source = GetNode<TemporaryCharacterTurnSource>("../TemporaryCharacterTurnSource");
    }

    /// <inheritdoc />
    public override void _Process(double delta)
    {
        _ = delta;
        SetProcess(false);

        if (!ReferenceEquals(controller.Activity, source.Activity))
        {
            Fail("Character-turn cancellation input smoke found no exact initialized activity.");
            return;
        }

        if (mode == SpeakingMode && !controller.TryReceiveResponse(source.Response))
        {
            Fail("Character-turn cancellation input smoke could not enter speaking state.");
            return;
        }

        CharacterTurnActivity before = controller.Activity!;
        WorldEventController worldEventController = GetNode<WorldEventController>("../WorldEventController");
        WorldObjectController worldObjectController = GetNode<WorldObjectController>("../WorldObjectController");
        FocusedSearchActivityController focusedSearchController = GetNode<FocusedSearchActivityController>(
            "../Seeker/FocusedSearchActivityController");
        CharacterBody2D seeker = GetNode<CharacterBody2D>("../Seeker");
        CharacterBody2D healer = GetNode<CharacterBody2D>("../Healer");
        Label modeStatus = GetNode<Label>("../CommandOverlay/Panel/ModeStatus");
        Label focusStatus = GetNode<Label>("../CommandOverlay/Panel/FocusStatus");
        Label searchStatus = GetNode<Label>("../CommandOverlay/Panel/FocusedSearchResult");
        WorldEventSnapshot? expectedWorldEvents = worldEventController.WorldEvents;
        WorldEventLogSnapshot? expectedEventLog = worldEventController.EventLog;
        WorldTimeSnapshot? expectedWorldTime = worldEventController.WorldTime;
        WorldObjectSnapshot? expectedWorldObjects = worldObjectController.WorldObjects;
        FocusedSearchActivitySnapshot expectedSearchActivities = focusedSearchController.Activities;
        FocusedSearchOutcomeSnapshot expectedSearchOutcomes = focusedSearchController.Outcomes;
        CharacterPerceptionSnapshot? expectedPerception = focusedSearchController.Perception;
        Vector2 expectedSeekerPosition = seeker.Position;
        Vector2 expectedHealerPosition = healer.Position;
        string expectedModeStatus = modeStatus.Text;
        string expectedFocusStatus = focusStatus.Text;
        string expectedSearchStatus = searchStatus.Text;

        main._UnhandledInput(new InputEventAction
        {
            Action = CharacterTurnCancelAction,
            Pressed = true,
        });

        CharacterTurnActivity? cancelled = controller.Activity;
        if (cancelled is null
            || ReferenceEquals(cancelled, before)
            || !ReferenceEquals(cancelled.Request, before.Request)
            || !ReferenceEquals(cancelled.Response, before.Response)
            || !ReferenceEquals(cancelled.State, CharacterTurnActivityState.Cancelled)
            || !ReferenceEquals(worldEventController.WorldEvents, expectedWorldEvents)
            || !ReferenceEquals(worldEventController.EventLog, expectedEventLog)
            || !ReferenceEquals(worldEventController.WorldTime, expectedWorldTime)
            || !ReferenceEquals(worldObjectController.WorldObjects, expectedWorldObjects)
            || !ReferenceEquals(focusedSearchController.Activities, expectedSearchActivities)
            || !ReferenceEquals(focusedSearchController.Outcomes, expectedSearchOutcomes)
            || !ReferenceEquals(focusedSearchController.Perception, expectedPerception)
            || seeker.Position != expectedSeekerPosition
            || healer.Position != expectedHealerPosition
            || !string.Equals(modeStatus.Text, expectedModeStatus, StringComparison.Ordinal)
            || !string.Equals(focusStatus.Text, expectedFocusStatus, StringComparison.Ordinal)
            || !string.Equals(searchStatus.Text, expectedSearchStatus, StringComparison.Ordinal))
        {
            Fail("Character-turn cancellation input changed the wrong state or failed cancellation.");
            return;
        }

        GD.Print(
            $"Character-turn cancellation input smoke passed ({mode}): the explicit player action "
            + "cancelled only the current exact character-turn activity.");
        GetTree().Quit();
    }

    private void Fail(string message)
    {
        GD.PushError(message);
        GetTree().Quit(exitCode: 1);
    }
}
