using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Discovery;
using Allrpcs.Domain.Identifiers;
using Allrpcs.Domain.Perception;
using Allrpcs.Domain.WorldObjects;

namespace Allrpcs.Domain.Tests.Characters;

public sealed class CharacterObjectSabotageConcealmentConsiderationTransitionTests
{
    [Fact]
    public void AppliesExactAcceptedSelectionThroughExistingTransition()
    {
        TransitionInput input = Input();
        CharacterObjectSabotageConcealmentConsiderationResult considerationResult = new(
            input.Request,
            new CharacterObjectSabotageConcealmentDecision(
                input.Character.Identity.Id,
                input.Candidate.Evidence.EvidenceId));

        CharacterObjectSabotageConcealmentResult? result =
            CharacterObjectSabotageConcealmentConsiderationTransition.Apply(considerationResult);

        Assert.NotNull(result);
        Assert.Same(input.Candidate, result.EvidenceResult.Candidate);
        Assert.Same(
            input.Candidate.Occurrence.Target.Identity.Id,
            Assert.Single(result.Concealment.ConcealedObjectIds));
        Assert.Same(input.Candidate.Evidence, Assert.Single(result.EvidenceResult.Evidence.Entries));
    }

    [Fact]
    public void AcceptedAbstentionReturnsNoApplicationAndLeavesInputsUnchanged()
    {
        TransitionInput input = Input();
        CharacterObjectSabotageConcealmentConsiderationResult considerationResult = new(
            input.Request,
            new CharacterObjectSabotageConcealmentDecision(input.Character.Identity.Id, null));

        CharacterObjectSabotageConcealmentResult? result =
            CharacterObjectSabotageConcealmentConsiderationTransition.Apply(considerationResult);

        Assert.Null(result);
        Assert.Empty(input.Request.Concealment.ConcealedObjectIds);
        Assert.Empty(input.Request.Evidence.Entries);
        Assert.Same(input.Candidate, Assert.Single(input.Request.Candidates));
        Assert.Null(considerationResult.SelectedCandidate);
    }

    [Fact]
    public void RejectsNullAndExposesOnlyOnePureApplicationOperation()
    {
        Assert.Throws<ArgumentNullException>(() =>
            CharacterObjectSabotageConcealmentConsiderationTransition.Apply(null!));

        Type transitionType = typeof(CharacterObjectSabotageConcealmentConsiderationTransition);
        System.Reflection.MethodInfo method = Assert.Single(transitionType.GetMethods().Where(
            candidate => candidate.DeclaringType == transitionType));

        Assert.Equal("Apply", method.Name);
        Assert.True(method.IsStatic);
        Assert.Equal(typeof(CharacterObjectSabotageConcealmentResult), method.ReturnType);
        Assert.Equal(
            [typeof(CharacterObjectSabotageConcealmentConsiderationResult)],
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
        CharacterPhysicalAccessSnapshot physicalAccess = new(
            character.Identity.Id,
            [target.Identity.Id]);
        WorldObjectSnapshot worldObjects = new([target]);
        WorldObjectConcealmentSnapshot concealment = new([]);
        DiscoveryEvidenceSnapshot evidence = new([]);
        CharacterObjectSabotageEvidence candidate = new(
            new CharacterObjectSabotageOccurrence(
                new CharacterObjectSabotageProposal(
                    character.Identity.Id,
                    new CharacterActionAccessRequirement(
                        target.Identity.Id,
                        "assess-local-influence",
                        character.Knowledge.KnowledgeReferences.Single()),
                    CharacterSabotageActionKind.Conceal),
                target),
            new DiscoveryEvidence(
                new EvidenceId("disturbed-cask-cover"),
                target.Identity.Id,
                VisibilityState.Noticed,
                false));
        CharacterObjectSabotageConcealmentConsiderationRequest request = new(
            character,
            physicalAccess,
            worldObjects,
            concealment,
            evidence,
            [candidate]);

        return new(character, candidate, request);
    }

    private sealed record TransitionInput(
        Character Character,
        CharacterObjectSabotageEvidence Candidate,
        CharacterObjectSabotageConcealmentConsiderationRequest Request);
}
