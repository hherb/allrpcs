namespace Allrpcs.Domain.Characters;

/// <summary>
/// Contains an ordered, duplicate-free set of authored character beliefs.
/// </summary>
public sealed class CharacterBeliefSet
{
    /// <summary>
    /// Creates a belief set. An empty sequence explicitly means no beliefs are authored.
    /// </summary>
    public CharacterBeliefSet(IEnumerable<CharacterBelief> entries)
    {
        Entries = AuthoredCharacterEntrySet.RequireUniqueOrdered(
            entries,
            belief => belief.Key,
            nameof(entries));
    }

    /// <summary>
    /// Gets beliefs in their stable authored order.
    /// </summary>
    public IReadOnlyList<CharacterBelief> Entries { get; }
}
