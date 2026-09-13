using Allrpcs.Domain.Dialogue;

namespace Allrpcs.Application.Dialogue;

/// <summary>
/// Retains the provider-neutral turn boundary supplied to a character model.
/// </summary>
public sealed record CharacterModelGenerationRequest
{
    /// <summary>
    /// Creates a model-generation request from one turn and its bounded typed context.
    /// </summary>
    public CharacterModelGenerationRequest(
        CharacterTurnRequest turnRequest,
        CharacterModelContextSnapshot context)
    {
        ArgumentNullException.ThrowIfNull(turnRequest);
        ArgumentNullException.ThrowIfNull(context);

        string playerDialogue = turnRequest.DialogueEvent.Utterance.Text;
        DialogueDeliveryMode playerDeliveryMode = turnRequest.DialogueEvent.Delivery.Mode;
        int untrustedDialogueCount = 0;
        foreach (CharacterModelContextEntry entry in context.Entries)
        {
            if (entry.Kind != CharacterModelContextEntryKind.UntrustedPlayerDialogue)
            {
                continue;
            }

            untrustedDialogueCount++;
            if (!ReferenceEquals(entry.Text, playerDialogue)
                || !ReferenceEquals(entry.DeliveryMode, playerDeliveryMode))
            {
                throw new ArgumentException(
                    "Untrusted player dialogue must retain the exact turn-request text and mode.",
                    nameof(context));
            }
        }

        if (untrustedDialogueCount != 1)
        {
            throw new ArgumentException(
                "Model context must contain exactly one untrusted player-dialogue entry.",
                nameof(context));
        }

        TurnRequest = turnRequest;
        Context = context;
    }

    /// <summary>
    /// Gets the exact provider-neutral character-turn request.
    /// </summary>
    public CharacterTurnRequest TurnRequest { get; }

    /// <summary>
    /// Gets the exact bounded provider-neutral context supplied for generation.
    /// </summary>
    public CharacterModelContextSnapshot Context { get; }
}
