using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Discovery;
using Allrpcs.Domain.WorldObjects;
using Godot;

namespace Allrpcs.Game;

/// <summary>Authors one conceal action request from exact eligible scene state.</summary>
public partial class CharacterSabotageActionConcealmentRequestController
    : Node, ICharacterSabotageActionRequestAuthor
{
    private TemporaryCharacterObjectSabotageSource source = null!;
    private CharacterObjectSabotageConcealmentController state = null!;

    /// <summary>Gets the exact one-shot sabotage-action request accepted by this controller.</summary>
    public CharacterSabotageActionConsiderationRequest? Request { get; private set; }

    /// <inheritdoc />
    public CharacterSabotageActionCandidateBinding? CandidateBinding { get; private set; }

    /// <inheritdoc />
    public override void _Ready()
    {
        source = GetNode<TemporaryCharacterObjectSabotageSource>(
            "../TemporaryCharacterObjectSabotageSource");
        state = GetNode<CharacterObjectSabotageConcealmentController>(
            "../CharacterObjectSabotageConcealmentController");
    }

    /// <summary>Creates one request only for the exact current eligible concealment graph.</summary>
    public bool TryCreate(
        Character actor,
        CharacterPhysicalAccessSnapshot physicalAccess,
        WorldObjectSnapshot worldObjects,
        WorldObjectConcealmentSnapshot concealment,
        DiscoveryEvidenceSnapshot evidence,
        CharacterObjectSabotageEvidence candidate)
    {
        ArgumentNullException.ThrowIfNull(actor);
        ArgumentNullException.ThrowIfNull(physicalAccess);
        ArgumentNullException.ThrowIfNull(worldObjects);
        ArgumentNullException.ThrowIfNull(concealment);
        ArgumentNullException.ThrowIfNull(evidence);
        ArgumentNullException.ThrowIfNull(candidate);

        if (Request is not null
            || !ReferenceEquals(source.Character, actor)
            || !ReferenceEquals(source.PhysicalAccess, physicalAccess)
            || !ReferenceEquals(source.WorldObjects, worldObjects)
            || !ReferenceEquals(source.Candidate, candidate)
            || !ReferenceEquals(state.Concealment, concealment)
            || !ReferenceEquals(state.Evidence, evidence)
            || !CharacterObjectSabotageConcealmentPolicy.CanApply(
                actor,
                physicalAccess,
                worldObjects,
                concealment,
                evidence,
                candidate))
        {
            return false;
        }

        CharacterSabotageActionConsiderationRequest request = new(
            actor,
            [CharacterSabotageActionKind.Conceal]);
        Request = request;
        CandidateBinding = new CharacterSabotageActionCandidateBinding<
            CharacterObjectSabotageEvidence>(
                request,
                CharacterSabotageActionKind.Conceal,
                candidate);
        return true;
    }
}
