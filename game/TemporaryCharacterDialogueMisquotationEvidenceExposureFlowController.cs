using Allrpcs.Domain.Characters;
using Godot;

namespace Allrpcs.Game;

/// <summary>Initializes exact empty misquotation-evidence exposure state without starting work.</summary>
public partial class TemporaryCharacterDialogueMisquotationEvidenceExposureFlowController : Node
{
    /// <inheritdoc />
    public override void _Ready()
    {
        TemporaryCharacterDialogueMisquotationEvidenceSource source =
            GetNode<TemporaryCharacterDialogueMisquotationEvidenceSource>(
                "../TemporaryCharacterDialogueMisquotationEvidenceSource");
        CharacterDialogueMisquotationEvidenceExposureController exposure =
            GetNode<CharacterDialogueMisquotationEvidenceExposureController>(
                "../CharacterDialogueMisquotationEvidenceExposureController");
        CharacterDialogueMisquotationEvidenceViewController view =
            GetNode<CharacterDialogueMisquotationEvidenceViewController>(
                "../CharacterDialogueMisquotationEvidenceViewController");
        CharacterDialogueMisquotationEvidenceExposureSnapshot initial = new([]);
        if (!exposure.TryInitialize(initial)
            || !view.TryInitialize(initial, source.WitnessCharacterId))
        {
            throw new InvalidOperationException(
                "The temporary misquotation-evidence flow requires uninitialized exposure and view state.");
        }
    }
}
