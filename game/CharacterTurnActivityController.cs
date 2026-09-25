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

    /// <summary>Initializes this controller once from one exact existing activity.</summary>
    public bool TryInitialize(CharacterTurnActivity activity)
    {
        ArgumentNullException.ThrowIfNull(activity);

        if (Activity is not null || SabotageReservation is not null)
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
        if (SabotageReservation is not null || !CharacterTurnStartPolicy.CanStart(activity))
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
            SabotageReservation))
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
}
