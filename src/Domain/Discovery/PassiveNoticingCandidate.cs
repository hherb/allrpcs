using Allrpcs.Domain.Identifiers;

namespace Allrpcs.Domain.Discovery;

/// <summary>
/// Identifies one world object that an existing character could passively notice without
/// selecting or evaluating the candidate.
/// </summary>
public sealed record PassiveNoticingCandidate
{
    /// <summary>
    /// Creates an immutable passive-noticing candidate for an existing observer and target.
    /// </summary>
    public PassiveNoticingCandidate(CharacterId observerCharacterId, ObjectId targetObjectId)
    {
        ArgumentNullException.ThrowIfNull(observerCharacterId);
        ArgumentNullException.ThrowIfNull(targetObjectId);

        ObserverCharacterId = observerCharacterId;
        TargetObjectId = targetObjectId;
    }

    /// <summary>
    /// Gets the stable identifier of the character that could notice the target.
    /// </summary>
    public CharacterId ObserverCharacterId { get; }

    /// <summary>
    /// Gets the stable identifier of the candidate world object.
    /// </summary>
    public ObjectId TargetObjectId { get; }
}
