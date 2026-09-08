using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Identifiers;
using Allrpcs.Domain.Perception;
using Allrpcs.Domain.WorldObjects;

namespace Allrpcs.Domain.Discovery;

/// <summary>Authors the pure representative discovery scenario used by validation.</summary>
public static class RepresentativeDiscoveryScenarioSource
{
    private const string ObserverCharacterIdText = "seeker";
    private const string CapabilityKey = "identify-healing-plants";
    private const string TriggerKnowledgeIdText = "read-old-shrine-script";

    /// <summary>Creates one complete, immutable representative discovery scenario.</summary>
    public static DiscoveryScenario Create()
    {
        CharacterId observerCharacterId = new(ObserverCharacterIdText);
        WorldObject gardenHerb = CreateWorldObject(
            "garden-herb",
            "Visible herb",
            "A familiar healing herb growing openly in the garden.");
        WorldObject woodlandHerb = CreateWorldObject(
            "woodland-herb",
            "Concealed herb",
            "A healing herb concealed among woodland plants.");
        WorldObject shrineInscription = CreateWorldObject(
            "shrine-inscription",
            "Partial inscription",
            "A weathered inscription whose marks can acquire new meaning.");
        WorldObject streamResidue = CreateWorldObject(
            "stream-residue",
            "Residue",
            "A faint residue near the stream bank.");
        WorldObject wagonClue = CreateWorldObject(
            "wagon-clue",
            "Tool-gated clue",
            "A small clue on the abandoned wagon that needs close inspection.");
        WorldObject fieldLens = CreateWorldObject(
            "field-lens",
            "Field lens",
            "A portable lens for inspecting fine details.");
        WorldObjectSnapshot worldObjects = new(
        [
            gardenHerb,
            woodlandHerb,
            shrineInscription,
            streamResidue,
            wagonClue,
            fieldLens,
        ]);

        DiscoveryEvidenceSnapshot evidence = new(
        [
            CreateEvidence("visible-herb", gardenHerb, VisibilityState.Noticed, isCritical: false),
            CreateEvidence(
                "concealed-herb",
                woodlandHerb,
                VisibilityState.Identified,
                isCritical: true),
            CreateEvidence(
                "partial-inscription",
                shrineInscription,
                VisibilityState.Identified,
                isCritical: true),
            CreateEvidence("residue", streamResidue, VisibilityState.Noticed, isCritical: false),
            CreateEvidence(
                "tool-gated-clue",
                wagonClue,
                VisibilityState.Identified,
                isCritical: true),
        ]);
        CharacterPerceptionSnapshot initialPerception = new(
            observerCharacterId,
            [
                CreateVisibility(gardenHerb, VisibilityState.Noticed),
                CreateVisibility(woodlandHerb, VisibilityState.Unnoticed),
                CreateVisibility(shrineInscription, VisibilityState.Noticed),
                CreateVisibility(streamResidue, VisibilityState.Unnoticed),
                CreateVisibility(wagonClue, VisibilityState.Unnoticed),
                CreateVisibility(fieldLens, VisibilityState.Collected),
            ]);
        CharacterCapability capability = new(
            CapabilityKey,
            "Identifies healing plants during focused search.");
        KnowledgeId triggerKnowledgeId = new(TriggerKnowledgeIdText);

        return new DiscoveryScenario(
            observerCharacterId,
            worldObjects,
            evidence,
            initialPerception,
            new DiscoveryConditionSet(
                new DiscoveryCapabilityRequirementSet(
                [
                    new DiscoveryCapabilityRequirement(
                        woodlandHerb.Identity.Id,
                        capability.Key),
                ]),
                new DiscoveryToolRequirementSet(
                [
                    new DiscoveryToolRequirement(wagonClue.Identity.Id, fieldLens.Identity.Id),
                ])),
            new CharacterCapabilitySet([capability]),
            new CharacterInventory([fieldLens.Identity.Id]),
            new CharacterKnowledge([triggerKnowledgeId]),
            new PassiveNoticingCandidateSnapshot(
            [
                new PassiveNoticingCandidate(observerCharacterId, streamResidue.Identity.Id),
            ]),
            new ObservationReinterpretationCandidateSnapshot(
            [
                new ObservationReinterpretationCandidate(
                    observerCharacterId,
                    shrineInscription.Identity.Id,
                    triggerKnowledgeId),
            ]));
    }

    private static WorldObject CreateWorldObject(string id, string name, string description) =>
        new(new WorldObjectIdentity(new ObjectId(id), name), new WorldObjectDescription(description));

    private static DiscoveryEvidence CreateEvidence(
        string evidenceId,
        WorldObject worldObject,
        VisibilityState minimumVisibilityState,
        bool isCritical) => new(
            new EvidenceId(evidenceId),
            worldObject.Identity.Id,
            minimumVisibilityState,
            isCritical);

    private static WorldObjectVisibility CreateVisibility(
        WorldObject worldObject,
        VisibilityState visibilityState) => new(worldObject.Identity.Id, visibilityState);
}
