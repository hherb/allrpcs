using Allrpcs.Domain.Identifiers;

namespace Allrpcs.Domain.Characters;

/// <summary>Contains bounded canonical local memory for all retained characters.</summary>
public sealed class CharacterMemorySnapshot
{
    /// <summary>Gets the maximum number of characters retained in one memory snapshot.</summary>
    public const int MaximumCharacterCount = 32;

    /// <summary>Creates one immutable all-character memory snapshot.</summary>
    public CharacterMemorySnapshot(IEnumerable<CharacterMemoryAggregate> characters)
    {
        ArgumentNullException.ThrowIfNull(characters);

        List<CharacterMemoryAggregate> retainedCharacters = new();
        HashSet<CharacterId> characterIds = new();
        foreach (CharacterMemoryAggregate character in characters)
        {
            if (character is null)
            {
                throw new ArgumentException(
                    "Character-memory snapshots cannot contain null values.",
                    nameof(characters));
            }

            if (!characterIds.Add(character.CharacterId))
            {
                throw new ArgumentException(
                    "Character-memory snapshots cannot contain duplicate character identities.",
                    nameof(characters));
            }

            retainedCharacters.Add(character);
            if (retainedCharacters.Count > MaximumCharacterCount)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(characters),
                    retainedCharacters.Count,
                    $"A memory snapshot accepts at most {MaximumCharacterCount} characters.");
            }
        }

        Characters = retainedCharacters.AsReadOnly();
    }

    /// <summary>Gets exact character-memory aggregates in stable supplied order.</summary>
    public IReadOnlyList<CharacterMemoryAggregate> Characters { get; }
}
