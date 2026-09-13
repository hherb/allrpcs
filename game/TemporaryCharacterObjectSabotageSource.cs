using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Discovery;
using Allrpcs.Domain.Identifiers;
using Allrpcs.Domain.Perception;
using Allrpcs.Domain.WorldObjects;
using Godot;

namespace Allrpcs.Game;

/// <summary>Authors one exact temporary eligible enemy concealment candidate and initial state.</summary>
public partial class TemporaryCharacterObjectSabotageSource : Node
{
    private const string TargetObjectIdText = "village-water-cask";
    private const string TargetDisplayName = "Village water cask";
    private const string TargetDescription = "A covered cask beside the village well.";
    private const string RequiredCapabilityKey = "assess-local-influence";
    private const string EvidenceIdText = "disturbed-cask-cover";

    /// <summary>Gets the exact authored enemy character.</summary>
    public Character Character { get; private set; } = null!;

    /// <summary>Gets the exact authored physical-access snapshot.</summary>
    public CharacterPhysicalAccessSnapshot PhysicalAccess { get; private set; } = null!;

    /// <summary>Gets the exact authored authoritative world-object snapshot.</summary>
    public WorldObjectSnapshot WorldObjects { get; private set; } = null!;

    /// <summary>Gets the exact empty initial concealment snapshot.</summary>
    public WorldObjectConcealmentSnapshot Concealment { get; private set; } = null!;

    /// <summary>Gets the exact empty initial evidence snapshot.</summary>
    public DiscoveryEvidenceSnapshot Evidence { get; private set; } = null!;

    /// <summary>Gets the exact authored eligible concealment candidate.</summary>
    public CharacterObjectSabotageEvidence Candidate { get; private set; } = null!;

    /// <inheritdoc />
    public override void _Ready()
    {
        Character = RepresentativeEnemyAgentCharacterSource.Create();
        WorldObject target = new(
            new WorldObjectIdentity(new ObjectId(TargetObjectIdText), TargetDisplayName),
            new WorldObjectDescription(TargetDescription));
        PhysicalAccess = new CharacterPhysicalAccessSnapshot(
            Character.Identity.Id,
            [target.Identity.Id]);
        WorldObjects = new WorldObjectSnapshot([target]);
        Concealment = new WorldObjectConcealmentSnapshot([]);
        Evidence = new DiscoveryEvidenceSnapshot([]);
        CharacterObjectSabotageOccurrence occurrence = new(
            new CharacterObjectSabotageProposal(
                Character.Identity.Id,
                new CharacterActionAccessRequirement(
                    target.Identity.Id,
                    RequiredCapabilityKey,
                    Character.Knowledge.KnowledgeReferences.Single()),
                CharacterSabotageActionKind.Conceal),
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
