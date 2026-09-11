using Allrpcs.Domain.WorldEvents;

namespace Allrpcs.Domain.Ingredients;

/// <summary>Decides whether an administration may be recorded as an experiment.</summary>
public static class IngredientAdministrationExperimentRecordingPolicy
{
    /// <summary>Returns true only when the exact authored event is the latest occurrence.</summary>
    public static bool CanRecord(
        IngredientAdministrationOutcome administration,
        WorldEventSnapshot worldEvents,
        WorldEventLogSnapshot eventLog)
    {
        ArgumentNullException.ThrowIfNull(administration);
        ArgumentNullException.ThrowIfNull(worldEvents);
        ArgumentNullException.ThrowIfNull(eventLog);

        return worldEvents.Entries.Any(worldEvent =>
                ReferenceEquals(worldEvent, administration.WorldEvent))
            && eventLog.Entries.Count > 0
            && ReferenceEquals(eventLog.Entries[^1], administration.WorldEvent.EventId);
    }
}
