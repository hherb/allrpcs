using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Discovery;
using Allrpcs.Domain.WorldObjects;
using Godot;

namespace Allrpcs.Game;

/// <summary>Authors one contaminate action request from exact eligible scene state.</summary>
public partial class CharacterSabotageActionContaminationRequestController : Node
{
    private TemporaryCharacterObjectSabotageContaminationSource source = null!;
    private CharacterObjectSabotageContaminationController state = null!;

    /// <summary>Gets the exact one-shot sabotage-action request accepted by this controller.</summary>
    public CharacterSabotageActionConsiderationRequest? Request { get; private set; }

    /// <inheritdoc />
    public override void _Ready()
    {
        source = GetNode<TemporaryCharacterObjectSabotageContaminationSource>(
            "../TemporaryCharacterObjectSabotageContaminationSource");
        state = GetNode<CharacterObjectSabotageContaminationController>(
            "../CharacterObjectSabotageContaminationController");
    }

    /// <summary>Creates one request only for the exact current eligible contamination graph.</summary>
    public bool TryCreate(
        Character actor,
        CharacterPhysicalAccessSnapshot physicalAccess,
        WorldObjectSnapshot worldObjects,
        WorldObjectContaminationSnapshot contamination,
        DiscoveryEvidenceSnapshot evidence,
        CharacterObjectSabotageEvidence candidate)
    {
        ArgumentNullException.ThrowIfNull(actor);
        ArgumentNullException.ThrowIfNull(physicalAccess);
        ArgumentNullException.ThrowIfNull(worldObjects);
        ArgumentNullException.ThrowIfNull(contamination);
        ArgumentNullException.ThrowIfNull(evidence);
        ArgumentNullException.ThrowIfNull(candidate);

        if (Request is not null
            || !ReferenceEquals(source.Character, actor)
            || !ReferenceEquals(source.PhysicalAccess, physicalAccess)
            || !ReferenceEquals(source.WorldObjects, worldObjects)
            || !ReferenceEquals(source.Candidate, candidate)
            || !ReferenceEquals(state.Contamination, contamination)
            || !ReferenceEquals(state.Evidence, evidence)
            || !CharacterObjectSabotageContaminationPolicy.CanApply(
                actor,
                physicalAccess,
                worldObjects,
                contamination,
                evidence,
                candidate))
        {
            return false;
        }

        Request = new CharacterSabotageActionConsiderationRequest(
            actor,
            [CharacterSabotageActionKind.Contaminate]);
        return true;
    }
}
