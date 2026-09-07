using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Discovery;
using Allrpcs.Domain.WorldObjects;
using Godot;

namespace Allrpcs.Game;

/// <summary>
/// Authors one temporary passive-noticing candidate from existing scene authority.
/// </summary>
public partial class TemporaryPassiveNoticingCandidateSource : Node
{
    private const int CandidateWorldObjectIndex = 0;

    /// <summary>Gets the single retained temporary passive-noticing candidate.</summary>
    public PassiveNoticingCandidate Candidate { get; private set; } = null!;

    /// <inheritdoc />
    public override void _Ready()
    {
        Character character = GetNode<TemporarySeekerCharacterSource>(
            "../TemporarySeekerCharacterSource").Character;
        WorldObject worldObject = GetNode<TemporaryWorldObjectSource>(
            "../TemporaryWorldObjectSource").WorldObjects.Entries[CandidateWorldObjectIndex];

        Candidate = CreateCandidate(character, worldObject);
    }

    private static PassiveNoticingCandidate CreateCandidate(
        Character character,
        WorldObject worldObject)
    {
        ArgumentNullException.ThrowIfNull(character);
        ArgumentNullException.ThrowIfNull(worldObject);

        return new PassiveNoticingCandidate(
            character.Identity.Id,
            worldObject.Identity.Id);
    }
}
