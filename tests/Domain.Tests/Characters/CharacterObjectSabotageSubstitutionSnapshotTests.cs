using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Identifiers;
using Allrpcs.Domain.WorldObjects;

namespace Allrpcs.Domain.Tests.Characters;

public sealed class CharacterObjectSabotageSubstitutionSnapshotTests
{
    [Fact]
    public void RetainsBoundedStableExactSubstitutionsAndCopiesCallerCollection()
    {
        CharacterObjectSabotageSubstitution first = Substitution("water-cask", "tainted-cask");
        CharacterObjectSabotageSubstitution second = Substitution("herb-bundle", "lookalike-bundle");
        CharacterObjectSabotageSubstitution replacement = Substitution("ledger", "false-ledger");
        CharacterObjectSabotageSubstitution[] source = [first, second];

        CharacterObjectSabotageSubstitutionSnapshot snapshot = new(source);
        source[0] = replacement;

        Assert.Collection(
            snapshot.Entries,
            entry => Assert.Same(first, entry),
            entry => Assert.Same(second, entry));
        Assert.Throws<NotSupportedException>(() =>
            ((IList<CharacterObjectSabotageSubstitution>)snapshot.Entries).Add(replacement));
    }

    [Fact]
    public void RejectsDuplicateTargetIdentifiersIncludingDistinctValueEqualInstances()
    {
        CharacterObjectSabotageSubstitution first = Substitution("water-cask", "tainted-cask");
        CharacterObjectSabotageSubstitution duplicate = Substitution("water-cask", "empty-cask");

        Assert.Throws<ArgumentException>(() =>
            new CharacterObjectSabotageSubstitutionSnapshot([first, duplicate]));
        Assert.NotSame(first.Occurrence.Target, duplicate.Occurrence.Target);
        Assert.NotSame(
            first.Occurrence.Target.Identity.Id,
            duplicate.Occurrence.Target.Identity.Id);
        Assert.Equal(
            first.Occurrence.Target.Identity.Id,
            duplicate.Occurrence.Target.Identity.Id);
    }

    [Fact]
    public void AcceptsMaximumAndRejectsNullOrOverMaximumWithoutEnumeratingFurther()
    {
        CharacterObjectSabotageSubstitution[] maximum = Enumerable
            .Range(0, CharacterObjectSabotageSubstitutionSnapshot.MaximumSubstitutionCount)
            .Select(index => Substitution($"target-{index}", $"replacement-{index}"))
            .ToArray();
        IEnumerable<CharacterObjectSabotageSubstitution> overMaximum = maximum
            .Append(Substitution("overflow-target", "overflow-replacement"))
            .Concat(ThrowIfEnumerated());

        CharacterObjectSabotageSubstitutionSnapshot accepted = new(maximum);

        Assert.Equal(maximum.Length, accepted.Entries.Count);
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new CharacterObjectSabotageSubstitutionSnapshot(overMaximum));
        Assert.Throws<ArgumentNullException>(() =>
            new CharacterObjectSabotageSubstitutionSnapshot(null!));
        Assert.Throws<ArgumentException>(() =>
            new CharacterObjectSabotageSubstitutionSnapshot(
                [Substitution("target", "replacement"), null!]));
    }

    [Fact]
    public void ExposesOnlyTheBoundedReadOnlySnapshotSurface()
    {
        Type snapshotType = typeof(CharacterObjectSabotageSubstitutionSnapshot);
        System.Reflection.PropertyInfo property = Assert.Single(snapshotType.GetProperties());
        System.Reflection.FieldInfo field = Assert.Single(snapshotType.GetFields());

        Assert.Equal("Entries", property.Name);
        Assert.Equal(
            typeof(IReadOnlyList<CharacterObjectSabotageSubstitution>),
            property.PropertyType);
        Assert.False(property.CanWrite);
        Assert.Equal("MaximumSubstitutionCount", field.Name);
        Assert.True(field.IsLiteral);
        Assert.Equal(64, field.GetRawConstantValue());
        Assert.Empty(snapshotType.GetEvents());
        Assert.Empty(snapshotType.GetMethods().Where(
            method => method.DeclaringType == snapshotType && !method.IsSpecialName));
        Assert.True(snapshotType.IsSealed);
    }

    private static IEnumerable<CharacterObjectSabotageSubstitution> ThrowIfEnumerated()
    {
        throw new InvalidOperationException("Enumeration continued beyond the maximum.");
#pragma warning disable CS0162
        yield break;
#pragma warning restore CS0162
    }

    private static CharacterObjectSabotageSubstitution Substitution(
        string targetId,
        string replacementId)
    {
        ObjectId targetObjectId = new(targetId);
        CharacterObjectSabotageOccurrence occurrence = new(
            new CharacterObjectSabotageProposal(
                new CharacterId("enemy-agent"),
                new CharacterActionAccessRequirement(
                    targetObjectId,
                    "substitute-supplies",
                    new KnowledgeId("supply-appearance")),
                CharacterSabotageActionKind.Substitute),
            WorldObject(targetObjectId));

        return new CharacterObjectSabotageSubstitution(
            occurrence,
            WorldObject(new ObjectId(replacementId)));
    }

    private static WorldObject WorldObject(ObjectId objectId) =>
        new(
            new WorldObjectIdentity(objectId, $"Object {objectId.Value}"),
            new WorldObjectDescription($"Description for {objectId.Value}."));
}
