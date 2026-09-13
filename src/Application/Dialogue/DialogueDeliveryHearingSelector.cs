using System.Numerics;
using Allrpcs.Domain.Dialogue;

namespace Allrpcs.Application.Dialogue;

/// <summary>
/// Selects a dialogue audience using the hearing range for one explicit delivery mode.
/// </summary>
public static class DialogueDeliveryHearingSelector
{
    /// <summary>
    /// Selects listeners through the existing bounded hearing rule after validating both ranges.
    /// </summary>
    public static DialogueAudienceSnapshot Select(
        DialogueDelivery delivery,
        Vector2 speakerPosition,
        float ordinaryRangePixels,
        float whisperRangePixels,
        IReadOnlyList<DialogueHearingCandidate> candidates)
    {
        ArgumentNullException.ThrowIfNull(delivery);
        ValidateRange(ordinaryRangePixels, nameof(ordinaryRangePixels));
        ValidateRange(whisperRangePixels, nameof(whisperRangePixels));

        float selectedRange = delivery.Mode == DialogueDeliveryMode.OrdinarySpeech
            ? ordinaryRangePixels
            : whisperRangePixels;

        return DialogueHearingSelector.Select(
            delivery.Utterance.SpeakerCharacterId,
            speakerPosition,
            selectedRange,
            candidates);
    }

    private static void ValidateRange(float rangePixels, string parameterName)
    {
        if (!float.IsFinite(rangePixels) || rangePixels <= 0.0f)
        {
            throw new ArgumentOutOfRangeException(
                parameterName,
                rangePixels,
                "Dialogue delivery hearing ranges must be finite and positive.");
        }
    }
}
