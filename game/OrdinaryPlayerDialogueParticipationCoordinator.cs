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
    internal void ResetSession() { activeRoundController = null; activeBudgetController = null; activeTurnController = null; }

    internal bool IsBusy => activeRoundController is not null;
    internal event Action<string>? StatusChanged;
    private CharacterTurnParticipationRoundTaskController? activeRoundController;
    private AutonomousConversationBudgetController? activeBudgetController;
    private CharacterTurnActivityController? activeTurnController;

    /// <summary>Starts one complete stable round of exact eligible heard LLM listeners.</summary>
    public bool TryStart(
        DialogueEvent heardDialogueEvent,
        IEnumerable<Character> scheduledCharacters,
        IEnumerable<CharacterId> directlyAddressedCharacterIds,
        TemporaryCharacterTurnParticipationModelSource modelSource,
        CharacterTurnParticipationRoundTaskController roundController,
        AutonomousConversationBudgetController budgetController,
        CharacterTurnActivityController turnController,
        TimeSpan deadline)
    {
        ValidateArguments(
            heardDialogueEvent,
            scheduledCharacters,
            directlyAddressedCharacterIds,
            modelSource,
            roundController,
            budgetController,
            turnController,
            deadline);

        AutonomousConversationBudget? budget = budgetController.Budget;
        if (budget is null
            || !AutonomousConversationContinuationPolicy.CanContinue(budget)
            || !TryPrepareRound(
                heardDialogueEvent,
                scheduledCharacters,
                directlyAddressedCharacterIds,
                roundController,
                turnController,
                out IReadOnlyList<Character> preparedCharacters,
                out IReadOnlyList<CharacterId> preparedDirectAddresses))
        {
            return false;
        }

        StartRound(
            heardDialogueEvent,
            preparedCharacters,
            preparedDirectAddresses,
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
        IEnumerable<Character> scheduledCharacters,
        IEnumerable<CharacterId> directlyAddressedCharacterIds,
        TemporaryCharacterTurnParticipationModelSource modelSource,
        CharacterTurnParticipationRoundTaskController roundController,
        AutonomousConversationBudgetController budgetController,
        CharacterTurnActivityController turnController,
        AutonomousConversationBudget initialBudget,
        TimeSpan deadline)
    {
        ValidateArguments(
            heardDialogueEvent,
            scheduledCharacters,
            directlyAddressedCharacterIds,
            modelSource,
            roundController,
            budgetController,
            turnController,
            deadline);
        ArgumentNullException.ThrowIfNull(initialBudget);

        if (initialBudget.CompletedTurnCount != 0
            || initialBudget.HasPlayerInterrupted
            || !TryPrepareRound(
                heardDialogueEvent,
                scheduledCharacters,
                directlyAddressedCharacterIds,
                roundController,
                turnController,
                out IReadOnlyList<Character> preparedCharacters,
                out IReadOnlyList<CharacterId> preparedDirectAddresses))
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
            preparedCharacters,
            preparedDirectAddresses,
            modelSource,
            roundController,
            budgetController,
            turnController,
            deadline);
        return true;
    }

    private bool TryPrepareRound(
        DialogueEvent heardDialogueEvent,
        IEnumerable<Character> scheduledCharacters,
        IEnumerable<CharacterId> directlyAddressedCharacterIds,
        CharacterTurnParticipationRoundTaskController roundController,
        CharacterTurnActivityController turnController,
        out IReadOnlyList<Character> preparedCharacters,
        out IReadOnlyList<CharacterId> preparedDirectAddresses)
    {
        List<Character> characters = scheduledCharacters.ToList();
        List<CharacterId> directAddresses = directlyAddressedCharacterIds.ToList();
        preparedCharacters = characters.AsReadOnly();
        preparedDirectAddresses = directAddresses.AsReadOnly();
        try
        {
            _ = CharacterTurnParticipationConsiderationRoundTransition.CreateRequests(
                preparedCharacters,
                heardDialogueEvent,
                preparedDirectAddresses);
        }
        catch (ArgumentException)
        {
            return false;
        }

        return characters.Count > 0
            && activeRoundController is null
            && roundController.Status == CharacterTurnParticipationRoundTaskStatus.Idle
            && roundController.Result is null
            && CharacterTurnStartPolicy.CanStart(turnController.Activity)
            && characters.All(character =>
                ReferenceEquals(
                    character.ControllerAssignment.CharacterId,
                    character.Identity.Id)
                && ReferenceEquals(
                    character.ControllerAssignment.ControllerType,
                    ControllerType.Llm));
    }

    private void StartRound(
        DialogueEvent heardDialogueEvent,
        IReadOnlyList<Character> scheduledCharacters,
        IReadOnlyList<CharacterId> directlyAddressedCharacterIds,
        TemporaryCharacterTurnParticipationModelSource modelSource,
        CharacterTurnParticipationRoundTaskController roundController,
        AutonomousConversationBudgetController budgetController,
        CharacterTurnActivityController turnController,
        TimeSpan deadline)
    {
        if (!roundController.TryStart(
                modelSource.TakeModel(),
                scheduledCharacters,
                heardDialogueEvent,
                directlyAddressedCharacterIds,
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

        if (roundController.Status is CharacterTurnParticipationRoundTaskStatus.Failed
            or CharacterTurnParticipationRoundTaskStatus.TimedOut)
        {
            StatusChanged?.Invoke(roundController.Status == CharacterTurnParticipationRoundTaskStatus.TimedOut
                ? "Dialogue response timed out. Please retry."
                : "Dialogue provider unavailable. Check provider settings and retry.");
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
        IEnumerable<Character> scheduledCharacters,
        IEnumerable<CharacterId> directlyAddressedCharacterIds,
        TemporaryCharacterTurnParticipationModelSource modelSource,
        CharacterTurnParticipationRoundTaskController roundController,
        AutonomousConversationBudgetController budgetController,
        CharacterTurnActivityController turnController,
        TimeSpan deadline)
    {
        ArgumentNullException.ThrowIfNull(heardDialogueEvent);
        ArgumentNullException.ThrowIfNull(scheduledCharacters);
        ArgumentNullException.ThrowIfNull(directlyAddressedCharacterIds);
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
