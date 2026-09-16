using Allrpcs.Domain.Characters;
using Godot;

namespace Allrpcs.Game;

/// <summary>Owns one explicit truth-neutral selection for each exact current witness view.</summary>
public partial class CharacterDialogueMisquotationEvidencePresentationSelectionController : Node
{
    private CharacterDialogueMisquotationEvidencePresentationSelection? selection;
    private CharacterDialogueMisquotationEvidenceViewController? view;

    /// <summary>Gets the accepted selection only while its exact witness view remains current.</summary>
    public CharacterDialogueMisquotationEvidencePresentationSelection? Selection =>
        selection is not null
        && view?.View is { } currentView
        && CharacterDialogueMisquotationEvidencePresentationSelectionPolicy.CanAccept(
            currentView,
            selection)
            ? selection
            : null;

    /// <inheritdoc />
    public override void _Ready()
    {
        view = GetNode<CharacterDialogueMisquotationEvidenceViewController>(
            "../CharacterDialogueMisquotationEvidenceViewController");
    }

    /// <summary>Chooses one exact member or abstention once for the exact current witness view.</summary>
    public bool TryChoose(
        CharacterDialogueMisquotationEvidenceViewSnapshot expectedView,
        CharacterDialogueMisquotationEvidence? selectedEvidence)
    {
        ArgumentNullException.ThrowIfNull(expectedView);

        if (!ReferenceEquals(view?.View, expectedView)
            || ReferenceEquals(selection?.WitnessView, expectedView)
            || selectedEvidence is not null
                && !expectedView.Entries.Any(evidence => ReferenceEquals(evidence, selectedEvidence)))
        {
            return false;
        }

        CharacterDialogueMisquotationEvidencePresentationSelection candidate = new(
            expectedView,
            selectedEvidence);
        if (!CharacterDialogueMisquotationEvidencePresentationSelectionPolicy.CanAccept(
            expectedView,
            candidate))
        {
            return false;
        }

        selection = candidate;
        return true;
    }
}
