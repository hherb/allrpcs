using Allrpcs.Application.Dialogue;
using Allrpcs.Domain.Dialogue;
using Allrpcs.Domain.Identifiers;
using Godot;

namespace Allrpcs.Game;

/// <summary>Proves exact streamed text is presented without executing terminal content.</summary>
public partial class CharacterModelStreamTextPresentationSmokeCheck : Node
{
    private const string CheckEnvironmentVariable =
        "ALLRPCS_CHARACTER_MODEL_STREAM_TEXT_PRESENTATION_CHECK";
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

        CharacterModelStreamController controller =
            GetNode<CharacterModelStreamController>("../CharacterModelStreamController");
        TemporaryCharacterModelStreamTextLabel label =
            GetNode<TemporaryCharacterModelStreamTextLabel>(
                "../DialogueOverlay/Panel/ModelStreamText");
        CharacterTurnActivityController turnController =
            GetNode<CharacterTurnActivityController>("../CharacterTurnActivityController");
        CharacterTurnTranscriptController transcriptController =
            GetNode<CharacterTurnTranscriptController>("../CharacterTurnTranscriptController");
        WorldEventController worldEventController =
            GetNode<WorldEventController>("../WorldEventController");
        CharacterModelTextDelta first = new("Heal");
        CharacterModelTextDelta second = new("er");
        CharacterModelStreamTermination completed = Completed();
        CharacterTurnResponse completedResponse = completed.Response
            ?? throw new InvalidOperationException("Completed stream has no response.");
        ObjectId completedTargetObjectId = completedResponse.ImmediateIntention.TargetObjectId
            ?? throw new InvalidOperationException("Completed response has no target.");
        object? turn = turnController.Activity;
        object? transcript = transcriptController.Transcript;
        object? eventLog = worldEventController.EventLog;
        object? worldTime = worldEventController.WorldTime;

        if (!label.Observe(controller)
            || label.Visible
            || !ReferenceEquals(label.Presentation, CharacterModelStreamTextPresentation.Empty)
            || label.Text.Length != 0
            || !controller.TryAppend(first)
            || !label.Observe(controller)
            || !label.Visible
            || label.Text != "Heal"
            || !controller.TryAppend(second)
            || !label.Observe(controller)
            || label.Text != "Healer")
        {
            Fail("Model stream text label did not present exact growing text prefixes.");
            return;
        }

        if (!controller.TryAppend(completed)
            || !label.Observe(controller)
            || label.Text != "Healer"
            || label.Text.Contains(completedResponse.Speech!.Text, StringComparison.Ordinal)
            || label.Text.Contains(
                completedTargetObjectId.Value,
                StringComparison.Ordinal)
            || !ReferenceEquals(turnController.Activity, turn)
            || !ReferenceEquals(transcriptController.Transcript, transcript)
            || !ReferenceEquals(worldEventController.EventLog, eventLog)
            || !ReferenceEquals(worldEventController.WorldTime, worldTime))
        {
            Fail("Model stream text label exposed or executed terminal response content.");
            return;
        }

        GD.Print(
            "Character-model stream text presentation smoke passed: hidden empty state, exact "
            + "growing text prefixes, terminal-content exclusion, and unchanged authoritative "
            + "scene state.");
        GetTree().Quit();
    }

    private static CharacterModelStreamTermination Completed()
    {
        CharacterId seekerId = new("seeker");
        CharacterId healerId = new("healer");
        DialogueUtterance utterance = new(seekerId, "Please inspect the herb.");
        CharacterTurnRequest request = new(
            healerId,
            new DialogueEvent(
                utterance,
                new DialogueAudienceSnapshot(seekerId, [healerId])),
            new DialogueAct(utterance, DialogueActType.Request));
        CharacterTurnResponse response = new(
            request,
            CharacterTurnDisposition.Accept,
            new DialogueUtterance(healerId, "private completed response"),
            new CharacterIntention(
                CharacterIntentionType.ExamineObject,
                new ObjectId("private-target")),
            [],
            []);
        return new CharacterModelStreamTermination(
            CharacterModelStreamTerminationStatus.Completed,
            response,
            null);
    }

    private void Fail(string message)
    {
        GD.PushError(message);
        GetTree().Quit(exitCode: 1);
    }
}
