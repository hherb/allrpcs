using Allrpcs.Domain.Characters;
using Godot;

namespace Allrpcs.Game;

/// <summary>Owns one exact scene snapshot of exposed misquotation evidence.</summary>
public partial class CharacterDialogueMisquotationEvidenceExposureController : Node
{
    private CharacterDialogueMisquotationEvidenceConsiderationRequest? lastResolvedRequest;

    /// <summary>Gets the initialized exact exposure snapshot, if any.</summary>
    public CharacterDialogueMisquotationEvidenceExposureSnapshot? Exposure { get; private set; }

    /// <summary>Initializes exact exposure state once.</summary>
    public bool TryInitialize(CharacterDialogueMisquotationEvidenceExposureSnapshot exposure)
    {
        ArgumentNullException.ThrowIfNull(exposure);

        if (Exposure is not null)
        {
            return false;
        }

        Exposure = exposure;
        return true;
    }

    /// <summary>Resolves one exact accepted result only from expected current exposure state.</summary>
    public bool TryExpose(
        CharacterDialogueMisquotationEvidenceExposureSnapshot expectedExposure,
        CharacterDialogueMisquotationEvidenceConsiderationResult considerationResult)
    {
        ArgumentNullException.ThrowIfNull(expectedExposure);
        ArgumentNullException.ThrowIfNull(considerationResult);

        CharacterDialogueMisquotationEvidenceConsiderationRequest request =
            considerationResult.Request;
        if (!ReferenceEquals(Exposure, expectedExposure)
            || ReferenceEquals(lastResolvedRequest, request))
        {
            return false;
        }

        CharacterDialogueMisquotationEvidenceExposureSnapshot result;
        try
        {
            result = CharacterDialogueMisquotationEvidenceExposureTransition.Expose(
                expectedExposure,
                considerationResult);
        }
        catch (InvalidOperationException)
        {
            return false;
        }

        Exposure = result;
        lastResolvedRequest = request;
        return true;
    }
}
