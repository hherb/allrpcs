using Allrpcs.Domain.Identifiers;

namespace Allrpcs.Domain.Investigations;

/// <summary>Describes a non-executable authored consequence of one exact conclusion.</summary>
public sealed class InvestigationConsequence
{
    /// <summary>Gets the maximum authored consequence length in UTF-16 code units.</summary>
    public const int MaximumDescriptionLength = 500;

    /// <summary>Creates one immutable non-executable investigation consequence.</summary>
    public InvestigationConsequence(
        InvestigationConsequenceId consequenceId,
        InvestigationConclusion conclusion,
        string description)
    {
        ArgumentNullException.ThrowIfNull(consequenceId);
        ArgumentNullException.ThrowIfNull(conclusion);
        ArgumentNullException.ThrowIfNull(description);
        if (string.IsNullOrWhiteSpace(description) || description.Length > MaximumDescriptionLength)
        {
            throw new ArgumentException(
                $"A consequence must be nonblank and at most {MaximumDescriptionLength} UTF-16 code units.",
                nameof(description));
        }

        ConsequenceId = consequenceId;
        Conclusion = conclusion;
        Description = description;
    }

    /// <summary>Gets the exact canonical consequence identity.</summary>
    public InvestigationConsequenceId ConsequenceId { get; }

    /// <summary>Gets the exact authored conclusion associated with this consequence.</summary>
    public InvestigationConclusion Conclusion { get; }

    /// <summary>Gets the authored consequence description exactly as supplied.</summary>
    public string Description { get; }
}
