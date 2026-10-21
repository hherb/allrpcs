using Allrpcs.Domain.Diseases;
using Allrpcs.Domain.Identifiers;
using Allrpcs.Domain.Persistence;
using Allrpcs.Domain.WorldEvents;

namespace Allrpcs.Application.Diseases;

/// <summary>How the vertical slice currently stands: still playing, won, or lost (and why).</summary>
public enum FirstPatientSliceOutcome
{
    /// <summary>The patient can still be saved, or is saved but the sage is not consulted yet.</summary>
    InProgress,

    /// <summary>The patient is stabilized and the sage has heard the testimony.</summary>
    Completed,

    /// <summary>A whole interval passed at final decline without a stabilizing remedy.</summary>
    LostToDecline,

    /// <summary>Both authored doses were spent without stabilizing the patient.</summary>
    LostAfterDoses,
}

/// <summary>
/// Derives the slice outcome from saved state only (disease truth, event log, recovery stage), so
/// a loaded save always shows the same ending it would have shown when it was written.
/// </summary>
public static class FirstPatientSliceOutcomePolicy
{
    /// <summary>Evaluates the current outcome; <paramref name="doseEventIds"/> are every authored dose (at least one).</summary>
    public static FirstPatientSliceOutcome Evaluate(
        DiseaseTruthSnapshot truth,
        CharacterId patientId,
        WorldEventSnapshot worldEvents,
        WorldEventLogSnapshot eventLog,
        IEnumerable<WorldEventId> doseEventIds,
        FirstPatientRecoveryStage recovery)
    {
        ArgumentNullException.ThrowIfNull(truth);
        ArgumentNullException.ThrowIfNull(patientId);
        ArgumentNullException.ThrowIfNull(worldEvents);
        ArgumentNullException.ThrowIfNull(eventLog);
        ArgumentNullException.ThrowIfNull(doseEventIds);
        ArgumentNullException.ThrowIfNull(recovery);
        WorldEventId[] doses = [.. doseEventIds];
        if (doses.Length == 0)
        {
            throw new ArgumentException("At least one authored dose is required.", nameof(doseEventIds));
        }

        CharacterDiseaseState? infection = truth.Infections.FirstOrDefault(
            candidate => candidate.CharacterId == patientId);
        if (infection is null)
        {
            return FirstPatientSliceOutcome.InProgress;
        }

        if (ReferenceEquals(infection.CareState, DiseaseCareState.Stabilized))
        {
            return recovery == FirstPatientRecoveryStage.SageConsulted
                ? FirstPatientSliceOutcome.Completed
                : FirstPatientSliceOutcome.InProgress;
        }

        // A remedy dose always stabilizes, so spent doses on an unstabilized patient were all wrong.
        if (doses.All(eventLog.Entries.Contains))
        {
            return FirstPatientSliceOutcome.LostAfterDoses;
        }

        return ReferenceEquals(infection.Stage, DiseaseStage.FinalDecline)
            && FirstPatientDiseaseProgressionSchedule.AdvanceEventsSinceProgression(infection, worldEvents, eventLog)
                >= FirstPatientDiseaseProgressionSchedule.StageInterval
            ? FirstPatientSliceOutcome.LostToDecline
            : FirstPatientSliceOutcome.InProgress;
    }
}
