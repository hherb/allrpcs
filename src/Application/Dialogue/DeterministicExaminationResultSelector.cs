using Allrpcs.Domain.Dialogue;
using Allrpcs.Domain.Identifiers;

namespace Allrpcs.Application.Dialogue;

/// <summary>
/// Selects the deterministic safe result for one exact observed examination target.
/// </summary>
public static class DeterministicExaminationResultSelector
{
    /// <summary>
    /// Returns a fresh safe result only when the occurrence examines the exact supplied target.
    /// </summary>
    public static CharacterExaminationResult? Select(
        CharacterTurnWorldEventOccurrence occurrence,
        ObjectId safeTargetObjectId)
    {
        ArgumentNullException.ThrowIfNull(occurrence);
        ArgumentNullException.ThrowIfNull(safeTargetObjectId);

        CharacterIntention? action = occurrence.TranscriptEntry.PhysicalAction;
        if (action?.Type != CharacterIntentionType.ExamineObject
            || !ReferenceEquals(action.TargetObjectId, safeTargetObjectId))
        {
            return null;
        }

        return new CharacterExaminationResult(
            occurrence,
            CharacterExaminationSafety.SafeToCollect);
    }
}
