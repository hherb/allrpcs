using Allrpcs.Domain.Dialogue;

namespace Allrpcs.Application.Dialogue;

/// <summary>
/// Selects one predictable provider-neutral response from an explicitly declared dialogue act.
/// </summary>
public static class DeterministicCharacterTurnResponseSelector
{
    /// <summary>Gets the fixed speech used when accepting a request.</summary>
    public const string AcceptSpeech = "I will help with that request.";

    /// <summary>Gets the fixed speech used when refusing a direct command.</summary>
    public const string RefuseCommandSpeech = "I will not follow that command.";

    /// <summary>Gets the fixed speech used when asking for clarification.</summary>
    public const string ClarifySpeech = "Please clarify your question.";

    /// <summary>Gets the fixed speech used when conditionally accepting a plan.</summary>
    public const string ConditionalSpeech = "I will agree if the condition is met.";

    /// <summary>Gets the fixed speech used when refusing every other defined dialogue act.</summary>
    public const string RefuseOtherSpeech = "I will not act on that statement.";

    /// <summary>
    /// Selects a fixed disposition and speech using only the request's declared dialogue-act type.
    /// </summary>
    public static CharacterTurnResponse Select(CharacterTurnRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        (CharacterTurnDisposition Disposition, string Speech) selection =
            SelectDispositionAndSpeech(request.DialogueAct.Type);

        return new CharacterTurnResponse(
            request,
            selection.Disposition,
            new DialogueDelivery(
                new DialogueUtterance(request.RespondingCharacterId, selection.Speech),
                DialogueDeliveryMode.OrdinarySpeech),
            new CharacterIntention(CharacterIntentionType.Wait, null),
            [],
            []);
    }

    private static (CharacterTurnDisposition Disposition, string Speech)
        SelectDispositionAndSpeech(DialogueActType actType)
    {
        if (actType == DialogueActType.Request)
        {
            return (CharacterTurnDisposition.Accept, AcceptSpeech);
        }

        if (actType == DialogueActType.Command)
        {
            return (CharacterTurnDisposition.Refuse, RefuseCommandSpeech);
        }

        if (actType == DialogueActType.Question)
        {
            return (CharacterTurnDisposition.Clarify, ClarifySpeech);
        }

        if (actType == DialogueActType.ConditionalPlan)
        {
            return (CharacterTurnDisposition.ConditionallyAccept, ConditionalSpeech);
        }

        return (CharacterTurnDisposition.Refuse, RefuseOtherSpeech);
    }
}
