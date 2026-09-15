using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Discovery;
using Allrpcs.Domain.Identifiers;
using Allrpcs.Domain.Perception;
using Allrpcs.Domain.WorldObjects;

namespace Allrpcs.Domain.Tests.Characters;

public sealed class CharacterObjectSabotageSubstitutionConsiderationResultTests
{
    [Fact]
    public void RetainsExactAcceptedSelectionAndCandidate()
    {
        ResultInput input = Input();
        CharacterObjectSabotageSubstitutionDecision decision = new(
            input.Character.Identity.Id,
            input.Candidate.Evidence.Evidence.EvidenceId);

        CharacterObjectSabotageSubstitutionConsiderationResult result = new(input.Request, decision);

        Assert.Same(input.Request, result.Request);
        Assert.Same(decision, result.Decision);
        Assert.Same(input.Candidate, result.SelectedCandidate);
    }

    [Fact]
    public void RetainsExactAcceptedAbstentionWithoutCandidate()
    {
        ResultInput input = Input();
        CharacterObjectSabotageSubstitutionDecision decision = new(input.Character.Identity.Id, null);

        CharacterObjectSabotageSubstitutionConsiderationResult result = new(input.Request, decision);

        Assert.Same(input.Request, result.Request);
        Assert.Same(decision, result.Decision);
        Assert.Null(result.SelectedCandidate);
    }

    [Fact]
    public void RejectsDistinctValueEqualCharacterOrEvidenceIdentifiers()
    {
        ResultInput input = Input();

        Assert.Throws<ArgumentException>(() =>
            new CharacterObjectSabotageSubstitutionConsiderationResult(
                input.Request,
                new CharacterObjectSabotageSubstitutionDecision(
                    new CharacterId(input.Character.Identity.Id.Value),
                    input.Candidate.Evidence.Evidence.EvidenceId)));
        Assert.Throws<ArgumentException>(() =>
            new CharacterObjectSabotageSubstitutionConsiderationResult(
                input.Request,
                new CharacterObjectSabotageSubstitutionDecision(
                    input.Character.Identity.Id,
                    new EvidenceId(input.Candidate.Evidence.Evidence.EvidenceId.Value))));
    }

    [Fact]
    public void RejectsUnknownOrNoLongerEligibleCandidateWithoutChangingInputs()
    {
        ResultInput input = Input();
        CharacterObjectSabotageSubstitutionDecision unknown = new(
            input.Character.Identity.Id,
            new EvidenceId("unknown-evidence"));
        CharacterObjectSabotageSubstitutionConsiderationRequest alreadySubstituted = new(
            input.Character,
            input.PhysicalAccess,
            input.WorldObjects,
            new CharacterObjectSabotageSubstitutionSnapshot([input.Candidate.Substitution]),
            input.Evidence,
            [input.Candidate]);
        CharacterObjectSabotageSubstitutionDecision selection = new(
            input.Character.Identity.Id,
            input.Candidate.Evidence.Evidence.EvidenceId);

        Assert.Throws<ArgumentException>(() =>
            new CharacterObjectSabotageSubstitutionConsiderationResult(input.Request, unknown));
        Assert.Throws<ArgumentException>(() =>
            new CharacterObjectSabotageSubstitutionConsiderationResult(alreadySubstituted, selection));
        Assert.Empty(input.Substitutions.Entries);
        Assert.Empty(input.Evidence.Entries);
        Assert.Same(input.Candidate, Assert.Single(input.Request.Candidates));
    }

    [Fact]
    public void RejectsNullsAndExposesOnlyImmutableResultSurface()
    {
        ResultInput input = Input();
        CharacterObjectSabotageSubstitutionDecision decision = new(input.Character.Identity.Id, null);

        Assert.Throws<ArgumentNullException>(() =>
            new CharacterObjectSabotageSubstitutionConsiderationResult(null!, decision));
        Assert.Throws<ArgumentNullException>(() =>
            new CharacterObjectSabotageSubstitutionConsiderationResult(input.Request, null!));

        Type resultType = typeof(CharacterObjectSabotageSubstitutionConsiderationResult);
        System.Reflection.ConstructorInfo constructor = Assert.Single(resultType.GetConstructors());
        (string Name, Type Type, bool CanWrite)[] properties = resultType
            .GetProperties()
            .OrderBy(property => property.Name, StringComparer.Ordinal)
            .Select(property => (property.Name, property.PropertyType, property.CanWrite))
            .ToArray();

        Assert.Equal(
            [
                typeof(CharacterObjectSabotageSubstitutionConsiderationRequest),
                typeof(CharacterObjectSabotageSubstitutionDecision),
            ],
            constructor.GetParameters().Select(parameter => parameter.ParameterType));
        Assert.Equal(
            [
                ("Decision", typeof(CharacterObjectSabotageSubstitutionDecision)),
                ("Request", typeof(CharacterObjectSabotageSubstitutionConsiderationRequest)),
                ("SelectedCandidate", typeof(CharacterObjectSabotageSubstitutionCandidate)),
            ],
            properties.Select(property => (property.Name, property.Type)).ToArray());
        Assert.All(properties, property => Assert.False(property.CanWrite));
        Assert.Empty(resultType.GetEvents());
        Assert.True(resultType.IsSealed);
    }

    private static ResultInput Input()
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
        CharacterPhysicalAccessSnapshot physicalAccess = new(
            character.Identity.Id,
            [target.Identity.Id]);
        WorldObjectSnapshot worldObjects = new([target]);
        CharacterObjectSabotageSubstitutionSnapshot substitutions = new([]);
        DiscoveryEvidenceSnapshot evidence = new([]);
        CharacterObjectSabotageSubstitutionConsiderationRequest request = new(
            character,
            physicalAccess,
            worldObjects,
            substitutions,
            evidence,
            [candidate]);

        return new(
            character,
            physicalAccess,
            worldObjects,
            substitutions,
            evidence,
            candidate,
            request);
    }

    private static WorldObject WorldObject(string id) =>
        new(
            new WorldObjectIdentity(new ObjectId(id), id),
            new WorldObjectDescription($"The authored {id} object."));

    private sealed record ResultInput(
        Character Character,
        CharacterPhysicalAccessSnapshot PhysicalAccess,
        WorldObjectSnapshot WorldObjects,
        CharacterObjectSabotageSubstitutionSnapshot Substitutions,
        DiscoveryEvidenceSnapshot Evidence,
        CharacterObjectSabotageSubstitutionCandidate Candidate,
        CharacterObjectSabotageSubstitutionConsiderationRequest Request);
}
