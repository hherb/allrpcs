using Allrpcs.Application.Dialogue;
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
    private const string RequestText = "Please wait while I examine the flowers.";
    private static readonly StringName CharacterTurnCancelAction = new("character_turn_cancel");

    private string mode = string.Empty;
    private Main main = null!;
    private CharacterTurnActivityController controller = null!;
    private AutonomousConversationBudgetController budgetController = null!;
    private AutonomousConversationBudget initialBudget = null!;
    private LineEdit input = null!;
    private int stage;

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
        budgetController = GetNode<AutonomousConversationBudgetController>(
            "../AutonomousConversationBudgetController");
        initialBudget = new AutonomousConversationBudget(0, 2, false);
        if (!budgetController.TryInitialize(initialBudget))
        {
            Fail("Character-turn cancellation input could not initialize its conversation budget.");
            return;
        }

        input = GetNode<LineEdit>("../DialogueOverlay/Panel/Input");
    }

    /// <inheritdoc />
    public override void _Process(double delta)
    {
        _ = delta;
        if (stage == 0)
        {
            if (controller.Activity is not null)
            {
                Fail("Character-turn cancellation input smoke did not begin with an empty controller.");
                return;
            }

            input.Text = RequestText;
            input.EmitSignal(LineEdit.SignalName.TextSubmitted, RequestText);
            stage = 1;
            return;
        }

        SetProcess(false);

        if (controller.Activity is null)
        {
            Fail("Character-turn cancellation input smoke found no live player request activity.");
            return;
        }

        if (mode == SpeakingMode
            && !controller.TryReceiveResponse(
                DeterministicExaminationResponseSelector.Select(
                    controller.Activity.Request,
                    GetNode<TemporaryWorldObjectSource>("../TemporaryWorldObjectSource")
                        .WorldObjects.Entries[0].Identity.Id)))
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
            || budgetController.Budget is not { HasPlayerInterrupted: true } interruptedBudget
            || ReferenceEquals(interruptedBudget, initialBudget)
            || interruptedBudget.CompletedTurnCount != 0
            || interruptedBudget.MaximumTurnCount != 2
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
            + "cancelled the current exact character-turn activity, interrupted its autonomous "
            + "conversation, and preserved unrelated scene state.");
        GetTree().Quit();
    }

    private void Fail(string message)
    {
        GD.PushError(message);
        GetTree().Quit(exitCode: 1);
    }
}
