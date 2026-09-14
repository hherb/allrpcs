using Allrpcs.Domain.Characters;

namespace Allrpcs.Application.Characters;

/// <summary>Obtains one validated incompatibility response round in stable declaration order.</summary>
public static class CharacterBeliefGoalIncompatibilityResponseRoundController
{
    /// <summary>Returns one complete response round within a shared finite deadline.</summary>
    public static async Task<CharacterBeliefGoalIncompatibilityResponseRound> RespondAsync(
        ICharacterBeliefGoalIncompatibilityResponseModel model,
        CharacterBeliefGoalIncompatibility declaration,
        TimeSpan deadline,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(model);
        ArgumentNullException.ThrowIfNull(declaration);
        if (deadline <= TimeSpan.Zero || deadline == Timeout.InfiniteTimeSpan)
        {
            throw new ArgumentOutOfRangeException(nameof(deadline));
        }

        cancellationToken.ThrowIfCancellationRequested();
        using CancellationTokenSource deadlineCancellation =
            CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        deadlineCancellation.CancelAfter(deadline);

        try
        {
            return await RespondAsync(model, declaration, deadlineCancellation.Token)
                .ConfigureAwait(false);
        }
        catch (OperationCanceledException) when (
            deadlineCancellation.IsCancellationRequested
            && !cancellationToken.IsCancellationRequested)
        {
            throw new TimeoutException(
                "The incompatibility-response round exceeded its shared deadline.");
        }
    }

    /// <summary>Returns one complete exact response round while observing caller cancellation.</summary>
    public static async Task<CharacterBeliefGoalIncompatibilityResponseRound> RespondAsync(
        ICharacterBeliefGoalIncompatibilityResponseModel model,
        CharacterBeliefGoalIncompatibility declaration,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(model);
        ArgumentNullException.ThrowIfNull(declaration);
        cancellationToken.ThrowIfCancellationRequested();

        IReadOnlyList<CharacterBeliefGoalIncompatibilityPerspective> perspectives =
            CharacterBeliefGoalIncompatibilityPerspectiveProjection.Project(declaration);
        List<CharacterBeliefGoalIncompatibilityResponse> responses = [];

        foreach (CharacterBeliefGoalIncompatibilityPerspective perspective in perspectives)
        {
            CharacterBeliefGoalIncompatibilityConsiderationResult result =
                await CharacterBeliefGoalIncompatibilityConsiderationController.RespondAsync(
                        model,
                        new CharacterBeliefGoalIncompatibilityConsiderationRequest(perspective),
                        cancellationToken)
                    .ConfigureAwait(false);
            responses.Add(result.Response);
        }

        return new CharacterBeliefGoalIncompatibilityResponseRound(perspectives, responses);
    }
}
