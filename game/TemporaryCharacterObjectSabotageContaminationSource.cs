using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Discovery;
using Allrpcs.Domain.Identifiers;
using Allrpcs.Domain.Perception;
using Allrpcs.Domain.WorldObjects;
using Godot;

namespace Allrpcs.Game;

/// <summary>Authors one exact temporary eligible enemy contamination candidate and initial state.</summary>
public partial class TemporaryCharacterObjectSabotageContaminationSource : Node
{
    private const int WaterCaskWorldObjectIndex = 2;
    private const string RequiredCapabilityKey = "assess-local-influence";
    private const string EvidenceIdText = "bitter-cask-residue";

    /// <summary>Gets the exact authored enemy character.</summary>
    public Character Character { get; private set; } = null!;

    /// <summary>Gets the exact authored physical-access snapshot.</summary>
    public CharacterPhysicalAccessSnapshot PhysicalAccess { get; private set; } = null!;

    /// <summary>Gets the exact existing authoritative world-object snapshot.</summary>
    public WorldObjectSnapshot WorldObjects { get; private set; } = null!;

    /// <summary>Gets the exact empty initial contamination snapshot.</summary>
    public WorldObjectContaminationSnapshot Contamination { get; private set; } = null!;

    /// <summary>Gets the exact empty initial evidence snapshot.</summary>
    public DiscoveryEvidenceSnapshot Evidence { get; private set; } = null!;

    /// <summary>Gets the exact authored eligible contamination candidate.</summary>
    public CharacterObjectSabotageEvidence Candidate { get; private set; } = null!;

    /// <inheritdoc />
    public override void _Ready()
    {
        Character = RepresentativeEnemyAgentCharacterSource.Create();
        WorldObjects = GetNode<TemporaryWorldObjectSource>(
            "../TemporaryWorldObjectSource").WorldObjects;
        WorldObject target = WorldObjects.Entries[WaterCaskWorldObjectIndex];
        PhysicalAccess = new CharacterPhysicalAccessSnapshot(
            Character.Identity.Id,
            [target.Identity.Id]);
        Contamination = new WorldObjectContaminationSnapshot([]);
        Evidence = new DiscoveryEvidenceSnapshot([]);
        CharacterObjectSabotageOccurrence occurrence = new(
            new CharacterObjectSabotageProposal(
                Character.Identity.Id,
                new CharacterActionAccessRequirement(
                    target.Identity.Id,
                    RequiredCapabilityKey,
                    Character.Knowledge.KnowledgeReferences.Single()),
                CharacterSabotageActionKind.Contaminate),
            target);
        Candidate = new CharacterObjectSabotageEvidence(
            occurrence,
            new DiscoveryEvidence(
                new EvidenceId(EvidenceIdText),
                target.Identity.Id,
                VisibilityState.Noticed,
                false));
    }
}
