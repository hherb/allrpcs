using Allrpcs.Domain.Characters;
using Godot;

namespace Allrpcs.Game;

/// <summary>Exposes and acknowledges each exact completed misquotation-evidence result once.</summary>
public partial class CharacterDialogueMisquotationEvidenceExposureCoordinator : Node
{
    private CharacterDialogueMisquotationEvidenceConsiderationResult? observedResult;
    private CharacterDialogueMisquotationEvidenceExposureSnapshot? resolvedExposure;
    private CharacterDialogueMisquotationEvidenceExposureController exposure = null!;
    private CharacterDialogueMisquotationEvidenceTaskController task = null!;

    /// <inheritdoc />
    public override void _Ready()
    {
        exposure = GetNode<CharacterDialogueMisquotationEvidenceExposureController>(
            "../CharacterDialogueMisquotationEvidenceExposureController");
        task = GetNode<CharacterDialogueMisquotationEvidenceTaskController>(
            "../CharacterDialogueMisquotationEvidenceTaskController");
    }

    /// <inheritdoc />
    public override void _Process(double delta)
    {
        _ = delta;
        _ = TryExposeCompletedResult();
    }

    /// <summary>Observes and exposes one fresh exact completed result from current exposure state.</summary>
    public bool TryExposeCompletedResult()
    {
        CharacterDialogueMisquotationEvidenceConsiderationResult? result = task.Result;
        if (task.Status != CharacterDialogueMisquotationEvidenceTaskStatus.Completed
            || result is null
            || ReferenceEquals(observedResult, result))
        {
            return false;
        }

        observedResult = result;
        resolvedExposure = null;
        CharacterDialogueMisquotationEvidenceExposureSnapshot? expectedExposure =
            exposure.Exposure;
        if (expectedExposure is null || !exposure.TryExpose(expectedExposure, result))
        {
            return false;
        }

        resolvedExposure = exposure.Exposure;
        return true;
    }

    /// <summary>Resets only the observed result whose exact resolved exposure remains current.</summary>
    public bool TryAcknowledgeCompletedResult()
    {
        CharacterDialogueMisquotationEvidenceConsiderationResult? result = task.Result;
        if (task.Status != CharacterDialogueMisquotationEvidenceTaskStatus.Completed
            || result is null
            || !ReferenceEquals(observedResult, result)
            || resolvedExposure is null
            || !ReferenceEquals(exposure.Exposure, resolvedExposure)
            || !task.TryReset())
        {
            return false;
        }

        observedResult = null;
        resolvedExposure = null;
        return true;
    }
}
