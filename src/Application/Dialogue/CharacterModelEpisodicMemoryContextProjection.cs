using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Identifiers;

namespace Allrpcs.Application.Dialogue;

/// <summary>
/// Projects one responding character's newest canonical episodes into optional model context.
/// </summary>
public static class CharacterModelEpisodicMemoryContextProjection
{
    /// <summary>Gets the maximum number of episodes offered to one turn.</summary>
    public const int MaximumEpisodeCount = 12;

    /// <summary>Returns fresh memory entries for the newest episodes, oldest first.</summary>
    public static IReadOnlyList<CharacterModelContextEntry> Project(
        CharacterId respondingCharacterId,
        CharacterMemorySnapshot memory)
    {
        ArgumentNullException.ThrowIfNull(respondingCharacterId);
        ArgumentNullException.ThrowIfNull(memory);

        IReadOnlyList<CharacterEpisodicMemory> episodes = memory.Characters
            .SingleOrDefault(candidate => candidate.CharacterId == respondingCharacterId)
            ?.EpisodicMemories.Memories ?? [];
        return episodes
            .Skip(Math.Max(0, episodes.Count - MaximumEpisodeCount))
            .Select(episode => new CharacterModelContextEntry(
                CharacterModelContextEntryKind.Memory,
                $"kind=episode;source-turn={episode.SourceTurnId.Value};"
                    + $"location={episode.LocationId.Value};summary={episode.Summary}"))
            .ToList()
            .AsReadOnly();
    }
}
