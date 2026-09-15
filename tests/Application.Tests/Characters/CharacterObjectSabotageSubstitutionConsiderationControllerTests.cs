using Allrpcs.Application.Characters;
using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Discovery;
using Allrpcs.Domain.Identifiers;
using Allrpcs.Domain.Perception;
using Allrpcs.Domain.WorldObjects;

namespace Allrpcs.Application.Tests.Characters;

public sealed class CharacterObjectSabotageSubstitutionConsiderationControllerTests
{
    [Fact]
    public async Task ObtainsOneDecisionForExactRequestAndReturnsValidatedResult()
    {
        ControllerInput input = Input();
        CharacterObjectSabotageSubstitutionDecision decision = new(
            input.Character.Identity.Id,
            input.Candidate.Evidence.Evidence.EvidenceId);
        RecordingModel model = new(decision);
        using CancellationTokenSource cancellation = new();

        CharacterObjectSabotageSubstitutionConsiderationResult result =
            await CharacterObjectSabotageSubstitutionConsiderationController.DecideAsync(
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
        CharacterObjectSabotageSubstitutionDecision decision = new(input.Character.Identity.Id, null);

        CharacterObjectSabotageSubstitutionConsiderationResult result =
            await CharacterObjectSabotageSubstitutionConsiderationController.DecideAsync(
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
        CharacterObjectSabotageSubstitutionDecision decision = new(
            new CharacterId(input.Character.Identity.Id.Value),
            input.Candidate.Evidence.Evidence.EvidenceId);

        await Assert.ThrowsAsync<ArgumentException>(async () =>
            await CharacterObjectSabotageSubstitutionConsiderationController.DecideAsync(
                new RecordingModel(decision),
                input.Request,
                CancellationToken.None));
        Assert.Empty(input.Request.Substitutions.Entries);
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
            await CharacterObjectSabotageSubstitutionConsiderationController.DecideAsync(
                null!,
                input.Request,
                CancellationToken.None));
        await Assert.ThrowsAsync<ArgumentNullException>(async () =>
            await CharacterObjectSabotageSubstitutionConsiderationController.DecideAsync(
                model,
                null!,
                CancellationToken.None));
        Assert.Equal(0, model.CallCount);
        await Assert.ThrowsAnyAsync<OperationCanceledException>(async () =>
            await CharacterObjectSabotageSubstitutionConsiderationController.DecideAsync(
                new CancellingModel(),
                input.Request,
                cancellation.Token));
    }

    [Fact]
    public void ExposesOnlyOneStaticDecisionOperation()
    {
        Type controllerType = typeof(CharacterObjectSabotageSubstitutionConsiderationController);
        System.Reflection.MethodInfo method = Assert.Single(controllerType.GetMethods().Where(
            candidate => candidate.DeclaringType == controllerType));

        Assert.Equal("DecideAsync", method.Name);
        Assert.True(method.IsStatic);
        Assert.Equal(
            typeof(Task<CharacterObjectSabotageSubstitutionConsiderationResult>),
            method.ReturnType);
        Assert.Equal(
            [
                typeof(ICharacterObjectSabotageSubstitutionDecisionModel),
                typeof(CharacterObjectSabotageSubstitutionConsiderationRequest),
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
        WorldObject target = WorldObject("village-water-cask");
        WorldObject replacement = WorldObject("tainted-water-cask");
        CharacterObjectSabotageOccurrence occurrence = new(
            new CharacterObjectSabotageProposal(
                character.Identity.Id,
                new CharacterActionAccessRequirement(
                    target.Identity.Id,
                    "assess-local-influence",
                    character.Knowledge.KnowledgeReferences.Single()),
                CharacterSabotageActionKind.Substitute),
            target);
        CharacterObjectSabotageSubstitutionCandidate candidate = new(
            new CharacterObjectSabotageSubstitution(occurrence, replacement),
            new CharacterObjectSabotageEvidence(
                occurrence,
                new DiscoveryEvidence(
                    new EvidenceId("water-cask-substitution-signs"),
                    target.Identity.Id,
                    VisibilityState.Noticed,
                    false)));
        CharacterPhysicalAccessSnapshot access = new(
            character.Identity.Id,
            [target.Identity.Id]);
        CharacterObjectSabotageSubstitutionConsiderationRequest request = new(
            character,
            access,
            new WorldObjectSnapshot([target]),
            new CharacterObjectSabotageSubstitutionSnapshot([]),
            new DiscoveryEvidenceSnapshot([]),
            [candidate]);

        return new(character, candidate, request);
    }

    private static WorldObject WorldObject(string id) =>
        new(
            new WorldObjectIdentity(new ObjectId(id), id),
            new WorldObjectDescription($"The authored {id} object."));

    private sealed class RecordingModel(CharacterObjectSabotageSubstitutionDecision decision)
        : ICharacterObjectSabotageSubstitutionDecisionModel
    {
        public int CallCount { get; private set; }

        public CharacterObjectSabotageSubstitutionConsiderationRequest? Request { get; private set; }

        public CancellationToken CancellationToken { get; private set; }

        public ValueTask<CharacterObjectSabotageSubstitutionDecision> DecideAsync(
            CharacterObjectSabotageSubstitutionConsiderationRequest request,
            CancellationToken cancellationToken)
        {
            CallCount++;
            Request = request;
            CancellationToken = cancellationToken;
            return ValueTask.FromResult(decision);
        }
    }

    private sealed class CancellingModel : ICharacterObjectSabotageSubstitutionDecisionModel
    {
        public ValueTask<CharacterObjectSabotageSubstitutionDecision> DecideAsync(
            CharacterObjectSabotageSubstitutionConsiderationRequest request,
            CancellationToken cancellationToken) =>
            ValueTask.FromCanceled<CharacterObjectSabotageSubstitutionDecision>(cancellationToken);
    }

    private sealed record ControllerInput(
        Character Character,
        CharacterObjectSabotageSubstitutionCandidate Candidate,
        CharacterObjectSabotageSubstitutionConsiderationRequest Request);
}
