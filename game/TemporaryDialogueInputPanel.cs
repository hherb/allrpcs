using Godot;

namespace Allrpcs.Game;

/// <summary>Connects temporary dialogue controls to the focused player-input controller.</summary>
public partial class TemporaryDialogueInputPanel : Control
{
    /// <summary>Displayed after an utterance is accepted.</summary>
    public const string AcceptedStatusText = "Dialogue submission: ACCEPTED";

    private const string RejectedStatusText = "Dialogue submission: REJECTED";

    private DialogueInputController controller = null!;
    private LineEdit input = null!;
    private Label status = null!;

    /// <inheritdoc />
    public override void _Ready()
    {
        controller = GetNode<DialogueInputController>("../../DialogueInputController");
        input = GetNode<LineEdit>("Input");
        status = GetNode<Label>("Status");

        input.TextSubmitted += OnTextSubmitted;
        GetNode<Button>("Submit").Pressed += OnSubmitPressed;
    }

    private void OnTextSubmitted(string text)
    {
        SubmitInput(text);
    }

    private void OnSubmitPressed()
    {
        SubmitInput(input.Text);
    }

    private void SubmitInput(string text)
    {
        bool accepted = controller.TrySubmit(text);
        status.Text = accepted ? AcceptedStatusText : RejectedStatusText;
        if (accepted)
        {
            input.Clear();
        }
    }
}
