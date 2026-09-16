using Allrpcs.Domain.Characters;
using Godot;

namespace Allrpcs.Game;

/// <summary>Renders one exact current witness presentation without judging either statement.</summary>
public partial class TemporaryCharacterDialogueMisquotationEvidencePresentationPanel
    : CanvasLayer
{
    private TemporaryCharacterDialogueMisquotationEvidencePresentationFlowController flow = null!;
    private CharacterDialogueMisquotationEvidencePresentationController presentations = null!;
    private Label sourceText = null!;
    private Label deliveredText = null!;
    private CharacterDialogueMisquotationEvidencePresentationSnapshot? presentation;

    /// <summary>Gets the exact rendered snapshot only while its presentation owner keeps it current.</summary>
    public CharacterDialogueMisquotationEvidencePresentationSnapshot? Presentation =>
        presentation is not null
        && ReferenceEquals(presentations.Presentation, presentation)
            ? presentation
            : null;

    /// <inheritdoc />
    public override void _Ready()
    {
        flow = GetNode<TemporaryCharacterDialogueMisquotationEvidencePresentationFlowController>(
            "../TemporaryCharacterDialogueMisquotationEvidencePresentationFlowController");
        presentations = GetNode<CharacterDialogueMisquotationEvidencePresentationController>(
            "../CharacterDialogueMisquotationEvidencePresentationController");
        sourceText = GetNode<Label>("Panel/SourceText");
        deliveredText = GetNode<Label>("Panel/DeliveredText");
        GetNode<Button>("Panel/Dismiss").Pressed += OnDismissPressed;
        ClearPresentation();
    }

    /// <inheritdoc />
    public override void _Process(double delta)
    {
        _ = delta;
        if (presentation is not null && Presentation is null)
        {
            ClearPresentation();
        }
    }

    /// <summary>Renders only the presentation owner's exact current non-null snapshot.</summary>
    public bool TryPresent(
        CharacterDialogueMisquotationEvidencePresentationSnapshot expectedPresentation)
    {
        ArgumentNullException.ThrowIfNull(expectedPresentation);

        if (!ReferenceEquals(presentations.Presentation, expectedPresentation)
            || ReferenceEquals(Presentation, expectedPresentation))
        {
            return false;
        }

        presentation = expectedPresentation;
        sourceText.Text = expectedPresentation.SourceEvent.Utterance.Text;
        deliveredText.Text = expectedPresentation.DeliveredEvent.Utterance.Text;
        Show();
        return true;
    }

    private void OnDismissPressed()
    {
        CharacterDialogueMisquotationEvidencePresentationSnapshot? currentPresentation =
            Presentation;
        if (currentPresentation is null
            || !flow.TryAcknowledge(
                currentPresentation.Selection.WitnessView,
                currentPresentation.Selection,
                currentPresentation))
        {
            return;
        }

        ClearPresentation();
    }

    private void ClearPresentation()
    {
        presentation = null;
        sourceText.Text = string.Empty;
        deliveredText.Text = string.Empty;
        Hide();
    }
}
