using Allrpcs.Domain.Identifiers;

namespace Allrpcs.Domain.Dialogue;

/// <summary>
/// Describes one proposed immediate character intention without validating or executing it.
/// </summary>
public sealed record CharacterIntention
{
    /// <summary>
    /// Creates an immutable intention payload that can retain a missing or unexpected target for
    /// deterministic validation.
    /// </summary>
    public CharacterIntention(
        CharacterIntentionType type,
        ObjectId? targetObjectId)
    {
        ArgumentNullException.ThrowIfNull(type);

        Type = type;
        TargetObjectId = targetObjectId;
    }

    /// <summary>
    /// Gets the exact defined intention type.
    /// </summary>
    public CharacterIntentionType Type { get; }

    /// <summary>
    /// Gets the exact target identifier when one was proposed; otherwise, <see langword="null"/>.
    /// </summary>
    public ObjectId? TargetObjectId { get; }
}
