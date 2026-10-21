namespace Allrpcs.Application.Investigations;

/// <summary>States what the recorded experiments say about one proposed hypothesis.</summary>
public enum InvestigationHypothesisTestOutcome
{
    /// <summary>No recorded experiment tests the hypothesis yet.</summary>
    Untested,

    /// <summary>A recorded experiment showed the predicted effect.</summary>
    Confirmed,

    /// <summary>A recorded experiment showed the predicted effect did not occur.</summary>
    Refuted,
}
