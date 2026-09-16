using Allrpcs.Domain.Characters;
using Godot;

namespace Allrpcs.Game;

/// <summary>Orchestrates one explicit exact witness presentation choice.</summary>
public partial class TemporaryCharacterDialogueMisquotationEvidencePresentationFlowController
    : Node
{
    private CharacterDialogueMisquotationEvidencePresentationController presentation = null!;
    private CharacterDialogueMisquotationEvidencePresentationSelectionController selections = null!;
    private CharacterDialogueMisquotationEvidenceViewController view = null!;

    /// <inheritdoc />
    public override void _Ready()
    {
        view = GetNode<CharacterDialogueMisquotationEvidenceViewController>(
            "../CharacterDialogueMisquotationEvidenceViewController");
        selections = GetNode<CharacterDialogueMisquotationEvidencePresentationSelectionController>(
            "../CharacterDialogueMisquotationEvidencePresentationSelectionController");
        presentation = GetNode<CharacterDialogueMisquotationEvidencePresentationController>(
            "../CharacterDialogueMisquotationEvidencePresentationController");
    }

    /// <summary>Chooses and projects one exact current member or explicit abstention.</summary>
    public bool TryChoose(
        CharacterDialogueMisquotationEvidenceViewSnapshot expectedView,
        CharacterDialogueMisquotationEvidence? selectedEvidence)
    {
        ArgumentNullException.ThrowIfNull(expectedView);

        if (!ReferenceEquals(view.View, expectedView)
            || !selections.TryChoose(expectedView, selectedEvidence)
            || selections.Selection is not { } selection)
        {
            return false;
        }

        return presentation.TryPresent(expectedView, selection);
    }
}
