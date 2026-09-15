using Allrpcs.Application.Characters;
using Allrpcs.Domain.Characters;
using Godot;

namespace Allrpcs.Game;

/// <summary>Starts one explicit misquotation-evidence decision through exact scene dependencies.</summary>
public partial class TemporaryCharacterDialogueMisquotationEvidenceConsiderationFlowController : Node
{
    private TemporaryCharacterDialogueMisquotationEvidenceSource source = null!;
    private TemporaryCharacterDialogueMisquotationEvidenceModelSource modelSource = null!;
    private CharacterDialogueMisquotationEvidenceTaskController task = null!;

    /// <inheritdoc />
    public override void _Ready()
    {
        source = GetNode<TemporaryCharacterDialogueMisquotationEvidenceSource>(
            "../TemporaryCharacterDialogueMisquotationEvidenceSource");
        modelSource = GetNode<TemporaryCharacterDialogueMisquotationEvidenceModelSource>(
            "../TemporaryCharacterDialogueMisquotationEvidenceModelSource");
        task = GetNode<CharacterDialogueMisquotationEvidenceTaskController>(
            "../CharacterDialogueMisquotationEvidenceTaskController");
    }

    /// <summary>Starts one decision for the exact authored source only when explicitly called.</summary>
    public bool TryStart()
    {
        if (task.Status != CharacterDialogueMisquotationEvidenceTaskStatus.Idle)
        {
            return false;
        }

        CharacterDialogueMisquotationEvidenceConsiderationRequest request = new(
            source.Actor,
            [source.Candidate]);
        ICharacterDialogueMisquotationEvidenceDecisionModel model = modelSource.TakeModel();
        return task.TryStart(model, request);
    }
}
