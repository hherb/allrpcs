using Allrpcs.Domain.Dialogue;
using Allrpcs.Domain.Identifiers;

namespace Allrpcs.Application.Dialogue;

/// <summary>
/// Retains the immutable duplicate-free targeted intentions currently available to one turn.
/// </summary>
public sealed class CharacterIntentionAffordanceSnapshot
{
    /// <summary>Creates a snapshot in stable caller-supplied order.</summary>
    public CharacterIntentionAffordanceSnapshot(
        IEnumerable<CharacterIntentionAffordance> entries)
    {
        ArgumentNullException.ThrowIfNull(entries);

        List<CharacterIntentionAffordance> retainedEntries = new();
        HashSet<(CharacterIntentionType Type, ObjectId TargetObjectId)> identities = new();
        foreach (CharacterIntentionAffordance entry in entries)
        {
            if (entry is null)
            {
                throw new ArgumentException(
                    "Intention affordances cannot contain null values.",
                    nameof(entries));
            }

            if (!identities.Add((entry.Type, entry.TargetObjectId)))
            {
                throw new ArgumentException(
                    "Intention affordances cannot contain duplicate type and target pairs.",
                    nameof(entries));
            }

            retainedEntries.Add(entry);
        }

        Entries = retainedEntries.AsReadOnly();
    }

    /// <summary>Gets currently permitted targeted intentions in stable supplied order.</summary>
    public IReadOnlyList<CharacterIntentionAffordance> Entries { get; }
}
