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
    private CheckButton whispering = null!;
    private int stage;
    private object? initialWorldEventLog;
    private object? initialWorldTime;
    private object? initialWorldObjects;
    private object? initialPerception;

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
        whispering = GetNode<CheckButton>("../DialogueOverlay/Panel/Whispering");
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

            if (!inputController.CanAccept!()) { return; }
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
            || ReferenceEquals(transcript.Entries[0].Delivery, transcript.Entries[1].Delivery)
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
            + "appended exact whisper deliveries in stable occurrence order, retained repeated "
            + "equal deliveries as distinct entries, rejected invalid calls, and preserved "
            + "unrelated state.");
        GetTree().Quit();
    }

    private void Submit()
    {
        whispering.ButtonPressed = true;
        input.Text = Text;
        GetNode<Healer>("../Healer").Visible = true; // Explicit present listener in this disposable fixture.
        input.EmitSignal(LineEdit.SignalName.TextSubmitted, Text);
    }

    private bool ProvesEntryAt(int index)
    {
        CharacterTurnTranscriptSnapshot? transcript = controller.Transcript;
        DialogueDelivery? submittedDelivery = inputController.LastSubmittedDelivery;
        DialogueUtterance? submitted = inputController.LastSubmittedUtterance;
        if (transcript is null
            || transcript.Entries.Count != index + 1
            || submittedDelivery is null
            || submitted is null)
        {
            Fail("Scene character-turn transcript did not append one submitted utterance.");
            return false;
        }

        CharacterTurnTranscriptEntry entry = transcript.Entries[index];
        if (!ReferenceEquals(entry.Kind, CharacterTurnTranscriptEntryKind.SpokenStatement)
            || !ReferenceEquals(entry.CharacterId, submitted.SpeakerCharacterId)
            || !ReferenceEquals(entry.Delivery, submittedDelivery)
            || !ReferenceEquals(entry.SpokenStatement, submitted)
            || !ReferenceEquals(entry.Delivery.Mode, DialogueDeliveryMode.Whispering)
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
        DialogueDelivery delivery = new(utterance, DialogueDeliveryMode.OrdinarySpeech);
        if (target.TryAppendSpokenStatement(delivery) || target.Transcript is not null)
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
                initialPerception);
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
