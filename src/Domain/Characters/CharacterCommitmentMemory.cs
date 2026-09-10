using Allrpcs.Domain.Identifiers;

namespace Allrpcs.Domain.Characters;

/// <summary>Retains one unfinished promise, assigned task, or other character commitment.</summary>
public sealed record CharacterCommitmentMemory
{
    /// <summary>Gets the maximum description length in UTF-16 code units.</summary>
    public const int MaximumDescriptionLength = 1_000;

    /// <summary>Creates one immutable unfinished character commitment.</summary>
    public CharacterCommitmentMemory(
        CharacterCommitmentId commitmentId,
        CharacterId characterId,
        CharacterCommitmentKind kind,
        CharacterId counterpartyCharacterId,
        CharacterTurnId sourceTurnId,
        string description)
    {
        ArgumentNullException.ThrowIfNull(commitmentId);
        ArgumentNullException.ThrowIfNull(characterId);
        ArgumentNullException.ThrowIfNull(kind);
        ArgumentNullException.ThrowIfNull(counterpartyCharacterId);
        ArgumentNullException.ThrowIfNull(sourceTurnId);
        if (characterId == counterpartyCharacterId)
        {
            throw new ArgumentException(
                "An unfinished commitment requires a different counterparty character.",
                nameof(counterpartyCharacterId));
        }

        CommitmentId = commitmentId;
        CharacterId = characterId;
        Kind = kind;
        CounterpartyCharacterId = counterpartyCharacterId;
        SourceTurnId = sourceTurnId;
        Description = AuthoredCharacterText.RequireNonBlank(
            description,
            MaximumDescriptionLength,
            nameof(description));
    }

    /// <summary>Gets the exact stable commitment identity.</summary>
    public CharacterCommitmentId CommitmentId { get; }

    /// <summary>Gets the exact responsible character identity.</summary>
    public CharacterId CharacterId { get; }

    /// <summary>Gets the exact closed commitment kind.</summary>
    public CharacterCommitmentKind Kind { get; }

    /// <summary>Gets the exact character to whom the promise is made or who assigned the task.</summary>
    public CharacterId CounterpartyCharacterId { get; }

    /// <summary>Gets the exact completed local turn providing provenance.</summary>
    public CharacterTurnId SourceTurnId { get; }

    /// <summary>Gets the bounded unfinished commitment text exactly as supplied.</summary>
    public string Description { get; }
}
