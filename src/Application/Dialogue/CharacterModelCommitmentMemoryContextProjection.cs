using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Identifiers;

namespace Allrpcs.Application.Dialogue;

/// <summary>
/// Projects one responding character's canonical unfinished commitments into model context.
/// </summary>
public static class CharacterModelCommitmentMemoryContextProjection
{
    /// <summary>
    /// Returns fresh memory entries in stable commitment order from the matching aggregate.
    /// </summary>
    public static IReadOnlyList<CharacterModelContextEntry> Project(
        CharacterId respondingCharacterId,
        CharacterMemorySnapshot memory)
    {
        ArgumentNullException.ThrowIfNull(respondingCharacterId);
        ArgumentNullException.ThrowIfNull(memory);

        CharacterMemoryAggregate? aggregate = memory.Characters.SingleOrDefault(
            candidate => candidate.CharacterId == respondingCharacterId);
        if (aggregate is null)
        {
            return Array.AsReadOnly(Array.Empty<CharacterModelContextEntry>());
        }

        List<CharacterModelContextEntry> entries = new();
        foreach (CharacterCommitmentMemory commitment in aggregate.Commitments.Commitments)
        {
            entries.Add(new CharacterModelContextEntry(
                CharacterModelContextEntryKind.Memory,
                $"kind={commitment.Kind.Value};"
                    + $"counterparty={commitment.CounterpartyCharacterId.Value};"
                    + $"source-turn={commitment.SourceTurnId.Value};"
                    + $"description={commitment.Description}"));
        }

        return entries.AsReadOnly();
    }
}
