using Allrpcs.Domain.Characters;
using Allrpcs.Domain.WorldEvents;
using Godot;

namespace Allrpcs.Game;

/// <summary>Observes qualitative world-event changes and schedules at most one exact opportunity.</summary>
public partial class CharacterSabotageTurnOpportunitySchedulingCoordinator : Node
{
    private CharacterSabotageTurnOpportunityController opportunities = null!;
    private WorldEventController world = null!;

    /// <summary>Gets the exact world-time snapshot most recently considered for scheduling.</summary>
    public WorldTimeSnapshot? ObservedWorldTime { get; private set; }

    /// <summary>Gets the exact authoritative event most recently considered for scheduling.</summary>
    public WorldEvent? ObservedWorldEvent { get; private set; }

    /// <inheritdoc />
    public override void _Ready()
    {
        opportunities = GetNode<CharacterSabotageTurnOpportunityController>(
            "../CharacterSabotageTurnOpportunityController");
        world = GetNode<WorldEventController>("../WorldEventController");
    }

    /// <inheritdoc />
    public override void _Process(double delta)
    {
        _ = delta;
        TryObserveCurrentEvent();
    }

    /// <summary>Considers one new exact qualitative-time event without using elapsed wall time.</summary>
    public bool TryObserveCurrentEvent()
    {
        Character? actor = opportunities.Actor;
        CharacterSabotageTurnOpportunityCadence? cadence = opportunities.Cadence;
        WorldTimeSnapshot? worldTime = world.WorldTime;
        if (actor is null
            || cadence is null
            || worldTime is null
            || worldTime.CurrentEventId is null
            || ReferenceEquals(worldTime, ObservedWorldTime)
            || world.FindWorldEvent(worldTime.CurrentEventId) is not { } currentEvent)
        {
            return false;
        }

        ObservedWorldTime = worldTime;
        ObservedWorldEvent = currentEvent;
        return opportunities.TryCreate(actor, cadence, currentEvent);
    }
}
