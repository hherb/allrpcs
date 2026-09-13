using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Discovery;
using Allrpcs.Domain.Identifiers;
using Allrpcs.Domain.Perception;
using Allrpcs.Domain.WorldObjects;

namespace Allrpcs.Domain.Tests.Characters;

public sealed class CharacterObjectSabotageConcealmentConsiderationResultTests
{
    [Fact]
    public void RetainsExactAcceptedSelectionAndCandidate()
    {
        ResultInput input = Input();
        CharacterObjectSabotageConcealmentDecision decision = new(
            input.Character.Identity.Id,
            input.Candidate.Evidence.EvidenceId);

        CharacterObjectSabotageConcealmentConsiderationResult result = new(input.Request, decision);

        Assert.Same(input.Request, result.Request);
        Assert.Same(decision, result.Decision);
        Assert.Same(input.Candidate, result.SelectedCandidate);
    }

    [Fact]
    public void RetainsExactAcceptedAbstentionWithoutCandidate()
    {
        ResultInput input = Input();
        CharacterObjectSabotageConcealmentDecision decision = new(input.Character.Identity.Id, null);

        CharacterObjectSabotageConcealmentConsiderationResult result = new(input.Request, decision);

        Assert.Same(input.Request, result.Request);
        Assert.Same(decision, result.Decision);
        Assert.Null(result.SelectedCandidate);
    }

    [Fact]
    public void RejectsDistinctValueEqualCharacterOrEvidenceIdentifiers()
    {
        ResultInput input = Input();

        Assert.Throws<ArgumentException>(() => new CharacterObjectSabotageConcealmentConsiderationResult(
            input.Request,
            new CharacterObjectSabotageConcealmentDecision(
                new CharacterId(input.Character.Identity.Id.Value),
                input.Candidate.Evidence.EvidenceId)));
        Assert.Throws<ArgumentException>(() => new CharacterObjectSabotageConcealmentConsiderationResult(
            input.Request,
            new CharacterObjectSabotageConcealmentDecision(
                input.Character.Identity.Id,
                new EvidenceId(input.Candidate.Evidence.EvidenceId.Value))));
    }

    [Fact]
    public void RejectsUnknownOrNoLongerEligibleCandidateWithoutChangingInputs()
    {
        ResultInput input = Input();
        CharacterObjectSabotageConcealmentDecision unknown = new(
            input.Character.Identity.Id,
            new EvidenceId("unknown-evidence"));
        CharacterObjectSabotageConcealmentConsiderationRequest concealed = new(
            input.Character,
            input.PhysicalAccess,
            input.WorldObjects,
            new WorldObjectConcealmentSnapshot([input.Target.Identity.Id]),
            input.Evidence,
            [input.Candidate]);
        CharacterObjectSabotageConcealmentDecision selection = new(
            input.Character.Identity.Id,
            input.Candidate.Evidence.EvidenceId);

        Assert.Throws<ArgumentException>(() =>
            new CharacterObjectSabotageConcealmentConsiderationResult(input.Request, unknown));
        Assert.Throws<ArgumentException>(() =>
            new CharacterObjectSabotageConcealmentConsiderationResult(concealed, selection));
        Assert.Empty(input.Concealment.ConcealedObjectIds);
        Assert.Empty(input.Evidence.Entries);
        Assert.Same(input.Candidate, Assert.Single(input.Request.Candidates));
    }

    [Fact]
    public void RejectsNullsAndExposesOnlyImmutableResultSurface()
    {
        ResultInput input = Input();
        CharacterObjectSabotageConcealmentDecision decision = new(input.Character.Identity.Id, null);

        Assert.Throws<ArgumentNullException>(() =>
            new CharacterObjectSabotageConcealmentConsiderationResult(null!, decision));
        Assert.Throws<ArgumentNullException>(() =>
            new CharacterObjectSabotageConcealmentConsiderationResult(input.Request, null!));

        Type resultType = typeof(CharacterObjectSabotageConcealmentConsiderationResult);
        System.Reflection.ConstructorInfo constructor = Assert.Single(resultType.GetConstructors());
        (string Name, Type Type, bool CanWrite)[] properties = resultType
            .GetProperties()
            .OrderBy(property => property.Name, StringComparer.Ordinal)
            .Select(property => (property.Name, property.PropertyType, property.CanWrite))
            .ToArray();

        Assert.Equal(
            [
                typeof(CharacterObjectSabotageConcealmentConsiderationRequest),
                typeof(CharacterObjectSabotageConcealmentDecision),
            ],
            constructor.GetParameters().Select(parameter => parameter.ParameterType));
        Assert.Equal(
            [
                ("Decision", typeof(CharacterObjectSabotageConcealmentDecision)),
                ("Request", typeof(CharacterObjectSabotageConcealmentConsiderationRequest)),
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

        return new(
            character,
            target,
            physicalAccess,
            worldObjects,
            concealment,
            evidence,
            candidate,
            request);
    }

    private sealed record ResultInput(
        Character Character,
        WorldObject Target,
        CharacterPhysicalAccessSnapshot PhysicalAccess,
        WorldObjectSnapshot WorldObjects,
        WorldObjectConcealmentSnapshot Concealment,
        DiscoveryEvidenceSnapshot Evidence,
        CharacterObjectSabotageEvidence Candidate,
        CharacterObjectSabotageConcealmentConsiderationRequest Request);
}
