using Allrpcs.Application.Input;
using Allrpcs.Domain.Discovery;
using Allrpcs.Domain.Identifiers;
using Allrpcs.Domain.Perception;
using Allrpcs.Domain.WorldEvents;
using Allrpcs.Domain.WorldObjects;
using Godot;

namespace Allrpcs.Game;

/// <summary>Forwards each focused-search input port to its Main-scene owners.</summary>
internal sealed class FocusedSearchInputNodePorts(
    FocusedSearchActivityController searches,
    InteractionFocusController interactionFocus,
    TemporaryFocusedSearchContextSource context,
    InventoryController inventory,
    TemporaryFocusedSearchOutcomeSource outcomeSource,
    WorldObjectController worldObjects,
    TemporaryWorldObjectSource worldObjectSource,
    WorldEventController worldEvents,
    TemporaryWorldEventSource worldEventSource,
    OpeningRegionWagonClueFocusedSearchRoutingCoordinator wagonRouting,
    TemporaryCharacterObjectSabotageSource sabotage,
    TemporaryCharacterObjectSabotagePresentationFlowController sabotageFlow,
    TemporaryCharacterObjectSabotageMoveSource moveSabotage,
    TemporaryCharacterObjectSabotageMovePresentationFlowController moveSabotageFlow,
    TemporaryCharacterObjectSabotageSubstitutionSource substitutionSabotage,
    TemporaryCharacterObjectSabotageSubstitutionPresentationFlowController substitutionSabotageFlow,
    TemporaryCharacterObjectSabotageContaminationSource contaminationSabotage,
    TemporaryCharacterObjectSabotageContaminationPresentationFlowController contaminationSabotageFlow,
    SeekerWalkAnimationPresenter seekerAnimation,
    Label resultStatus,
    Action refreshResolvedPerception)
    : IFocusedSearchInputPorts
{
    public CharacterId SearcherCharacterId => searches.SearcherCharacterId;

    public FocusedSearchActivitySnapshot Activities => searches.Activities;

    public FocusedSearchOutcomeSnapshot Outcomes => searches.Outcomes;

    public CharacterPerceptionSnapshot? Perception => searches.Perception;

    public ObjectId? FocusedTargetObjectId => interactionFocus.FocusedTargetObjectId;

    public WorldObject AuthoredHerbPatch => worldObjectSource.WorldObjects.Entries[0];

    public WorldObject WagonClue => wagonRouting.WagonClue;

    public WorldEvent HerbPatchIdentifiedWorldEvent => worldEventSource.HerbPatchIdentifiedWorldEvent;

    public WorldEventSnapshot? WorldEvents => worldEvents.WorldEvents;

    public WorldEventLogSnapshot? EventLog => worldEvents.EventLog;

    public WorldTimeSnapshot? WorldTime => worldEvents.WorldTime;

    public IReadOnlyList<FocusedSearchEvidenceRecovery> EvidenceRecoveries =>
    [
        new(sabotage.Candidate.Evidence.ObjectId, sabotageFlow.TryRecover),
        new(moveSabotage.Candidate.Evidence.Evidence.ObjectId, moveSabotageFlow.TryRecover),
        new(substitutionSabotage.Candidate.Evidence.Evidence.ObjectId, substitutionSabotageFlow.TryRecover),
        new(contaminationSabotage.Candidate.Evidence.ObjectId, contaminationSabotageFlow.TryRecover),
    ];

    public WorldObject? FindWorldObject(ObjectId objectId) => worldObjects.FindWorldObject(objectId);

    public bool TryCancelSearch(FocusedSearchRequest request) => searches.TryCancelSearch(request);

    public FocusedSearchOutcome? CreateOutcome(FocusedSearchRequest request, WorldObject? worldObject) =>
        outcomeSource.CreateOutcome(request, worldObject);

    public bool TryCompleteSearch(FocusedSearchOutcome outcome) =>
        searches.TryCompleteSearchWithOutcome(outcome);

    public bool TryResolveOutcome(FocusedSearchOutcome outcome, WorldObject worldObject) =>
        searches.TryResolveOutcome(outcome, worldObject);

    public bool TryRouteWagonClue(
        FocusedSearchOutcome outcome,
        WorldObject worldObject,
        CharacterPerceptionSnapshot resolvedPerception) =>
        wagonRouting.TryRouteResolvedOutcome(outcome, worldObject, resolvedPerception);

    public bool TryOccurWorldEvent(WorldEventId eventId) => worldEvents.TryOccur(eventId);

    public bool TryStartFocusedSearch(WorldObject worldObject) =>
        interactionFocus.TryStartFocusedSearch(
            worldObject,
            context.Conditions,
            context.Capabilities,
            inventory.InventoryOf(searches.SearcherCharacterId));

    public void RefreshResolvedPerception() => refreshResolvedPerception();

    public void PresentSuccessfulInteraction() => seekerAnimation.PresentSuccessfulInteraction();

    public void PresentResult(string text) => resultStatus.Text = text;
}
