using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Discovery;
using Allrpcs.Domain.Identifiers;
using Allrpcs.Domain.Perception;
using Allrpcs.Domain.WorldObjects;
using Godot;

namespace Allrpcs.Game;

/// <summary>
/// Owns the Seeker's live focused-search activity, outcome, and perception snapshots and presents
/// successful activity transitions.
/// </summary>
public partial class FocusedSearchActivityController : Node
{
    private FocusedSearchActivityIndicator indicator = null!;

    /// <summary>Gets or sets the Seeker's authored stable character identifier text.</summary>
    [Export]
    public string SearcherCharacterIdText { get; set; } = string.Empty;

    /// <summary>Gets the Seeker identifier owned by this controller.</summary>
    public CharacterId SearcherCharacterId { get; private set; } = null!;

    /// <summary>Gets the current immutable focused-search activity snapshot.</summary>
    public FocusedSearchActivitySnapshot Activities { get; private set; } = new([]);

    /// <summary>Gets the current immutable focused-search outcome snapshot.</summary>
    public FocusedSearchOutcomeSnapshot Outcomes { get; private set; } = new([]);

    /// <summary>Gets the current immutable passive-noticing candidate snapshot.</summary>
    public PassiveNoticingCandidateSnapshot PassiveNoticingCandidates { get; private set; } =
        new([]);

    /// <summary>Gets the current immutable observation-reinterpretation candidate snapshot.</summary>
    public ObservationReinterpretationCandidateSnapshot ObservationReinterpretationCandidates
    {
        get;
        private set;
    } = new([]);

    /// <summary>Gets the Seeker's initialized immutable perception snapshot, if any.</summary>
    public CharacterPerceptionSnapshot? Perception { get; private set; }

    /// <inheritdoc />
    public override void _Ready()
    {
        SearcherCharacterId = CharacterId.Parse(SearcherCharacterIdText);
        indicator = GetNode<FocusedSearchActivityIndicator>(
            "../FocusedSearchActivityIndicator");
        Activities = new FocusedSearchActivitySnapshot([]);
        Outcomes = new FocusedSearchOutcomeSnapshot([]);
        PassiveNoticingCandidates = new PassiveNoticingCandidateSnapshot([]);
        ObservationReinterpretationCandidates =
            new ObservationReinterpretationCandidateSnapshot([]);
        Perception = null;
        indicator.Present(SearcherCharacterId, Activities);
    }

    /// <summary>
    /// Produces one observation-reinterpretation candidate for this controller's Seeker when its
    /// character and world object already exist.
    /// </summary>
    public bool TryProduceObservationReinterpretationCandidate(
        ObservationReinterpretationCandidate candidate,
        Character character,
        WorldObject worldObject)
    {
        ArgumentNullException.ThrowIfNull(candidate);
        ArgumentNullException.ThrowIfNull(character);
        ArgumentNullException.ThrowIfNull(worldObject);

        if (candidate.ObserverCharacterId != SearcherCharacterId
            || !ObservationReinterpretationCandidateProductionPolicy.CanProduce(
                candidate,
                character,
                worldObject,
                ObservationReinterpretationCandidates))
        {
            return false;
        }

        ObservationReinterpretationCandidates =
            ObservationReinterpretationCandidateProductionTransition.Produce(
                candidate,
                character,
                worldObject,
                ObservationReinterpretationCandidates);
        return true;
    }

    /// <summary>
    /// Resolves the exact first eligible retained observation-reinterpretation candidate for this
    /// controller's Seeker against its owned perception and the supplied complete character.
    /// </summary>
    public bool TryResolveNextObservationReinterpretationCandidate(Character character)
    {
        ArgumentNullException.ThrowIfNull(character);

        if (character.Identity.Id != SearcherCharacterId
            || Perception is null
            || ObservationReinterpretationCandidateResolutionSelection.SelectNext(
                character,
                Perception,
                ObservationReinterpretationCandidates) is null)
        {
            return false;
        }

        ObservationReinterpretationCandidateResolutionResult result =
            ObservationReinterpretationCandidateResolutionSequence.ResolveNext(
                character,
                Perception,
                ObservationReinterpretationCandidates);
        Perception = result.Perception;
        ObservationReinterpretationCandidates = result.Candidates;
        return true;
    }

    /// <summary>
    /// Produces one candidate for this controller's Seeker when its character and world object
    /// already exist.
    /// </summary>
    public bool TryProducePassiveNoticingCandidate(
        PassiveNoticingCandidate candidate,
        Character character,
        WorldObject worldObject)
    {
        ArgumentNullException.ThrowIfNull(candidate);
        ArgumentNullException.ThrowIfNull(character);
        ArgumentNullException.ThrowIfNull(worldObject);

        if (candidate.ObserverCharacterId != SearcherCharacterId
            || !PassiveNoticingCandidateProductionPolicy.CanProduce(
                candidate,
                character,
                worldObject,
                PassiveNoticingCandidates))
        {
            return false;
        }

        PassiveNoticingCandidates = PassiveNoticingCandidateProductionTransition.Produce(
            candidate,
            character,
            worldObject,
            PassiveNoticingCandidates);
        return true;
    }

    /// <summary>
    /// Resolves the exact first eligible retained passive-noticing candidate for this controller's
    /// Seeker against its owned perception.
    /// </summary>
    public bool TryResolveNextPassiveNoticingCandidate()
    {
        if (Perception is null
            || PassiveNoticingCandidateResolutionSelection.SelectNext(
                SearcherCharacterId,
                Perception,
                PassiveNoticingCandidates) is null)
        {
            return false;
        }

        PassiveNoticingCandidateResolutionResult result =
            PassiveNoticingCandidateResolutionSequence.ResolveNext(
                SearcherCharacterId,
                Perception,
                PassiveNoticingCandidates);
        Perception = result.Perception;
        PassiveNoticingCandidates = result.Candidates;
        return true;
    }

    /// <summary>
    /// Initializes this controller's perception once from an existing Seeker snapshot.
    /// </summary>
    public bool TryInitializePerception(CharacterPerceptionSnapshot perception)
    {
        ArgumentNullException.ThrowIfNull(perception);

        if (perception.CharacterId != SearcherCharacterId || Perception is not null)
        {
            return false;
        }

        Perception = perception;
        return true;
    }

    /// <summary>
    /// Starts an owned focused search when its authored discovery conditions are satisfied.
    /// </summary>
    public bool TryStartSearch(
        FocusedSearchRequest request,
        DiscoveryConditionSet conditions,
        CharacterCapabilitySet capabilities,
        CharacterInventory inventory)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (!IsRequestForSearcher(request)
            || !FocusedSearchEligibilityPolicy.CanStartSearch(
                request,
                Activities,
                conditions,
                capabilities,
                inventory))
        {
            return false;
        }

        ReplaceAndPresent(FocusedSearchStartTransition.StartSearch(request, Activities));
        return true;
    }

    /// <summary>Cancels an active focused search for this controller's Seeker.</summary>
    public bool TryCancelSearch(FocusedSearchRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (!IsRequestForSearcher(request)
            || !FocusedSearchCancellationPolicy.CanCancelSearch(request, Activities))
        {
            return false;
        }

        ReplaceAndPresent(FocusedSearchCancellationTransition.CancelSearch(request, Activities));
        return true;
    }

    /// <summary>Completes an active focused search for this controller's Seeker.</summary>
    public bool TryCompleteSearch(FocusedSearchRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (!IsRequestForSearcher(request)
            || !FocusedSearchCompletionPolicy.CanCompleteSearch(request, Activities))
        {
            return false;
        }

        ReplaceAndPresent(FocusedSearchCompletionTransition.CompleteSearch(request, Activities));
        return true;
    }

    /// <summary>
    /// Completes an active focused search with an outcome for this controller's Seeker.
    /// </summary>
    public bool TryCompleteSearchWithOutcome(FocusedSearchOutcome outcome)
    {
        ArgumentNullException.ThrowIfNull(outcome);

        if (!IsRequestForSearcher(outcome.Request)
            || !FocusedSearchOutcomeCompletionPolicy.CanCompleteWithOutcome(
                outcome,
                Activities,
                Outcomes))
        {
            return false;
        }

        FocusedSearchOutcomeCompletionResult result =
            FocusedSearchOutcomeCompletionTransition.CompleteWithOutcome(
                outcome,
                Activities,
                Outcomes);
        Activities = result.Activities;
        Outcomes = result.Outcomes;
        indicator.Present(SearcherCharacterId, Activities);
        return true;
    }

    /// <summary>
    /// Resolves one recorded outcome for its exact existing world object against this controller's
    /// initialized Seeker perception.
    /// </summary>
    public bool TryResolveOutcome(FocusedSearchOutcome outcome, WorldObject worldObject)
    {
        ArgumentNullException.ThrowIfNull(outcome);
        ArgumentNullException.ThrowIfNull(worldObject);

        if (Perception is null
            || !WorldObjectBoundFocusedSearchOutcomeResolutionPolicy.CanResolve(
                outcome,
                worldObject,
                Perception,
                Outcomes))
        {
            return false;
        }

        FocusedSearchOutcomeResolutionResult result =
            WorldObjectBoundFocusedSearchOutcomeResolutionTransition.Resolve(
                outcome,
                worldObject,
                Perception,
                Outcomes);
        Perception = result.Perception;
        Outcomes = result.Outcomes;
        return true;
    }

    private bool IsRequestForSearcher(FocusedSearchRequest request)
    {
        return request.SearcherCharacterId == SearcherCharacterId;
    }

    private void ReplaceAndPresent(FocusedSearchActivitySnapshot nextActivities)
    {
        Activities = nextActivities;
        indicator.Present(SearcherCharacterId, Activities);
    }
}
