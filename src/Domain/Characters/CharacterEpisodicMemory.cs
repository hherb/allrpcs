using Allrpcs.Domain.Identifiers;
using Allrpcs.Domain.WorldEvents;

namespace Allrpcs.Domain.Characters;

/// <summary>
/// Retains one bounded significant experience with local participants and exact provenance.
/// </summary>
/// <remarks>Instances use reference identity; copied collections do not define value equality.</remarks>
public sealed class CharacterEpisodicMemory
{
    /// <summary>Gets the maximum number of participants retained by one memory.</summary>
    public const int MaximumParticipantCount = 32;

    /// <summary>Gets the maximum summary length in UTF-16 code units.</summary>
    public const int MaximumSummaryLength = 1_000;

    /// <summary>Creates one immutable significant episodic memory.</summary>
    public CharacterEpisodicMemory(
        EpisodicMemoryId memoryId,
        CharacterId characterId,
        IEnumerable<CharacterId> participantCharacterIds,
        LocationId locationId,
        WorldTimeSnapshot worldTime,
        CharacterTurnId sourceTurnId,
        string summary)
    {
        ArgumentNullException.ThrowIfNull(memoryId);
        ArgumentNullException.ThrowIfNull(characterId);
        ArgumentNullException.ThrowIfNull(participantCharacterIds);
        ArgumentNullException.ThrowIfNull(locationId);
        ArgumentNullException.ThrowIfNull(worldTime);
        ArgumentNullException.ThrowIfNull(sourceTurnId);

        List<CharacterId> participants = new();
        HashSet<CharacterId> participantIds = new();
        foreach (CharacterId participantId in participantCharacterIds)
        {
            if (participantId is null)
            {
                throw new ArgumentException(
                    "Episodic-memory participants cannot contain null values.",
                    nameof(participantCharacterIds));
            }

            if (!participantIds.Add(participantId))
            {
                throw new ArgumentException(
                    "Episodic-memory participant identifiers must be unique.",
                    nameof(participantCharacterIds));
            }

            participants.Add(participantId);
            if (participants.Count > MaximumParticipantCount)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(participantCharacterIds),
                    participants.Count,
                    $"An episodic memory accepts at most {MaximumParticipantCount} participants.");
            }
        }

        if (!participantIds.Contains(characterId))
        {
            throw new ArgumentException(
                "An episodic memory must include its remembering character as a participant.",
                nameof(participantCharacterIds));
        }

        MemoryId = memoryId;
        CharacterId = characterId;
        ParticipantCharacterIds = participants.AsReadOnly();
        LocationId = locationId;
        WorldTime = worldTime;
        SourceTurnId = sourceTurnId;
        Summary = AuthoredCharacterText.RequireNonBlank(
            summary,
            MaximumSummaryLength,
            nameof(summary));
    }

    /// <summary>Gets the exact stable memory identity.</summary>
    public EpisodicMemoryId MemoryId { get; }

    /// <summary>Gets the exact identity of the character retaining this memory.</summary>
    public CharacterId CharacterId { get; }

    /// <summary>Gets exact participant identities in stable supplied order.</summary>
    public IReadOnlyList<CharacterId> ParticipantCharacterIds { get; }

    /// <summary>Gets the exact location identity where the remembered episode occurred.</summary>
    public LocationId LocationId { get; }

    /// <summary>Gets the exact qualitative-time snapshot when the episode occurred.</summary>
    public WorldTimeSnapshot WorldTime { get; }

    /// <summary>Gets the exact completed local turn from which this memory was derived.</summary>
    public CharacterTurnId SourceTurnId { get; }

    /// <summary>Gets the exact bounded summary text.</summary>
    public string Summary { get; }
}
