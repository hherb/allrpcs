using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Discovery;
using Allrpcs.Domain.Identifiers;
using Allrpcs.Domain.Locations;
using Allrpcs.Domain.Perception;
using Allrpcs.Domain.WorldObjects;

namespace Allrpcs.Domain.Tests.Characters;

public sealed class CharacterObjectSabotageMoveRecoveryCandidateTests
{
    [Fact]
    public void RetainsExactMoveCandidateAndPreMoveSourcePlacement()
    {
        RecoveryInput input = Input();

        CharacterObjectSabotageMoveRecoveryCandidate recovery = new(
            input.Candidate,
            input.SourcePlacement);

        Assert.Same(input.Candidate, recovery.Candidate);
        Assert.Same(input.SourcePlacement, recovery.SourcePlacement);
        Assert.Same(input.Occurrence, recovery.Candidate.Destination.Occurrence);
        Assert.Same(input.Target, recovery.SourcePlacement.WorldObject);
        Assert.Same(input.SourceLocation, recovery.SourcePlacement.Location);
        Assert.Same(input.Destination, recovery.Candidate.Destination.Destination);
        Assert.Same(input.Evidence, recovery.Candidate.Evidence);
    }

    [Fact]
    public void RejectsDistinctValueEqualTargetWithoutChangingInputs()
    {
        RecoveryInput input = Input();
        WorldObject valueEqualTarget = Target();
        WorldObjectPlacement valueEqualSource = new(
            valueEqualTarget,
            input.SourceLocation);

        Assert.Throws<ArgumentException>(() =>
            new CharacterObjectSabotageMoveRecoveryCandidate(
                input.Candidate,
                valueEqualSource));

        Assert.NotSame(input.Target, valueEqualTarget);
        Assert.Equal(input.Target.Identity, valueEqualTarget.Identity);
        Assert.Same(valueEqualTarget, valueEqualSource.WorldObject);
        Assert.Same(input.SourceLocation, valueEqualSource.Location);
        Assert.Same(input.Occurrence, input.Candidate.Destination.Occurrence);
        Assert.Same(input.SourcePlacement.WorldObject, input.Target);
    }

    [Fact]
    public void RejectsExactOrValueEqualDestinationAsSourceWithoutChangingInputs()
    {
        RecoveryInput input = Input();
        WorldObjectPlacement exactDestinationSource = new(
            input.Target,
            input.Destination);
        Location valueEqualDestination = new(
            new LocationIdentity(
                new LocationId(input.Destination.Identity.Id.Value),
                input.Destination.Identity.DisplayName),
            new LocationDescription(input.Destination.Description.Text));
        WorldObjectPlacement valueEqualDestinationSource = new(
            input.Target,
            valueEqualDestination);

        Assert.Throws<ArgumentException>(() =>
            new CharacterObjectSabotageMoveRecoveryCandidate(
                input.Candidate,
                exactDestinationSource));
        Assert.Throws<ArgumentException>(() =>
            new CharacterObjectSabotageMoveRecoveryCandidate(
                input.Candidate,
                valueEqualDestinationSource));

        Assert.NotSame(input.Destination, valueEqualDestination);
        Assert.Equal(input.Destination.Identity, valueEqualDestination.Identity);
        Assert.Same(input.Destination, exactDestinationSource.Location);
        Assert.Same(valueEqualDestination, valueEqualDestinationSource.Location);
        Assert.Same(input.SourceLocation, input.SourcePlacement.Location);
        Assert.Same(input.Destination, input.Candidate.Destination.Destination);
    }

    [Fact]
    public void RejectsNullsAndExposesOnlyClosedReadOnlyState()
    {
        RecoveryInput input = Input();

        Assert.Throws<ArgumentNullException>(() =>
            new CharacterObjectSabotageMoveRecoveryCandidate(
                null!,
                input.SourcePlacement));
        Assert.Throws<ArgumentNullException>(() =>
            new CharacterObjectSabotageMoveRecoveryCandidate(
                input.Candidate,
                null!));

        Type recoveryType = typeof(CharacterObjectSabotageMoveRecoveryCandidate);
        (string Name, Type Type)[] expectedProperties =
        [
            ("Candidate", typeof(CharacterObjectSabotageMoveCandidate)),
            ("SourcePlacement", typeof(WorldObjectPlacement)),
        ];
        (string Name, Type Type, bool CanWrite)[] actualProperties = recoveryType
            .GetProperties()
            .OrderBy(property => property.Name, StringComparer.Ordinal)
            .Select(property => (property.Name, property.PropertyType, property.CanWrite))
            .ToArray();
        string[] publicMethods = recoveryType
            .GetMethods()
            .Where(method => method.DeclaringType == recoveryType && !method.IsSpecialName)
            .Select(method => method.Name)
            .ToArray();

        Assert.Equal(
            expectedProperties,
            actualProperties.Select(property => (property.Name, property.Type)).ToArray());
        Assert.All(actualProperties, property => Assert.False(property.CanWrite));
        Assert.Empty(recoveryType.GetEvents());
        Assert.Empty(publicMethods);
        Assert.True(recoveryType.IsSealed);
    }

    private static RecoveryInput Input()
    {
        Character character = RepresentativeEnemyAgentCharacterSource.Create();
        WorldObject target = Target();
        Location sourceLocation = Location("village-square", "Village square");
        Location destination = Location("abandoned-wagon", "Abandoned wagon");
        CharacterObjectSabotageOccurrence occurrence = new(
            new CharacterObjectSabotageProposal(
                character.Identity.Id,
                new CharacterActionAccessRequirement(
                    target.Identity.Id,
                    "relocate-supplies",
                    character.Knowledge.KnowledgeReferences.Single()),
                CharacterSabotageActionKind.Move),
            target);
        CharacterObjectSabotageEvidence evidence = new(
            occurrence,
            new DiscoveryEvidence(
                new EvidenceId("cask-drag-marks"),
                target.Identity.Id,
                VisibilityState.Noticed,
                false));
        CharacterObjectSabotageMoveCandidate candidate = new(
            new CharacterObjectSabotageMoveDestination(occurrence, destination),
            evidence);
        WorldObjectPlacement sourcePlacement = new(target, sourceLocation);

        return new RecoveryInput(
            target,
            sourceLocation,
            destination,
            occurrence,
            evidence,
            candidate,
            sourcePlacement);
    }

    private static WorldObject Target() =>
        new(
            new WorldObjectIdentity(
                new ObjectId("village-water-cask"),
                "Village water cask"),
            new WorldObjectDescription("A covered cask beside the village well."));

    private static Location Location(string id, string displayName) =>
        new(
            new LocationIdentity(new LocationId(id), displayName),
            new LocationDescription($"Description for {id}."));

    private sealed record RecoveryInput(
        WorldObject Target,
        Location SourceLocation,
        Location Destination,
        CharacterObjectSabotageOccurrence Occurrence,
        CharacterObjectSabotageEvidence Evidence,
        CharacterObjectSabotageMoveCandidate Candidate,
        WorldObjectPlacement SourcePlacement);
}
