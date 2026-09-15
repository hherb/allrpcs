using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Discovery;
using Allrpcs.Domain.Identifiers;
using Allrpcs.Domain.Perception;
using Allrpcs.Domain.WorldObjects;

namespace Allrpcs.Domain.Tests.Characters;

public sealed class CharacterObjectSabotageContaminationConsiderationResultTests
{
    [Fact]
    public void RetainsExactAcceptedSelectionAndCandidate()
    {
        ResultInput input = Input();
        CharacterObjectSabotageContaminationDecision decision = new(
            input.Character.Identity.Id,
            input.Candidate.Evidence.EvidenceId);

        CharacterObjectSabotageContaminationConsiderationResult result = new(input.Request, decision);

        Assert.Same(input.Request, result.Request);
        Assert.Same(decision, result.Decision);
        Assert.Same(input.Candidate, result.SelectedCandidate);
    }

    [Fact]
    public void RetainsExactAcceptedAbstentionWithoutCandidate()
    {
        ResultInput input = Input();
        CharacterObjectSabotageContaminationDecision decision = new(input.Character.Identity.Id, null);

        CharacterObjectSabotageContaminationConsiderationResult result = new(input.Request, decision);

        Assert.Same(input.Request, result.Request);
        Assert.Same(decision, result.Decision);
        Assert.Null(result.SelectedCandidate);
    }

    [Fact]
    public void RejectsDistinctValueEqualCharacterOrEvidenceIdentifiers()
    {
        ResultInput input = Input();

        Assert.Throws<ArgumentException>(() =>
            new CharacterObjectSabotageContaminationConsiderationResult(
                input.Request,
                new CharacterObjectSabotageContaminationDecision(
                    new CharacterId(input.Character.Identity.Id.Value),
                    input.Candidate.Evidence.EvidenceId)));
        Assert.Throws<ArgumentException>(() =>
            new CharacterObjectSabotageContaminationConsiderationResult(
                input.Request,
                new CharacterObjectSabotageContaminationDecision(
                    input.Character.Identity.Id,
                    new EvidenceId(input.Candidate.Evidence.EvidenceId.Value))));
    }

    [Fact]
    public void RejectsUnknownOrNoLongerEligibleCandidateWithoutChangingInputs()
    {
        ResultInput input = Input();
        CharacterObjectSabotageContaminationDecision unknown = new(
            input.Character.Identity.Id,
            new EvidenceId("unknown-evidence"));
        CharacterObjectSabotageContaminationConsiderationRequest alreadyContaminated = new(
            input.Character,
            input.PhysicalAccess,
            input.WorldObjects,
            new WorldObjectContaminationSnapshot([input.Target.Identity.Id]),
            input.Evidence,
            [input.Candidate]);
        CharacterObjectSabotageContaminationDecision selection = new(
            input.Character.Identity.Id,
            input.Candidate.Evidence.EvidenceId);

        Assert.Throws<ArgumentException>(() =>
            new CharacterObjectSabotageContaminationConsiderationResult(input.Request, unknown));
        Assert.Throws<ArgumentException>(() =>
            new CharacterObjectSabotageContaminationConsiderationResult(alreadyContaminated, selection));
        Assert.Empty(input.Contamination.ContaminatedObjectIds);
        Assert.Empty(input.Evidence.Entries);
        Assert.Same(input.Candidate, Assert.Single(input.Request.Candidates));
    }

    [Fact]
    public void RejectsNullsAndExposesOnlyImmutableResultSurface()
    {
        ResultInput input = Input();
        CharacterObjectSabotageContaminationDecision decision = new(input.Character.Identity.Id, null);

        Assert.Throws<ArgumentNullException>(() =>
            new CharacterObjectSabotageContaminationConsiderationResult(null!, decision));
        Assert.Throws<ArgumentNullException>(() =>
            new CharacterObjectSabotageContaminationConsiderationResult(input.Request, null!));

        Type resultType = typeof(CharacterObjectSabotageContaminationConsiderationResult);
        System.Reflection.ConstructorInfo constructor = Assert.Single(resultType.GetConstructors());
        (string Name, Type Type, bool CanWrite)[] properties = resultType
            .GetProperties()
            .OrderBy(property => property.Name, StringComparer.Ordinal)
            .Select(property => (property.Name, property.PropertyType, property.CanWrite))
            .ToArray();

        Assert.Equal(
            [
                typeof(CharacterObjectSabotageContaminationConsiderationRequest),
                typeof(CharacterObjectSabotageContaminationDecision),
            ],
            constructor.GetParameters().Select(parameter => parameter.ParameterType));
        Assert.Equal(
            [
                ("Decision", typeof(CharacterObjectSabotageContaminationDecision)),
                ("Request", typeof(CharacterObjectSabotageContaminationConsiderationRequest)),
                ("SelectedCandidate", typeof(CharacterObjectSabotageEvidence)),
            ],
            properties.Select(property => (property.Name, property.Type)).ToArray());
        Assert.All(properties, property => Assert.False(property.CanWrite));
        Assert.Empty(resultType.GetEvents());
        Assert.True(resultType.IsSealed);
    }

    private static ResultInput Input()
    {
        Character character = RepresentativeEnemyAgentCharacterSource.Create();
        WorldObject target = new(
            new WorldObjectIdentity(new ObjectId("village-water-cask"), "Water cask"),
            new WorldObjectDescription("A covered cask beside the village well."));
        CharacterPhysicalAccessSnapshot physicalAccess = new(
            character.Identity.Id,
            [target.Identity.Id]);
        WorldObjectSnapshot worldObjects = new([target]);
        WorldObjectContaminationSnapshot contamination = new([]);
        DiscoveryEvidenceSnapshot evidence = new([]);
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
            physicalAccess,
            worldObjects,
            contamination,
            evidence,
            [candidate]);

        return new(
            character,
            target,
            physicalAccess,
            worldObjects,
            contamination,
            evidence,
            candidate,
            request);
    }

    private sealed record ResultInput(
        Character Character,
        WorldObject Target,
        CharacterPhysicalAccessSnapshot PhysicalAccess,
        WorldObjectSnapshot WorldObjects,
        WorldObjectContaminationSnapshot Contamination,
        DiscoveryEvidenceSnapshot Evidence,
        CharacterObjectSabotageEvidence Candidate,
        CharacterObjectSabotageContaminationConsiderationRequest Request);
}
