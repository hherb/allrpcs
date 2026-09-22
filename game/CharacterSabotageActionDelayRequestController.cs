using Allrpcs.Domain.Characters;
using Allrpcs.Domain.WorldEvents;
using Godot;

namespace Allrpcs.Game;

/// <summary>Authors one delay action request from exact eligible current world state.</summary>
public partial class CharacterSabotageActionDelayRequestController : Node
{
    private CharacterTargetSabotageDelayController state = null!;

    /// <summary>Gets the exact one-shot sabotage-action request accepted by this controller.</summary>
    public CharacterSabotageActionConsiderationRequest? Request { get; private set; }

    /// <inheritdoc />
    public override void _Ready()
    {
        state = GetNode<CharacterTargetSabotageDelayController>(
            "../CharacterTargetSabotageDelayController");
    }

    /// <summary>Creates one request only for the exact current eligible target-delay graph.</summary>
    public bool TryCreate(
        CharacterTargetSabotageDelayConsiderationRequest delayRequest,
        Character actor,
        CharacterInteractionAccessSnapshot interactionAccess,
        WorldEventSnapshot worldEvents,
        WorldEventLogSnapshot eventLog,
        WorldTimeSnapshot worldTime,
        CharacterTargetSabotageDelayCandidate candidate)
    {
        ArgumentNullException.ThrowIfNull(delayRequest);
        ArgumentNullException.ThrowIfNull(actor);
        ArgumentNullException.ThrowIfNull(interactionAccess);
        ArgumentNullException.ThrowIfNull(worldEvents);
        ArgumentNullException.ThrowIfNull(eventLog);
        ArgumentNullException.ThrowIfNull(worldTime);
        ArgumentNullException.ThrowIfNull(candidate);

        if (Request is not null
            || !ReferenceEquals(delayRequest.Actor, actor)
            || !ReferenceEquals(delayRequest.InteractionAccess, interactionAccess)
            || !ReferenceEquals(delayRequest.WorldEvents, worldEvents)
            || !ReferenceEquals(delayRequest.EventLog, eventLog)
            || !ReferenceEquals(delayRequest.WorldTime, worldTime)
            || !delayRequest.Candidates.Any(requestCandidate =>
                ReferenceEquals(requestCandidate, candidate))
            || !ReferenceEquals(state.EventLog, eventLog)
            || !ReferenceEquals(state.WorldTime, worldTime)
            || !CharacterTargetSabotageDelayPolicy.CanApply(
                actor,
                interactionAccess,
                worldEvents,
                eventLog,
                worldTime,
                candidate))
        {
            return false;
        }

        Request = new CharacterSabotageActionConsiderationRequest(
            actor,
            [CharacterSabotageActionKind.Delay]);
        return true;
    }
}
