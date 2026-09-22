using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Discovery;
using Allrpcs.Domain.WorldObjects;
using Godot;

namespace Allrpcs.Game;

/// <summary>Authors one substitute action request from exact eligible scene state.</summary>
public partial class CharacterSabotageActionSubstitutionRequestController
    : Node, ICharacterSabotageActionRequestAuthor
{
    private TemporaryCharacterObjectSabotageSubstitutionSource source = null!;
    private CharacterObjectSabotageSubstitutionController state = null!;

    /// <summary>Gets the exact one-shot sabotage-action request accepted by this controller.</summary>
    public CharacterSabotageActionConsiderationRequest? Request { get; private set; }

    /// <inheritdoc />
    public CharacterSabotageActionCandidateBinding? CandidateBinding { get; private set; }

    /// <inheritdoc />
    public override void _Ready()
    {
        source = GetNode<TemporaryCharacterObjectSabotageSubstitutionSource>(
            "../TemporaryCharacterObjectSabotageSubstitutionSource");
        state = GetNode<CharacterObjectSabotageSubstitutionController>(
            "../CharacterObjectSabotageSubstitutionController");
    }

    /// <summary>Creates one request only for the exact current eligible substitution graph.</summary>
    public bool TryCreate(
        Character actor,
        CharacterPhysicalAccessSnapshot physicalAccess,
        WorldObjectSnapshot worldObjects,
        CharacterObjectSabotageSubstitutionSnapshot substitutions,
        DiscoveryEvidenceSnapshot evidence,
        CharacterObjectSabotageSubstitutionCandidate candidate)
    {
        ArgumentNullException.ThrowIfNull(actor);
        ArgumentNullException.ThrowIfNull(physicalAccess);
        ArgumentNullException.ThrowIfNull(worldObjects);
        ArgumentNullException.ThrowIfNull(substitutions);
        ArgumentNullException.ThrowIfNull(evidence);
        ArgumentNullException.ThrowIfNull(candidate);

        if (Request is not null
            || !ReferenceEquals(source.Character, actor)
            || !ReferenceEquals(source.PhysicalAccess, physicalAccess)
            || !ReferenceEquals(source.WorldObjects, worldObjects)
            || !ReferenceEquals(source.Candidate, candidate)
            || !ReferenceEquals(state.Substitutions, substitutions)
            || !ReferenceEquals(state.Evidence, evidence)
            || !CharacterObjectSabotageSubstitutionPolicy.CanApply(
                actor,
                physicalAccess,
                worldObjects,
                substitutions,
                evidence,
                candidate))
        {
            return false;
        }

        CharacterSabotageActionConsiderationRequest request = new(
            actor,
            [CharacterSabotageActionKind.Substitute]);
        Request = request;
        CandidateBinding = new CharacterSabotageActionCandidateBinding<
            CharacterObjectSabotageSubstitutionCandidate>(
                request,
                CharacterSabotageActionKind.Substitute,
                candidate);
        return true;
    }
}
