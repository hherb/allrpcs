using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Discovery;
using Allrpcs.Domain.Identifiers;
using Allrpcs.Domain.WorldObjects;
using Godot;

namespace Allrpcs.Game;

/// <summary>
/// Authors one temporary observation-reinterpretation candidate from existing scene authority.
/// </summary>
public partial class TemporaryObservationReinterpretationCandidateSource : Node
{
    private const int CandidateWorldObjectIndex = 1;
    private const int TriggerKnowledgeIndex = 0;

    /// <summary>Gets the single retained temporary observation-reinterpretation candidate.</summary>
    public ObservationReinterpretationCandidate Candidate { get; private set; } = null!;

    /// <inheritdoc />
    public override void _Ready()
    {
        Character character = GetNode<TemporarySeekerCharacterSource>(
            "../TemporarySeekerCharacterSource").Character;
        WorldObject worldObject = GetNode<TemporaryWorldObjectSource>(
            "../TemporaryWorldObjectSource").WorldObjects.Entries[CandidateWorldObjectIndex];

        Candidate = CreateCandidate(character, worldObject);
    }

    private static ObservationReinterpretationCandidate CreateCandidate(
        Character character,
        WorldObject worldObject)
    {
        ArgumentNullException.ThrowIfNull(character);
        ArgumentNullException.ThrowIfNull(worldObject);

        KnowledgeId triggerKnowledgeId = character.Knowledge.KnowledgeReferences
            .ElementAtOrDefault(TriggerKnowledgeIndex)
            ?? throw new InvalidOperationException(
                "The temporary Seeker has no trigger knowledge for observation reinterpretation.");

        return new ObservationReinterpretationCandidate(
            character.Identity.Id,
            worldObject.Identity.Id,
            triggerKnowledgeId);
    }
}
