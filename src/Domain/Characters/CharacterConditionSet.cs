namespace Allrpcs.Domain.Characters;

/// <summary>
/// Contains an ordered, duplicate-free set of authored qualitative character conditions.
/// </summary>
public sealed class CharacterConditionSet
{
    /// <summary>
    /// Creates a condition set. An empty sequence explicitly means no conditions are authored.
    /// </summary>
    public CharacterConditionSet(IEnumerable<CharacterCondition> entries)
    {
        Entries = AuthoredCharacterEntrySet.RequireUniqueOrdered(
            entries,
            condition => condition.Key,
            nameof(entries));
    }

    /// <summary>
    /// Gets conditions in their stable authored order.
    /// </summary>
    public IReadOnlyList<CharacterCondition> Entries { get; }
}
