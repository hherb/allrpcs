using Allrpcs.Domain.Characters;
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
    // The herb patch starts unnoticed; only the Seeker's focused search reveals it.
    private const string UnnoticedObjectId = "herb-patch";
    private const string WagonClueObjectId = "wagon-clue";
    private readonly VisibilityState initialVisibilityState = VisibilityState.Noticed;

    /// <summary>Gets the single retained temporary Seeker perception snapshot.</summary>
    public CharacterPerceptionSnapshot Perception { get; private set; } = null!;

    /// <inheritdoc />
    public override void _Ready()
    {
        Character seeker = GetNode<TemporarySeekerCharacterSource>(
            "../TemporarySeekerCharacterSource").Character;
        TemporaryWorldObjectSource worldObjectSource = GetNode<TemporaryWorldObjectSource>(
            "../TemporaryWorldObjectSource");
        WorldObjectSnapshot worldObjects = worldObjectSource.WorldObjects;

        CharacterPerceptionSnapshot temporary = CreatePerception(seeker.Identity.Id, worldObjects);
        WorldObjectVisibility exactWagonVisibility = worldObjectSource
            .RepresentativeDiscoveryScenario.InitialPerception.Entries.Single(entry =>
                entry.ObjectId.Value == WagonClueObjectId);
        Perception = new CharacterPerceptionSnapshot(
            temporary.CharacterId,
            temporary.Entries.Select(entry =>
                ReferenceEquals(entry.ObjectId, exactWagonVisibility.ObjectId)
                    ? exactWagonVisibility
                    : entry));
    }

    private CharacterPerceptionSnapshot CreatePerception(
        CharacterId observerId,
        WorldObjectSnapshot worldObjects)
    {
        ArgumentNullException.ThrowIfNull(observerId);
        ArgumentNullException.ThrowIfNull(worldObjects);

        if (!worldObjects.Entries.Any(worldObject => worldObject.Identity.Id.Value == UnnoticedObjectId))
        {
            throw new InvalidOperationException(
                "The unnoticed herb patch is absent from the world-object snapshot.");
        }

        WorldObjectVisibility[] entries = worldObjects.Entries
            .Select(
                worldObject => new WorldObjectVisibility(
                    worldObject.Identity.Id,
                    worldObject.Identity.Id.Value == UnnoticedObjectId
                        ? VisibilityState.Unnoticed
                        : initialVisibilityState))
            .ToArray();

        return new CharacterPerceptionSnapshot(observerId, entries);
    }
}
