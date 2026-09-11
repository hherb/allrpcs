using Allrpcs.Domain.WorldEvents;

namespace Allrpcs.Domain.Diseases;

/// <summary>Decides whether one hidden infection may advance at an occurred meaningful event.</summary>
public static class DiseaseProgressionPolicy
{
    /// <summary>Returns true only for exact authoritative state and unused meaningful-time events.</summary>
    public static bool CanAdvance(
        CharacterDiseaseState infection,
        DiseaseTruthSnapshot truth,
        WorldEvent worldEvent,
        WorldEventSnapshot worldEvents,
        WorldEventLogSnapshot eventLog,
        WorldTimeSnapshot worldTime)
    {
        ArgumentNullException.ThrowIfNull(infection);
        ArgumentNullException.ThrowIfNull(truth);
        ArgumentNullException.ThrowIfNull(worldEvent);
        ArgumentNullException.ThrowIfNull(worldEvents);
        ArgumentNullException.ThrowIfNull(eventLog);
        ArgumentNullException.ThrowIfNull(worldTime);

        return truth.Infections.Any(candidate => ReferenceEquals(candidate, infection))
            && worldEvents.Entries.Any(candidate => ReferenceEquals(candidate, worldEvent))
            && ReferenceEquals(worldEvent.TimeEffect, WorldEventTimeEffect.Advance)
            && eventLog.Entries.Any(eventId => ReferenceEquals(eventId, worldEvent.EventId))
            && ReferenceEquals(worldTime.CurrentEventId, worldEvent.EventId)
            && infection.LastProgressionEventId != worldEvent.EventId
            && !ReferenceEquals(infection.CareState, DiseaseCareState.Stabilized)
            && !ReferenceEquals(infection.Stage, DiseaseStage.FinalDecline);
    }
}
