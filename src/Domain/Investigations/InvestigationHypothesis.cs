using Allrpcs.Domain.Identifiers;

namespace Allrpcs.Domain.Investigations;

/// <summary>Describes one authored possible answer without promoting it to fact.</summary>
public sealed class InvestigationHypothesis
{
    /// <summary>Gets the maximum authored hypothesis length in UTF-16 code units.</summary>
    public const int MaximumStatementLength = 500;

    /// <summary>Creates one immutable hypothesis tied to an exact authored question.</summary>
    public InvestigationHypothesis(
        InvestigationHypothesisId hypothesisId,
        InvestigationQuestion question,
        string statement)
    {
        ArgumentNullException.ThrowIfNull(hypothesisId);
        ArgumentNullException.ThrowIfNull(question);
        ArgumentNullException.ThrowIfNull(statement);
        if (string.IsNullOrWhiteSpace(statement) || statement.Length > MaximumStatementLength)
        {
            throw new ArgumentException(
                $"A hypothesis must be nonblank and at most {MaximumStatementLength} UTF-16 code units.",
                nameof(statement));
        }

        HypothesisId = hypothesisId;
        Question = question;
        Statement = statement;
    }

    /// <summary>Gets the exact canonical hypothesis identity.</summary>
    public InvestigationHypothesisId HypothesisId { get; }

    /// <summary>Gets the exact authored question this hypothesis may answer.</summary>
    public InvestigationQuestion Question { get; }

    /// <summary>Gets the authored hypothesis statement exactly as supplied.</summary>
    public string Statement { get; }
}
