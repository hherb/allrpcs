using Allrpcs.Domain.Identifiers;

namespace Allrpcs.Domain.Discovery;

/// <summary>
/// Identifies one earlier observation that an existing character could reinterpret after gaining
/// specific knowledge, without evaluating the candidate.
/// </summary>
public sealed record ObservationReinterpretationCandidate
{
    /// <summary>
    /// Creates an immutable observation-reinterpretation candidate for existing identifiers.
    /// </summary>
    public ObservationReinterpretationCandidate(
        CharacterId observerCharacterId,
        ObjectId targetObjectId,
        KnowledgeId triggerKnowledgeId)
    {
        ArgumentNullException.ThrowIfNull(observerCharacterId);
        ArgumentNullException.ThrowIfNull(targetObjectId);
        ArgumentNullException.ThrowIfNull(triggerKnowledgeId);

        ObserverCharacterId = observerCharacterId;
        TargetObjectId = targetObjectId;
        TriggerKnowledgeId = triggerKnowledgeId;
    }

    /// <summary>
    /// Gets the stable identifier of the character that could reinterpret the observation.
    /// </summary>
    public CharacterId ObserverCharacterId { get; }

    /// <summary>
    /// Gets the stable identifier of the observed world object.
    /// </summary>
    public ObjectId TargetObjectId { get; }

    /// <summary>
    /// Gets the stable identifier of the knowledge that could trigger reinterpretation.
    /// </summary>
    public KnowledgeId TriggerKnowledgeId { get; }
}
