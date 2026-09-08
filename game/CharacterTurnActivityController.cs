using Allrpcs.Domain.Dialogue;
using Godot;

namespace Allrpcs.Game;

/// <summary>Owns one current immutable character-turn activity and delegates cancellation.</summary>
public partial class CharacterTurnActivityController : Node
{
    /// <summary>Gets the exact initialized current activity, if any.</summary>
    public CharacterTurnActivity? Activity { get; private set; }

    /// <summary>Initializes this controller once from one exact existing activity.</summary>
    public bool TryInitialize(CharacterTurnActivity activity)
    {
        ArgumentNullException.ThrowIfNull(activity);

        if (Activity is not null)
        {
            return false;
        }

        Activity = activity;
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
}
