using Allrpcs.Application.Investigations;

namespace Allrpcs.Infrastructure.Investigations;

/// <summary>Retains one accepted interpretation: an offered hypothesis, or none.</summary>
public sealed class InvestigationHypothesisDecision
{
    internal InvestigationHypothesisDecision(InvestigationHypothesisProposal? proposal)
    {
        Proposal = proposal;
    }

    /// <summary>Gets the proposed authored identity pair, or null when nothing was proposed.</summary>
    public InvestigationHypothesisProposal? Proposal { get; }
}
