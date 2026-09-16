using Allrpcs.Domain.Characters;
using Godot;

namespace Allrpcs.Game;

/// <summary>Owns one explicit truth-neutral presentation for each exact selected witness view.</summary>
public partial class CharacterDialogueMisquotationEvidencePresentationController : Node
{
    private CharacterDialogueMisquotationEvidencePresentationSnapshot? presentation;
    private CharacterDialogueMisquotationEvidencePresentationSelection? lastProjectedSelection;
    private CharacterDialogueMisquotationEvidencePresentationSelectionController? selections;
    private CharacterDialogueMisquotationEvidenceViewController? view;

    /// <summary>Gets the presentation only while its exact selection and view remain current.</summary>
    public CharacterDialogueMisquotationEvidencePresentationSnapshot? Presentation =>
        presentation is not null
        && view?.View is { } currentView
        && selections?.Selection is { } currentSelection
        && ReferenceEquals(presentation.Selection, currentSelection)
        && CharacterDialogueMisquotationEvidencePresentationSelectionPolicy.CanAccept(
            currentView,
            currentSelection)
            ? presentation
            : null;

    /// <inheritdoc />
    public override void _Ready()
    {
        view = GetNode<CharacterDialogueMisquotationEvidenceViewController>(
            "../CharacterDialogueMisquotationEvidenceViewController");
        selections = GetNode<CharacterDialogueMisquotationEvidencePresentationSelectionController>(
            "../CharacterDialogueMisquotationEvidencePresentationSelectionController");
    }

    /// <summary>Projects one exact current selection at most once without presenting UI.</summary>
    public bool TryPresent(
        CharacterDialogueMisquotationEvidenceViewSnapshot expectedView,
        CharacterDialogueMisquotationEvidencePresentationSelection expectedSelection)
    {
        ArgumentNullException.ThrowIfNull(expectedView);
        ArgumentNullException.ThrowIfNull(expectedSelection);

        if (!ReferenceEquals(view?.View, expectedView)
            || !ReferenceEquals(selections?.Selection, expectedSelection)
            || ReferenceEquals(lastProjectedSelection, expectedSelection))
        {
            return false;
        }

        CharacterDialogueMisquotationEvidencePresentationSnapshot? projected =
            CharacterDialogueMisquotationEvidencePresentationProjection.Project(
                expectedView,
                expectedSelection);
        lastProjectedSelection = expectedSelection;
        presentation = projected;
        return true;
    }
}
