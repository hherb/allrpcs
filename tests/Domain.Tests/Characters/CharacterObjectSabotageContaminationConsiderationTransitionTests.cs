using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Discovery;
using Allrpcs.Domain.Identifiers;
using Allrpcs.Domain.Perception;
using Allrpcs.Domain.WorldObjects;

namespace Allrpcs.Domain.Tests.Characters;

public sealed class CharacterObjectSabotageContaminationConsiderationTransitionTests
{
    [Fact]
    public void AppliesExactAcceptedSelectionThroughExistingTransition()
    {
        TransitionInput input = Input();
        CharacterObjectSabotageContaminationConsiderationResult considerationResult = new(
            input.Request,
            new CharacterObjectSabotageContaminationDecision(
                input.Character.Identity.Id,
                input.Candidate.Evidence.EvidenceId));

        CharacterObjectSabotageContaminationResult? result =
            CharacterObjectSabotageContaminationConsiderationTransition.Apply(considerationResult);

        Assert.NotNull(result);
        Assert.Same(input.Target.Identity.Id, Assert.Single(result.Contamination.ContaminatedObjectIds));
        Assert.Same(input.Candidate, result.EvidenceResult.Candidate);
        Assert.Same(input.Candidate.Evidence, Assert.Single(result.EvidenceResult.Evidence.Entries));
        Assert.NotSame(input.Request.Contamination, result.Contamination);
        Assert.NotSame(input.Request.Evidence, result.EvidenceResult.Evidence);
        Assert.Empty(input.Request.Contamination.ContaminatedObjectIds);
        Assert.Empty(input.Request.Evidence.Entries);
    }

    [Fact]
    public void AcceptedAbstentionReturnsNoApplicationAndLeavesInputsUnchanged()
    {
        TransitionInput input = Input();
        CharacterObjectSabotageContaminationConsiderationResult considerationResult = new(
            input.Request,
            new CharacterObjectSabotageContaminationDecision(input.Character.Identity.Id, null));

        CharacterObjectSabotageContaminationResult? result =
            CharacterObjectSabotageContaminationConsiderationTransition.Apply(considerationResult);

        Assert.Null(result);
        Assert.Empty(input.Request.Contamination.ContaminatedObjectIds);
        Assert.Empty(input.Request.Evidence.Entries);
        Assert.Same(input.Candidate, Assert.Single(input.Request.Candidates));
        Assert.Null(considerationResult.SelectedCandidate);
    }

    [Fact]
    public void CannotConstructStaleSelectionForApplicationAndLeavesInputsUnchanged()
    {
        TransitionInput input = Input();
        CharacterObjectSabotageContaminationConsiderationRequest staleRequest = new(
            input.Character,
            input.Request.PhysicalAccess,
            input.Request.WorldObjects,
            new WorldObjectContaminationSnapshot([input.Target.Identity.Id]),
            input.Request.Evidence,
            [input.Candidate]);
        CharacterObjectSabotageContaminationDecision decision = new(
            input.Character.Identity.Id,
            input.Candidate.Evidence.EvidenceId);

        Assert.Throws<ArgumentException>(() =>
            new CharacterObjectSabotageContaminationConsiderationResult(staleRequest, decision));
        Assert.Empty(input.Request.Contamination.ContaminatedObjectIds);
        Assert.Empty(input.Request.Evidence.Entries);
    }

    [Fact]
    public void RejectsNullAndExposesOnlyOnePureApplicationOperation()
    {
        Assert.Throws<ArgumentNullException>(() =>
            CharacterObjectSabotageContaminationConsiderationTransition.Apply(null!));

        Type transitionType = typeof(CharacterObjectSabotageContaminationConsiderationTransition);
        System.Reflection.MethodInfo method = Assert.Single(transitionType.GetMethods().Where(
            candidate => candidate.DeclaringType == transitionType));

        Assert.Equal("Apply", method.Name);
        Assert.True(method.IsStatic);
        Assert.Equal(typeof(CharacterObjectSabotageContaminationResult), method.ReturnType);
        Assert.Equal(
            [typeof(CharacterObjectSabotageContaminationConsiderationResult)],
            method.GetParameters().Select(parameter => parameter.ParameterType));
        Assert.Empty(transitionType.GetProperties());
        Assert.Empty(transitionType.GetConstructors());
        Assert.True(transitionType.IsAbstract && transitionType.IsSealed);
    }

    private static TransitionInput Input()
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

        return new(character, target, candidate, request);
    }

    private sealed record TransitionInput(
        Character Character,
        WorldObject Target,
        CharacterObjectSabotageEvidence Candidate,
        CharacterObjectSabotageContaminationConsiderationRequest Request);
}
