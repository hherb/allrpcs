using Allrpcs.Application.Dialogue;
using Allrpcs.Domain.Characters;
using Allrpcs.Infrastructure.Characters;
using Allrpcs.Infrastructure.Dialogue;

namespace Allrpcs.Infrastructure.Tests;

internal static class StructuredDecisionTestModels
{
    internal static CharacterDialogueMisquotationEvidenceModel CreateCharacterDialogueMisquotationEvidence(
        HttpClient client, IOpenAiCredentialSource credentialSource, CharacterModelSettings settings,
        OpenAiEndpoint? endpoint = null, ProviderHttpTimeoutOptions? timeoutOptions = null) =>
        new(StructuredDecisionClientFactory.Create(client, settings, CharacterStructuredDecisions.DialogueMisquotationEvidence,
            credentialSource, openAiEndpoint: endpoint, timeoutOptions: timeoutOptions));

    internal static OpenAiStructuredDecisionAccumulator<CharacterDialogueMisquotationEvidenceConsiderationRequest, CharacterDialogueMisquotationEvidenceDecision>
        AccumulateCharacterDialogueMisquotationEvidence(CharacterDialogueMisquotationEvidenceConsiderationRequest request) =>
        CharacterStructuredDecisions.DialogueMisquotationEvidence.CreateOpenAiAccumulator(request);

    internal static Task<CharacterDialogueMisquotationEvidenceDecision> CompleteCharacterDialogueMisquotationEvidenceAsync(
        CharacterDialogueMisquotationEvidenceConsiderationRequest request, Stream stream, CancellationToken cancellationToken = default) =>
        CharacterStructuredDecisions.DialogueMisquotationEvidence.CreateOpenAiAccumulator(request).CompleteAsync(stream, cancellationToken);

    internal static CharacterObjectSabotageConcealmentModel CreateCharacterObjectSabotageConcealment(
        HttpClient client, IOpenAiCredentialSource credentialSource, CharacterModelSettings settings,
        OpenAiEndpoint? endpoint = null, ProviderHttpTimeoutOptions? timeoutOptions = null) =>
        new(StructuredDecisionClientFactory.Create(client, settings, CharacterStructuredDecisions.ObjectSabotageConcealment,
            credentialSource, openAiEndpoint: endpoint, timeoutOptions: timeoutOptions));

    internal static OpenAiStructuredDecisionAccumulator<CharacterObjectSabotageConcealmentConsiderationRequest, CharacterObjectSabotageConcealmentDecision>
        AccumulateCharacterObjectSabotageConcealment(CharacterObjectSabotageConcealmentConsiderationRequest request) =>
        CharacterStructuredDecisions.ObjectSabotageConcealment.CreateOpenAiAccumulator(request);

    internal static Task<CharacterObjectSabotageConcealmentDecision> CompleteCharacterObjectSabotageConcealmentAsync(
        CharacterObjectSabotageConcealmentConsiderationRequest request, Stream stream, CancellationToken cancellationToken = default) =>
        CharacterStructuredDecisions.ObjectSabotageConcealment.CreateOpenAiAccumulator(request).CompleteAsync(stream, cancellationToken);

    internal static CharacterObjectSabotageContaminationModel CreateCharacterObjectSabotageContamination(
        HttpClient client, IOpenAiCredentialSource credentialSource, CharacterModelSettings settings,
        OpenAiEndpoint? endpoint = null, ProviderHttpTimeoutOptions? timeoutOptions = null) =>
        new(StructuredDecisionClientFactory.Create(client, settings, CharacterStructuredDecisions.ObjectSabotageContamination,
            credentialSource, openAiEndpoint: endpoint, timeoutOptions: timeoutOptions));

    internal static OpenAiStructuredDecisionAccumulator<CharacterObjectSabotageContaminationConsiderationRequest, CharacterObjectSabotageContaminationDecision>
        AccumulateCharacterObjectSabotageContamination(CharacterObjectSabotageContaminationConsiderationRequest request) =>
        CharacterStructuredDecisions.ObjectSabotageContamination.CreateOpenAiAccumulator(request);

    internal static Task<CharacterObjectSabotageContaminationDecision> CompleteCharacterObjectSabotageContaminationAsync(
        CharacterObjectSabotageContaminationConsiderationRequest request, Stream stream, CancellationToken cancellationToken = default) =>
        CharacterStructuredDecisions.ObjectSabotageContamination.CreateOpenAiAccumulator(request).CompleteAsync(stream, cancellationToken);

    internal static CharacterObjectSabotageMoveModel CreateCharacterObjectSabotageMove(
        HttpClient client, IOpenAiCredentialSource credentialSource, CharacterModelSettings settings,
        OpenAiEndpoint? endpoint = null, ProviderHttpTimeoutOptions? timeoutOptions = null) =>
        new(StructuredDecisionClientFactory.Create(client, settings, CharacterStructuredDecisions.ObjectSabotageMove,
            credentialSource, openAiEndpoint: endpoint, timeoutOptions: timeoutOptions));

    internal static OpenAiStructuredDecisionAccumulator<CharacterObjectSabotageMoveConsiderationRequest, CharacterObjectSabotageMoveDecision>
        AccumulateCharacterObjectSabotageMove(CharacterObjectSabotageMoveConsiderationRequest request) =>
        CharacterStructuredDecisions.ObjectSabotageMove.CreateOpenAiAccumulator(request);

    internal static Task<CharacterObjectSabotageMoveDecision> CompleteCharacterObjectSabotageMoveAsync(
        CharacterObjectSabotageMoveConsiderationRequest request, Stream stream, CancellationToken cancellationToken = default) =>
        CharacterStructuredDecisions.ObjectSabotageMove.CreateOpenAiAccumulator(request).CompleteAsync(stream, cancellationToken);

    internal static CharacterObjectSabotageSubstitutionModel CreateCharacterObjectSabotageSubstitution(
        HttpClient client, IOpenAiCredentialSource credentialSource, CharacterModelSettings settings,
        OpenAiEndpoint? endpoint = null, ProviderHttpTimeoutOptions? timeoutOptions = null) =>
        new(StructuredDecisionClientFactory.Create(client, settings, CharacterStructuredDecisions.ObjectSabotageSubstitution,
            credentialSource, openAiEndpoint: endpoint, timeoutOptions: timeoutOptions));

    internal static OpenAiStructuredDecisionAccumulator<CharacterObjectSabotageSubstitutionConsiderationRequest, CharacterObjectSabotageSubstitutionDecision>
        AccumulateCharacterObjectSabotageSubstitution(CharacterObjectSabotageSubstitutionConsiderationRequest request) =>
        CharacterStructuredDecisions.ObjectSabotageSubstitution.CreateOpenAiAccumulator(request);

    internal static Task<CharacterObjectSabotageSubstitutionDecision> CompleteCharacterObjectSabotageSubstitutionAsync(
        CharacterObjectSabotageSubstitutionConsiderationRequest request, Stream stream, CancellationToken cancellationToken = default) =>
        CharacterStructuredDecisions.ObjectSabotageSubstitution.CreateOpenAiAccumulator(request).CompleteAsync(stream, cancellationToken);

    internal static CharacterSabotageActionModel CreateCharacterSabotageAction(
        HttpClient client, IOpenAiCredentialSource credentialSource, CharacterModelSettings settings,
        OpenAiEndpoint? endpoint = null, ProviderHttpTimeoutOptions? timeoutOptions = null) =>
        new(StructuredDecisionClientFactory.Create(client, settings, CharacterStructuredDecisions.SabotageAction,
            credentialSource, openAiEndpoint: endpoint, timeoutOptions: timeoutOptions));

    internal static OpenAiStructuredDecisionAccumulator<CharacterSabotageActionConsiderationRequest, CharacterSabotageActionDecision>
        AccumulateCharacterSabotageAction(CharacterSabotageActionConsiderationRequest request) =>
        CharacterStructuredDecisions.SabotageAction.CreateOpenAiAccumulator(request);

    internal static Task<CharacterSabotageActionDecision> CompleteCharacterSabotageActionAsync(
        CharacterSabotageActionConsiderationRequest request, Stream stream, CancellationToken cancellationToken = default) =>
        CharacterStructuredDecisions.SabotageAction.CreateOpenAiAccumulator(request).CompleteAsync(stream, cancellationToken);

    internal static CharacterTargetSabotageDelayModel CreateCharacterTargetSabotageDelay(
        HttpClient client, IOpenAiCredentialSource credentialSource, CharacterModelSettings settings,
        OpenAiEndpoint? endpoint = null, ProviderHttpTimeoutOptions? timeoutOptions = null) =>
        new(StructuredDecisionClientFactory.Create(client, settings, CharacterStructuredDecisions.TargetSabotageDelay,
            credentialSource, openAiEndpoint: endpoint, timeoutOptions: timeoutOptions));

    internal static OpenAiStructuredDecisionAccumulator<CharacterTargetSabotageDelayConsiderationRequest, CharacterTargetSabotageDelayDecision>
        AccumulateCharacterTargetSabotageDelay(CharacterTargetSabotageDelayConsiderationRequest request) =>
        CharacterStructuredDecisions.TargetSabotageDelay.CreateOpenAiAccumulator(request);

    internal static Task<CharacterTargetSabotageDelayDecision> CompleteCharacterTargetSabotageDelayAsync(
        CharacterTargetSabotageDelayConsiderationRequest request, Stream stream, CancellationToken cancellationToken = default) =>
        CharacterStructuredDecisions.TargetSabotageDelay.CreateOpenAiAccumulator(request).CompleteAsync(stream, cancellationToken);

    internal static CharacterTargetSabotageDelayRecoveryModel CreateCharacterTargetSabotageDelayRecovery(
        HttpClient client, IOpenAiCredentialSource credentialSource, CharacterModelSettings settings,
        OpenAiEndpoint? endpoint = null, ProviderHttpTimeoutOptions? timeoutOptions = null) =>
        new(StructuredDecisionClientFactory.Create(client, settings, CharacterStructuredDecisions.TargetSabotageDelayRecovery,
            credentialSource, openAiEndpoint: endpoint, timeoutOptions: timeoutOptions));

    internal static OpenAiStructuredDecisionAccumulator<CharacterTargetSabotageDelayRecoveryConsiderationRequest, CharacterTargetSabotageDelayRecoveryDecision>
        AccumulateCharacterTargetSabotageDelayRecovery(CharacterTargetSabotageDelayRecoveryConsiderationRequest request) =>
        CharacterStructuredDecisions.TargetSabotageDelayRecovery.CreateOpenAiAccumulator(request);

    internal static Task<CharacterTargetSabotageDelayRecoveryDecision> CompleteCharacterTargetSabotageDelayRecoveryAsync(
        CharacterTargetSabotageDelayRecoveryConsiderationRequest request, Stream stream, CancellationToken cancellationToken = default) =>
        CharacterStructuredDecisions.TargetSabotageDelayRecovery.CreateOpenAiAccumulator(request).CompleteAsync(stream, cancellationToken);

    internal static CharacterTargetSabotageInfluenceModel CreateCharacterTargetSabotageInfluence(
        HttpClient client, IOpenAiCredentialSource credentialSource, CharacterModelSettings settings,
        OpenAiEndpoint? endpoint = null, ProviderHttpTimeoutOptions? timeoutOptions = null) =>
        new(StructuredDecisionClientFactory.Create(client, settings, CharacterStructuredDecisions.TargetSabotageInfluence,
            credentialSource, openAiEndpoint: endpoint, timeoutOptions: timeoutOptions));

    internal static OpenAiStructuredDecisionAccumulator<CharacterTargetSabotageInfluenceConsiderationRequest, CharacterTargetSabotageInfluenceDecision>
        AccumulateCharacterTargetSabotageInfluence(CharacterTargetSabotageInfluenceConsiderationRequest request) =>
        CharacterStructuredDecisions.TargetSabotageInfluence.CreateOpenAiAccumulator(request);

    internal static Task<CharacterTargetSabotageInfluenceDecision> CompleteCharacterTargetSabotageInfluenceAsync(
        CharacterTargetSabotageInfluenceConsiderationRequest request, Stream stream, CancellationToken cancellationToken = default) =>
        CharacterStructuredDecisions.TargetSabotageInfluence.CreateOpenAiAccumulator(request).CompleteAsync(stream, cancellationToken);

    internal static CharacterTargetSabotageInfluenceRecoveryModel CreateCharacterTargetSabotageInfluenceRecovery(
        HttpClient client, IOpenAiCredentialSource credentialSource, CharacterModelSettings settings,
        OpenAiEndpoint? endpoint = null, ProviderHttpTimeoutOptions? timeoutOptions = null) =>
        new(StructuredDecisionClientFactory.Create(client, settings, CharacterStructuredDecisions.TargetSabotageInfluenceRecovery,
            credentialSource, openAiEndpoint: endpoint, timeoutOptions: timeoutOptions));

    internal static OpenAiStructuredDecisionAccumulator<CharacterTargetSabotageInfluenceRecoveryConsiderationRequest, CharacterTargetSabotageInfluenceRecoveryDecision>
        AccumulateCharacterTargetSabotageInfluenceRecovery(CharacterTargetSabotageInfluenceRecoveryConsiderationRequest request) =>
        CharacterStructuredDecisions.TargetSabotageInfluenceRecovery.CreateOpenAiAccumulator(request);

    internal static Task<CharacterTargetSabotageInfluenceRecoveryDecision> CompleteCharacterTargetSabotageInfluenceRecoveryAsync(
        CharacterTargetSabotageInfluenceRecoveryConsiderationRequest request, Stream stream, CancellationToken cancellationToken = default) =>
        CharacterStructuredDecisions.TargetSabotageInfluenceRecovery.CreateOpenAiAccumulator(request).CompleteAsync(stream, cancellationToken);

}
