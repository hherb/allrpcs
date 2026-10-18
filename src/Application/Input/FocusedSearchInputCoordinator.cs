using Allrpcs.Domain.Discovery;
using Allrpcs.Domain.Perception;
using Allrpcs.Domain.WorldEvents;
using Allrpcs.Domain.WorldObjects;

namespace Allrpcs.Application.Input;

/// <summary>
/// Applies one player focused-search action to the Seeker's searches and reports whether it
/// changed anything. Identifying the authored herb patch also occurs its world event, atomically.
/// </summary>
public sealed class FocusedSearchInputCoordinator
{
    /// <summary>The status shown before any focused-search action.</summary>
    public const string InitialResultText = "Focused search: NO ATTEMPT";

    private readonly IFocusedSearchInputPorts ports;

    /// <summary>Creates a coordinator over the exact native owners.</summary>
    public FocusedSearchInputCoordinator(IFocusedSearchInputPorts ports)
    {
        ArgumentNullException.ThrowIfNull(ports);
        this.ports = ports;
    }

    /// <summary>Applies one action and presents its CHANGED or UNCHANGED status.</summary>
    public void Handle(FocusedSearchInputAction action)
    {
        bool changed = action switch
        {
            FocusedSearchInputAction.Cancel => TryCancel(),
            FocusedSearchInputAction.Complete => TryComplete(),
            FocusedSearchInputAction.Resolve => TryResolve(),
            FocusedSearchInputAction.Start => TryStart(),
            _ => throw new ArgumentOutOfRangeException(nameof(action), action, null),
        };
        ports.PresentResult(
            $"Focused search {action.ToString().ToUpperInvariant()}: {(changed ? "CHANGED" : "UNCHANGED")}");
    }

    private bool TryCancel()
    {
        FocusedSearchRequest? activeRequest = GetActiveRequest();
        return activeRequest is not null && ports.TryCancelSearch(activeRequest);
    }

    private bool TryComplete()
    {
        FocusedSearchRequest? activeRequest = GetActiveRequest();
        FocusedSearchOutcome? outcome = activeRequest is null
            ? null
            : ports.CreateOutcome(
                activeRequest,
                ports.FindWorldObject(activeRequest.TargetObjectId));
        return outcome is not null && ports.TryCompleteSearch(outcome);
    }

    private bool TryResolve()
    {
        FocusedSearchOutcome? outcome = ports.Outcomes.Entries.FirstOrDefault(
            retained => retained.Request.SearcherCharacterId == ports.SearcherCharacterId);
        bool changed = outcome is not null
            && ports.FindWorldObject(outcome.Request.TargetObjectId) is { } worldObject
            && TryResolveOutcome(outcome, worldObject);
        if (changed)
        {
            ports.RefreshResolvedPerception();
            RecoverSabotagesFor(outcome!);
            ports.PresentSuccessfulInteraction();
        }

        return changed;
    }

    private bool TryStart() =>
        ports.FocusedTargetObjectId is { } focusedTargetObjectId
        && ports.FindWorldObject(focusedTargetObjectId) is { } focusedWorldObject
        && ports.TryStartFocusedSearch(focusedWorldObject);

    private FocusedSearchRequest? GetActiveRequest() =>
        ports.Activities.Entries
            .FirstOrDefault(
                activity => activity.Request.SearcherCharacterId == ports.SearcherCharacterId)
            ?.Request;

    private bool TryResolveOutcome(FocusedSearchOutcome outcome, WorldObject worldObject)
    {
        if (!ReferenceEquals(worldObject, ports.AuthoredHerbPatch)
            || !ReferenceEquals(outcome.ResultingVisibilityState, VisibilityState.Identified))
        {
            bool resolved = ports.TryResolveOutcome(outcome, worldObject);
            if (resolved && ReferenceEquals(worldObject, ports.WagonClue))
            {
                _ = ports.TryRouteWagonClue(outcome, worldObject, ports.Perception!);
            }

            return resolved;
        }

        WorldEvent advancingEvent = ports.HerbPatchIdentifiedWorldEvent;
        if (ports.Perception is not { } perception
            || ports.WorldEvents is not { } worldEvents
            || ports.EventLog is not { } eventLog
            || ports.WorldTime is not { } worldTime
            || !WorldObjectBoundFocusedSearchOutcomeResolutionPolicy.CanResolve(
                outcome,
                worldObject,
                perception,
                ports.Outcomes)
            || !WorldEventOccurrencePolicy.CanOccur(
                advancingEvent.EventId,
                worldEvents,
                eventLog,
                worldTime))
        {
            return false;
        }

        if (!ports.TryResolveOutcome(outcome, worldObject)
            || !ports.TryOccurWorldEvent(advancingEvent.EventId))
        {
            throw new InvalidOperationException(
                "Prevalidated focused-search world-event resolution failed to commit.");
        }

        return true;
    }

    private void RecoverSabotagesFor(FocusedSearchOutcome outcome)
    {
        if (ports.Perception is not { } perception)
        {
            return;
        }

        foreach (FocusedSearchEvidenceRecovery recovery in ports.EvidenceRecoveries)
        {
            if (outcome.Request.TargetObjectId == recovery.EvidenceObjectId)
            {
                _ = recovery.TryRecover(perception);
            }
        }
    }
}
