using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Discovery;
using Allrpcs.Domain.Identifiers;
using Allrpcs.Domain.Locations;
using Allrpcs.Domain.Perception;
using Allrpcs.Domain.WorldObjects;

namespace Allrpcs.Domain.Tests.Characters;

public sealed class CharacterObjectSabotageMoveCandidateTests
{
    [Fact]
    public void RetainsExactDestinationBindingAndEvidenceCandidate()
    {
        CharacterObjectSabotageOccurrence occurrence = Occurrence();
        CharacterObjectSabotageMoveDestination destination = new(
            occurrence,
            Destination());
        CharacterObjectSabotageEvidence evidence = new(occurrence, Evidence());

        CharacterObjectSabotageMoveCandidate candidate = new(destination, evidence);

        Assert.Same(destination, candidate.Destination);
        Assert.Same(evidence, candidate.Evidence);
        Assert.Same(occurrence, candidate.Destination.Occurrence);
        Assert.Same(occurrence, candidate.Evidence.Occurrence);
        Assert.Same(occurrence.Target, candidate.Destination.Occurrence.Target);
        Assert.Same(evidence.Evidence, candidate.Evidence.Evidence);
    }

    [Fact]
    public void RejectsDistinctAndValueEqualOccurrenceAlignment()
    {
        CharacterObjectSabotageOccurrence exactOccurrence = Occurrence();
        CharacterObjectSabotageOccurrence valueEqualOccurrence = Occurrence();
        CharacterObjectSabotageMoveDestination destination = new(
            exactOccurrence,
            Destination());
        CharacterObjectSabotageEvidence distinctEvidence = new(
            valueEqualOccurrence,
            Evidence());

        Assert.NotSame(exactOccurrence, valueEqualOccurrence);
        Assert.Equal(
            exactOccurrence.Proposal.ActorId,
            valueEqualOccurrence.Proposal.ActorId);
        Assert.Equal(
            exactOccurrence.Target.Identity,
            valueEqualOccurrence.Target.Identity);
        Assert.Throws<ArgumentException>(() =>
            new CharacterObjectSabotageMoveCandidate(destination, distinctEvidence));
    }

    [Fact]
    public void RejectsNullsAndExposesOnlyClosedReadOnlyState()
    {
        CharacterObjectSabotageOccurrence occurrence = Occurrence();
        CharacterObjectSabotageMoveDestination destination = new(
            occurrence,
            Destination());
        CharacterObjectSabotageEvidence evidence = new(occurrence, Evidence());

        Assert.Throws<ArgumentNullException>(() =>
            new CharacterObjectSabotageMoveCandidate(null!, evidence));
        Assert.Throws<ArgumentNullException>(() =>
            new CharacterObjectSabotageMoveCandidate(destination, null!));

        Type candidateType = typeof(CharacterObjectSabotageMoveCandidate);
        (string Name, Type Type)[] expectedProperties =
        [
            ("Destination", typeof(CharacterObjectSabotageMoveDestination)),
            ("Evidence", typeof(CharacterObjectSabotageEvidence)),
        ];
        (string Name, Type Type, bool CanWrite)[] actualProperties = candidateType
            .GetProperties()
            .OrderBy(property => property.Name, StringComparer.Ordinal)
            .Select(property => (property.Name, property.PropertyType, property.CanWrite))
            .ToArray();
        string[] publicMethods = candidateType
            .GetMethods()
            .Where(method => method.DeclaringType == candidateType && !method.IsSpecialName)
            .Select(method => method.Name)
            .ToArray();

        Assert.Equal(
            expectedProperties,
            actualProperties.Select(property => (property.Name, property.Type)).ToArray());
        Assert.All(actualProperties, property => Assert.False(property.CanWrite));
        Assert.Empty(candidateType.GetEvents());
        Assert.Empty(publicMethods);
        Assert.True(candidateType.IsSealed);
    }

    private static CharacterObjectSabotageOccurrence Occurrence()
    {
        ObjectId targetId = new("village-water-cask");

        return new CharacterObjectSabotageOccurrence(
            new CharacterObjectSabotageProposal(
                new CharacterId("enemy-agent"),
                new CharacterActionAccessRequirement(
                    targetId,
                    "relocate-supplies",
                    new KnowledgeId("water-cask-contents")),
                CharacterSabotageActionKind.Move),
            new WorldObject(
                new WorldObjectIdentity(targetId, "Village water cask"),
                new WorldObjectDescription("A covered cask beside the village well.")));
    }

    private static Location Destination() =>
        new(
            new LocationIdentity(new LocationId("abandoned-wagon"), "Abandoned wagon"),
            new LocationDescription("A broken wagon rests beside the southern road."));

    private static DiscoveryEvidence Evidence() =>
        new(
            new EvidenceId("cask-drag-marks"),
            new ObjectId("village-water-cask"),
            VisibilityState.Noticed,
            false);
}
