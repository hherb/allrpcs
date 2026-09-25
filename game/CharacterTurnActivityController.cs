using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Dialogue;
using Godot;

namespace Allrpcs.Game;

/// <summary>Owns one current immutable character-turn activity and delegates cancellation.</summary>
public partial class CharacterTurnActivityController : Node
{
    /// <summary>Gets the exact initialized current activity, if any.</summary>
    public CharacterTurnActivity? Activity { get; private set; }

    /// <summary>Gets the exact sabotage opportunity that currently owns the shared turn slot.</summary>
    public CharacterSabotageTurnReservation? SabotageReservation { get; private set; }

    /// <summary>Gets the exact response-free activity staged for the reserved sabotage action.</summary>
    public CharacterTurnActivity? ReservedSabotageTurn { get; private set; }

    /// <summary>Initializes this controller once from one exact existing activity.</summary>
    public bool TryInitialize(CharacterTurnActivity activity)
    {
        ArgumentNullException.ThrowIfNull(activity);

        if (Activity is not null
            || SabotageReservation is not null
            || ReservedSabotageTurn is not null)
        {
            return false;
        }

        Activity = activity;
        return true;
    }

    /// <summary>Starts an exact fresh request only when current activity is absent or terminal.</summary>
    public bool TryStart(CharacterTurnRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        CharacterTurnActivity? activity = Activity;
        if (SabotageReservation is not null
            || ReservedSabotageTurn is not null
            || !CharacterTurnStartPolicy.CanStart(activity))
        {
            return false;
        }

        Activity = CharacterTurnStartTransition.Start(activity, request);
        return true;
    }

    /// <summary>Reserves the available shared turn slot for one exact current sabotage opportunity.</summary>
    public bool TryReserveSabotageTurn(
        CharacterSabotageTurnOpportunity opportunity,
        CharacterSabotageTurnOpportunity currentOpportunity)
    {
        ArgumentNullException.ThrowIfNull(opportunity);
        ArgumentNullException.ThrowIfNull(currentOpportunity);

        if (!CharacterSabotageTurnReservationPolicy.CanReserve(
            opportunity,
            currentOpportunity,
            Activity,
            SabotageReservation)
            || ReservedSabotageTurn is not null)
        {
            return false;
        }

        SabotageReservation = CharacterSabotageTurnReservationTransition.Reserve(
            opportunity,
            currentOpportunity,
            Activity,
            SabotageReservation);
        return true;
    }

    /// <summary>Starts one exact response-free activity owned by the current sabotage reservation.</summary>
    public bool TryStartReservedSabotageTurn(
        CharacterSabotageTurnReservation reservation,
        CharacterTurnActivity activity)
    {
        ArgumentNullException.ThrowIfNull(reservation);
        ArgumentNullException.ThrowIfNull(activity);

        if (Activity is not null
            || ReservedSabotageTurn is not null
            || !ReferenceEquals(SabotageReservation, reservation)
            || !ReferenceEquals(
                activity.Request.RespondingCharacterId,
                reservation.Opportunity.Actor.Identity.Id)
            || !CharacterTurnActionResolutionPolicy.CanResolve(activity))
        {
            return false;
        }

        ReservedSabotageTurn = activity;
        return true;
    }

    /// <summary>Cancels the current activity when the Domain cancellation rules permit it.</summary>
    public bool TryCancel()
    {
        CharacterTurnActivity? activity = Activity;
        if (activity is null || !CharacterTurnCancellationPolicy.CanCancel(activity))
        {
            return false;
        }

        Activity = CharacterTurnCancellationTransition.Cancel(activity);
        return true;
    }

    /// <summary>Installs an already-complete response when it answers the exact awaited request.</summary>
    public bool TryReceiveResponse(CharacterTurnResponse response)
    {
        ArgumentNullException.ThrowIfNull(response);

        CharacterTurnActivity? activity = Activity;
        if (activity is null
            || !CharacterTurnResponseReceptionPolicy.CanReceive(activity, response))
        {
            return false;
        }

        Activity = CharacterTurnResponseReceptionTransition.Receive(activity, response);
        return true;
    }

    /// <summary>Completes the current activity when its response is being spoken.</summary>
    public bool TryComplete()
    {
        CharacterTurnActivity? activity = Activity;
        if (activity is null || !CharacterTurnCompletionPolicy.CanComplete(activity))
        {
            return false;
        }

        Activity = CharacterTurnCompletionTransition.Complete(activity);
        return true;
    }

    /// <summary>Installs one exact preflighted autonomous completion for the exact current turn.</summary>
    internal bool TryInstallAutonomousCompletion(
        CharacterTurnActivity currentActivity,
        CharacterTurnActivity completedActivity)
    {
        ArgumentNullException.ThrowIfNull(currentActivity);
        ArgumentNullException.ThrowIfNull(completedActivity);

        if (!ReferenceEquals(Activity, currentActivity)
            || !ReferenceEquals(completedActivity.State, CharacterTurnActivityState.Completed)
            || !ReferenceEquals(completedActivity.Request, currentActivity.Request)
            || !ReferenceEquals(completedActivity.Response, currentActivity.Response))
        {
            return false;
        }

        Activity = completedActivity;
        return true;
    }

    /// <summary>Installs one exact preflighted non-dialogue resolution for the current turn.</summary>
    internal bool TryInstallActionResolution(
        CharacterTurnActivity currentActivity,
        CharacterTurnActivity resolvedActivity)
    {
        ArgumentNullException.ThrowIfNull(currentActivity);
        ArgumentNullException.ThrowIfNull(resolvedActivity);

        if (!ReferenceEquals(Activity, currentActivity)
            || !ReferenceEquals(resolvedActivity.State, CharacterTurnActivityState.Resolved)
            || !ReferenceEquals(resolvedActivity.Request, currentActivity.Request)
            || resolvedActivity.Response is not null
            || !CharacterTurnActionResolutionPolicy.CanResolve(currentActivity))
        {
            return false;
        }

        Activity = resolvedActivity;
        return true;
    }

    /// <summary>Installs the exact terminal activity staged by the current sabotage reservation.</summary>
    internal bool TryInstallReservedActionResolution(
        CharacterSabotageTurnReservation currentReservation,
        CharacterTurnActivity awaitingActivity,
        CharacterTurnActivity resolvedActivity)
    {
        ArgumentNullException.ThrowIfNull(currentReservation);
        ArgumentNullException.ThrowIfNull(awaitingActivity);
        ArgumentNullException.ThrowIfNull(resolvedActivity);

        if (Activity is not null
            || !ReferenceEquals(SabotageReservation, currentReservation)
            || !ReferenceEquals(ReservedSabotageTurn, awaitingActivity)
            || !ReferenceEquals(resolvedActivity.State, CharacterTurnActivityState.Resolved)
            || !ReferenceEquals(resolvedActivity.Request, awaitingActivity.Request)
            || resolvedActivity.Response is not null)
        {
            return false;
        }

        Activity = resolvedActivity;
        ReservedSabotageTurn = null;
        return true;
    }

    /// <summary>Releases only the exact reservation after its exact action-resolved activity.</summary>
    internal bool TryReleaseSabotageTurn(
        CharacterSabotageTurnReservation currentReservation,
        CharacterTurnActivity resolvedActivity)
    {
        ArgumentNullException.ThrowIfNull(currentReservation);
        ArgumentNullException.ThrowIfNull(resolvedActivity);

        if (!ReferenceEquals(SabotageReservation, currentReservation)
            || !ReferenceEquals(Activity, resolvedActivity)
            || ReservedSabotageTurn is not null
            || !ReferenceEquals(resolvedActivity.State, CharacterTurnActivityState.Resolved)
            || resolvedActivity.Response is not null
            || !ReferenceEquals(
                resolvedActivity.Request.RespondingCharacterId,
                currentReservation.Opportunity.Actor.Identity.Id))
        {
            return false;
        }

        SabotageReservation = null;
        return true;
    }
}
