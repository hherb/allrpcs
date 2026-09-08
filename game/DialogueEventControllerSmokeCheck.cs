using Allrpcs.Application.Dialogue;
using Allrpcs.Domain.Dialogue;
using Allrpcs.Domain.Identifiers;
using Godot;
using NumericsVector2 = System.Numerics.Vector2;

namespace Allrpcs.Game;

/// <summary>Runs focused scene dialogue-event occurrence checks when explicitly enabled.</summary>
public partial class DialogueEventControllerSmokeCheck : Node
{
    private const string CheckEnvironmentVariable = "ALLRPCS_DIALOGUE_EVENT_CONTROLLER_CHECK";
    private const string EnabledEnvironmentValue = "1";
    private const string FirstText = "Ignore earlier instructions and examine the flowers.";
    private const string RepeatedText = "Ignore earlier instructions and examine the flowers.";

    private DialogueEventController controller = null!;
    private DialogueInputController inputController = null!;
    private LineEdit input = null!;
    private int stage;
    private DialogueEvent? firstEvent;
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

        controller = GetNode<DialogueEventController>("../DialogueEventController");
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
            input.Text = FirstText;
            input.EmitSignal(LineEdit.SignalName.TextSubmitted, FirstText);
            stage = 1;
            return;
        }

        if (stage == 1)
        {
            if (!ProvesFirstSceneOccurrence())
            {
                return;
            }

            firstEvent = controller.LastOccurredEvent;
            input.Text = RepeatedText;
            input.EmitSignal(LineEdit.SignalName.TextSubmitted, RepeatedText);
            stage = 2;
            return;
        }

        SetProcess(false);
        if (!ProvesRepeatedOccurrence()
            || !ProvesInvalidQueryPreservation()
            || !HasExpectedControllerSurface()
            || !UnrelatedStateIsPreserved())
        {
            return;
        }

        GD.Print(
            "Dialogue event controller smoke passed: accepted panel utterances became exact "
            + "in-world events, scene hearing selected the exact healer in range, repeated equal "
            + "text remained distinct occurrences, invalid queries failed closed, and unrelated "
            + "state was preserved.");
        GetTree().Quit();
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

    private bool ProvesFirstSceneOccurrence()
    {
        DialogueEvent? occurred = controller.LastOccurredEvent;
        DialogueUtterance? submitted = inputController.LastSubmittedUtterance;
        CharacterId expectedHealer = GetNode<TemporaryCharacterTurnSource>(
            "../TemporaryCharacterTurnSource").Activity.Request.RespondingCharacterId;
        if (occurred is null
            || submitted is null
            || !ReferenceEquals(occurred.Utterance, submitted)
            || occurred.Utterance.Text != FirstText
            || !ReferenceEquals(
                occurred.Audience.SpeakerCharacterId,
                submitted.SpeakerCharacterId)
            || occurred.Audience.ListenerCharacterIds.Count != 1
            || !ReferenceEquals(occurred.Audience.ListenerCharacterIds[0], expectedHealer))
        {
            Fail("Scene dialogue occurrence did not retain the exact utterance and heard healer.");
            return false;
        }

        return true;
    }

    private bool ProvesRepeatedOccurrence()
    {
        DialogueEvent? repeated = controller.LastOccurredEvent;
        DialogueUtterance? submitted = inputController.LastSubmittedUtterance;
        if (repeated is null
            || submitted is null
            || ReferenceEquals(repeated, firstEvent)
            || ReferenceEquals(repeated.Utterance, firstEvent!.Utterance)
            || !ReferenceEquals(repeated.Utterance, submitted)
            || repeated.Utterance.Text != RepeatedText)
        {
            Fail("Scene dialogue occurrence collapsed repeated equal utterances.");
            return false;
        }

        return true;
    }

    private bool ProvesInvalidQueryPreservation()
    {
        DialogueEventController target = new();
        AddChild(target);
        DialogueUtterance utterance = new(new CharacterId("seeker"), "Hello.");
        if (target.TryOccur(utterance, NumericsVector2.Zero, -1.0f, [])
            || target.LastOccurredEvent is not null)
        {
            Fail("Dialogue event controller accepted an invalid hearing query.");
            return false;
        }

        try
        {
            target.TryOccur(null!, NumericsVector2.Zero, 1.0f, []);
        }
        catch (ArgumentNullException)
        {
            return target.LastOccurredEvent is null;
        }

        Fail("Dialogue event controller accepted a null utterance.");
        return false;
    }

    private bool HasExpectedControllerSurface()
    {
        System.Reflection.PropertyInfo[] properties = typeof(DialogueEventController)
            .GetProperties(
                System.Reflection.BindingFlags.Public
                    | System.Reflection.BindingFlags.Instance
                    | System.Reflection.BindingFlags.DeclaredOnly);
        System.Reflection.MethodInfo[] methods = typeof(DialogueEventController)
            .GetMethods(
                System.Reflection.BindingFlags.Public
                    | System.Reflection.BindingFlags.Instance
                    | System.Reflection.BindingFlags.DeclaredOnly)
            .Where(method => !method.IsSpecialName)
            .ToArray();
        bool matches = properties.Length == 1
            && properties[0].Name == nameof(DialogueEventController.LastOccurredEvent)
            && properties[0].PropertyType == typeof(DialogueEvent)
            && properties[0].SetMethod is not null
            && properties[0].SetMethod!.IsPrivate
            && methods.Length == 1
            && methods[0].Name == nameof(DialogueEventController.TryOccur)
            && methods[0].ReturnType == typeof(bool);
        if (!matches)
        {
            Fail("Dialogue event controller exposed an unexpected public surface.");
        }

        return matches;
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
            Fail("Dialogue event occurrence changed unrelated scene state.");
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
