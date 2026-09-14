using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Discovery;
using Allrpcs.Domain.Identifiers;
using Allrpcs.Domain.Locations;
using Allrpcs.Domain.Perception;
using Allrpcs.Domain.WorldObjects;

namespace Allrpcs.Domain.Tests.Characters;

public sealed class CharacterObjectSabotageMoveConsiderationResultTests
{
    [Fact]
    public void RetainsExactAcceptedSelectionAndCandidate()
    {
        ResultInput input = Input();
        CharacterObjectSabotageMoveDecision decision = new(
            input.Character.Identity.Id,
            input.Candidate.Evidence.Evidence.EvidenceId);

        CharacterObjectSabotageMoveConsiderationResult result = new(input.Request, decision);

        Assert.Same(input.Request, result.Request);
        Assert.Same(decision, result.Decision);
        Assert.Same(input.Candidate, result.SelectedCandidate);
    }

    [Fact]
    public void RetainsExactAcceptedAbstentionWithoutCandidate()
    {
        ResultInput input = Input();
        CharacterObjectSabotageMoveDecision decision = new(input.Character.Identity.Id, null);

        CharacterObjectSabotageMoveConsiderationResult result = new(input.Request, decision);

        Assert.Same(input.Request, result.Request);
        Assert.Same(decision, result.Decision);
        Assert.Null(result.SelectedCandidate);
    }

    [Fact]
    public void RejectsDistinctValueEqualCharacterOrEvidenceIdentifiers()
    {
        ResultInput input = Input();

        Assert.Throws<ArgumentException>(() => new CharacterObjectSabotageMoveConsiderationResult(
            input.Request,
            new CharacterObjectSabotageMoveDecision(
                new CharacterId(input.Character.Identity.Id.Value),
                input.Candidate.Evidence.Evidence.EvidenceId)));
        Assert.Throws<ArgumentException>(() => new CharacterObjectSabotageMoveConsiderationResult(
            input.Request,
            new CharacterObjectSabotageMoveDecision(
                input.Character.Identity.Id,
                new EvidenceId(input.Candidate.Evidence.Evidence.EvidenceId.Value))));
    }

    [Fact]
    public void RejectsUnknownOrNoLongerEligibleCandidateWithoutChangingInputs()
    {
        ResultInput input = Input();
        CharacterObjectSabotageMoveDecision unknown = new(
            input.Character.Identity.Id,
            new EvidenceId("unknown-evidence"));
        WorldObjectPlacement atDestination = new(input.Target, input.Destination);
        CharacterObjectSabotageMoveConsiderationRequest alreadyMoved = new(
            input.Character,
            input.PhysicalAccess,
            input.WorldObjects,
            new WorldObjectPlacementSnapshot([atDestination]),
            input.Evidence,
            [input.Candidate]);
        CharacterObjectSabotageMoveDecision selection = new(
            input.Character.Identity.Id,
            input.Candidate.Evidence.Evidence.EvidenceId);

        Assert.Throws<ArgumentException>(() =>
            new CharacterObjectSabotageMoveConsiderationResult(input.Request, unknown));
        Assert.Throws<ArgumentException>(() =>
            new CharacterObjectSabotageMoveConsiderationResult(alreadyMoved, selection));
        Assert.Same(input.SourcePlacement, Assert.Single(input.Placements.Entries));
        Assert.Empty(input.Evidence.Entries);
        Assert.Same(input.Candidate, Assert.Single(input.Request.Candidates));
    }

    [Fact]
    public void RejectsNullsAndExposesOnlyImmutableResultSurface()
    {
        ResultInput input = Input();
        CharacterObjectSabotageMoveDecision decision = new(input.Character.Identity.Id, null);

        Assert.Throws<ArgumentNullException>(() =>
            new CharacterObjectSabotageMoveConsiderationResult(null!, decision));
        Assert.Throws<ArgumentNullException>(() =>
            new CharacterObjectSabotageMoveConsiderationResult(input.Request, null!));

        Type resultType = typeof(CharacterObjectSabotageMoveConsiderationResult);
        System.Reflection.ConstructorInfo constructor = Assert.Single(resultType.GetConstructors());
        (string Name, Type Type, bool CanWrite)[] properties = resultType
            .GetProperties()
            .OrderBy(property => property.Name, StringComparer.Ordinal)
            .Select(property => (property.Name, property.PropertyType, property.CanWrite))
            .ToArray();

        Assert.Equal(
            [
                typeof(CharacterObjectSabotageMoveConsiderationRequest),
                typeof(CharacterObjectSabotageMoveDecision),
            ],
            constructor.GetParameters().Select(parameter => parameter.ParameterType));
        Assert.Equal(
            [
                ("Decision", typeof(CharacterObjectSabotageMoveDecision)),
                ("Request", typeof(CharacterObjectSabotageMoveConsiderationRequest)),
                ("SelectedCandidate", typeof(CharacterObjectSabotageMoveCandidate)),
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
        Location source = Location("village-square");
        Location destination = Location("abandoned-wagon");
        CharacterObjectSabotageOccurrence occurrence = new(
            new CharacterObjectSabotageProposal(
                character.Identity.Id,
                new CharacterActionAccessRequirement(
                    target.Identity.Id,
                    "assess-local-influence",
                    character.Knowledge.KnowledgeReferences.Single()),
                CharacterSabotageActionKind.Move),
            target);
        CharacterObjectSabotageMoveCandidate candidate = new(
            new CharacterObjectSabotageMoveDestination(occurrence, destination),
            new CharacterObjectSabotageEvidence(
                occurrence,
                new DiscoveryEvidence(
                    new EvidenceId("cask-drag-marks"),
                    target.Identity.Id,
                    VisibilityState.Noticed,
                    false)));
        CharacterPhysicalAccessSnapshot access = new(
            character.Identity.Id,
            [target.Identity.Id]);
        WorldObjectSnapshot worldObjects = new([target]);
        WorldObjectPlacement sourcePlacement = new(target, source);
        WorldObjectPlacementSnapshot placements = new([sourcePlacement]);
        DiscoveryEvidenceSnapshot evidence = new([]);
        CharacterObjectSabotageMoveConsiderationRequest request = new(
            character,
            access,
            worldObjects,
            placements,
            evidence,
            [candidate]);

        return new(
            character,
            target,
            destination,
            access,
            worldObjects,
            sourcePlacement,
            placements,
            evidence,
            candidate,
            request);
    }

    private static Location Location(string id) =>
        new(
            new LocationIdentity(new LocationId(id), id),
            new LocationDescription($"The authored {id} location."));

    private sealed record ResultInput(
        Character Character,
        WorldObject Target,
        Location Destination,
        CharacterPhysicalAccessSnapshot PhysicalAccess,
        WorldObjectSnapshot WorldObjects,
        WorldObjectPlacement SourcePlacement,
        WorldObjectPlacementSnapshot Placements,
        DiscoveryEvidenceSnapshot Evidence,
        CharacterObjectSabotageMoveCandidate Candidate,
        CharacterObjectSabotageMoveConsiderationRequest Request);
}
