using Allrpcs.Domain.Dialogue;
using Allrpcs.Domain.Identifiers;

namespace Allrpcs.Application.Dialogue;

/// <summary>Retains the exact accepted response independently of the later dialogue lifecycle.</summary>
public sealed class AcceptedCharacterExamination
{
    private AcceptedCharacterExamination(CharacterTurnResponse response) { Response = response; }

    /// <summary>The exact response that authorized this examination, never a reconstructed reply.</summary>
    public CharacterTurnResponse Response { get; }

    /// <summary>
    /// Accepts only the live responder's accepted or conditionally accepted examination, after any
    /// exact speech was recorded.
    /// </summary>
    public static AcceptedCharacterExamination? TryAccept(CharacterTurnActivity activity,
        CharacterId actor, ObjectId target, CharacterTurnTranscriptSnapshot transcript)
    {
        ArgumentNullException.ThrowIfNull(activity);
        ArgumentNullException.ThrowIfNull(actor);
        ArgumentNullException.ThrowIfNull(target);
        ArgumentNullException.ThrowIfNull(transcript);
        CharacterTurnResponse? response = activity.Response;
        if (activity.State != CharacterTurnActivityState.Speaking || response is null
            || (response.Disposition != CharacterTurnDisposition.Accept
                && response.Disposition != CharacterTurnDisposition.ConditionallyAccept)
            || !ReferenceEquals(activity.Request.RespondingCharacterId, actor)
            || response.ImmediateIntention.Type != CharacterIntentionType.ExamineObject
            || response.ImmediateIntention.TargetObjectId != target
            || (response.Speech is { } speech && !transcript.Entries.Any(
                entry => ReferenceEquals(entry.SpokenStatement, speech)))) { return null; }
        return new(response);
    }

    /// <summary>Creates an action entry retaining the original actor and exact proposed action.</summary>
    public CharacterTurnTranscriptEntry CreateTranscriptEntry() =>
        new(Response.Request.RespondingCharacterId, Response.ImmediateIntention);
}
