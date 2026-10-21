using Allrpcs.Domain.Characters;

namespace Allrpcs.Application.Diseases;

/// <summary>Player-facing text for the patient's worsening and the slice's end screen.</summary>
public static class FirstPatientStakesCopy
{
    /// <summary>The patient-condition line shown once the illness has moved on a stage.</summary>
    public static string WorsenedStatus(CharacterCondition condition)
    {
        ArgumentNullException.ThrowIfNull(condition);
        return "Patient: " + condition.Description;
    }

    /// <summary>The end screen's title and text for a finished slice.</summary>
    public static (string Title, string Body) Ending(FirstPatientSliceOutcome outcome) => outcome switch
    {
        FirstPatientSliceOutcome.Completed => (
            "The patient sleeps safely",
            "The remedy held, and the sage has heard what the patient remembered. This part of the journey is complete."),
        FirstPatientSliceOutcome.LostToDecline => (
            "The patient is lost",
            "The grey sleep ran its course before a remedy could hold. Return to the title to continue from your save or begin again."),
        FirstPatientSliceOutcome.LostAfterDoses => (
            "The patient is lost",
            "Neither dose was the remedy, and the patient could take no more. Return to the title to continue from your save or begin again."),
        _ => throw new ArgumentOutOfRangeException(nameof(outcome), outcome, "Only a finished slice has an ending."),
    };
}
