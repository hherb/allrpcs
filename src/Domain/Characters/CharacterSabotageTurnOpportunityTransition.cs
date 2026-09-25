using Allrpcs.Domain.WorldEvents;

namespace Allrpcs.Domain.Characters;

/// <summary>Creates one immutable sabotage-turn opportunity from exact current authority.</summary>
public static class CharacterSabotageTurnOpportunityTransition
{
    /// <summary>Returns one opportunity or fails before publishing partial output.</summary>
    public static CharacterSabotageTurnOpportunity Create(
        Character actor,
        Character currentActor,
        CharacterSabotageTurnOpportunityCadenceSignal signal,
        CharacterSabotageTurnOpportunityCadence currentCadence,
        WorldEventSnapshot worldEvents,
        WorldEventLogSnapshot eventLog,
        WorldTimeSnapshot worldTime,
        CharacterSabotageTurnOpportunity? currentOpportunity)
    {
        if (!CharacterSabotageTurnOpportunityPolicy.CanCreate(
            actor,
            currentActor,
            signal,
            currentCadence,
            worldEvents,
            eventLog,
            worldTime,
            currentOpportunity))
        {
            throw new InvalidOperationException(
                "The supplied authority cannot produce a sabotage-turn opportunity.");
        }

        return new CharacterSabotageTurnOpportunity(actor, signal);
    }
}
