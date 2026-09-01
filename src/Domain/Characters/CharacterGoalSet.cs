namespace Allrpcs.Domain.Characters;

/// <summary>
/// Contains an ordered, duplicate-free set of authored character goals.
/// </summary>
public sealed class CharacterGoalSet
{
    /// <summary>
    /// Creates a goal set. An empty sequence explicitly means no goals are authored.
    /// </summary>
    public CharacterGoalSet(IEnumerable<CharacterGoal> entries)
    {
        Entries = AuthoredCharacterEntrySet.RequireUniqueOrdered(
            entries,
            goal => goal.Key,
            nameof(entries));
    }

    /// <summary>
    /// Gets goals in their stable authored order.
    /// </summary>
    public IReadOnlyList<CharacterGoal> Entries { get; }
}
