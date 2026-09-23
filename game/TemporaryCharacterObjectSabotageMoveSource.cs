using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Discovery;
using Allrpcs.Domain.Identifiers;
using Allrpcs.Domain.Locations;
using Allrpcs.Domain.Perception;
using Allrpcs.Domain.WorldObjects;
using Godot;

namespace Allrpcs.Game;

/// <summary>Authors exact temporary initial and follow-up enemy move candidates.</summary>
public partial class TemporaryCharacterObjectSabotageMoveSource : Node
{
    private const string RequiredCapabilityKey = "assess-local-influence";
    private const string EvidenceIdText = "cask-drag-marks";
    private const string FollowUpEvidenceIdText = "cask-wheel-ruts";
    private const string CycleEvidenceIdText = "cask-spoke-scuffs";
    private const string SourceLocationIdText = "village-square";
    private const string SourceLocationDisplayName = "Village square";
    private const string SourceLocationDescription = "The cask stands beside the village well.";
    private const string DestinationLocationIdText = "abandoned-wagon";
    private const string DestinationLocationDisplayName = "Abandoned wagon";
    private const string DestinationLocationDescription =
        "A weathered wagon rests beyond the village square.";

    /// <summary>Gets the exact authored enemy character.</summary>
    public Character Character { get; private set; } = null!;

    /// <summary>Gets the exact authored physical-access snapshot.</summary>
    public CharacterPhysicalAccessSnapshot PhysicalAccess { get; private set; } = null!;

    /// <summary>Gets the exact existing authoritative world-object snapshot.</summary>
    public WorldObjectSnapshot WorldObjects { get; private set; } = null!;

    /// <summary>Gets the exact authored source location.</summary>
    public Location SourceLocation { get; private set; } = null!;

    /// <summary>Gets the exact authored move destination.</summary>
    public Location Destination { get; private set; } = null!;

    /// <summary>Gets the exact initial target placement snapshot.</summary>
    public WorldObjectPlacementSnapshot Placements { get; private set; } = null!;

    /// <summary>Gets the exact empty initial evidence snapshot.</summary>
    public DiscoveryEvidenceSnapshot Evidence { get; private set; } = null!;

    /// <summary>Gets the exact authored eligible move candidate.</summary>
    public CharacterObjectSabotageMoveCandidate Candidate { get; private set; } = null!;

    /// <summary>Gets one fresh exact candidate for a later admitted move-authoring seam.</summary>
    public CharacterObjectSabotageMoveCandidate FollowUpCandidate { get; private set; } = null!;

    /// <summary>Gets one fresh exact candidate for the next admitted sabotage cycle.</summary>
    public CharacterObjectSabotageMoveCandidate CycleCandidate { get; private set; } = null!;

    /// <summary>Gets the exact aligned move-recovery candidate.</summary>
    public CharacterObjectSabotageMoveRecoveryCandidate RecoveryCandidate { get; private set; } = null!;

    /// <inheritdoc />
    public override void _Ready()
    {
        Character = RepresentativeEnemyAgentCharacterSource.Create();
        WorldObjects = GetNode<TemporaryWorldObjectSource>("../TemporaryWorldObjectSource").WorldObjects;
        WorldObject target = WorldObjects.Entries[^1];
        PhysicalAccess = new CharacterPhysicalAccessSnapshot(
            Character.Identity.Id,
            [target.Identity.Id]);
        SourceLocation = CreateLocation(
            SourceLocationIdText,
            SourceLocationDisplayName,
            SourceLocationDescription);
        Destination = CreateLocation(
            DestinationLocationIdText,
            DestinationLocationDisplayName,
            DestinationLocationDescription);
        WorldObjectPlacement sourcePlacement = new(target, SourceLocation);
        Placements = new WorldObjectPlacementSnapshot([sourcePlacement]);
        Evidence = new DiscoveryEvidenceSnapshot([]);
        CharacterObjectSabotageOccurrence occurrence = new(
            new CharacterObjectSabotageProposal(
                Character.Identity.Id,
                new CharacterActionAccessRequirement(
                    target.Identity.Id,
                    RequiredCapabilityKey,
                    Character.Knowledge.KnowledgeReferences.Single()),
                CharacterSabotageActionKind.Move),
            target);
        CharacterObjectSabotageMoveDestination destination = new(occurrence, Destination);
        CharacterObjectSabotageEvidence evidence = new(
            occurrence,
            new DiscoveryEvidence(
                new EvidenceId(EvidenceIdText),
                target.Identity.Id,
                VisibilityState.Noticed,
                false));
        Candidate = new CharacterObjectSabotageMoveCandidate(destination, evidence);
        CharacterObjectSabotageEvidence followUpEvidence = new(
            occurrence,
            new DiscoveryEvidence(
                new EvidenceId(FollowUpEvidenceIdText),
                target.Identity.Id,
                VisibilityState.Noticed,
                false));
        FollowUpCandidate = new CharacterObjectSabotageMoveCandidate(destination, followUpEvidence);
        CharacterObjectSabotageEvidence cycleEvidence = new(
            occurrence,
            new DiscoveryEvidence(
                new EvidenceId(CycleEvidenceIdText),
                target.Identity.Id,
                VisibilityState.Noticed,
                false));
        CycleCandidate = new CharacterObjectSabotageMoveCandidate(destination, cycleEvidence);
        RecoveryCandidate = new CharacterObjectSabotageMoveRecoveryCandidate(
            Candidate,
            sourcePlacement);
    }

    private static Location CreateLocation(
        string idText,
        string displayName,
        string description) =>
        new(
            new LocationIdentity(new LocationId(idText), displayName),
            new LocationDescription(description));
}
