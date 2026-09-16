using Allrpcs.Domain.Characters;
using Godot;

namespace Allrpcs.Game;

/// <summary>Initializes exact empty misquotation-evidence exposure state without starting work.</summary>
public partial class TemporaryCharacterDialogueMisquotationEvidenceExposureFlowController : Node
{
    /// <inheritdoc />
    public override void _Ready()
    {
        CharacterDialogueMisquotationEvidenceExposureController exposure =
            GetNode<CharacterDialogueMisquotationEvidenceExposureController>(
                "../CharacterDialogueMisquotationEvidenceExposureController");
        if (!exposure.TryInitialize(new CharacterDialogueMisquotationEvidenceExposureSnapshot([])))
        {
            throw new InvalidOperationException(
                "The temporary misquotation-evidence flow requires uninitialized exposure state.");
        }
    }
}
