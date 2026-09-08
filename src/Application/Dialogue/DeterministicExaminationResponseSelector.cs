using Allrpcs.Domain.Dialogue;
using Allrpcs.Domain.Identifiers;

namespace Allrpcs.Application.Dialogue;

/// <summary>
/// Selects one predictable examination response for an explicitly supplied target.
/// </summary>
public static class DeterministicExaminationResponseSelector
{
    /// <summary>Gets the fixed speech accompanying the examination intention.</summary>
    public const string ExaminationSpeech = "I will examine that object first.";

    /// <summary>
    /// Selects a fresh accepted response that retains the exact request and target without
    /// interpreting dialogue text or looking up world state.
    /// </summary>
    public static CharacterTurnResponse Select(
        CharacterTurnRequest request,
        ObjectId targetObjectId)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(targetObjectId);

        return new CharacterTurnResponse(
            request,
            CharacterTurnDisposition.Accept,
            new DialogueUtterance(request.RespondingCharacterId, ExaminationSpeech),
            new CharacterIntention(CharacterIntentionType.ExamineObject, targetObjectId),
            [],
            []);
    }
}
