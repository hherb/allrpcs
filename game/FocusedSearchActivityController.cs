using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Discovery;
using Allrpcs.Domain.Identifiers;
using Allrpcs.Domain.Perception;
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
        Perception = null;
        indicator.Present(SearcherCharacterId, Activities);
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
