using Allrpcs.Application.Dialogue;
using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Dialogue;
using Allrpcs.Domain.Identifiers;
using Allrpcs.Domain.Knowledge;
using Allrpcs.Domain.Perception;
using Allrpcs.Domain.WorldObjects;
using Godot;

namespace Allrpcs.Game;

/// <summary>Builds bounded provider-neutral input for an exact ordinary participant turn.</summary>
public partial class TemporaryCharacterModelGenerationInputSource : Node
{
    private const string ReferencedKnowledgeText =
        "The healer knows the herb patch must be examined before any collection decision.";
    private const string UnreferencedKnowledgeIdText = "private-village-secret";
    private const string UnreferencedKnowledgeText =
        "A secret that must remain unavailable to this healer.";
    private const string SageKnowledgeText =
        "The sage knows a village oral history about comparing conflicting accounts.";

    private IReadOnlyDictionary<CharacterId, CharacterIntentionAffordanceSnapshot> affordances =
        null!;
    private CharacterMemoryController memoryController = null!;
    private KnowledgeSnapshot knowledge = null!;
    private TemporaryOrdinaryCharacterParticipantSource participantSource = null!;
    private IReadOnlyDictionary<CharacterId, CharacterPerceptionSnapshot> perceptions = null!;
    private WorldObjectSnapshot worldObjects = null!;
    private DialogueEventController dialogue = null!;
    private CharacterTurnTranscriptController transcript = null!;
    private SaveSnapshotRestorationController restoration = null!;
    private CharacterTurnRequest? invalidatedRequest;
    internal CharacterModelContextAssemblyStatus? LastAssemblyStatus { get; private set; }

    internal bool IsCurrent(CharacterTurnRequest request) => !ReferenceEquals(request, invalidatedRequest);

    internal PreparedRestore PrepareSessionReset()
    {
        CharacterTurnRequest? request = GetNode<CharacterTurnActivityController>(
            "../CharacterTurnActivityController").Activity?.Request;
        return new PreparedRestore(() => invalidatedRequest = request ?? invalidatedRequest);
    }

    private void OnRestored()
    {
        invalidatedRequest = GetNode<CharacterTurnActivityController>("../CharacterTurnActivityController")
            .Activity?.Request ?? invalidatedRequest;
    }

    /// <inheritdoc />
    public override void _ExitTree() => restoration.PresentationReset -= OnRestored;

    /// <inheritdoc />
    public override void _Ready()
    {
        participantSource = GetNode<TemporaryOrdinaryCharacterParticipantSource>(
            "../TemporaryOrdinaryCharacterParticipantSource");
        memoryController = GetNode<CharacterMemoryController>("../CharacterMemoryController");
        dialogue = GetNode<DialogueEventController>("../DialogueEventController");
        transcript = GetNode<CharacterTurnTranscriptController>("../CharacterTurnTranscriptController");
        restoration = GetNode<SaveSnapshotRestorationController>("../SaveSnapshotRestorationController");
        restoration.PresentationReset += OnRestored;
        worldObjects = GetNode<TemporaryWorldObjectSource>(
            "../TemporaryWorldObjectSource").WorldObjects;

        Character healer = participantSource.Participants[0];
        Character sage = participantSource.Participants[1];
        ObjectId identifiedObjectId = worldObjects.Entries[0].Identity.Id;
        perceptions = new Dictionary<CharacterId, CharacterPerceptionSnapshot>
        {
            [healer.Identity.Id] = new CharacterPerceptionSnapshot(
                healer.Identity.Id,
                worldObjects.Entries.Select(
                    worldObject => new WorldObjectVisibility(
                        worldObject.Identity.Id,
                        ReferenceEquals(worldObject.Identity.Id, identifiedObjectId)
                            ? VisibilityState.Identified
                            : VisibilityState.Unnoticed))),
            [sage.Identity.Id] = new CharacterPerceptionSnapshot(
                sage.Identity.Id,
                worldObjects.Entries.Select(worldObject => new WorldObjectVisibility(
                    worldObject.Identity.Id,
                    VisibilityState.Unnoticed))),
        };

        KnowledgeId referencedKnowledgeId =
            healer.Knowledge.KnowledgeReferences[0];
        knowledge = new KnowledgeSnapshot(
            [
                new KnowledgeEntry(referencedKnowledgeId, ReferencedKnowledgeText),
                new KnowledgeEntry(sage.Knowledge.KnowledgeReferences[0], SageKnowledgeText),
                new KnowledgeEntry(
                    new KnowledgeId(UnreferencedKnowledgeIdText),
                    UnreferencedKnowledgeText),
            ]);

        affordances = new Dictionary<CharacterId, CharacterIntentionAffordanceSnapshot>
        {
            [healer.Identity.Id] = new CharacterIntentionAffordanceSnapshot(
                [
                    new CharacterIntentionAffordance(
                        CharacterIntentionType.ExamineObject,
                        identifiedObjectId),
                ]),
            [sage.Identity.Id] = new CharacterIntentionAffordanceSnapshot([]),
        };
    }

    /// <summary>
    /// Returns bounded input only for an exact awaiting-response turn owned by the ordinary roster.
    /// </summary>
    public TemporaryCharacterModelGenerationInput? CreateInput(CharacterTurnActivity activity)
    {
        ArgumentNullException.ThrowIfNull(activity);
        LastAssemblyStatus = null;

        Character? character = CharacterTurnParticipantLookup.Find(
            participantSource.Participants,
            activity);
        if (character is null)
        {
            return null;
        }

        CharacterId characterId = character.Identity.Id;
        CharacterIntentionAffordanceSnapshot characterAffordances = affordances[characterId];
        IReadOnlyList<CharacterModelContextEntry> actionOutcomes = transcript.Transcript is { } recorded
            ? CharacterModelExaminationContextProjection.Project(characterId,
                GetNodeOrNull<HerbExaminationActionController>("../HerbExaminationActionController")?.LastResult,
                recorded)
            : [];
        LastAssemblyStatus = CharacterModelContextAssembler.TryAssemble(
            activity.Request,
            character,
            perceptions[characterId],
            worldObjects,
            knowledge,
            CharacterModelCommitmentMemoryContextProjection.Project(
                characterId,
                memoryController.Memory),
            dialogue.History.RecentFor(characterId, activity.Request.DialogueEvent),
            characterAffordances,
            out CharacterModelGenerationRequest? generationRequest,
            actionOutcomes);
        if (LastAssemblyStatus == CharacterModelContextAssemblyStatus.RequiredContextBudgetExceeded) { return null; }

        return new TemporaryCharacterModelGenerationInput(
            generationRequest!,
            worldObjects,
            characterAffordances);
    }
}
