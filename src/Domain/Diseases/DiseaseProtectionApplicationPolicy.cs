using Allrpcs.Domain.WorldEvents;

namespace Allrpcs.Domain.Diseases;

/// <summary>Decides whether a character may apply one discovered disease protection.</summary>
public static class DiseaseProtectionApplicationPolicy
{
    /// <summary>Returns true only for known exact protection and occurred authored event.</summary>
    public static bool CanApply(
        DiseaseProtectionApplicationOutcome outcome,
        DiseaseProtectionSnapshot protections,
        WorldEventSnapshot worldEvents,
        WorldEventLogSnapshot eventLog)
    {
        ArgumentNullException.ThrowIfNull(outcome);
        ArgumentNullException.ThrowIfNull(protections);
        ArgumentNullException.ThrowIfNull(worldEvents);
        ArgumentNullException.ThrowIfNull(eventLog);

        bool isAuthoredDefinition = protections.Definitions.Definitions.Any(definition =>
            ReferenceEquals(definition, outcome.Definition));
        bool isKnown = outcome.EvidenceView.EvidenceIds.Any(evidenceId =>
            ReferenceEquals(evidenceId, outcome.Definition.EvidenceId));
        bool isAuthoredEvent = worldEvents.Entries.Any(worldEvent =>
            ReferenceEquals(worldEvent, outcome.WorldEvent));
        bool isCurrentOccurrence = eventLog.Entries.Count > 0
            && ReferenceEquals(eventLog.Entries[^1], outcome.WorldEvent.EventId);
        bool isAlreadyActive = protections.ActiveProtections.Any(protection =>
            protection.CharacterId == outcome.EvidenceView.CharacterId
            && ReferenceEquals(protection.Definition, outcome.Definition));

        return isAuthoredDefinition
            && isKnown
            && isAuthoredEvent
            && isCurrentOccurrence
            && protections.ActiveProtections.Count
                < DiseaseProtectionSnapshot.MaximumActiveProtectionCount
            && !isAlreadyActive;
    }
}
