using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Controllers;
using Allrpcs.Domain.Dialogue;
using Allrpcs.Domain.Identifiers;
using Godot;

namespace Allrpcs.Game;

/// <summary>
/// Coordinates one eligible ordinary player-dialogue participation round without copying its state.
/// </summary>
public partial class OrdinaryPlayerDialogueParticipationCoordinator : Node
{
    private CharacterTurnParticipationRoundTaskController? activeRoundController;
    private AutonomousConversationBudgetController? activeBudgetController;
    private CharacterTurnActivityController? activeTurnController;

    /// <summary>Starts one exact eligible directly addressed LLM participation round.</summary>
    public bool TryStart(
        DialogueEvent heardDialogueEvent,
        Character scheduledCharacter,
        CharacterId directlyAddressedCharacterId,
        TemporaryCharacterTurnParticipationModelSource modelSource,
        CharacterTurnParticipationRoundTaskController roundController,
        AutonomousConversationBudgetController budgetController,
        CharacterTurnActivityController turnController,
        TimeSpan deadline)
    {
        ValidateArguments(
            heardDialogueEvent,
            scheduledCharacter,
            directlyAddressedCharacterId,
            modelSource,
            roundController,
            budgetController,
            turnController,
            deadline);

        AutonomousConversationBudget? budget = budgetController.Budget;
        if (budget is null
            || !AutonomousConversationContinuationPolicy.CanContinue(budget)
            || !CanStartRound(
                heardDialogueEvent,
                scheduledCharacter,
                directlyAddressedCharacterId,
                roundController,
                turnController))
        {
            return false;
        }

        StartRound(
            heardDialogueEvent,
            scheduledCharacter,
            directlyAddressedCharacterId,
            modelSource,
            roundController,
            budgetController,
            turnController,
            deadline);
        return true;
    }

    /// <summary>Installs and starts one exact fresh finite player-rooted chain.</summary>
    public bool TryStartNewChain(
        DialogueEvent heardDialogueEvent,
        Character scheduledCharacter,
        CharacterId directlyAddressedCharacterId,
        TemporaryCharacterTurnParticipationModelSource modelSource,
        CharacterTurnParticipationRoundTaskController roundController,
        AutonomousConversationBudgetController budgetController,
        CharacterTurnActivityController turnController,
        AutonomousConversationBudget initialBudget,
        TimeSpan deadline)
    {
        ValidateArguments(
            heardDialogueEvent,
            scheduledCharacter,
            directlyAddressedCharacterId,
            modelSource,
            roundController,
            budgetController,
            turnController,
            deadline);
        ArgumentNullException.ThrowIfNull(initialBudget);

        if (initialBudget.CompletedTurnCount != 0
            || initialBudget.HasPlayerInterrupted
            || !CanStartRound(
                heardDialogueEvent,
                scheduledCharacter,
                directlyAddressedCharacterId,
                roundController,
                turnController))
        {
            return false;
        }

        AutonomousConversationBudget? expectedBudget = budgetController.Budget;
        if (!budgetController.TryInstallNewChain(expectedBudget, initialBudget))
        {
            return false;
        }

        StartRound(
            heardDialogueEvent,
            scheduledCharacter,
            directlyAddressedCharacterId,
            modelSource,
            roundController,
            budgetController,
            turnController,
            deadline);
        return true;
    }

    private bool CanStartRound(
        DialogueEvent heardDialogueEvent,
        Character scheduledCharacter,
        CharacterId directlyAddressedCharacterId,
        CharacterTurnParticipationRoundTaskController roundController,
        CharacterTurnActivityController turnController)
    {
        CharacterId scheduledCharacterId = scheduledCharacter.Identity.Id;
        CharacterControllerAssignment assignment = scheduledCharacter.ControllerAssignment;
        return activeRoundController is null
            && roundController.Status == CharacterTurnParticipationRoundTaskStatus.Idle
            && roundController.Result is null
            && CharacterTurnStartPolicy.CanStart(turnController.Activity)
            && ReferenceEquals(assignment.CharacterId, scheduledCharacterId)
            && ReferenceEquals(assignment.ControllerType, ControllerType.Llm)
            && heardDialogueEvent.Audience.ListenerCharacterIds.Contains(scheduledCharacterId)
            && directlyAddressedCharacterId.Equals(scheduledCharacterId);
    }

    private void StartRound(
        DialogueEvent heardDialogueEvent,
        Character scheduledCharacter,
        CharacterId directlyAddressedCharacterId,
        TemporaryCharacterTurnParticipationModelSource modelSource,
        CharacterTurnParticipationRoundTaskController roundController,
        AutonomousConversationBudgetController budgetController,
        CharacterTurnActivityController turnController,
        TimeSpan deadline)
    {
        if (!roundController.TryStart(
                modelSource.TakeModel(),
                [scheduledCharacter],
                heardDialogueEvent,
                [directlyAddressedCharacterId],
                deadline))
        {
            throw new InvalidOperationException(
                "The preflighted ordinary participation round could not start.");
        }

        activeRoundController = roundController;
        activeBudgetController = budgetController;
        activeTurnController = turnController;
    }

    /// <inheritdoc />
    public override void _Process(double delta)
    {
        _ = delta;
        CharacterTurnParticipationRoundTaskController? roundController = activeRoundController;
        if (roundController is null
            || roundController.Status == CharacterTurnParticipationRoundTaskStatus.Running)
        {
            return;
        }

        if (roundController.Status == CharacterTurnParticipationRoundTaskStatus.Completed
            && roundController.Result is { } result)
        {
            _ = activeBudgetController!.TrySelectAndStartFollowUp(
                activeTurnController!,
                result,
                DialogueActType.Request);
        }

        if (!roundController.TryReset())
        {
            throw new InvalidOperationException(
                "The terminal ordinary participation round could not reset.");
        }

        activeRoundController = null;
        activeBudgetController = null;
        activeTurnController = null;
    }

    private static void ValidateArguments(
        DialogueEvent heardDialogueEvent,
        Character scheduledCharacter,
        CharacterId directlyAddressedCharacterId,
        TemporaryCharacterTurnParticipationModelSource modelSource,
        CharacterTurnParticipationRoundTaskController roundController,
        AutonomousConversationBudgetController budgetController,
        CharacterTurnActivityController turnController,
        TimeSpan deadline)
    {
        ArgumentNullException.ThrowIfNull(heardDialogueEvent);
        ArgumentNullException.ThrowIfNull(scheduledCharacter);
        ArgumentNullException.ThrowIfNull(directlyAddressedCharacterId);
        ArgumentNullException.ThrowIfNull(modelSource);
        ArgumentNullException.ThrowIfNull(roundController);
        ArgumentNullException.ThrowIfNull(budgetController);
        ArgumentNullException.ThrowIfNull(turnController);
        if (deadline <= TimeSpan.Zero || deadline == Timeout.InfiniteTimeSpan)
        {
            throw new ArgumentOutOfRangeException(nameof(deadline));
        }
    }
}
