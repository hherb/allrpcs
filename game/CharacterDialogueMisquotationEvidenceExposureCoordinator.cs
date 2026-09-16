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
    private CharacterDialogueMisquotationEvidenceViewController view = null!;

    /// <inheritdoc />
    public override void _Ready()
    {
        exposure = GetNode<CharacterDialogueMisquotationEvidenceExposureController>(
            "../CharacterDialogueMisquotationEvidenceExposureController");
        task = GetNode<CharacterDialogueMisquotationEvidenceTaskController>(
            "../CharacterDialogueMisquotationEvidenceTaskController");
        view = GetNode<CharacterDialogueMisquotationEvidenceViewController>(
            "../CharacterDialogueMisquotationEvidenceViewController");
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
        if (expectedExposure is null
            || !ReferenceEquals(view.SourceExposure, expectedExposure)
            || !exposure.TryExpose(expectedExposure, result))
        {
            return false;
        }

        resolvedExposure = exposure.Exposure;
        if (resolvedExposure is null)
        {
            throw new InvalidOperationException(
                "Successful misquotation-evidence exposure must retain an exact snapshot.");
        }

        if (!ReferenceEquals(resolvedExposure, expectedExposure)
            && !view.TryRefresh(expectedExposure, resolvedExposure))
        {
            throw new InvalidOperationException(
                "The misquotation-evidence view must advance with successful exposure.");
        }

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
            || !ReferenceEquals(view.SourceExposure, resolvedExposure)
            || !task.TryReset())
        {
            return false;
        }

        observedResult = null;
        resolvedExposure = null;
        return true;
    }
}
