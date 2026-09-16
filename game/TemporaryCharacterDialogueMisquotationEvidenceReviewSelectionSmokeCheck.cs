using Allrpcs.Domain.Characters;
using Godot;

namespace Allrpcs.Game;

/// <summary>Checks explicit exact selection from one truth-neutral reviewed witness view.</summary>
public partial class TemporaryCharacterDialogueMisquotationEvidenceReviewSelectionSmokeCheck : Node
{
    /// <inheritdoc />
    public override void _Ready()
    {
        TemporaryCharacterDialogueMisquotationEvidenceSource source =
            GetNode<TemporaryCharacterDialogueMisquotationEvidenceSource>(
                "../TemporaryCharacterDialogueMisquotationEvidenceSource");
        CharacterDialogueMisquotationEvidenceViewController view =
            GetNode<CharacterDialogueMisquotationEvidenceViewController>(
                "../CharacterDialogueMisquotationEvidenceViewController");
        CharacterDialogueMisquotationEvidencePresentationSelectionController selections =
            GetNode<CharacterDialogueMisquotationEvidencePresentationSelectionController>(
                "../CharacterDialogueMisquotationEvidencePresentationSelectionController");
        CharacterDialogueMisquotationEvidencePresentationController presentations =
            GetNode<CharacterDialogueMisquotationEvidencePresentationController>(
                "../CharacterDialogueMisquotationEvidencePresentationController");
        TemporaryCharacterDialogueMisquotationEvidenceReviewPanel reviewPanel =
            GetNode<TemporaryCharacterDialogueMisquotationEvidenceReviewPanel>(
                "../TemporaryCharacterDialogueMisquotationEvidenceReviewPanel");
        TemporaryCharacterDialogueMisquotationEvidencePresentationPanel presentationPanel =
            GetNode<TemporaryCharacterDialogueMisquotationEvidencePresentationPanel>(
                "../TemporaryCharacterDialogueMisquotationEvidencePresentationPanel");
        Button review = reviewPanel.GetNode<Button>("Review");
        ItemList statements = reviewPanel.GetNode<ItemList>("Statements");
        CharacterDialogueMisquotationEvidenceExposureSnapshot initialExposure = new([]);
        CharacterDialogueMisquotationEvidenceExposureSnapshot selectedExposure = new(
            [source.Candidate]);

        statements.EmitSignal(ItemList.SignalName.ItemSelected, 0L);
        bool inert = selections.Selection is null
            && presentations.Presentation is null
            && presentationPanel.Presentation is null
            && !presentationPanel.Visible;
        if (!view.TryInitialize(initialExposure, source.WitnessCharacterId)
            || !view.TryRefresh(initialExposure, selectedExposure)
            || view.View is not { } selectedView)
        {
            Fail("Misquotation-evidence review selection could not arrange an exposed view.");
            return;
        }

        review.EmitSignal(BaseButton.SignalName.Pressed);
        statements.EmitSignal(ItemList.SignalName.ItemSelected, 1L);
        bool invalidInert = selections.Selection is null
            && presentations.Presentation is null
            && presentationPanel.Presentation is null
            && !presentationPanel.Visible;
        statements.EmitSignal(ItemList.SignalName.ItemSelected, 0L);
        CharacterDialogueMisquotationEvidencePresentationSelection? selection =
            selections.Selection;
        CharacterDialogueMisquotationEvidencePresentationSnapshot? presentation =
            presentations.Presentation;
        bool selected = selection is not null
            && presentation is not null
            && ReferenceEquals(selection.WitnessView, selectedView)
            && ReferenceEquals(selection.SelectedEvidence, source.Candidate)
            && ReferenceEquals(presentation.Selection, selection)
            && ReferenceEquals(presentation.SourceEvent, source.SourceEvent)
            && ReferenceEquals(presentation.DeliveredEvent, source.DeliveredEvent)
            && ReferenceEquals(presentationPanel.Presentation, presentation)
            && presentationPanel.Visible;
        statements.EmitSignal(ItemList.SignalName.ItemSelected, 0L);
        selected = selected
            && ReferenceEquals(selections.Selection, selection)
            && ReferenceEquals(presentations.Presentation, presentation)
            && ReferenceEquals(presentationPanel.Presentation, presentation);

        CharacterDialogueMisquotationEvidenceExposureSnapshot laterExposure = new(
            [source.Candidate]);
        bool staleInert = view.TryRefresh(selectedExposure, laterExposure);
        reviewPanel._Process(0);
        presentationPanel._Process(0);
        statements.EmitSignal(ItemList.SignalName.ItemSelected, 0L);
        staleInert = staleInert
            && selections.Selection is null
            && presentations.Presentation is null
            && presentationPanel.Presentation is null
            && !presentationPanel.Visible
            && !statements.Visible
            && statements.ItemCount == 0
            && ReferenceEquals(source.Candidate.Occurrence.Proposal.SourceEvent, source.SourceEvent)
            && ReferenceEquals(source.Candidate.Occurrence.DeliveredEvent, source.DeliveredEvent);

        if (!inert || !invalidInert || !selected || !staleInert)
        {
            Fail("Misquotation-evidence review selection was not exact and one-shot.");
            return;
        }

        GD.Print(
            "Misquotation-evidence review selection smoke passed: absent and out-of-range "
            + "inertness, exact reviewed-entry selection and presentation, repeated one-shot "
            + "preservation, stale hiding, and unchanged source evidence.");
        GetTree().Quit();
    }

    private void Fail(string message)
    {
        GD.PushError(message);
        GetTree().Quit(1);
    }
}
