namespace Allrpcs.Application.Investigations;

/// <summary>States what the recorded doses say about one proposed hypothesis.</summary>
public enum InvestigationHypothesisTestOutcome
{
    /// <summary>The hypothesis's recipe has not been dosed yet.</summary>
    Untested,

    /// <summary>The dosed recipe is the remedy, which stabilizes the patient.</summary>
    Confirmed,

    /// <summary>The dosed recipe is not the remedy, so it changes nothing.</summary>
    Refuted,
}
