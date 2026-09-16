using Allrpcs.Domain.Characters;
using Godot;

namespace Allrpcs.Game;

/// <summary>Renders one exact current witness view only after explicit neutral review input.</summary>
public partial class TemporaryCharacterDialogueMisquotationEvidenceReviewPanel : CanvasLayer
{
    private Button close = null!;
    private TemporaryCharacterDialogueMisquotationEvidencePresentationFlowController flow = null!;
    private TemporaryCharacterDialogueMisquotationEvidencePresentationPanel presentationPanel =
        null!;
    private CharacterDialogueMisquotationEvidencePresentationController presentations = null!;
    private CharacterDialogueMisquotationEvidenceViewSnapshot? reviewedView;
    private ItemList statements = null!;
    private CharacterDialogueMisquotationEvidenceViewController view = null!;

    /// <inheritdoc />
    public override void _Ready()
    {
        flow = GetNode<TemporaryCharacterDialogueMisquotationEvidencePresentationFlowController>(
            "../TemporaryCharacterDialogueMisquotationEvidencePresentationFlowController");
        presentationPanel =
            GetNode<TemporaryCharacterDialogueMisquotationEvidencePresentationPanel>(
                "../TemporaryCharacterDialogueMisquotationEvidencePresentationPanel");
        presentations = GetNode<CharacterDialogueMisquotationEvidencePresentationController>(
            "../CharacterDialogueMisquotationEvidencePresentationController");
        view = GetNode<CharacterDialogueMisquotationEvidenceViewController>(
            "../CharacterDialogueMisquotationEvidenceViewController");
        close = GetNode<Button>("Close");
        statements = GetNode<ItemList>("Statements");
        GetNode<Button>("Review").Pressed += OnReviewPressed;
        close.Pressed += OnClosePressed;
        statements.ItemSelected += OnStatementSelected;
        ClearReview();
    }

    /// <inheritdoc />
    public override void _Process(double delta)
    {
        _ = delta;
        if (reviewedView is not null && !ReferenceEquals(view.View, reviewedView))
        {
            ClearReview();
        }
    }

    private void OnReviewPressed()
    {
        CharacterDialogueMisquotationEvidenceViewSnapshot? currentView = view.View;
        if (currentView is null || currentView.Entries.Count == 0)
        {
            ClearReview();
            return;
        }

        statements.Clear();
        foreach (CharacterDialogueMisquotationEvidence evidence in currentView.Entries)
        {
            statements.AddItem(
                $"Source statement: {evidence.Occurrence.Proposal.SourceEvent.Utterance.Text}\n"
                + "Delivered statement: "
                + evidence.Occurrence.DeliveredEvent.Utterance.Text);
        }

        reviewedView = currentView;
        statements.Show();
        close.Show();
    }

    private void OnStatementSelected(long index)
    {
        CharacterDialogueMisquotationEvidenceViewSnapshot? currentReview = reviewedView;
        if (currentReview is null
            || !ReferenceEquals(view.View, currentReview)
            || index < 0
            || index >= currentReview.Entries.Count
            || !flow.TryChoose(currentReview, currentReview.Entries[(int)index])
            || presentations.Presentation is not { } presentation
            || !presentationPanel.TryPresent(presentation))
        {
            return;
        }

        statements.Hide();
        close.Hide();
    }

    private void OnClosePressed()
    {
        ClearReview();
    }

    private void ClearReview()
    {
        reviewedView = null;
        statements.Clear();
        statements.Hide();
        close.Hide();
    }
}
