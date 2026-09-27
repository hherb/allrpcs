using Allrpcs.Domain.Discovery;
using Allrpcs.Domain.Perception;
using Allrpcs.Domain.WorldObjects;
using Godot;

namespace Allrpcs.Game;

/// <summary>
/// Authors one temporary immutable perception snapshot for the existing Seeker and world objects.
/// </summary>
public partial class TemporarySeekerPerceptionSource : Node
{
    private const string WagonClueObjectId = "wagon-clue";
    private readonly VisibilityState initialVisibilityState = VisibilityState.Noticed;

    /// <summary>Gets the single retained temporary Seeker perception snapshot.</summary>
    public CharacterPerceptionSnapshot Perception { get; private set; } = null!;

    /// <inheritdoc />
    public override void _Ready()
    {
        PassiveNoticingCandidate candidate =
            GetNode<TemporaryPassiveNoticingCandidateSource>(
                "../TemporaryPassiveNoticingCandidateSource").Candidate;
        TemporaryWorldObjectSource worldObjectSource = GetNode<TemporaryWorldObjectSource>(
            "../TemporaryWorldObjectSource");
        WorldObjectSnapshot worldObjects = worldObjectSource.WorldObjects;

        CharacterPerceptionSnapshot temporary = CreatePerception(candidate, worldObjects);
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
        PassiveNoticingCandidate candidate,
        WorldObjectSnapshot worldObjects)
    {
        ArgumentNullException.ThrowIfNull(candidate);
        ArgumentNullException.ThrowIfNull(worldObjects);

        WorldObject candidateWorldObject = WorldObjectLookup.FindById(
            candidate.TargetObjectId,
            worldObjects) ?? throw new InvalidOperationException(
                "The passive-noticing candidate target is absent from the world-object snapshot.");
        WorldObjectVisibility[] entries = worldObjects.Entries
            .Select(
                worldObject => new WorldObjectVisibility(
                    worldObject.Identity.Id,
                    ReferenceEquals(worldObject, candidateWorldObject)
                        ? VisibilityState.Unnoticed
                        : initialVisibilityState))
            .ToArray();

        return new CharacterPerceptionSnapshot(candidate.ObserverCharacterId, entries);
    }
}
