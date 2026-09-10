using Allrpcs.Application.Dialogue;
using Allrpcs.Domain.Dialogue;
using Allrpcs.Domain.Identifiers;
using Allrpcs.Domain.Knowledge;
using Allrpcs.Domain.Perception;
using Allrpcs.Domain.WorldObjects;
using Godot;

namespace Allrpcs.Game;

/// <summary>Builds bounded provider-neutral input for an exact temporary healer turn.</summary>
public partial class TemporaryCharacterModelGenerationInputSource : Node
{
    private const string ReferencedKnowledgeText =
        "The healer knows the herb patch must be examined before any collection decision.";
    private const string UnreferencedKnowledgeIdText = "private-village-secret";
    private const string UnreferencedKnowledgeText =
        "A secret that must remain unavailable to this healer.";

    private CharacterIntentionAffordanceSnapshot affordances = null!;
    private TemporaryHealerCharacterSource healerSource = null!;
    private KnowledgeSnapshot knowledge = null!;
    private CharacterPerceptionSnapshot perception = null!;
    private WorldObjectSnapshot worldObjects = null!;

    /// <inheritdoc />
    public override void _Ready()
    {
        healerSource = GetNode<TemporaryHealerCharacterSource>(
            "../TemporaryHealerCharacterSource");
        worldObjects = GetNode<TemporaryWorldObjectSource>(
            "../TemporaryWorldObjectSource").WorldObjects;

        ObjectId identifiedObjectId = worldObjects.Entries[0].Identity.Id;
        perception = new CharacterPerceptionSnapshot(
            healerSource.Character.Identity.Id,
            worldObjects.Entries.Select(
                worldObject => new WorldObjectVisibility(
                    worldObject.Identity.Id,
                    ReferenceEquals(worldObject.Identity.Id, identifiedObjectId)
                        ? VisibilityState.Identified
                        : VisibilityState.Unnoticed)));

        KnowledgeId referencedKnowledgeId =
            healerSource.Character.Knowledge.KnowledgeReferences[0];
        knowledge = new KnowledgeSnapshot(
            [
                new KnowledgeEntry(referencedKnowledgeId, ReferencedKnowledgeText),
                new KnowledgeEntry(
                    new KnowledgeId(UnreferencedKnowledgeIdText),
                    UnreferencedKnowledgeText),
            ]);

        affordances = new CharacterIntentionAffordanceSnapshot(
            [
                new CharacterIntentionAffordance(
                    CharacterIntentionType.ExamineObject,
                    identifiedObjectId),
            ]);
    }

    /// <summary>
    /// Returns bounded input only for an exact awaiting-response turn owned by the authored healer.
    /// </summary>
    public TemporaryCharacterModelGenerationInput? CreateInput(CharacterTurnActivity activity)
    {
        ArgumentNullException.ThrowIfNull(activity);

        if (!ReferenceEquals(activity.State, CharacterTurnActivityState.AwaitingResponse)
            || !ReferenceEquals(
                activity.Request.RespondingCharacterId,
                healerSource.Character.Identity.Id))
        {
            return null;
        }

        CharacterModelGenerationRequest generationRequest =
            CharacterModelContextAssembler.Assemble(
                activity.Request,
                healerSource.Character,
                perception,
                worldObjects,
                knowledge,
                [],
                [activity.Request.DialogueEvent],
                affordances);

        return new TemporaryCharacterModelGenerationInput(
            generationRequest,
            worldObjects,
            affordances);
    }
}
