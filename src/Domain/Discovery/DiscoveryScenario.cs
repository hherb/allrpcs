using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Identifiers;
using Allrpcs.Domain.Perception;
using Allrpcs.Domain.WorldObjects;

namespace Allrpcs.Domain.Discovery;

/// <summary>
/// Composes the immutable inputs needed to validate one character's authored discovery scenario.
/// </summary>
public sealed class DiscoveryScenario
{
    /// <summary>Creates a scenario by retaining each supplied authored input exactly.</summary>
    public DiscoveryScenario(
        CharacterId observerCharacterId,
        WorldObjectSnapshot worldObjects,
        DiscoveryEvidenceSnapshot evidence,
        CharacterPerceptionSnapshot initialPerception,
        DiscoveryConditionSet conditions,
        CharacterCapabilitySet availableCapabilities,
        CharacterInventory availableInventory,
        CharacterKnowledge availableKnowledge,
        PassiveNoticingCandidateSnapshot passiveNoticingCandidates,
        ObservationReinterpretationCandidateSnapshot reinterpretationCandidates)
    {
        ArgumentNullException.ThrowIfNull(observerCharacterId);
        ArgumentNullException.ThrowIfNull(worldObjects);
        ArgumentNullException.ThrowIfNull(evidence);
        ArgumentNullException.ThrowIfNull(initialPerception);
        ArgumentNullException.ThrowIfNull(conditions);
        ArgumentNullException.ThrowIfNull(availableCapabilities);
        ArgumentNullException.ThrowIfNull(availableInventory);
        ArgumentNullException.ThrowIfNull(availableKnowledge);
        ArgumentNullException.ThrowIfNull(passiveNoticingCandidates);
        ArgumentNullException.ThrowIfNull(reinterpretationCandidates);

        ObserverCharacterId = observerCharacterId;
        WorldObjects = worldObjects;
        Evidence = evidence;
        InitialPerception = initialPerception;
        Conditions = conditions;
        AvailableCapabilities = availableCapabilities;
        AvailableInventory = availableInventory;
        AvailableKnowledge = availableKnowledge;
        PassiveNoticingCandidates = passiveNoticingCandidates;
        ReinterpretationCandidates = reinterpretationCandidates;
    }

    /// <summary>Gets the character whose authored discovery routes are validated.</summary>
    public CharacterId ObserverCharacterId { get; }

    /// <summary>Gets the authoritative world objects available to discovery rules.</summary>
    public WorldObjectSnapshot WorldObjects { get; }

    /// <summary>Gets the authored evidence and its visibility thresholds.</summary>
    public DiscoveryEvidenceSnapshot Evidence { get; }

    /// <summary>Gets the character's authored initial visibility state.</summary>
    public CharacterPerceptionSnapshot InitialPerception { get; }

    /// <summary>Gets the authored focused-search requirements.</summary>
    public DiscoveryConditionSet Conditions { get; }

    /// <summary>Gets all capabilities available through the bounded scenario.</summary>
    public CharacterCapabilitySet AvailableCapabilities { get; }

    /// <summary>Gets all tool objects available through the bounded scenario.</summary>
    public CharacterInventory AvailableInventory { get; }

    /// <summary>Gets all knowledge available through the bounded scenario.</summary>
    public CharacterKnowledge AvailableKnowledge { get; }

    /// <summary>Gets the authored passive-noticing routes.</summary>
    public PassiveNoticingCandidateSnapshot PassiveNoticingCandidates { get; }

    /// <summary>Gets the authored knowledge-triggered reinterpretation routes.</summary>
    public ObservationReinterpretationCandidateSnapshot ReinterpretationCandidates { get; }
}
