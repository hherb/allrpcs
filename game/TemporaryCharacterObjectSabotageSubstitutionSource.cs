using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Discovery;
using Allrpcs.Domain.Identifiers;
using Allrpcs.Domain.Perception;
using Allrpcs.Domain.WorldObjects;
using Godot;

namespace Allrpcs.Game;

/// <summary>Authors one exact temporary eligible enemy substitution candidate and initial state.</summary>
public partial class TemporaryCharacterObjectSabotageSubstitutionSource : Node
{
    private const int WaterCaskWorldObjectIndex = 2;
    private const string RequiredCapabilityKey = "assess-local-influence";
    private const string EvidenceIdText = "water-cask-substitution-signs";
    private const string ReplacementObjectIdText = "tainted-water-cask";
    private const string ReplacementDisplayName = "Tainted water cask";
    private const string ReplacementDescription =
        "A matching covered cask contains water with an unfamiliar residue.";

    /// <summary>Gets the exact authored enemy character.</summary>
    public Character Character { get; private set; } = null!;

    /// <summary>Gets the exact authored physical-access snapshot.</summary>
    public CharacterPhysicalAccessSnapshot PhysicalAccess { get; private set; } = null!;

    /// <summary>Gets the exact existing authoritative world-object snapshot.</summary>
    public WorldObjectSnapshot WorldObjects { get; private set; } = null!;

    /// <summary>Gets the exact empty initial substitution snapshot.</summary>
    public CharacterObjectSabotageSubstitutionSnapshot Substitutions { get; private set; } = null!;

    /// <summary>Gets the exact empty initial evidence snapshot.</summary>
    public DiscoveryEvidenceSnapshot Evidence { get; private set; } = null!;

    /// <summary>Gets the exact authored eligible substitution candidate.</summary>
    public CharacterObjectSabotageSubstitutionCandidate Candidate { get; private set; } = null!;

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
        Substitutions = new CharacterObjectSabotageSubstitutionSnapshot([]);
        Evidence = new DiscoveryEvidenceSnapshot([]);
        CharacterObjectSabotageOccurrence occurrence = new(
            new CharacterObjectSabotageProposal(
                Character.Identity.Id,
                new CharacterActionAccessRequirement(
                    target.Identity.Id,
                    RequiredCapabilityKey,
                    Character.Knowledge.KnowledgeReferences.Single()),
                CharacterSabotageActionKind.Substitute),
            target);
        WorldObject replacement = new(
            new WorldObjectIdentity(
                new ObjectId(ReplacementObjectIdText),
                ReplacementDisplayName),
            new WorldObjectDescription(ReplacementDescription));
        CharacterObjectSabotageSubstitution substitution = new(occurrence, replacement);
        CharacterObjectSabotageEvidence evidence = new(
            occurrence,
            new DiscoveryEvidence(
                new EvidenceId(EvidenceIdText),
                target.Identity.Id,
                VisibilityState.Noticed,
                false));
        Candidate = new CharacterObjectSabotageSubstitutionCandidate(substitution, evidence);
    }
}
