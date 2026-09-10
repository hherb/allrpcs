using Allrpcs.Domain.Identifiers;

namespace Allrpcs.Domain.Characters;

/// <summary>Retains one categorized character memory with exact local-turn provenance.</summary>
public sealed record CharacterEpistemicMemory
{
    /// <summary>Gets the maximum statement length in UTF-16 code units.</summary>
    public const int MaximumStatementLength = 1_000;

    /// <summary>Creates one immutable categorized epistemic memory.</summary>
    public CharacterEpistemicMemory(
        EpistemicMemoryId memoryId,
        CharacterId characterId,
        CharacterEpistemicMemoryCategory category,
        CharacterTurnId sourceTurnId,
        string statement,
        CharacterId? attributedSpeakerCharacterId)
    {
        ArgumentNullException.ThrowIfNull(memoryId);
        ArgumentNullException.ThrowIfNull(characterId);
        ArgumentNullException.ThrowIfNull(category);
        ArgumentNullException.ThrowIfNull(sourceTurnId);

        bool isClaim = ReferenceEquals(category, CharacterEpistemicMemoryCategory.AttributedClaim);
        if (isClaim && attributedSpeakerCharacterId is null)
        {
            throw new ArgumentException(
                "An attributed claim requires an attributable speaker.",
                nameof(attributedSpeakerCharacterId));
        }

        if (!isClaim && attributedSpeakerCharacterId is not null)
        {
            throw new ArgumentException(
                "Only an attributed claim accepts an attributable speaker.",
                nameof(attributedSpeakerCharacterId));
        }

        MemoryId = memoryId;
        CharacterId = characterId;
        Category = category;
        SourceTurnId = sourceTurnId;
        Statement = AuthoredCharacterText.RequireNonBlank(
            statement,
            MaximumStatementLength,
            nameof(statement));
        AttributedSpeakerCharacterId = attributedSpeakerCharacterId;
    }

    /// <summary>Gets the exact stable memory identity.</summary>
    public EpistemicMemoryId MemoryId { get; }

    /// <summary>Gets the exact remembering character identity.</summary>
    public CharacterId CharacterId { get; }

    /// <summary>Gets the exact closed epistemic category.</summary>
    public CharacterEpistemicMemoryCategory Category { get; }

    /// <summary>Gets the exact completed local turn that provides provenance.</summary>
    public CharacterTurnId SourceTurnId { get; }

    /// <summary>Gets the retained statement exactly as supplied.</summary>
    public string Statement { get; }

    /// <summary>Gets the exact attributed speaker for a claim, or null for other categories.</summary>
    public CharacterId? AttributedSpeakerCharacterId { get; }
}
