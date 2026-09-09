using Allrpcs.Domain.Dialogue;
using Allrpcs.Domain.Identifiers;

namespace Allrpcs.Application.Dialogue;

/// <summary>
/// Identifies one currently permitted targeted intention without executing it.
/// </summary>
public sealed record CharacterIntentionAffordance
{
    /// <summary>Creates an immutable targeted intention affordance.</summary>
    public CharacterIntentionAffordance(
        CharacterIntentionType type,
        ObjectId targetObjectId)
    {
        ArgumentNullException.ThrowIfNull(type);
        ArgumentNullException.ThrowIfNull(targetObjectId);

        if (type == CharacterIntentionType.Wait)
        {
            throw new ArgumentException(
                "Wait does not require an explicit action affordance.",
                nameof(type));
        }

        Type = type;
        TargetObjectId = targetObjectId;
    }

    /// <summary>Gets the exact permitted targeted intention type.</summary>
    public CharacterIntentionType Type { get; }

    /// <summary>Gets the exact authoritative target identifier.</summary>
    public ObjectId TargetObjectId { get; }
}
