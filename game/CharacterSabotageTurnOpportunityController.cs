using Allrpcs.Domain.Characters;
using Allrpcs.Domain.WorldEvents;
using Godot;

namespace Allrpcs.Game;

/// <summary>Owns one exact scene actor, meaningful-event cadence, and resulting opportunity.</summary>
public partial class CharacterSabotageTurnOpportunityController : Node
{
    private TemporaryCharacterSabotageOrdinaryTurnAuthoringSource source = null!;
    private WorldEventController world = null!;

    /// <summary>Gets the exact initialized scene-owned enemy actor.</summary>
    public Character? Actor { get; private set; }

    /// <summary>Gets the exact initialized meaningful-event cadence.</summary>
    public CharacterSabotageTurnOpportunityCadence? Cadence { get; private set; }

    /// <summary>Gets the one exact produced sabotage-turn opportunity, if any.</summary>
    public CharacterSabotageTurnOpportunity? Opportunity { get; private set; }

    /// <inheritdoc />
    public override void _Ready()
    {
        source = GetNode<TemporaryCharacterSabotageOrdinaryTurnAuthoringSource>(
            "../TemporaryCharacterSabotageOrdinaryTurnAuthoringSource");
        world = GetNode<WorldEventController>("../WorldEventController");
    }

    /// <summary>Initializes one exact current actor and cadence without producing an opportunity.</summary>
    public bool TryInitialize(
        Character? actor,
        CharacterSabotageTurnOpportunityCadence? cadence)
    {
        if (Actor is not null
            || Cadence is not null
            || Opportunity is not null
            || actor is null
            || cadence is null
            || !ReferenceEquals(actor, source.Actor))
        {
            return false;
        }

        Actor = actor;
        Cadence = cadence;
        return true;
    }

    /// <summary>Creates one opportunity only from exact current scene authority and signal.</summary>
    public bool TryCreate(
        Character? expectedActor,
        CharacterSabotageTurnOpportunityCadence? expectedCadence,
        WorldEvent? currentEvent)
    {
        Character? actor = Actor;
        CharacterSabotageTurnOpportunityCadence? cadence = Cadence;
        if (actor is null
            || cadence is null
            || currentEvent is null
            || !ReferenceEquals(actor, expectedActor)
            || !ReferenceEquals(cadence, expectedCadence)
            || !ReferenceEquals(actor, source.Actor)
            || world.WorldEvents is not { } worldEvents
            || world.EventLog is not { } eventLog
            || world.WorldTime is not { } worldTime
            || !CharacterSabotageTurnOpportunityCadenceSignalPolicy.CanCreate(
                cadence,
                currentEvent,
                worldEvents,
                eventLog,
                worldTime))
        {
            return false;
        }

        CharacterSabotageTurnOpportunityCadenceSignal signal =
            CharacterSabotageTurnOpportunityCadenceSignalTransition.Create(
                cadence,
                currentEvent,
                worldEvents,
                eventLog,
                worldTime);
        if (!CharacterSabotageTurnOpportunityPolicy.CanCreate(
                actor,
                source.Actor,
                signal,
                cadence,
                worldEvents,
                eventLog,
                worldTime,
                Opportunity))
        {
            return false;
        }

        Opportunity = CharacterSabotageTurnOpportunityTransition.Create(
            actor,
            source.Actor,
            signal,
            cadence,
            worldEvents,
            eventLog,
            worldTime,
            Opportunity);
        return true;
    }
}
