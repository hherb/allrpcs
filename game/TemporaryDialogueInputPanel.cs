using Allrpcs.Domain.Dialogue;
using Godot;

namespace Allrpcs.Game;

/// <summary>Connects the existing submission owner to contained dialogue navigation and semantics.</summary>
public partial class TemporaryDialogueInputPanel : Control
{
    /// <summary>Displayed after an utterance is accepted.</summary>
    public const string AcceptedStatusText = "Dialogue submission: ACCEPTED — BUSY";
    private const string RejectedStatusText = "Dialogue submission: REJECTED";
    private DialogueInputController controller = null!;
    private LineEdit input = null!;
    private Label status = null!;
    private CheckButton whispering = null!;
    private Button submit = null!;
    private Button close = null!;
    private Control[] cycle = [];
    internal event Action? Closed;

    /// <inheritdoc />
    public override void _Ready()
    {
        controller = GetNode<DialogueInputController>("../../DialogueInputController");
        input = GetNode<LineEdit>("Input");
        status = GetNode<Label>("Status");
        controller.StatusChanged += PresentStatus;
        whispering = GetNode<CheckButton>("Whispering");
        submit = GetNode<Button>("Submit");
        close = GetNode<Button>("Close");
        cycle = [input, whispering, submit, close];
        input.KeepEditingOnTextSubmit = true;
        input.TextSubmitted += OnTextSubmitted;
        submit.Pressed += OnSubmitPressed;
        close.Pressed += OnClosePressed;
        input.AccessibilityName = "Dialogue as Seeker";
        input.AccessibilityDescription = "Enter in-world dialogue. Enter submits using the selected delivery mode.";
        input.AccessibilityLabeledByNodes = [new NodePath("../Prompt")];
        input.AccessibilityDescribedByNodes = [new NodePath("../Prompt"), new NodePath("../Status")];
        whispering.AccessibilityName = "Whisper";
        whispering.AccessibilityDescription = "Checked sends whispering; unchecked sends ordinary speech.";
        submit.AccessibilityName = "Submit dialogue";
        submit.AccessibilityDescription = "Submit the current text as Seeker using the selected delivery mode.";
        close.AccessibilityName = "Close dialogue";
        close.AccessibilityDescription = "Return to the Dialogue settings entry.";
        status.AccessibilityName = "Dialogue submission status";
        status.AccessibilityLive = DisplayServer.AccessibilityLiveMode.Polite;
        GetNode<Label>("Response").AccessibilityName = "Healer final response";
        for (int i = 0; i < cycle.Length; i++)
        {
            cycle[i].FocusNext = cycle[i].GetPathTo(cycle[(i + 1) % cycle.Length]);
            cycle[i].FocusPrevious = cycle[i].GetPathTo(cycle[(i + cycle.Length - 1) % cycle.Length]);
        }
    }

    /// <summary>Adds optional speech controls while retaining the exact typed submit/delivery owner.</summary>
    internal void RegisterSpeechControls(Control[] controls)
    {
        cycle = [input, whispering, .. controls, submit, close];
        for (int index = 0; index < cycle.Length; index++)
        {
            cycle[index].FocusNext = cycle[index].GetPathTo(cycle[(index + 1) % cycle.Length]);
            cycle[index].FocusPrevious = cycle[index].GetPathTo(cycle[(index + cycle.Length - 1) % cycle.Length]);
        }
    }

    internal void FocusInput() => input.GrabFocus();

    /// <inheritdoc />
    public override void _Input(InputEvent @event)
    {
        if (!IsVisibleInTree()) { return; }
        // Keep native Submit/Close/checkbox activation even when Left is mapped to UI cancel.
        if (@event is InputEventMouseButton { ButtonIndex: MouseButton.Left } mouse
            && cycle.Any(control => control.GetGlobalRect().HasPoint(mouse.Position))) { return; }
        Control? focus = GetViewport().GuiGetFocusOwner();
        if (focus is null || !IsAncestorOf(focus)) { return; }
        if (ControlInputMatcher.Pressed(@event, "ui_cancel"))
        { OnClosePressed(); GetViewport().SetInputAsHandled(); return; }
        bool next = ControlInputMatcher.Pressed(@event, "ui_focus_next");
        bool previous = ControlInputMatcher.Pressed(@event, "ui_focus_prev");
        if (!next && !previous) { return; }
        int index = Array.IndexOf(cycle, focus);
        cycle[(index + (previous ? cycle.Length - 1 : 1)) % cycle.Length].GrabFocus();
        GetViewport().SetInputAsHandled();
    }

    private void PresentStatus(string value) { if (status.Text != value) { status.Text = value; } }

    private void OnClosePressed() => Closed?.Invoke();
    private void OnTextSubmitted(string text) => SubmitInput(text);
    private void OnSubmitPressed() => SubmitInput(input.Text);
    private void SubmitInput(string text)
    {
        DialogueDeliveryMode mode = whispering.ButtonPressed
            ? DialogueDeliveryMode.Whispering : DialogueDeliveryMode.OrdinarySpeech;
        bool accepted = controller.TrySubmit(text, mode);
        string result = accepted || controller.Status.Contains("BUSY", StringComparison.Ordinal) ? controller.Status : RejectedStatusText;
        // Only a changed submission result updates the polite region; no frame polling or duplicate writes.
        if (status.Text != result) { status.Text = result; }
        if (accepted) { input.Clear(); }
    }

    /// <inheritdoc />
    public override void _ExitTree()
    {
        controller.StatusChanged -= PresentStatus;
        input.TextSubmitted -= OnTextSubmitted;
        submit.Pressed -= OnSubmitPressed;
        close.Pressed -= OnClosePressed;
    }
}
