using Allrpcs.Domain.Controllers;
using Allrpcs.Domain.WorldEvents;

namespace Allrpcs.Domain.Characters;

/// <summary>Decides whether exact current scene authority permits one sabotage-turn opportunity.</summary>
public static class CharacterSabotageTurnOpportunityPolicy
{
    /// <summary>Returns true only at an exact due authoritative meaningful-event boundary.</summary>
    public static bool CanCreate(
        Character actor,
        Character currentActor,
        CharacterSabotageTurnOpportunityCadenceSignal signal,
        CharacterSabotageTurnOpportunityCadence currentCadence,
        WorldEventSnapshot worldEvents,
        WorldEventLogSnapshot eventLog,
        WorldTimeSnapshot worldTime,
        CharacterSabotageTurnOpportunity? currentOpportunity)
    {
        ArgumentNullException.ThrowIfNull(actor);
        ArgumentNullException.ThrowIfNull(currentActor);
        ArgumentNullException.ThrowIfNull(signal);
        ArgumentNullException.ThrowIfNull(currentCadence);
        ArgumentNullException.ThrowIfNull(worldEvents);
        ArgumentNullException.ThrowIfNull(eventLog);
        ArgumentNullException.ThrowIfNull(worldTime);

        if (currentOpportunity is not null
            || !ReferenceEquals(actor, currentActor)
            || !ReferenceEquals(actor.ControllerAssignment.ControllerType, ControllerType.Llm)
            || !ReferenceEquals(signal.Cadence, currentCadence)
            || !CharacterSabotageTurnOpportunityCadenceSignalPolicy.CanCreate(
                currentCadence,
                signal.WorldEvent,
                worldEvents,
                eventLog,
                worldTime))
        {
            return false;
        }

        int occurredMeaningfulEventCount =
            CharacterSabotageTurnOpportunityCadenceSignalPolicy.CountMeaningfulEvents(
                worldEvents,
                eventLog);

        return occurredMeaningfulEventCount == signal.OccurredMeaningfulEventCount
            && occurredMeaningfulEventCount % currentCadence.MeaningfulEventInterval == 0;
    }
}
