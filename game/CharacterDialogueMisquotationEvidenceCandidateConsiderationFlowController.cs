using Allrpcs.Application.Characters;
using Allrpcs.Domain.Characters;
using Godot;

namespace Allrpcs.Game;

/// <summary>Starts one decision for the exact current live-candidate request.</summary>
public partial class CharacterDialogueMisquotationEvidenceCandidateConsiderationFlowController
    : Node
{
    private CharacterDialogueMisquotationEvidenceCandidateRequestController requests = null!;
    private TemporaryCharacterDialogueMisquotationEvidenceModelSource modelSource = null!;
    private CharacterDialogueMisquotationEvidenceTaskController task = null!;

    /// <inheritdoc />
    public override void _Ready()
    {
        requests = GetNode<CharacterDialogueMisquotationEvidenceCandidateRequestController>(
            "../CharacterDialogueMisquotationEvidenceCandidateRequestController");
        modelSource = GetNode<TemporaryCharacterDialogueMisquotationEvidenceModelSource>(
            "../TemporaryCharacterDialogueMisquotationEvidenceModelSource");
        task = GetNode<CharacterDialogueMisquotationEvidenceTaskController>(
            "../CharacterDialogueMisquotationEvidenceTaskController");
    }

    /// <summary>Starts once only for the exact current request while the decision task is idle.</summary>
    public bool TryStart(
        CharacterDialogueMisquotationEvidenceConsiderationRequest expectedRequest)
    {
        ArgumentNullException.ThrowIfNull(expectedRequest);
        if (!ReferenceEquals(requests.Request, expectedRequest)
            || task.Status != CharacterDialogueMisquotationEvidenceTaskStatus.Idle)
        {
            return false;
        }

        ICharacterDialogueMisquotationEvidenceDecisionModel model = modelSource.TakeModel();
        return task.TryStart(model, expectedRequest);
    }
}
