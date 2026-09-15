using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Discovery;
using Allrpcs.Domain.Identifiers;
using Allrpcs.Domain.Perception;
using Allrpcs.Domain.WorldObjects;

namespace Allrpcs.Domain.Tests.Characters;

public sealed class CharacterObjectSabotageSubstitutionConsiderationTransitionTests
{
    [Fact]
    public void AppliesExactAcceptedSelectionThroughExistingTransition()
    {
        TransitionInput input = Input();
        CharacterObjectSabotageSubstitutionConsiderationResult considerationResult = new(
            input.Request,
            new CharacterObjectSabotageSubstitutionDecision(
                input.Character.Identity.Id,
                input.Candidate.Evidence.Evidence.EvidenceId));

        CharacterObjectSabotageSubstitutionResult? result =
            CharacterObjectSabotageSubstitutionConsiderationTransition.Apply(considerationResult);

        Assert.NotNull(result);
        Assert.Same(input.Candidate.Substitution, Assert.Single(result.Substitutions.Entries));
        Assert.Same(input.Candidate.Evidence, result.EvidenceResult.Candidate);
        Assert.Same(input.Candidate.Evidence.Evidence, Assert.Single(result.EvidenceResult.Evidence.Entries));
    }

    [Fact]
    public void AcceptedAbstentionReturnsNoApplicationAndLeavesInputsUnchanged()
    {
        TransitionInput input = Input();
        CharacterObjectSabotageSubstitutionConsiderationResult considerationResult = new(
            input.Request,
            new CharacterObjectSabotageSubstitutionDecision(input.Character.Identity.Id, null));

        CharacterObjectSabotageSubstitutionResult? result =
            CharacterObjectSabotageSubstitutionConsiderationTransition.Apply(considerationResult);

        Assert.Null(result);
        Assert.Empty(input.Request.Substitutions.Entries);
        Assert.Empty(input.Request.Evidence.Entries);
        Assert.Same(input.Candidate, Assert.Single(input.Request.Candidates));
        Assert.Null(considerationResult.SelectedCandidate);
    }

    [Fact]
    public void CannotConstructStaleSelectionForApplicationAndLeavesInputsUnchanged()
    {
        TransitionInput input = Input();
        CharacterObjectSabotageSubstitutionConsiderationRequest staleRequest = new(
            input.Character,
            input.Request.PhysicalAccess,
            input.Request.WorldObjects,
            new CharacterObjectSabotageSubstitutionSnapshot([input.Candidate.Substitution]),
            input.Request.Evidence,
            [input.Candidate]);
        CharacterObjectSabotageSubstitutionDecision decision = new(
            input.Character.Identity.Id,
            input.Candidate.Evidence.Evidence.EvidenceId);

        Assert.Throws<ArgumentException>(() =>
            new CharacterObjectSabotageSubstitutionConsiderationResult(staleRequest, decision));
        Assert.Empty(input.Request.Substitutions.Entries);
        Assert.Empty(input.Request.Evidence.Entries);
    }

    [Fact]
    public void RejectsNullAndExposesOnlyOnePureApplicationOperation()
    {
        Assert.Throws<ArgumentNullException>(() =>
            CharacterObjectSabotageSubstitutionConsiderationTransition.Apply(null!));

        Type transitionType = typeof(CharacterObjectSabotageSubstitutionConsiderationTransition);
        System.Reflection.MethodInfo method = Assert.Single(transitionType.GetMethods().Where(
            candidate => candidate.DeclaringType == transitionType));

        Assert.Equal("Apply", method.Name);
        Assert.True(method.IsStatic);
        Assert.Equal(typeof(CharacterObjectSabotageSubstitutionResult), method.ReturnType);
        Assert.Equal(
            [typeof(CharacterObjectSabotageSubstitutionConsiderationResult)],
            method.GetParameters().Select(parameter => parameter.ParameterType));
        Assert.Empty(transitionType.GetProperties());
        Assert.Empty(transitionType.GetConstructors());
        Assert.True(transitionType.IsAbstract && transitionType.IsSealed);
    }

    private static TransitionInput Input()
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
        CharacterObjectSabotageSubstitutionConsiderationRequest request = new(
            character,
            new CharacterPhysicalAccessSnapshot(character.Identity.Id, [target.Identity.Id]),
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

    private sealed record TransitionInput(
        Character Character,
        CharacterObjectSabotageSubstitutionCandidate Candidate,
        CharacterObjectSabotageSubstitutionConsiderationRequest Request);
}
