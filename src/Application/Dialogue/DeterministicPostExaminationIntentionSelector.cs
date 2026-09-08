using Allrpcs.Domain.Dialogue;

namespace Allrpcs.Application.Dialogue;

/// <summary>
/// Selects the deterministic next intention from an authoritative examination result.
/// </summary>
public static class DeterministicPostExaminationIntentionSelector
{
    /// <summary>
    /// Selects collection of the exact examined target only when it is safe; otherwise waits.
    /// </summary>
    public static CharacterIntention Select(CharacterExaminationResult result)
    {
        ArgumentNullException.ThrowIfNull(result);

        return result.Safety == CharacterExaminationSafety.SafeToCollect
            ? new CharacterIntention(CharacterIntentionType.CollectObject, result.TargetObjectId)
            : new CharacterIntention(CharacterIntentionType.Wait, null);
    }
}
