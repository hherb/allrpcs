using Allrpcs.Domain.WorldEvents;

namespace Allrpcs.Domain.Diseases;

/// <summary>Decides whether one authoritative exposure creates a new infection.</summary>
public static class DiseaseExposurePolicy
{
    /// <summary>Returns true only for an occurred authored exposure without matching protection.</summary>
    public static bool CanApply(
        DiseaseExposureOutcome outcome,
        DiseaseTruthSnapshot truth,
        DiseaseProtectionSnapshot protections,
        WorldEventSnapshot worldEvents,
        WorldEventLogSnapshot eventLog)
    {
        ArgumentNullException.ThrowIfNull(outcome);
        ArgumentNullException.ThrowIfNull(truth);
        ArgumentNullException.ThrowIfNull(protections);
        ArgumentNullException.ThrowIfNull(worldEvents);
        ArgumentNullException.ThrowIfNull(eventLog);

        bool isAuthoredDisease = truth.Definitions.Definitions.Any(disease =>
            ReferenceEquals(disease, outcome.Disease));
        bool isAuthoredEvent = worldEvents.Entries.Any(worldEvent =>
            ReferenceEquals(worldEvent, outcome.WorldEvent));
        bool isCurrentOccurrence = eventLog.Entries.Count > 0
            && ReferenceEquals(eventLog.Entries[^1], outcome.WorldEvent.EventId);
        bool isAlreadyInfected = truth.Infections.Any(infection =>
            infection.CharacterId == outcome.CharacterId
            && infection.Disease.DiseaseId == outcome.Disease.DiseaseId);
        bool hasCanonicalProtectionDefinitions = ReferenceEquals(
            protections.Definitions.Diseases,
            truth.Definitions);
        bool isProtected = protections.ActiveProtections.Any(protection =>
                protection.CharacterId == outcome.CharacterId
                && ReferenceEquals(protection.Definition.Disease, outcome.Disease));

        return isAuthoredDisease
            && isAuthoredEvent
            && isCurrentOccurrence
            && !isAlreadyInfected
            && truth.Infections.Count < DiseaseTruthSnapshot.MaximumInfectionCount
            && hasCanonicalProtectionDefinitions
            && !isProtected;
    }
}
