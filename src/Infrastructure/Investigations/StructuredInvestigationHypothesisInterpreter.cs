using Allrpcs.Application.Characters;
using Allrpcs.Application.Dialogue;
using Allrpcs.Application.Investigations;
using Allrpcs.Infrastructure.Characters;
using Allrpcs.Infrastructure.Dialogue;

namespace Allrpcs.Infrastructure.Investigations;

/// <summary>Interprets a proposed hypothesis through the shared structured-decision provider client.</summary>
public sealed class StructuredInvestigationHypothesisInterpreter : IInvestigationHypothesisInterpreter
{
    private readonly IStructuredDecisionClient<InvestigationHypothesisRecognitionRequest, InvestigationHypothesisDecision> client;

    /// <summary>Retains the exact provider-neutral client without performing I/O.</summary>
    public StructuredInvestigationHypothesisInterpreter(
        IStructuredDecisionClient<InvestigationHypothesisRecognitionRequest, InvestigationHypothesisDecision> client)
    {
        ArgumentNullException.ThrowIfNull(client);
        this.client = client;
    }

    /// <summary>Composes the interpreter for any supported provider without I/O.</summary>
    public static StructuredInvestigationHypothesisInterpreter Create(
        CharacterModelSettings settings, HttpClient client, IOpenAiCredentialSource? credentialSource = null,
        IAnthropicCredentialSource? anthropicCredentials = null, OllamaEndpoint? ollamaEndpoint = null) =>
        new(StructuredDecisionClientFactory.Create(client, settings, InvestigationHypothesisDecisions.Recognition,
            credentialSource, anthropicCredentials, ollamaEndpoint: ollamaEndpoint));

    /// <inheritdoc />
    public async ValueTask<InvestigationHypothesisProposal?> InterpretAsync(
        InvestigationHypothesisRecognitionRequest request,
        CancellationToken cancellationToken) =>
        (await client.DecideAsync(request, cancellationToken).ConfigureAwait(false)).Proposal;
}
