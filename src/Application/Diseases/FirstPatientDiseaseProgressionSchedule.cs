using Allrpcs.Domain.Diseases;
using Allrpcs.Domain.Identifiers;
using Allrpcs.Domain.WorldEvents;

namespace Allrpcs.Application.Diseases;

/// <summary>
/// Decides when the first patient's illness worsens. Time in this game is a sequence of world
/// events, so "waiting too long" means too many time-advancing events (searches, examinations,
/// doses) without a stabilizing remedy. Every <see cref="StageInterval"/> such events the patient
/// moves one disease stage on (deep sleep, then wasting, then final decline).
/// </summary>
public static class FirstPatientDiseaseProgressionSchedule
{
    /// <summary>
    /// Time-advancing events per worsened stage. A normal successful path takes about four, so
    /// the patient visibly weakens once; idling or wrong remedies cost the patient.
    /// shortcut: one fixed interval, tune it after the owner playtest (Phase 4 item 7).
    /// </summary>
    public const int StageInterval = 3;

    /// <summary>
    /// Returns the truth with the patient worsened one stage when a whole interval has passed since
    /// the last progression (or since the start); otherwise returns the same truth instance.
    /// </summary>
    public static DiseaseTruthSnapshot Apply(
        DiseaseTruthSnapshot truth,
        CharacterId patientId,
        WorldEventSnapshot worldEvents,
        WorldEventLogSnapshot eventLog,
        WorldTimeSnapshot worldTime)
    {
        ArgumentNullException.ThrowIfNull(truth);
        ArgumentNullException.ThrowIfNull(patientId);
        ArgumentNullException.ThrowIfNull(worldEvents);
        ArgumentNullException.ThrowIfNull(eventLog);
        ArgumentNullException.ThrowIfNull(worldTime);

        CharacterDiseaseState? infection = truth.Infections.FirstOrDefault(
            candidate => candidate.CharacterId == patientId);
        WorldEvent? current = worldTime.CurrentEventId is { } currentId
            ? WorldEventLookup.FindById(currentId, worldEvents)
            : null;
        // The Domain policy refuses a stabilized patient, final decline and a consumed event.
        if (infection is null
            || current is null
            || AdvanceEventsSinceProgression(infection, worldEvents, eventLog) < StageInterval
            || !DiseaseProgressionPolicy.CanAdvance(infection, truth, current, worldEvents, eventLog, worldTime))
        {
            return truth;
        }

        return DiseaseProgressionTransition.Advance(infection, truth, current, worldEvents, eventLog, worldTime);
    }

    /// <summary>
    /// Counts time-advancing events logged after the infection's last progression event, or in the
    /// whole log when it has not progressed yet.
    /// </summary>
    public static int AdvanceEventsSinceProgression(
        CharacterDiseaseState infection,
        WorldEventSnapshot worldEvents,
        WorldEventLogSnapshot eventLog)
    {
        ArgumentNullException.ThrowIfNull(infection);
        ArgumentNullException.ThrowIfNull(worldEvents);
        ArgumentNullException.ThrowIfNull(eventLog);

        IEnumerable<WorldEventId> since = infection.LastProgressionEventId is { } last
            && eventLog.Entries.Contains(last)
            ? eventLog.Entries.SkipWhile(id => id != last).Skip(1)
            : eventLog.Entries;
        return since.Count(id =>
            WorldEventLookup.FindById(id, worldEvents)?.TimeEffect == WorldEventTimeEffect.Advance);
    }
}
