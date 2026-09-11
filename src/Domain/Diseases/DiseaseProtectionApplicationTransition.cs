using Allrpcs.Domain.WorldEvents;

namespace Allrpcs.Domain.Diseases;

/// <summary>Activates one exact discovered disease protection for a character.</summary>
public static class DiseaseProtectionApplicationTransition
{
    /// <summary>Returns protections with one exact character-measure pair appended.</summary>
    public static DiseaseProtectionSnapshot Apply(
        DiseaseProtectionApplicationOutcome outcome,
        DiseaseProtectionSnapshot protections,
        WorldEventSnapshot worldEvents,
        WorldEventLogSnapshot eventLog)
    {
        if (!DiseaseProtectionApplicationPolicy.CanApply(
            outcome,
            protections,
            worldEvents,
            eventLog))
        {
            throw new InvalidOperationException(
                "The supplied disease protection cannot be applied.");
        }

        CharacterDiseaseProtection active = new(
            outcome.EvidenceView.CharacterId,
            outcome.Definition);

        return new DiseaseProtectionSnapshot(
            protections.Definitions,
            protections.ActiveProtections.Append(active));
    }
}
