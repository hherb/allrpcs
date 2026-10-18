using Allrpcs.Domain.Discovery;
using Allrpcs.Domain.Identifiers;
using Allrpcs.Domain.Perception;
using Allrpcs.Domain.WorldEvents;
using Allrpcs.Domain.WorldObjects;

namespace Allrpcs.Application.Input;

/// <summary>
/// The native owners a <see cref="FocusedSearchInputCoordinator"/> sequences. Each member
/// is a thin forward to existing node members; none decides anything.
/// </summary>
public interface IFocusedSearchInputPorts
{
    /// <summary>Gets the Seeker's searcher identity.</summary>
    CharacterId SearcherCharacterId { get; }

    /// <summary>Gets the live focused-search activities.</summary>
    FocusedSearchActivitySnapshot Activities { get; }

    /// <summary>Gets the retained focused-search outcomes.</summary>
    FocusedSearchOutcomeSnapshot Outcomes { get; }

    /// <summary>Gets the Seeker's current perception.</summary>
    CharacterPerceptionSnapshot? Perception { get; }

    /// <summary>Gets the focused interaction target, if any.</summary>
    ObjectId? FocusedTargetObjectId { get; }

    /// <summary>Gets the authored herb patch whose identification advances world time.</summary>
    WorldObject AuthoredHerbPatch { get; }

    /// <summary>Gets the exact opening-region wagon clue.</summary>
    WorldObject WagonClue { get; }

    /// <summary>Gets the world event the herb-patch identification occurs.</summary>
    WorldEvent HerbPatchIdentifiedWorldEvent { get; }

    /// <summary>Gets the authoritative world events.</summary>
    WorldEventSnapshot? WorldEvents { get; }

    /// <summary>Gets the authoritative world-event log.</summary>
    WorldEventLogSnapshot? EventLog { get; }

    /// <summary>Gets the authoritative world time.</summary>
    WorldTimeSnapshot? WorldTime { get; }

    /// <summary>Gets the current sabotage recoveries, read when a search resolves.</summary>
    IReadOnlyList<FocusedSearchEvidenceRecovery> EvidenceRecoveries { get; }

    /// <summary>Finds one live world object.</summary>
    WorldObject? FindWorldObject(ObjectId objectId);

    /// <summary>Cancels one active search.</summary>
    bool TryCancelSearch(FocusedSearchRequest request);

    /// <summary>Creates the authored outcome for one active search.</summary>
    FocusedSearchOutcome? CreateOutcome(FocusedSearchRequest request, WorldObject? worldObject);

    /// <summary>Completes one active search with its outcome.</summary>
    bool TryCompleteSearch(FocusedSearchOutcome outcome);

    /// <summary>Resolves one retained outcome against its world object.</summary>
    bool TryResolveOutcome(FocusedSearchOutcome outcome, WorldObject worldObject);

    /// <summary>Routes a resolved wagon-clue outcome.</summary>
    bool TryRouteWagonClue(
        FocusedSearchOutcome outcome,
        WorldObject worldObject,
        CharacterPerceptionSnapshot resolvedPerception);

    /// <summary>Occurs one world event.</summary>
    bool TryOccurWorldEvent(WorldEventId eventId);

    /// <summary>Starts a focused search of one focused world object with the authored context.</summary>
    bool TryStartFocusedSearch(WorldObject worldObject);

    /// <summary>Refreshes the resolved-perception indicators.</summary>
    void RefreshResolvedPerception();

    /// <summary>Plays the Seeker's successful-interaction presentation.</summary>
    void PresentSuccessfulInteraction();

    /// <summary>Shows the focused-search result status.</summary>
    void PresentResult(string text);
}
