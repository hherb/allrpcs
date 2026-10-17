using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Dialogue;
using Allrpcs.Domain.Identifiers;
using Godot;

namespace Allrpcs.Game;

/// <summary>Owns one exact immutable autonomous-conversation budget.</summary>
public partial class AutonomousConversationBudgetController : Node
{
    internal void ResetSession() { Budget = null; }

    /// <summary>Gets the exact initialized budget, if any.</summary>
    public AutonomousConversationBudget? Budget { get; private set; }

    /// <summary>Initializes this controller once from one exact existing budget.</summary>
    public bool TryInitialize(AutonomousConversationBudget budget)
    {
        ArgumentNullException.ThrowIfNull(budget);

        if (Budget is not null)
        {
            return false;
        }

        Budget = budget;
        return true;
    }

    /// <summary>Atomically installs one exact fresh player-rooted chain budget.</summary>
    internal bool TryInstallNewChain(
        AutonomousConversationBudget? expectedCurrentBudget,
        AutonomousConversationBudget initialBudget)
    {
        ArgumentNullException.ThrowIfNull(initialBudget);

        if (!ReferenceEquals(Budget, expectedCurrentBudget)
            || ReferenceEquals(initialBudget, expectedCurrentBudget)
            || initialBudget.CompletedTurnCount != 0
            || initialBudget.HasPlayerInterrupted)
        {
            return false;
        }

        Budget = initialBudget;
        return true;
    }

    /// <summary>Records exactly one further completed turn when the budget permits it.</summary>
    public bool TryRecordCompletedTurn()
    {
        AutonomousConversationBudget? budget = Budget;
        if (budget is null || !AutonomousConversationContinuationPolicy.CanContinue(budget))
        {
            return false;
        }

        Budget = AutonomousConversationBudgetTransition.RecordCompletedTurn(budget);
        return true;
    }

    /// <summary>Starts one exact selected follow-up while this budget and the turn slot allow it.</summary>
    internal bool TryStartFollowUp(
        CharacterTurnActivityController turnController,
        CharacterTurnParticipationDecision decision,
        CharacterTurnRequest request)
    {
        ArgumentNullException.ThrowIfNull(turnController);
        ArgumentNullException.ThrowIfNull(decision);
        ArgumentNullException.ThrowIfNull(request);

        AutonomousConversationBudget? budget = Budget;
        if (budget is null
            || !AutonomousConversationTurnStartPolicy.CanStart(
                budget,
                decision,
                request,
                turnController.Activity))
        {
            return false;
        }

        return turnController.TryStart(request);
    }

    /// <summary>Creates and starts one selected follow-up from the exact heard dialogue event.</summary>
    internal bool TryCreateAndStartFollowUp(
        CharacterTurnActivityController turnController,
        CharacterTurnParticipationDecision decision,
        DialogueEvent dialogueEvent,
        DialogueActType dialogueActType)
    {
        ArgumentNullException.ThrowIfNull(turnController);
        ArgumentNullException.ThrowIfNull(decision);
        ArgumentNullException.ThrowIfNull(dialogueEvent);
        ArgumentNullException.ThrowIfNull(dialogueActType);

        bool isSpeakingDecision = decision.Kind == CharacterTurnParticipationKind.Respond
            || decision.Kind == CharacterTurnParticipationKind.Interject;
        if (!isSpeakingDecision
            || !dialogueEvent.Audience.ListenerCharacterIds.Contains(
                decision.ScheduledCharacterId))
        {
            return false;
        }

        CharacterTurnRequest request =
            AutonomousConversationFollowUpRequestTransition.Create(
                decision,
                dialogueEvent,
                dialogueActType);
        return TryStartFollowUp(turnController, decision, request);
    }

    /// <summary>Validates a complete round and starts its exact selected heard-event follow-up.</summary>
    public bool TrySelectAndStartFollowUp(
        CharacterTurnActivityController turnController,
        IEnumerable<Character> scheduledCharacters,
        IEnumerable<CharacterTurnParticipationDecision> decisions,
        IEnumerable<CharacterId> directlyAddressedCharacterIds,
        DialogueEvent dialogueEvent,
        DialogueActType dialogueActType)
    {
        ArgumentNullException.ThrowIfNull(turnController);
        ArgumentNullException.ThrowIfNull(scheduledCharacters);
        ArgumentNullException.ThrowIfNull(decisions);
        ArgumentNullException.ThrowIfNull(directlyAddressedCharacterIds);
        ArgumentNullException.ThrowIfNull(dialogueEvent);
        ArgumentNullException.ThrowIfNull(dialogueActType);

        AutonomousConversationBudget? budget = Budget;
        if (budget is null)
        {
            return false;
        }

        CharacterTurnParticipationDecision? selected =
            AutonomousConversationParticipationPolicy.SelectNext(
                budget,
                scheduledCharacters,
                decisions,
                directlyAddressedCharacterIds);
        return selected is not null
            && TryCreateAndStartFollowUp(
                turnController,
                selected,
                dialogueEvent,
                dialogueActType);
    }

    /// <summary>Starts the exact selected follow-up from one complete consideration round.</summary>
    public bool TrySelectAndStartFollowUp(
        CharacterTurnActivityController turnController,
        CharacterTurnParticipationConsiderationRoundResult round,
        DialogueActType dialogueActType)
    {
        ArgumentNullException.ThrowIfNull(turnController);
        ArgumentNullException.ThrowIfNull(round);
        ArgumentNullException.ThrowIfNull(dialogueActType);

        AutonomousConversationBudget? budget = Budget;
        if (budget is null)
        {
            return false;
        }

        CharacterTurnParticipationConsiderationResult? selected =
            AutonomousConversationConsiderationSelectionPolicy.SelectNext(budget, round);
        return selected is not null
            && TryCreateAndStartFollowUp(
                turnController,
                selected.Decision,
                selected.Request.HeardDialogueEvent,
                dialogueActType);
    }

    /// <summary>Completes one speaking turn and advances this budget as one scene operation.</summary>
    public bool TryCompleteTurn(CharacterTurnActivityController turnController)
    {
        ArgumentNullException.ThrowIfNull(turnController);

        AutonomousConversationBudget? budget = Budget;
        CharacterTurnActivity? activity = turnController.Activity;
        if (budget is null
            || activity is null
            || !AutonomousConversationContinuationPolicy.CanContinue(budget)
            || !CharacterTurnCompletionPolicy.CanComplete(activity))
        {
            return false;
        }

        AutonomousConversationTurnCompletionResult result =
            AutonomousConversationTurnCompletionTransition.Complete(budget, activity);
        if (!turnController.TryInstallAutonomousCompletion(activity, result.Activity))
        {
            return false;
        }

        Budget = result.Budget;
        return true;
    }

    /// <summary>Monotonically records the player's first interruption.</summary>
    public bool TryInterrupt()
    {
        AutonomousConversationBudget? budget = Budget;
        if (budget is null || budget.HasPlayerInterrupted)
        {
            return false;
        }

        Budget = AutonomousConversationBudgetTransition.Interrupt(budget);
        return true;
    }
}
