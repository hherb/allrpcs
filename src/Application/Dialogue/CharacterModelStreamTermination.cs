using Allrpcs.Domain.Dialogue;

namespace Allrpcs.Application.Dialogue;

/// <summary>
/// Retains the single terminal outcome of a character-model response stream.
/// </summary>
public sealed record CharacterModelStreamTermination : CharacterModelStreamUpdate
{
    /// <summary>
    /// Creates a terminal stream update, requiring a response only for completion and failure
    /// details only for a failed outcome.
    /// </summary>
    public CharacterModelStreamTermination(
        CharacterModelStreamTerminationStatus status,
        CharacterTurnResponse? response,
        CharacterModelFailure? failure)
    {
        if (!Enum.IsDefined(status))
        {
            throw new ArgumentOutOfRangeException(nameof(status));
        }

        if (status == CharacterModelStreamTerminationStatus.Completed)
        {
            ArgumentNullException.ThrowIfNull(response);
        }
        else if (response is not null)
        {
            throw new ArgumentException(
                "Only a completed stream termination can retain a response.",
                nameof(response));
        }

        if (status == CharacterModelStreamTerminationStatus.Failed)
        {
            ArgumentNullException.ThrowIfNull(failure);
        }
        else if (failure is not null)
        {
            throw new ArgumentException(
                "Only a failed stream termination can retain a model failure.",
                nameof(failure));
        }

        Status = status;
        Response = response;
        Failure = failure;
    }

    /// <summary>
    /// Gets the terminal stream status.
    /// </summary>
    public CharacterModelStreamTerminationStatus Status { get; }

    /// <summary>
    /// Gets the exact complete provider-neutral response, or <see langword="null"/> otherwise.
    /// </summary>
    public CharacterTurnResponse? Response { get; }

    /// <summary>
    /// Gets the exact failure for a failed stream, or <see langword="null"/> otherwise.
    /// </summary>
    public CharacterModelFailure? Failure { get; }
}
