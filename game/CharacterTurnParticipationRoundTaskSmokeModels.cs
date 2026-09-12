using Allrpcs.Application.Dialogue;
using Allrpcs.Domain.Dialogue;

namespace Allrpcs.Game;

/// <summary>Records exact participation requests and decisions for scene smoke checks.</summary>
internal sealed class RecordingParticipationModel(
    Func<CharacterTurnParticipationConsiderationRequest, CharacterTurnParticipationDecision> decide)
    : ICharacterTurnParticipationModel
{
    internal List<CharacterTurnParticipationConsiderationRequest> Requests { get; } = [];

    internal List<CharacterTurnParticipationDecision> Decisions { get; } = [];

    public ValueTask<CharacterTurnParticipationDecision> DecideAsync(
        CharacterTurnParticipationConsiderationRequest request,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        Requests.Add(request);
        CharacterTurnParticipationDecision decision = decide(request);
        Decisions.Add(decision);
        return ValueTask.FromResult(decision);
    }
}

/// <summary>Waits until participation-round cancellation for scene smoke checks.</summary>
internal sealed class WaitingParticipationModel : ICharacterTurnParticipationModel
{
    public async ValueTask<CharacterTurnParticipationDecision> DecideAsync(
        CharacterTurnParticipationConsiderationRequest request,
        CancellationToken cancellationToken)
    {
        _ = request;
        await Task.Delay(Timeout.InfiniteTimeSpan, cancellationToken);
        throw new InvalidOperationException("The cancelled wait unexpectedly resumed.");
    }
}

/// <summary>Fails without exposing its private detail through the scene controller.</summary>
internal sealed class FailingParticipationModel : ICharacterTurnParticipationModel
{
    public ValueTask<CharacterTurnParticipationDecision> DecideAsync(
        CharacterTurnParticipationConsiderationRequest request,
        CancellationToken cancellationToken)
    {
        _ = request;
        _ = cancellationToken;
        return ValueTask.FromException<CharacterTurnParticipationDecision>(
            new InvalidOperationException("Private model failure detail."));
    }
}
