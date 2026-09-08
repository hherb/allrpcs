using Allrpcs.Domain.Dialogue;
using Godot;

namespace Allrpcs.Game;

/// <summary>Runs focused scene character-turn transcript checks when explicitly enabled.</summary>
public partial class CharacterTurnTranscriptControllerSmokeCheck : Node
{
    private const string CheckEnvironmentVariable = "ALLRPCS_CHARACTER_TURN_TRANSCRIPT_CHECK";
    private const string EnabledEnvironmentValue = "1";
    private const string Text = "  Please examine the flowers first.  ";

    private CharacterTurnTranscriptController controller = null!;
    private DialogueInputController inputController = null!;
    private LineEdit input = null!;
    private int stage;
    private object? initialWorldEventLog;
    private object? initialWorldTime;
    private object? initialWorldObjects;
    private object? initialPerception;
    private CharacterTurnActivity? initialTurn;

    /// <inheritdoc />
    public override void _Ready()
    {
        bool isEnabled = OS.GetEnvironment(CheckEnvironmentVariable) == EnabledEnvironmentValue;
        SetProcess(isEnabled);
        if (!isEnabled)
        {
            return;
        }

        controller = GetNode<CharacterTurnTranscriptController>(
            "../CharacterTurnTranscriptController");
        inputController = GetNode<DialogueInputController>("../DialogueInputController");
        input = GetNode<LineEdit>("../DialogueOverlay/Panel/Input");
    }

    /// <inheritdoc />
    public override void _Process(double delta)
    {
        _ = delta;
        if (stage == 0)
        {
            CaptureInitialState();
            if (controller.Transcript is null || controller.Transcript.Entries.Count != 0)
            {
                Fail("Scene character-turn transcript did not initialize empty.");
                return;
            }

            Submit();
            stage = 1;
            return;
        }

        if (stage == 1)
        {
            if (!ProvesEntryAt(0))
            {
                return;
            }

            Submit();
            stage = 2;
            return;
        }

        SetProcess(false);
        CharacterTurnTranscriptSnapshot? transcript = controller.Transcript;
        if (transcript is null
            || transcript.Entries.Count != 2
            || !ProvesEntryAt(1)
            || ReferenceEquals(transcript.Entries[0], transcript.Entries[1])
            || ReferenceEquals(
                transcript.Entries[0].SpokenStatement,
                transcript.Entries[1].SpokenStatement)
            || !ProvesNullAndUninitializedPreservation()
            || !UnrelatedStateIsPreserved())
        {
            return;
        }

        GD.Print(
            "Character-turn transcript controller smoke passed: the empty scene transcript "
            + "appended exact player speech in stable occurrence order, retained repeated equal "
            + "utterances as distinct entries, rejected invalid calls, and preserved unrelated state.");
        GetTree().Quit();
    }

    private void Submit()
    {
        input.Text = Text;
        input.EmitSignal(LineEdit.SignalName.TextSubmitted, Text);
    }

    private bool ProvesEntryAt(int index)
    {
        CharacterTurnTranscriptSnapshot? transcript = controller.Transcript;
        DialogueUtterance? submitted = inputController.LastSubmittedUtterance;
        if (transcript is null
            || transcript.Entries.Count != index + 1
            || submitted is null)
        {
            Fail("Scene character-turn transcript did not append one submitted utterance.");
            return false;
        }

        CharacterTurnTranscriptEntry entry = transcript.Entries[index];
        if (!ReferenceEquals(entry.Kind, CharacterTurnTranscriptEntryKind.SpokenStatement)
            || !ReferenceEquals(entry.CharacterId, submitted.SpeakerCharacterId)
            || !ReferenceEquals(entry.SpokenStatement, submitted)
            || entry.PhysicalAction is not null
            || submitted.Text != Text)
        {
            Fail("Scene character-turn transcript did not retain exact structured player speech.");
            return false;
        }

        return true;
    }

    private bool ProvesNullAndUninitializedPreservation()
    {
        CharacterTurnTranscriptController target = new();
        AddChild(target);
        DialogueUtterance utterance = new(inputController.SpeakerCharacterId!, "Hello.");
        if (target.TryAppendSpokenStatement(utterance) || target.Transcript is not null)
        {
            Fail("Uninitialized transcript controller accepted speech.");
            return false;
        }

        CharacterTurnTranscriptSnapshot empty = new([]);
        if (!target.TryInitialize(empty) || !ReferenceEquals(target.Transcript, empty))
        {
            Fail("Transcript controller failed exact initialization.");
            return false;
        }

        try
        {
            target.TryAppendSpokenStatement(null!);
        }
        catch (ArgumentNullException)
        {
            return ReferenceEquals(target.Transcript, empty);
        }

        Fail("Transcript controller accepted null speech.");
        return false;
    }

    private void CaptureInitialState()
    {
        WorldEventController worldEvents = GetNode<WorldEventController>("../WorldEventController");
        initialWorldEventLog = worldEvents.EventLog;
        initialWorldTime = worldEvents.WorldTime;
        initialWorldObjects = GetNode<WorldObjectController>("../WorldObjectController").WorldObjects;
        initialPerception = GetNode<FocusedSearchActivityController>(
            "../Seeker/FocusedSearchActivityController").Perception;
        initialTurn = GetNode<CharacterTurnActivityController>(
            "../CharacterTurnActivityController").Activity;
    }

    private bool UnrelatedStateIsPreserved()
    {
        WorldEventController worldEvents = GetNode<WorldEventController>("../WorldEventController");
        bool preserved = ReferenceEquals(worldEvents.EventLog, initialWorldEventLog)
            && ReferenceEquals(worldEvents.WorldTime, initialWorldTime)
            && ReferenceEquals(
                GetNode<WorldObjectController>("../WorldObjectController").WorldObjects,
                initialWorldObjects)
            && ReferenceEquals(
                GetNode<FocusedSearchActivityController>(
                    "../Seeker/FocusedSearchActivityController").Perception,
                initialPerception)
            && ReferenceEquals(
                GetNode<CharacterTurnActivityController>("../CharacterTurnActivityController").Activity,
                initialTurn);
        if (!preserved)
        {
            Fail("Player transcript recording changed unrelated scene state.");
        }

        return preserved;
    }

    private void Fail(string message)
    {
        SetProcess(false);
        GD.PushError(message);
        GetTree().Quit(exitCode: 1);
    }
}
