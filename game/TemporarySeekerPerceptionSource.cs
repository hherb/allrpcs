using Allrpcs.Domain.Identifiers;
using Allrpcs.Domain.Perception;
using Godot;

namespace Allrpcs.Game;

/// <summary>
/// Authors one temporary immutable perception snapshot for the existing Seeker and greybox
/// targets.
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
        ObjectId herbPatchObjectId = GetNode<GreyboxInteractionTarget>(
            "../InteractionCandidates/HerbPatch").ObjectId;
        ObjectId stoneMarkerObjectId = GetNode<GreyboxInteractionTarget>(
            "../InteractionCandidates/StoneMarker").ObjectId;
        WorldObjectVisibility herbPatchVisibility = new(
            herbPatchObjectId,
            initialVisibilityState);
        WorldObjectVisibility stoneMarkerVisibility = new(
            stoneMarkerObjectId,
            initialVisibilityState);

        Perception = new CharacterPerceptionSnapshot(
            seekerCharacterId,
            [herbPatchVisibility, stoneMarkerVisibility]);
    }
}
