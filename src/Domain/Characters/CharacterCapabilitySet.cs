namespace Allrpcs.Domain.Characters;

/// <summary>
/// Contains an ordered, duplicate-free set of authored character capabilities.
/// </summary>
public sealed class CharacterCapabilitySet
{
    /// <summary>
    /// Creates a capability set. An empty sequence explicitly means no capabilities are authored.
    /// </summary>
    public CharacterCapabilitySet(IEnumerable<CharacterCapability> entries)
    {
        Entries = AuthoredCharacterEntrySet.RequireUniqueOrdered(
            entries,
            capability => capability.Key,
            nameof(entries));
    }

    /// <summary>
    /// Gets capabilities in their stable authored order.
    /// </summary>
    public IReadOnlyList<CharacterCapability> Entries { get; }
}
