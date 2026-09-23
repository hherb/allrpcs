using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Discovery;
using Allrpcs.Domain.Locations;
using Allrpcs.Domain.WorldObjects;
using Godot;

namespace Allrpcs.Game;

/// <summary>Authors one move action request from exact eligible scene state.</summary>
public partial class CharacterSabotageActionMoveRequestController
    : Node, ICharacterSabotageActionRequestAuthor
{
    private TemporaryCharacterObjectSabotageMoveSource source = null!;
    private CharacterObjectSabotageMoveController state = null!;

    /// <summary>Gets the exact one-shot sabotage-action request accepted by this controller.</summary>
    public CharacterSabotageActionConsiderationRequest? Request { get; private set; }

    /// <inheritdoc />
    public CharacterSabotageActionCandidateBinding? CandidateBinding { get; private set; }

    /// <inheritdoc />
    public override void _Ready()
    {
        source = GetNode<TemporaryCharacterObjectSabotageMoveSource>(
            "../TemporaryCharacterObjectSabotageMoveSource");
        state = GetNode<CharacterObjectSabotageMoveController>(
            "../CharacterObjectSabotageMoveController");
    }

    /// <summary>Creates one request only for the exact current eligible move graph.</summary>
    public bool TryCreate(
        Character actor,
        CharacterPhysicalAccessSnapshot physicalAccess,
        WorldObjectSnapshot worldObjects,
        WorldObjectPlacementSnapshot placements,
        DiscoveryEvidenceSnapshot evidence,
        Location destination,
        CharacterObjectSabotageMoveCandidate candidate)
    {
        ArgumentNullException.ThrowIfNull(actor);
        ArgumentNullException.ThrowIfNull(physicalAccess);
        ArgumentNullException.ThrowIfNull(worldObjects);
        ArgumentNullException.ThrowIfNull(placements);
        ArgumentNullException.ThrowIfNull(evidence);
        ArgumentNullException.ThrowIfNull(destination);
        ArgumentNullException.ThrowIfNull(candidate);

        if (Request is not null
            || !ReferenceEquals(source.Character, actor)
            || !ReferenceEquals(source.PhysicalAccess, physicalAccess)
            || !ReferenceEquals(source.WorldObjects, worldObjects)
            || !ReferenceEquals(source.Destination, destination)
            || (!ReferenceEquals(source.Candidate, candidate)
                && !ReferenceEquals(source.FollowUpCandidate, candidate))
            || !ReferenceEquals(state.Placements, placements)
            || !ReferenceEquals(state.Evidence, evidence)
            || !CharacterObjectSabotageMovePolicy.CanApply(
                actor,
                physicalAccess,
                worldObjects,
                placements,
                evidence,
                candidate))
        {
            return false;
        }

        CharacterSabotageActionConsiderationRequest request = new(
            actor,
            [CharacterSabotageActionKind.Move]);
        Request = request;
        CandidateBinding = new CharacterSabotageActionCandidateBinding<
            CharacterObjectSabotageMoveCandidate>(
                request,
                CharacterSabotageActionKind.Move,
                candidate);
        return true;
    }
}
