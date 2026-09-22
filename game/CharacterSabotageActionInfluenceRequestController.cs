using Allrpcs.Domain.Characters;
using Godot;

namespace Allrpcs.Game;

/// <summary>Authors one influence action request from exact eligible relationship state.</summary>
public partial class CharacterSabotageActionInfluenceRequestController
    : Node, ICharacterSabotageActionRequestAuthor
{
    private CharacterTargetSabotageInfluenceController state = null!;

    /// <summary>Gets the exact one-shot sabotage-action request accepted by this controller.</summary>
    public CharacterSabotageActionConsiderationRequest? Request { get; private set; }

    /// <inheritdoc />
    public CharacterSabotageActionCandidateBinding? CandidateBinding { get; private set; }

    /// <inheritdoc />
    public override void _Ready()
    {
        state = GetNode<CharacterTargetSabotageInfluenceController>(
            "../CharacterTargetSabotageInfluenceController");
    }

    /// <summary>Creates one request only for the exact current eligible influence graph.</summary>
    public bool TryCreate(
        CharacterTargetSabotageInfluenceConsiderationRequest influenceRequest,
        Character actor,
        CharacterInteractionAccessSnapshot interactionAccess,
        CharacterRelationshipMemory relationship,
        CharacterTargetSabotageInfluenceCandidate candidate)
    {
        ArgumentNullException.ThrowIfNull(influenceRequest);
        ArgumentNullException.ThrowIfNull(actor);
        ArgumentNullException.ThrowIfNull(interactionAccess);
        ArgumentNullException.ThrowIfNull(relationship);
        ArgumentNullException.ThrowIfNull(candidate);

        if (Request is not null
            || !ReferenceEquals(influenceRequest.Actor, actor)
            || !ReferenceEquals(influenceRequest.InteractionAccess, interactionAccess)
            || !ReferenceEquals(influenceRequest.Relationship, relationship)
            || !influenceRequest.Candidates.Any(requestCandidate =>
                ReferenceEquals(requestCandidate, candidate))
            || !ReferenceEquals(state.Relationship, relationship)
            || !CharacterTargetSabotageInfluencePolicy.CanApply(
                actor,
                interactionAccess,
                relationship,
                candidate))
        {
            return false;
        }

        CharacterSabotageActionConsiderationRequest request = new(
            actor,
            [CharacterSabotageActionKind.Influence]);
        Request = request;
        CandidateBinding = new CharacterSabotageActionCandidateBinding<
            CharacterTargetSabotageInfluenceCandidate>(
                request,
                CharacterSabotageActionKind.Influence,
                candidate);
        return true;
    }
}
