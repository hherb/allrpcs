using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Dialogue;
using Allrpcs.Domain.Identifiers;
using NumericsVector2 = System.Numerics.Vector2;

namespace Allrpcs.Application.Dialogue;

/// <summary>
/// The native owners a <see cref="ConversationTurnCoordinator{TInput}"/> sequences. Each member
/// is a thin forward to existing node members; none decides anything.
/// </summary>
/// <typeparam name="TInput">The exact prepared generation input the input source fences.</typeparam>
public interface IConversationTurnPorts<TInput>
    where TInput : class, ICharacterModelGenerationInput
{
    /// <summary>Gets whether a generation has started or retains a result.</summary>
    bool GenerationHasStarted { get; }

    /// <summary>Gets whether a generation is still running.</summary>
    bool IsGenerating { get; }

    /// <summary>Gets the retained terminal generation result, if any.</summary>
    CharacterModelStreamSnapshot? GenerationResult { get; }

    /// <summary>Gets the context-assembly status of the last input creation.</summary>
    CharacterModelContextAssemblyStatus? LastGenerationAssemblyStatus { get; }

    /// <summary>Gets the current exact character turn.</summary>
    CharacterTurnActivity? TurnActivity { get; }

    /// <summary>Gets the ordinary LLM participants; index 0 is the healer and index 1 the sage.</summary>
    IReadOnlyList<Character> OrdinaryParticipants { get; }

    /// <summary>Gets whether the healer's body is visible; an invisible healer neither hears nor is addressed.</summary>
    bool HealerPresent { get; }

    /// <summary>Gets the Seeker's current position.</summary>
    NumericsVector2 SeekerPosition { get; }

    /// <summary>Gets the healer's current position.</summary>
    NumericsVector2 HealerPosition { get; }

    /// <summary>Gets the sage's fixed hearing position.</summary>
    NumericsVector2 SageHearingPosition { get; }

    /// <summary>Gets the last accepted player delivery.</summary>
    DialogueDelivery? LastSubmittedDelivery { get; }

    /// <summary>Gets the visible dialogue status.</summary>
    string DialogueStatus { get; }

    /// <summary>Gets the last dialogue event that occurred.</summary>
    DialogueEvent? LastOccurredDialogueEvent { get; }

    /// <summary>Gets whether a participation round is owned by the player-dialogue coordinator.</summary>
    bool ParticipationBusy { get; }

    /// <summary>Gets whether the participation-round task is idle.</summary>
    bool ParticipationRoundIdle { get; }

    /// <summary>Resets a completed generation.</summary>
    bool TryResetGeneration();

    /// <summary>Cancels a running generation.</summary>
    bool TryCancelGeneration();

    /// <summary>Creates the exact fenced generation input for an awaiting turn.</summary>
    TInput? CreateGenerationInput(CharacterTurnActivity activity);

    /// <summary>Gets whether the input source still treats the request as current.</summary>
    bool IsGenerationInputCurrent(CharacterTurnRequest request);

    /// <summary>Takes the supplied model and starts bounded generation for the exact input.</summary>
    bool TryStartGeneration(TInput input, TimeSpan deadline);

    /// <summary>Cancels the current turn.</summary>
    bool TryCancelTurn();

    /// <summary>Delivers a validated response to the awaiting turn.</summary>
    bool TryReceiveTurnResponse(CharacterTurnResponse response);

    /// <summary>Replaces the visible dialogue status.</summary>
    void SetDialogueStatus(string status);

    /// <summary>Occurs one delivery for the given hearing candidates.</summary>
    bool TryOccurDialogue(
        DialogueDelivery delivery,
        NumericsVector2 speakerPosition,
        float ordinaryHearingRangePixels,
        float whisperHearingRangePixels,
        IReadOnlyList<DialogueHearingCandidate> candidates);

    /// <summary>Appends one occurred delivery to the transcript.</summary>
    bool TryAppendSpokenStatement(DialogueDelivery delivery);

    /// <summary>Continues the current participation chain after a completed turn.</summary>
    bool TryContinueParticipation(
        DialogueEvent heardDialogueEvent,
        IReadOnlyList<Character> scheduledCharacters,
        IReadOnlyList<CharacterId> directlyAddressedCharacterIds,
        TimeSpan deadline);

    /// <summary>Starts a new participation chain for player speech.</summary>
    bool TryStartParticipationChain(
        DialogueEvent heardDialogueEvent,
        IReadOnlyList<Character> scheduledCharacters,
        IReadOnlyList<CharacterId> directlyAddressedCharacterIds,
        AutonomousConversationBudget initialBudget,
        TimeSpan deadline);

    /// <summary>Cancels the participation-round task.</summary>
    bool TryCancelParticipationRound();

    /// <summary>Interrupts the autonomous conversation budget.</summary>
    bool TryInterruptConversationBudget();

    /// <summary>Hands the received turn to the examination action unless greybox scaffolding owns it.</summary>
    void OfferExamination(CharacterTurnActivity activity);

    /// <summary>Cancels pending examination ownership.</summary>
    void CancelExamination();
}
