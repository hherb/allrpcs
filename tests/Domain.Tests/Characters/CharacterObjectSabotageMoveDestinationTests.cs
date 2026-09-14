using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Identifiers;
using Allrpcs.Domain.Locations;
using Allrpcs.Domain.WorldObjects;

namespace Allrpcs.Domain.Tests.Characters;

public sealed class CharacterObjectSabotageMoveDestinationTests
{
    [Fact]
    public void RetainsExactMoveOccurrenceAndAuthoredDestination()
    {
        CharacterId actorId = new("enemy-agent");
        ObjectId targetId = new("village-water-cask");
        CharacterActionAccessRequirement accessRequirement = new(
            targetId,
            "relocate-supplies",
            new KnowledgeId("water-cask-contents"));
        WorldObject target = Target(targetId);
        CharacterObjectSabotageOccurrence occurrence = new(
            new CharacterObjectSabotageProposal(
                actorId,
                accessRequirement,
                CharacterSabotageActionKind.Move),
            target);
        Location destination = Destination();

        CharacterObjectSabotageMoveDestination binding = new(occurrence, destination);

        Assert.Same(occurrence, binding.Occurrence);
        Assert.Same(actorId, binding.Occurrence.Proposal.ActorId);
        Assert.Same(accessRequirement, binding.Occurrence.Proposal.AccessRequirement);
        Assert.Same(target, binding.Occurrence.Target);
        Assert.Same(destination, binding.Destination);
    }

    [Fact]
    public void RetainsOnlyTheExactIndependentlyComposedDestination()
    {
        Location selected = Destination();
        Location valueEqual = Destination();

        CharacterObjectSabotageMoveDestination binding = new(MoveOccurrence(), selected);

        Assert.NotSame(selected, valueEqual);
        Assert.Equal(selected.Identity, valueEqual.Identity);
        Assert.Equal(selected.Description, valueEqual.Description);
        Assert.Same(selected, binding.Destination);
        Assert.NotSame(valueEqual, binding.Destination);
    }

    [Theory]
    [MemberData(nameof(NonMoveActionKinds))]
    public void RejectsNonMoveOccurrences(CharacterSabotageActionKind actionKind)
    {
        CharacterObjectSabotageOccurrence occurrence = Occurrence(actionKind);
        Location destination = Destination();
        CharacterId actorId = occurrence.Proposal.ActorId;
        CharacterActionAccessRequirement accessRequirement =
            occurrence.Proposal.AccessRequirement;
        WorldObject target = occurrence.Target;
        LocationIdentity destinationIdentity = destination.Identity;
        LocationDescription destinationDescription = destination.Description;

        Assert.Throws<ArgumentException>(() =>
            new CharacterObjectSabotageMoveDestination(occurrence, destination));
        Assert.Same(actionKind, occurrence.Proposal.ActionKind);
        Assert.Same(actorId, occurrence.Proposal.ActorId);
        Assert.Same(accessRequirement, occurrence.Proposal.AccessRequirement);
        Assert.Same(target, occurrence.Target);
        Assert.Same(destinationIdentity, destination.Identity);
        Assert.Same(destinationDescription, destination.Description);
    }

    [Fact]
    public void RejectsNullsAndExposesOnlyClosedReadOnlyState()
    {
        CharacterObjectSabotageOccurrence occurrence = MoveOccurrence();
        Location destination = Destination();

        Assert.Throws<ArgumentNullException>(() =>
            new CharacterObjectSabotageMoveDestination(null!, destination));
        Assert.Throws<ArgumentNullException>(() =>
            new CharacterObjectSabotageMoveDestination(occurrence, null!));

        Type bindingType = typeof(CharacterObjectSabotageMoveDestination);
        (string Name, Type Type)[] expectedProperties =
        [
            ("Destination", typeof(Location)),
            ("Occurrence", typeof(CharacterObjectSabotageOccurrence)),
        ];
        (string Name, Type Type, bool CanWrite)[] actualProperties = bindingType
            .GetProperties()
            .OrderBy(property => property.Name, StringComparer.Ordinal)
            .Select(property => (property.Name, property.PropertyType, property.CanWrite))
            .ToArray();
        string[] publicMethods = bindingType
            .GetMethods()
            .Where(method => method.DeclaringType == bindingType && !method.IsSpecialName)
            .Select(method => method.Name)
            .ToArray();

        Assert.Equal(
            expectedProperties,
            actualProperties.Select(property => (property.Name, property.Type)).ToArray());
        Assert.All(actualProperties, property => Assert.False(property.CanWrite));
        Assert.Empty(bindingType.GetEvents());
        Assert.Empty(publicMethods);
        Assert.True(bindingType.IsSealed);
    }

    public static TheoryData<CharacterSabotageActionKind> NonMoveActionKinds => new()
    {
        CharacterSabotageActionKind.Conceal,
        CharacterSabotageActionKind.Substitute,
        CharacterSabotageActionKind.Contaminate,
    };

    private static CharacterObjectSabotageOccurrence MoveOccurrence() =>
        Occurrence(CharacterSabotageActionKind.Move);

    private static CharacterObjectSabotageOccurrence Occurrence(
        CharacterSabotageActionKind actionKind)
    {
        ObjectId targetId = new("village-water-cask");

        return new CharacterObjectSabotageOccurrence(
            new CharacterObjectSabotageProposal(
                new CharacterId("enemy-agent"),
                new CharacterActionAccessRequirement(
                    targetId,
                    "relocate-supplies",
                    new KnowledgeId("water-cask-contents")),
                actionKind),
            Target(targetId));
    }

    private static WorldObject Target(ObjectId? targetId = null) =>
        new(
            new WorldObjectIdentity(
                targetId ?? new ObjectId("village-water-cask"),
                "Village water cask"),
            new WorldObjectDescription("A covered cask beside the village well."));

    private static Location Destination() =>
        new(
            new LocationIdentity(new LocationId("abandoned-wagon"), "Abandoned wagon"),
            new LocationDescription("A broken wagon rests beside the southern road."));
}
