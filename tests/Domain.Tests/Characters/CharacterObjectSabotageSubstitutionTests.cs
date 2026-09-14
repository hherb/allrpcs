using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Identifiers;
using Allrpcs.Domain.WorldObjects;

namespace Allrpcs.Domain.Tests.Characters;

public sealed class CharacterObjectSabotageSubstitutionTests
{
    [Fact]
    public void RetainsExactSubstituteOccurrenceAndAuthoredReplacement()
    {
        CharacterId actorId = new("enemy-agent");
        ObjectId targetId = new("village-water-cask");
        CharacterActionAccessRequirement accessRequirement = new(
            targetId,
            "substitute-supplies",
            new KnowledgeId("water-cask-contents"));
        WorldObject target = Target(targetId);
        CharacterObjectSabotageOccurrence occurrence = new(
            new CharacterObjectSabotageProposal(
                actorId,
                accessRequirement,
                CharacterSabotageActionKind.Substitute),
            target);
        WorldObject replacement = Replacement();

        CharacterObjectSabotageSubstitution substitution = new(occurrence, replacement);

        Assert.Same(occurrence, substitution.Occurrence);
        Assert.Same(actorId, substitution.Occurrence.Proposal.ActorId);
        Assert.Same(accessRequirement, substitution.Occurrence.Proposal.AccessRequirement);
        Assert.Same(target, substitution.Occurrence.Target);
        Assert.Same(replacement, substitution.Replacement);
    }

    [Theory]
    [MemberData(nameof(NonSubstituteActionKinds))]
    public void RejectsNonSubstituteOccurrences(CharacterSabotageActionKind actionKind)
    {
        CharacterObjectSabotageOccurrence occurrence = Occurrence(actionKind);
        WorldObject replacement = Replacement();
        CharacterObjectSabotageProposal proposal = occurrence.Proposal;
        WorldObject target = occurrence.Target;
        WorldObjectIdentity replacementIdentity = replacement.Identity;
        WorldObjectDescription replacementDescription = replacement.Description;

        Assert.Throws<ArgumentException>(() =>
            new CharacterObjectSabotageSubstitution(occurrence, replacement));
        Assert.Same(proposal, occurrence.Proposal);
        Assert.Same(target, occurrence.Target);
        Assert.Same(actionKind, occurrence.Proposal.ActionKind);
        Assert.Same(replacementIdentity, replacement.Identity);
        Assert.Same(replacementDescription, replacement.Description);
    }

    [Fact]
    public void RejectsTheExactTargetAndAValueEqualReplacementIdentifier()
    {
        CharacterObjectSabotageOccurrence occurrence = SubstituteOccurrence();
        WorldObject valueEqualReplacement = Target(
            new ObjectId(occurrence.Target.Identity.Id.Value));

        Assert.Throws<ArgumentException>(() =>
            new CharacterObjectSabotageSubstitution(occurrence, occurrence.Target));
        Assert.Throws<ArgumentException>(() =>
            new CharacterObjectSabotageSubstitution(occurrence, valueEqualReplacement));
        Assert.NotSame(occurrence.Target, valueEqualReplacement);
        Assert.Equal(occurrence.Target.Identity.Id, valueEqualReplacement.Identity.Id);
    }

    [Fact]
    public void LeavesSourceObjectsUnchangedAndExposesOnlyClosedReadOnlyState()
    {
        CharacterObjectSabotageOccurrence occurrence = SubstituteOccurrence();
        WorldObject replacement = Replacement();
        WorldObjectIdentity targetIdentity = occurrence.Target.Identity;
        WorldObjectDescription targetDescription = occurrence.Target.Description;
        WorldObjectIdentity replacementIdentity = replacement.Identity;
        WorldObjectDescription replacementDescription = replacement.Description;

        _ = new CharacterObjectSabotageSubstitution(occurrence, replacement);

        Assert.Same(targetIdentity, occurrence.Target.Identity);
        Assert.Same(targetDescription, occurrence.Target.Description);
        Assert.Same(replacementIdentity, replacement.Identity);
        Assert.Same(replacementDescription, replacement.Description);

        Type substitutionType = typeof(CharacterObjectSabotageSubstitution);
        (string Name, Type Type)[] expectedProperties =
        [
            ("Occurrence", typeof(CharacterObjectSabotageOccurrence)),
            ("Replacement", typeof(WorldObject)),
        ];
        (string Name, Type Type, bool CanWrite)[] actualProperties = substitutionType
            .GetProperties()
            .OrderBy(property => property.Name, StringComparer.Ordinal)
            .Select(property => (property.Name, property.PropertyType, property.CanWrite))
            .ToArray();
        string[] publicMethods = substitutionType
            .GetMethods()
            .Where(method => method.DeclaringType == substitutionType && !method.IsSpecialName)
            .Select(method => method.Name)
            .ToArray();

        Assert.Equal(
            expectedProperties,
            actualProperties.Select(property => (property.Name, property.Type)).ToArray());
        Assert.All(actualProperties, property => Assert.False(property.CanWrite));
        Assert.Empty(substitutionType.GetEvents());
        Assert.Empty(publicMethods);
        Assert.True(substitutionType.IsSealed);
    }

    [Fact]
    public void RejectsNulls()
    {
        CharacterObjectSabotageOccurrence occurrence = SubstituteOccurrence();
        WorldObject replacement = Replacement();

        Assert.Throws<ArgumentNullException>(() =>
            new CharacterObjectSabotageSubstitution(null!, replacement));
        Assert.Throws<ArgumentNullException>(() =>
            new CharacterObjectSabotageSubstitution(occurrence, null!));
    }

    public static TheoryData<CharacterSabotageActionKind> NonSubstituteActionKinds => new()
    {
        CharacterSabotageActionKind.Conceal,
        CharacterSabotageActionKind.Move,
        CharacterSabotageActionKind.Contaminate,
    };

    private static CharacterObjectSabotageOccurrence SubstituteOccurrence() =>
        Occurrence(CharacterSabotageActionKind.Substitute);

    private static CharacterObjectSabotageOccurrence Occurrence(
        CharacterSabotageActionKind actionKind)
    {
        ObjectId targetId = new("village-water-cask");

        return new CharacterObjectSabotageOccurrence(
            new CharacterObjectSabotageProposal(
                new CharacterId("enemy-agent"),
                new CharacterActionAccessRequirement(
                    targetId,
                    "substitute-supplies",
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

    private static WorldObject Replacement() =>
        new(
            new WorldObjectIdentity(new ObjectId("tainted-water-cask"), "Tainted water cask"),
            new WorldObjectDescription("A similar cask filled from a stagnant pool."));
}
