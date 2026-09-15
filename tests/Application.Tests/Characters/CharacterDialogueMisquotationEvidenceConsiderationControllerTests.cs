using Allrpcs.Application.Characters;
using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Dialogue;
using Allrpcs.Domain.Identifiers;

namespace Allrpcs.Application.Tests.Characters;

public sealed class CharacterDialogueMisquotationEvidenceConsiderationControllerTests
{
    [Fact]
    public async Task ObtainsOneDecisionForExactRequestAndReturnsValidatedResult()
    {
        ControllerInput input = CreateInput();
        CharacterDialogueMisquotationEvidenceDecision decision = new(
            input.Actor.Identity.Id,
            input.Evidence);
        RecordingModel model = new(decision);
        using CancellationTokenSource cancellation = new();

        CharacterDialogueMisquotationEvidenceConsiderationResult result =
            await CharacterDialogueMisquotationEvidenceConsiderationController.DecideAsync(
                model,
                input.Request,
                cancellation.Token);

        Assert.Equal(1, model.CallCount);
        Assert.Same(input.Request, model.Request);
        Assert.Equal(cancellation.Token, model.CancellationToken);
        Assert.Same(input.Request, result.Request);
        Assert.Same(decision, result.Decision);
        Assert.Same(input.Evidence, result.SelectedEvidence);
    }

    [Fact]
    public async Task RetainsExactAcceptedAbstention()
    {
        ControllerInput input = CreateInput();
        CharacterDialogueMisquotationEvidenceDecision decision = new(
            input.Actor.Identity.Id,
            null);

        CharacterDialogueMisquotationEvidenceConsiderationResult result =
            await CharacterDialogueMisquotationEvidenceConsiderationController.DecideAsync(
                new RecordingModel(decision),
                input.Request,
                CancellationToken.None);

        Assert.Same(decision, result.Decision);
        Assert.Null(result.SelectedEvidence);
    }

    [Fact]
    public async Task RejectsInvalidOrNullDecisionWithoutChangingRequest()
    {
        ControllerInput input = CreateInput();
        CharacterDialogueMisquotationEvidenceDecision invalid = new(
            new CharacterId(input.Actor.Identity.Id.Value),
            input.Evidence);

        await Assert.ThrowsAsync<ArgumentException>(async () =>
            await CharacterDialogueMisquotationEvidenceConsiderationController.DecideAsync(
                new RecordingModel(invalid),
                input.Request,
                CancellationToken.None));
        await Assert.ThrowsAsync<ArgumentNullException>(async () =>
            await CharacterDialogueMisquotationEvidenceConsiderationController.DecideAsync(
                new NullModel(),
                input.Request,
                CancellationToken.None));
        Assert.Same(input.Actor, input.Request.Actor);
        Assert.Same(input.Evidence, Assert.Single(input.Request.Candidates));
        Assert.Same(input.SourceEvent, input.Evidence.Occurrence.Proposal.SourceEvent);
        Assert.Same(input.DeliveredEvent, input.Evidence.Occurrence.DeliveredEvent);
    }

    [Fact]
    public async Task RejectsNullsAndPreCancellationBeforeModelInvocation()
    {
        ControllerInput input = CreateInput();
        RecordingModel model = new(new(input.Actor.Identity.Id, null));
        using CancellationTokenSource cancellation = new();
        cancellation.Cancel();

        await Assert.ThrowsAsync<ArgumentNullException>(async () =>
            await CharacterDialogueMisquotationEvidenceConsiderationController.DecideAsync(
                null!,
                input.Request,
                CancellationToken.None));
        await Assert.ThrowsAsync<ArgumentNullException>(async () =>
            await CharacterDialogueMisquotationEvidenceConsiderationController.DecideAsync(
                model,
                null!,
                CancellationToken.None));
        await Assert.ThrowsAnyAsync<OperationCanceledException>(async () =>
            await CharacterDialogueMisquotationEvidenceConsiderationController.DecideAsync(
                model,
                input.Request,
                cancellation.Token));
        Assert.Equal(0, model.CallCount);
    }

    [Fact]
    public async Task PropagatesModelCancellationAndFailureUnchanged()
    {
        ControllerInput input = CreateInput();
        using CancellationTokenSource cancellation = new();
        cancellation.Cancel();
        InvalidOperationException expected = new("model failed");

        OperationCanceledException cancelled = await Assert.ThrowsAnyAsync<OperationCanceledException>(
            async () => await CharacterDialogueMisquotationEvidenceConsiderationController.DecideAsync(
                new CancellingModel(cancellation.Token),
                input.Request,
                CancellationToken.None));
        InvalidOperationException failed = await Assert.ThrowsAsync<InvalidOperationException>(
            async () => await CharacterDialogueMisquotationEvidenceConsiderationController.DecideAsync(
                new FailingModel(expected),
                input.Request,
                CancellationToken.None));

        Assert.Equal(cancellation.Token, cancelled.CancellationToken);
        Assert.Same(expected, failed);
    }

    [Fact]
    public void ExposesOnlyOneProviderNeutralModelAndStaticDecisionOperation()
    {
        Type modelType = typeof(ICharacterDialogueMisquotationEvidenceDecisionModel);
        System.Reflection.MethodInfo modelMethod = Assert.Single(modelType.GetMethods());
        Assert.Equal("DecideAsync", modelMethod.Name);
        Assert.Equal(
            typeof(ValueTask<CharacterDialogueMisquotationEvidenceDecision>),
            modelMethod.ReturnType);
        Assert.Equal(
            [
                typeof(CharacterDialogueMisquotationEvidenceConsiderationRequest),
                typeof(CancellationToken),
            ],
            modelMethod.GetParameters().Select(parameter => parameter.ParameterType));
        Assert.Empty(modelType.GetProperties());

        Type controllerType = typeof(CharacterDialogueMisquotationEvidenceConsiderationController);
        System.Reflection.MethodInfo controllerMethod = Assert.Single(controllerType.GetMethods().Where(
            candidate => candidate.DeclaringType == controllerType));
        Assert.Equal("DecideAsync", controllerMethod.Name);
        Assert.True(controllerMethod.IsStatic);
        Assert.Equal(
            typeof(Task<CharacterDialogueMisquotationEvidenceConsiderationResult>),
            controllerMethod.ReturnType);
        Assert.Equal(
            [
                typeof(ICharacterDialogueMisquotationEvidenceDecisionModel),
                typeof(CharacterDialogueMisquotationEvidenceConsiderationRequest),
                typeof(CancellationToken),
            ],
            controllerMethod.GetParameters().Select(parameter => parameter.ParameterType));
        Assert.Empty(controllerType.GetProperties());
        Assert.Empty(controllerType.GetConstructors());
        Assert.True(controllerType.IsAbstract && controllerType.IsSealed);
    }

    private static ControllerInput CreateInput()
    {
        Character actor = RepresentativeEnemyAgentCharacterSource.Create();
        CharacterId witnessId = new("seeker");
        CharacterId sourceSpeakerId = new("sage");
        DialogueEvent sourceEvent = new(
            new DialogueDelivery(
                new DialogueUtterance(sourceSpeakerId, "Use the clean herb now."),
                DialogueDeliveryMode.OrdinarySpeech),
            new DialogueAudienceSnapshot(
                sourceSpeakerId,
                [actor.Identity.Id, witnessId]));
        CharacterDialogueMisquotationProposal proposal = new(
            actor.Identity.Id,
            sourceEvent,
            "The sage said the remedy should wait for royal approval.",
            "assess-local-influence");
        DialogueEvent deliveredEvent = new(
            new DialogueDelivery(
                new DialogueUtterance(proposal.ActorId, proposal.MisquotedText),
                DialogueDeliveryMode.OrdinarySpeech),
            new DialogueAudienceSnapshot(proposal.ActorId, [witnessId]));
        CharacterDialogueMisquotationEvidence evidence = new(
            new CharacterDialogueMisquotationOccurrence(proposal, deliveredEvent),
            witnessId);
        CharacterDialogueMisquotationEvidenceConsiderationRequest request = new(
            actor,
            [evidence]);
        return new(actor, sourceEvent, deliveredEvent, evidence, request);
    }

    private sealed class RecordingModel(CharacterDialogueMisquotationEvidenceDecision decision)
        : ICharacterDialogueMisquotationEvidenceDecisionModel
    {
        public int CallCount { get; private set; }

        public CharacterDialogueMisquotationEvidenceConsiderationRequest? Request { get; private set; }

        public CancellationToken CancellationToken { get; private set; }

        public ValueTask<CharacterDialogueMisquotationEvidenceDecision> DecideAsync(
            CharacterDialogueMisquotationEvidenceConsiderationRequest request,
            CancellationToken cancellationToken)
        {
            CallCount++;
            Request = request;
            CancellationToken = cancellationToken;
            return ValueTask.FromResult(decision);
        }
    }

    private sealed class NullModel : ICharacterDialogueMisquotationEvidenceDecisionModel
    {
        public ValueTask<CharacterDialogueMisquotationEvidenceDecision> DecideAsync(
            CharacterDialogueMisquotationEvidenceConsiderationRequest request,
            CancellationToken cancellationToken) =>
            ValueTask.FromResult<CharacterDialogueMisquotationEvidenceDecision>(null!);
    }

    private sealed class CancellingModel(CancellationToken cancellationToken)
        : ICharacterDialogueMisquotationEvidenceDecisionModel
    {
        public ValueTask<CharacterDialogueMisquotationEvidenceDecision> DecideAsync(
            CharacterDialogueMisquotationEvidenceConsiderationRequest request,
            CancellationToken ignoredCancellationToken) =>
            ValueTask.FromCanceled<CharacterDialogueMisquotationEvidenceDecision>(cancellationToken);
    }

    private sealed class FailingModel(Exception exception)
        : ICharacterDialogueMisquotationEvidenceDecisionModel
    {
        public ValueTask<CharacterDialogueMisquotationEvidenceDecision> DecideAsync(
            CharacterDialogueMisquotationEvidenceConsiderationRequest request,
            CancellationToken cancellationToken) =>
            ValueTask.FromException<CharacterDialogueMisquotationEvidenceDecision>(exception);
    }

    private sealed record ControllerInput(
        Character Actor,
        DialogueEvent SourceEvent,
        DialogueEvent DeliveredEvent,
        CharacterDialogueMisquotationEvidence Evidence,
        CharacterDialogueMisquotationEvidenceConsiderationRequest Request);
}
