using Allrpcs.Application.Characters;
using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Discovery;
using Allrpcs.Domain.Identifiers;
using Allrpcs.Domain.Perception;
using Allrpcs.Domain.WorldObjects;

namespace Allrpcs.Application.Tests.Characters;

public sealed class CharacterObjectSabotageContaminationConsiderationControllerTests
{
    [Fact]
    public async Task ObtainsOneDecisionForExactRequestAndReturnsValidatedResult()
    {
        ControllerInput input = Input();
        CharacterObjectSabotageContaminationDecision decision = new(
            input.Character.Identity.Id,
            input.Candidate.Evidence.EvidenceId);
        RecordingModel model = new(decision);
        using CancellationTokenSource cancellation = new();

        CharacterObjectSabotageContaminationConsiderationResult result =
            await CharacterObjectSabotageContaminationConsiderationController.DecideAsync(
                model,
                input.Request,
                cancellation.Token);

        Assert.Equal(1, model.CallCount);
        Assert.Same(input.Request, model.Request);
        Assert.Equal(cancellation.Token, model.CancellationToken);
        Assert.Same(input.Request, result.Request);
        Assert.Same(decision, result.Decision);
        Assert.Same(input.Candidate, result.SelectedCandidate);
    }

    [Fact]
    public async Task RetainsExactAcceptedAbstention()
    {
        ControllerInput input = Input();
        CharacterObjectSabotageContaminationDecision decision = new(input.Character.Identity.Id, null);

        CharacterObjectSabotageContaminationConsiderationResult result =
            await CharacterObjectSabotageContaminationConsiderationController.DecideAsync(
                new RecordingModel(decision),
                input.Request,
                CancellationToken.None);

        Assert.Same(decision, result.Decision);
        Assert.Null(result.SelectedCandidate);
    }

    [Fact]
    public async Task RejectsInvalidDecisionWithoutChangingRequestState()
    {
        ControllerInput input = Input();
        CharacterObjectSabotageContaminationDecision decision = new(
            new CharacterId(input.Character.Identity.Id.Value),
            input.Candidate.Evidence.EvidenceId);

        await Assert.ThrowsAsync<ArgumentException>(async () =>
            await CharacterObjectSabotageContaminationConsiderationController.DecideAsync(
                new RecordingModel(decision),
                input.Request,
                CancellationToken.None));
        Assert.Empty(input.Request.Contamination.ContaminatedObjectIds);
        Assert.Empty(input.Request.Evidence.Entries);
        Assert.Same(input.Candidate, Assert.Single(input.Request.Candidates));
    }

    [Fact]
    public async Task RejectsNullsBeforeModelInvocationAndPropagatesCancellation()
    {
        ControllerInput input = Input();
        RecordingModel model = new(new(input.Character.Identity.Id, null));
        using CancellationTokenSource cancellation = new();
        cancellation.Cancel();

        await Assert.ThrowsAsync<ArgumentNullException>(async () =>
            await CharacterObjectSabotageContaminationConsiderationController.DecideAsync(
                null!,
                input.Request,
                CancellationToken.None));
        await Assert.ThrowsAsync<ArgumentNullException>(async () =>
            await CharacterObjectSabotageContaminationConsiderationController.DecideAsync(
                model,
                null!,
                CancellationToken.None));
        Assert.Equal(0, model.CallCount);
        await Assert.ThrowsAnyAsync<OperationCanceledException>(async () =>
            await CharacterObjectSabotageContaminationConsiderationController.DecideAsync(
                new CancellingModel(),
                input.Request,
                cancellation.Token));
    }

    [Fact]
    public void ExposesOnlyOneStaticDecisionOperation()
    {
        Type controllerType = typeof(CharacterObjectSabotageContaminationConsiderationController);
        System.Reflection.MethodInfo method = Assert.Single(controllerType.GetMethods().Where(
            candidate => candidate.DeclaringType == controllerType));

        Assert.Equal("DecideAsync", method.Name);
        Assert.True(method.IsStatic);
        Assert.Equal(
            typeof(Task<CharacterObjectSabotageContaminationConsiderationResult>),
            method.ReturnType);
        Assert.Equal(
            [
                typeof(ICharacterObjectSabotageContaminationDecisionModel),
                typeof(CharacterObjectSabotageContaminationConsiderationRequest),
                typeof(CancellationToken),
            ],
            method.GetParameters().Select(parameter => parameter.ParameterType));
        Assert.Empty(controllerType.GetProperties());
        Assert.Empty(controllerType.GetConstructors());
        Assert.True(controllerType.IsAbstract && controllerType.IsSealed);
    }

    private static ControllerInput Input()
    {
        Character character = RepresentativeEnemyAgentCharacterSource.Create();
        WorldObject target = new(
            new WorldObjectIdentity(new ObjectId("village-water-cask"), "Water cask"),
            new WorldObjectDescription("A covered cask beside the village well."));
        CharacterObjectSabotageEvidence candidate = new(
            new CharacterObjectSabotageOccurrence(
                new CharacterObjectSabotageProposal(
                    character.Identity.Id,
                    new CharacterActionAccessRequirement(
                        target.Identity.Id,
                        "assess-local-influence",
                        character.Knowledge.KnowledgeReferences.Single()),
                    CharacterSabotageActionKind.Contaminate),
                target),
            new DiscoveryEvidence(
                new EvidenceId("bitter-cask-residue"),
                target.Identity.Id,
                VisibilityState.Noticed,
                false));
        CharacterObjectSabotageContaminationConsiderationRequest request = new(
            character,
            new CharacterPhysicalAccessSnapshot(character.Identity.Id, [target.Identity.Id]),
            new WorldObjectSnapshot([target]),
            new WorldObjectContaminationSnapshot([]),
            new DiscoveryEvidenceSnapshot([]),
            [candidate]);

        return new(character, candidate, request);
    }

    private sealed class RecordingModel(CharacterObjectSabotageContaminationDecision decision)
        : ICharacterObjectSabotageContaminationDecisionModel
    {
        public int CallCount { get; private set; }

        public CharacterObjectSabotageContaminationConsiderationRequest? Request { get; private set; }

        public CancellationToken CancellationToken { get; private set; }

        public ValueTask<CharacterObjectSabotageContaminationDecision> DecideAsync(
            CharacterObjectSabotageContaminationConsiderationRequest request,
            CancellationToken cancellationToken)
        {
            CallCount++;
            Request = request;
            CancellationToken = cancellationToken;
            return ValueTask.FromResult(decision);
        }
    }

    private sealed class CancellingModel : ICharacterObjectSabotageContaminationDecisionModel
    {
        public ValueTask<CharacterObjectSabotageContaminationDecision> DecideAsync(
            CharacterObjectSabotageContaminationConsiderationRequest request,
            CancellationToken cancellationToken) =>
            ValueTask.FromCanceled<CharacterObjectSabotageContaminationDecision>(cancellationToken);
    }

    private sealed record ControllerInput(
        Character Character,
        CharacterObjectSabotageEvidence Candidate,
        CharacterObjectSabotageContaminationConsiderationRequest Request);
}
