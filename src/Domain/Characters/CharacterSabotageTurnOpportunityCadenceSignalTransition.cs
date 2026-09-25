using Allrpcs.Domain.WorldEvents;

namespace Allrpcs.Domain.Characters;

/// <summary>Creates one exact sabotage cadence signal from current authoritative world state.</summary>
public static class CharacterSabotageTurnOpportunityCadenceSignalTransition
{
    /// <summary>Returns one derived signal or fails before publishing partial output.</summary>
    public static CharacterSabotageTurnOpportunityCadenceSignal Create(
        CharacterSabotageTurnOpportunityCadence cadence,
        WorldEvent currentEvent,
        WorldEventSnapshot worldEvents,
        WorldEventLogSnapshot eventLog,
        WorldTimeSnapshot worldTime)
    {
        if (!CharacterSabotageTurnOpportunityCadenceSignalPolicy.CanCreate(
            cadence,
            currentEvent,
            worldEvents,
            eventLog,
            worldTime))
        {
            throw new InvalidOperationException(
                "The supplied world state cannot produce a sabotage cadence signal.");
        }

        return new CharacterSabotageTurnOpportunityCadenceSignal(
            cadence,
            currentEvent,
            CharacterSabotageTurnOpportunityCadenceSignalPolicy.CountMeaningfulEvents(
                worldEvents,
                eventLog));
    }
}
