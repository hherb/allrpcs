namespace Allrpcs.Domain.Characters;

/// <summary>
/// Contains an ordered, duplicate-free set of authored character limitations.
/// </summary>
public sealed class CharacterLimitationSet
{
    /// <summary>
    /// Creates a limitation set. An empty sequence explicitly means no limitations are authored.
    /// </summary>
    public CharacterLimitationSet(IEnumerable<CharacterLimitation> entries)
    {
        Entries = AuthoredCharacterEntrySet.RequireUniqueOrdered(
            entries,
            limitation => limitation.Key,
            nameof(entries));
    }

    /// <summary>
    /// Gets limitations in their stable authored order.
    /// </summary>
    public IReadOnlyList<CharacterLimitation> Entries { get; }
}
