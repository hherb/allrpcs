using Allrpcs.Domain.Identifiers;

namespace Allrpcs.Domain.Dialogue;

/// <summary>
/// Retains one exact observed examination occurrence and its authoritative collection safety.
/// </summary>
public sealed record CharacterExaminationResult
{
    /// <summary>Creates an immutable result for one exact examination occurrence.</summary>
    public CharacterExaminationResult(
        CharacterTurnWorldEventOccurrence occurrence,
        CharacterExaminationSafety safety)
    {
        ArgumentNullException.ThrowIfNull(occurrence);
        ArgumentNullException.ThrowIfNull(safety);

        CharacterIntention? action = occurrence.TranscriptEntry.PhysicalAction;
        if (action?.Type != CharacterIntentionType.ExamineObject
            || action.TargetObjectId is null)
        {
            throw new ArgumentException(
                "An examination result requires a targeted examine-object occurrence.",
                nameof(occurrence));
        }

        Occurrence = occurrence;
        TargetObjectId = action.TargetObjectId;
        Safety = safety;
    }

    /// <summary>Gets the exact observed examination occurrence.</summary>
    public CharacterTurnWorldEventOccurrence Occurrence { get; }

    /// <summary>Gets the exact target retained by the observed examination action.</summary>
    public ObjectId TargetObjectId { get; }

    /// <summary>Gets the exact authoritative safety result.</summary>
    public CharacterExaminationSafety Safety { get; }
}
