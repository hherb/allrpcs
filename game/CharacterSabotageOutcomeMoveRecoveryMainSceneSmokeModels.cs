using Allrpcs.Application.Characters;
using Allrpcs.Domain.Characters;

namespace Allrpcs.Game;

/// <summary>Records one deterministic generic Move choice for the focused main-scene smoke.</summary>
internal sealed class RecordingActionModel : ICharacterSabotageActionDecisionModel
{
    /// <summary>Gets the number of model invocations.</summary>
    public int CallCount { get; private set; }

    /// <inheritdoc />
    public ValueTask<CharacterSabotageActionDecision> DecideAsync(
        CharacterSabotageActionConsiderationRequest request,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        CallCount++;
        return ValueTask.FromResult(
            new CharacterSabotageActionDecision(
                request.Actor.Identity.Id,
                CharacterSabotageActionKind.Move));
    }
}

/// <summary>Records one deterministic exact Move candidate for the focused main-scene smoke.</summary>
internal sealed class RecordingMoveModel : ICharacterObjectSabotageMoveDecisionModel
{
    /// <summary>Gets the number of model invocations.</summary>
    public int CallCount { get; private set; }

    /// <inheritdoc />
    public ValueTask<CharacterObjectSabotageMoveDecision> DecideAsync(
        CharacterObjectSabotageMoveConsiderationRequest request,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        CallCount++;
        return ValueTask.FromResult(
            new CharacterObjectSabotageMoveDecision(
                request.Character.Identity.Id,
                request.Candidates.Single().Evidence.Evidence.EvidenceId));
    }
}
