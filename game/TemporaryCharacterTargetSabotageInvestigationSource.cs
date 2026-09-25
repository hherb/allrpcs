using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Identifiers;
using Godot;

namespace Allrpcs.Game;

/// <summary>Owns authored Seeker access and evidence identifiers for target-sabotage investigation.</summary>
public partial class TemporaryCharacterTargetSabotageInvestigationSource : Node
{
    /// <summary>Gets the exact scene-owned Seeker who may investigate the affected target.</summary>
    public Character Investigator { get; private set; } = null!;

    /// <summary>Gets the exact ordinary interaction access used by target investigation.</summary>
    public CharacterInteractionAccessSnapshot InteractionAccess { get; private set; } = null!;

    /// <summary>Gets the authored evidence identifier for recovered Delay history.</summary>
    public EvidenceId DelayEvidenceId { get; private set; } = null!;

    /// <summary>Gets the authored evidence identifier for recovered Influence history.</summary>
    public EvidenceId InfluenceEvidenceId { get; private set; } = null!;

    /// <inheritdoc />
    public override void _Ready()
    {
        Investigator = GetNode<TemporarySeekerCharacterSource>(
            "../TemporarySeekerCharacterSource").Character;
        TemporaryCharacterSabotageOrdinaryTurnAuthoringSource sabotage =
            GetNode<TemporaryCharacterSabotageOrdinaryTurnAuthoringSource>(
                "../TemporaryCharacterSabotageOrdinaryTurnAuthoringSource");
        Character target = sabotage.DelayRequest.Candidates.Single().Occurrence.Target;
        InteractionAccess = new CharacterInteractionAccessSnapshot(
            Investigator.Identity.Id,
            [target.Identity.Id]);
        DelayEvidenceId = new EvidenceId("sage-delay-history");
        InfluenceEvidenceId = new EvidenceId("sage-influence-history");
    }
}
