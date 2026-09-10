using Allrpcs.Domain.Identifiers;

namespace Allrpcs.Domain.Dialogue;

/// <summary>
/// Retains one exact completed local character turn under a stable provider-neutral identifier.
/// </summary>
public sealed record CharacterRecentConversationTurn
{
    /// <summary>Creates one recent-conversation entry from an exact completed activity.</summary>
    public CharacterRecentConversationTurn(
        CharacterTurnId turnId,
        CharacterTurnActivity activity)
    {
        ArgumentNullException.ThrowIfNull(turnId);
        ArgumentNullException.ThrowIfNull(activity);

        if (activity.State != CharacterTurnActivityState.Completed)
        {
            throw new ArgumentException(
                "A recent-conversation turn must retain a completed activity.",
                nameof(activity));
        }

        TurnId = turnId;
        CharacterId = activity.Request.RespondingCharacterId;
        Activity = activity;
    }

    /// <summary>Gets the exact stable identity assigned to this completed turn.</summary>
    public CharacterTurnId TurnId { get; }

    /// <summary>Gets the exact responding-character identity retained by the completed activity.</summary>
    public CharacterId CharacterId { get; }

    /// <summary>Gets the exact completed provider-neutral activity.</summary>
    public CharacterTurnActivity Activity { get; }
}
