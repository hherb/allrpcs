using Allrpcs.Domain.Identifiers;
using Allrpcs.Domain.Perception;
using Allrpcs.Domain.WorldObjects;
using Godot;

namespace Allrpcs.Game;

/// <summary>
/// Authors one temporary immutable perception snapshot for the existing Seeker and world objects.
/// </summary>
public partial class TemporarySeekerPerceptionSource : Node
{
    private readonly VisibilityState initialVisibilityState = VisibilityState.Noticed;

    /// <summary>Gets the single retained temporary Seeker perception snapshot.</summary>
    public CharacterPerceptionSnapshot Perception { get; private set; } = null!;

    /// <inheritdoc />
    public override void _Ready()
    {
        CharacterId seekerCharacterId = GetNode<FocusedSearchActivityController>(
            "../Seeker/FocusedSearchActivityController").SearcherCharacterId;
        WorldObjectSnapshot worldObjects = GetNode<TemporaryWorldObjectSource>(
            "../TemporaryWorldObjectSource").WorldObjects;

        Perception = CreatePerception(seekerCharacterId, worldObjects);
    }

    private CharacterPerceptionSnapshot CreatePerception(
        CharacterId characterId,
        WorldObjectSnapshot worldObjects)
    {
        ArgumentNullException.ThrowIfNull(characterId);
        ArgumentNullException.ThrowIfNull(worldObjects);

        WorldObjectVisibility[] entries = worldObjects.Entries
            .Select(
                worldObject => new WorldObjectVisibility(
                    worldObject.Identity.Id,
                    initialVisibilityState))
            .ToArray();

        return new CharacterPerceptionSnapshot(characterId, entries);
    }
}
