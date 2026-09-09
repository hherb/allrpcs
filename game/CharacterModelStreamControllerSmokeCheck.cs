using Allrpcs.Application.Dialogue;
using Godot;

namespace Allrpcs.Game;

/// <summary>Proves one scene-owned model stream drives only content-free status.</summary>
public partial class CharacterModelStreamControllerSmokeCheck : Node
{
    private const string CheckEnvironmentVariable = "ALLRPCS_CHARACTER_MODEL_STREAM_CHECK";
    private const string EnabledEnvironmentValue = "1";

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
        if (OS.GetEnvironment(CheckEnvironmentVariable) != EnabledEnvironmentValue)
        {
            return;
        }

        CharacterModelStreamController controller =
            GetNode<CharacterModelStreamController>("../CharacterModelStreamController");
        TemporaryCharacterModelGenerationStatusLabel label =
            GetNode<TemporaryCharacterModelGenerationStatusLabel>(
                "../DialogueOverlay/Panel/ModelStatus");
        CharacterTurnActivityController turnController =
            GetNode<CharacterTurnActivityController>("../CharacterTurnActivityController");
        CharacterTurnTranscriptController transcriptController =
            GetNode<CharacterTurnTranscriptController>("../CharacterTurnTranscriptController");
        WorldEventController worldEventController =
            GetNode<WorldEventController>("../WorldEventController");
        FocusedSearchActivityController searchController =
            GetNode<FocusedSearchActivityController>(
                "../Seeker/FocusedSearchActivityController");
        TemporaryCharacterTurnResponseLabel responseLabel =
            GetNode<TemporaryCharacterTurnResponseLabel>(
                "../DialogueOverlay/Panel/Response");

        CharacterModelStreamSnapshot initial = controller.Snapshot
            ?? throw new InvalidOperationException("Scene model stream was not initialized.");
        object? turn = turnController.Activity;
        object? transcript = transcriptController.Transcript;
        object? worldEvents = worldEventController.WorldEvents;
        object? eventLog = worldEventController.EventLog;
        object? worldTime = worldEventController.WorldTime;
        object activities = searchController.Activities;
        object outcomes = searchController.Outcomes;
        object? perception = searchController.Perception;
        string responseText = responseLabel.Text;
        CharacterModelTextDelta text = new("private streamed response prefix");
        CharacterModelStreamTermination timedOut = new(
            CharacterModelStreamTerminationStatus.TimedOut,
            null,
            null);

        CharacterModelStreamController uninitialized = new();
        AddChild(uninitialized);
        if (uninitialized.TryAppend(text)
            || label.Observe(uninitialized)
            || uninitialized.Snapshot is not null
            || label.Visible
            || label.Presentation is not null)
        {
            Fail("Uninitialized model-stream observation changed controller or label state.");
            return;
        }

        try
        {
            uninitialized.TryInitialize(null!);
            Fail("Scene model stream accepted null initialization.");
            return;
        }
        catch (ArgumentNullException)
        {
            if (uninitialized.Snapshot is not null)
            {
                Fail("Null model-stream initialization changed controller state.");
                return;
            }
        }

        if (initial.Updates.Count != 0
            || !label.Observe(controller)
            || !ReferenceEquals(
                label.Presentation,
                CharacterModelGenerationStatusPresentation.Generating)
            || !controller.TryAppend(text)
            || !label.Observe(controller)
            || controller.Snapshot is not { } generating
            || !ReferenceEquals(text, AssertSingle(generating))
            || !ReferenceEquals(
                label.Presentation,
                CharacterModelGenerationStatusPresentation.Generating)
            || label.Text.Contains(text.Text, StringComparison.Ordinal))
        {
            Fail("Scene model stream did not preserve exact generating state safely.");
            return;
        }

        if (!controller.TryAppend(timedOut)
            || !label.Observe(controller)
            || controller.Snapshot is not { } terminal
            || terminal.Updates.Count != 2
            || !ReferenceEquals(terminal.Updates[0], text)
            || !ReferenceEquals(terminal.Updates[1], timedOut)
            || !ReferenceEquals(
                label.Presentation,
                CharacterModelGenerationStatusPresentation.TimedOut)
            || label.Text != "Character response: TIMED OUT")
        {
            Fail("Scene model stream did not present the exact timeout termination safely.");
            return;
        }

        try
        {
            controller.TryAppend(null!);
            Fail("Scene model stream accepted a null update.");
            return;
        }
        catch (ArgumentNullException)
        {
        }

        if (controller.TryInitialize(new CharacterModelStreamSnapshot([]))
            || controller.TryAppend(new CharacterModelTextDelta("late private response"))
            || !ReferenceEquals(controller.Snapshot, terminal)
            || !label.Observe(controller)
            || !ReferenceEquals(
                label.Presentation,
                CharacterModelGenerationStatusPresentation.TimedOut)
            || !SurroundingStateIsUnchanged(
                turnController,
                transcriptController,
                worldEventController,
                searchController,
                responseLabel,
                turn,
                transcript,
                worldEvents,
                eventLog,
                worldTime,
                activities,
                outcomes,
                perception,
                responseText))
        {
            Fail("Rejected model-stream updates changed scene or presentation state.");
            return;
        }

        GD.Print(
            "Character-model stream controller smoke passed: explicit empty initialization, "
            + "exact text-prefix and timeout appends, canonical content-free presentation, "
            + "rejected-state preservation, and unchanged surrounding scene state.");
        GetTree().Quit();
    }

    private static CharacterModelStreamUpdate? AssertSingle(CharacterModelStreamSnapshot snapshot) =>
        snapshot.Updates.Count == 1 ? snapshot.Updates[0] : null;

    private static bool SurroundingStateIsUnchanged(
        CharacterTurnActivityController turnController,
        CharacterTurnTranscriptController transcriptController,
        WorldEventController worldEventController,
        FocusedSearchActivityController searchController,
        TemporaryCharacterTurnResponseLabel responseLabel,
        object? turn,
        object? transcript,
        object? worldEvents,
        object? eventLog,
        object? worldTime,
        object activities,
        object outcomes,
        object? perception,
        string responseText) =>
        ReferenceEquals(turnController.Activity, turn)
        && ReferenceEquals(transcriptController.Transcript, transcript)
        && ReferenceEquals(worldEventController.WorldEvents, worldEvents)
        && ReferenceEquals(worldEventController.EventLog, eventLog)
        && ReferenceEquals(worldEventController.WorldTime, worldTime)
        && ReferenceEquals(searchController.Activities, activities)
        && ReferenceEquals(searchController.Outcomes, outcomes)
        && ReferenceEquals(searchController.Perception, perception)
        && responseLabel.Text == responseText;

    private void Fail(string message)
    {
        GD.PushError(message);
        GetTree().Quit(exitCode: 1);
    }
}
