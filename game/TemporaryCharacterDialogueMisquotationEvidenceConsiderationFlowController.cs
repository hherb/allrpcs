using Allrpcs.Application.Characters;
using Allrpcs.Domain.Characters;
using Godot;

namespace Allrpcs.Game;

/// <summary>Starts one explicit misquotation-evidence decision through exact scene dependencies.</summary>
public partial class TemporaryCharacterDialogueMisquotationEvidenceConsiderationFlowController : Node
{
    private TemporaryCharacterDialogueMisquotationEvidenceModelSource modelSource = null!;
    private CharacterDialogueMisquotationEvidenceTaskController task = null!;

    /// <inheritdoc />
    public override void _Ready()
    {
        modelSource = GetNode<TemporaryCharacterDialogueMisquotationEvidenceModelSource>(
            "../TemporaryCharacterDialogueMisquotationEvidenceModelSource");
        task = GetNode<CharacterDialogueMisquotationEvidenceTaskController>(
            "../CharacterDialogueMisquotationEvidenceTaskController");
    }

    /// <summary>Starts one decision for the exact supplied request only when explicitly called.</summary>
    public bool TryStart(CharacterDialogueMisquotationEvidenceConsiderationRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        if (task.Status != CharacterDialogueMisquotationEvidenceTaskStatus.Idle)
        {
            return false;
        }

        ICharacterDialogueMisquotationEvidenceDecisionModel model = modelSource.TakeModel();
        return task.TryStart(model, request);
    }
}
