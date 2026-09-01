using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Identifiers;

namespace Allrpcs.Domain.Tests.Characters;

public sealed class CharacterRelationshipSetTests
{
    [Fact]
    public void EmptySetIsExplicitlyValid()
    {
        CharacterRelationshipSet relationships = new(Array.Empty<CharacterRelationship>());

        Assert.Empty(relationships.Entries);
    }

    [Fact]
    public void PreservesAuthoredEntryOrderAndTypedIdentifiers()
    {
        CharacterRelationship first = Create("travelling-healer", "Trusted companion.");
        CharacterRelationship second = Create("village-sage", "Respected adviser.");

        CharacterRelationshipSet relationships = new(new[] { first, second });

        Assert.Equal(new[] { first, second }, relationships.Entries);
        Assert.All(
            relationships.Entries,
            relationship => Assert.IsType<CharacterId>(relationship.RelatedCharacterId));
    }

    [Fact]
    public void CopiesCallerCollection()
    {
        CharacterRelationship original = Create("travelling-healer", "Trusted companion.");
        CharacterRelationship replacement = Create("village-sage", "Respected adviser.");
        CharacterRelationship[] source = [original];

        CharacterRelationshipSet relationships = new(source);
        source[0] = replacement;

        Assert.Equal(new[] { original }, relationships.Entries);
    }

    [Fact]
    public void RejectsDuplicateRelatedCharacterIdentifiersEvenWhenSummariesDiffer()
    {
        CharacterRelationship first = Create("travelling-healer", "Trusted companion.");
        CharacterRelationship duplicate = Create("travelling-healer", "Respected companion.");

        Assert.Throws<ArgumentException>(
            () => new CharacterRelationshipSet(new[] { first, duplicate }));
    }

    [Fact]
    public void RejectsNullCollectionAndEntries()
    {
        Assert.Throws<ArgumentNullException>(() => new CharacterRelationshipSet(null!));
        Assert.Throws<ArgumentException>(
            () => new CharacterRelationshipSet(new CharacterRelationship[] { null! }));
    }

    [Fact]
    public void ExposesReadOnlyEntriesWithoutRelationshipMechanics()
    {
        CharacterRelationship relationship = Create("travelling-healer", "Trusted companion.");
        CharacterRelationshipSet relationships = new(new[] { relationship });

        Assert.IsAssignableFrom<IReadOnlyList<CharacterRelationship>>(relationships.Entries);
        Assert.Throws<NotSupportedException>(
            () => ((IList<CharacterRelationship>)relationships.Entries).Add(relationship));

        string[] publicProperties = typeof(CharacterRelationshipSet)
            .GetProperties()
            .Select(property => property.Name)
            .OrderBy(name => name, StringComparer.Ordinal)
            .ToArray();
        string[] publicMethods = typeof(CharacterRelationshipSet)
            .GetMethods()
            .Where(
                method => method.DeclaringType == typeof(CharacterRelationshipSet)
                    && !method.IsSpecialName)
            .Select(method => method.Name)
            .OrderBy(name => name, StringComparer.Ordinal)
            .ToArray();

        Assert.Collection(publicProperties, property => Assert.Equal("Entries", property));
        Assert.Empty(publicMethods);
        Assert.Equal(
            typeof(IReadOnlyList<CharacterRelationship>),
            typeof(CharacterRelationshipSet).GetProperty("Entries")!.PropertyType);
    }

    private static CharacterRelationship Create(string characterId, string summary) =>
        new(new CharacterId(characterId), summary);
}
