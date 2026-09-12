using Allrpcs.Application.Dialogue;
using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Dialogue;
using Allrpcs.Domain.Identifiers;

namespace Allrpcs.Game;

/// <summary>Owns isolated scene collaborators for one ordinary participation smoke scenario.</summary>
internal sealed record OrdinaryPlayerDialogueParticipationSmokeScenario(
    OrdinaryPlayerDialogueParticipationCoordinator Coordinator,
    CharacterTurnParticipationRoundTaskController RoundController,
    TemporaryCharacterTurnParticipationModelSource ModelSource,
    AutonomousConversationBudgetController BudgetController,
    CharacterTurnActivityController TurnController,
    Character Character,
    CharacterId DirectAddress,
    DialogueEvent Event,
    AutonomousConversationBudget InitialBudget,
    OrdinaryParticipationRecordingModel Model) : IDisposable
{
    public void Dispose()
    {
        Coordinator.Free();
        RoundController.Free();
        ModelSource.Free();
        BudgetController.Free();
        TurnController.Free();
    }
}

/// <summary>Records each exact request before returning one deterministic decision.</summary>
internal sealed class OrdinaryParticipationRecordingModel(
    Func<CharacterTurnParticipationConsiderationRequest, CharacterTurnParticipationDecision>
        decide) : ICharacterTurnParticipationModel
{
    public List<CharacterTurnParticipationConsiderationRequest> Requests { get; } = [];

    public ValueTask<CharacterTurnParticipationDecision> DecideAsync(
        CharacterTurnParticipationConsiderationRequest request,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        Requests.Add(request);
        return ValueTask.FromResult(decide(request));
    }
}

/// <summary>Waits until the bounded scene controller cancels the exact request.</summary>
internal sealed class OrdinaryParticipationWaitingModel : ICharacterTurnParticipationModel
{
    public async ValueTask<CharacterTurnParticipationDecision> DecideAsync(
        CharacterTurnParticipationConsiderationRequest request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        await Task.Delay(Timeout.InfiniteTimeSpan, cancellationToken);
        throw new InvalidOperationException("The waiting participation model completed.");
    }
}
