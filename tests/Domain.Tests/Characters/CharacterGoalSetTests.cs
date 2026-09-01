using Allrpcs.Domain.Characters;

namespace Allrpcs.Domain.Tests.Characters;

public sealed class CharacterGoalSetTests
{
    [Fact]
    public void EmptySetIsExplicitlyValid()
    {
        CharacterGoalSet goals = new(Array.Empty<CharacterGoal>());

        Assert.Empty(goals.Entries);
    }

    [Fact]
    public void PreservesAuthoredEntryOrder()
    {
        CharacterGoal first = new("find-safe-passage", "Find safe passage through the woodland.");
        CharacterGoal second = new("reach-the-village", "Reach the village safely.");

        CharacterGoalSet goals = new(new[] { first, second });

        Assert.Equal(new[] { first, second }, goals.Entries);
    }

    [Fact]
    public void CopiesCallerCollection()
    {
        CharacterGoal original = new("find-safe-passage", "Find safe passage through the woodland.");
        CharacterGoal replacement = new("reach-the-village", "Reach the village safely.");
        CharacterGoal[] source = [original];

        CharacterGoalSet goals = new(source);
        source[0] = replacement;

        Assert.Equal(new[] { original }, goals.Entries);
    }

    [Fact]
    public void RejectsDuplicateKeysEvenWhenDescriptionsDiffer()
    {
        CharacterGoal first = new("find-safe-passage", "Find safe passage through the woodland.");
        CharacterGoal duplicate = new("find-safe-passage", "Find another safe woodland passage.");

        Assert.Throws<ArgumentException>(() => new CharacterGoalSet(new[] { first, duplicate }));
    }

    [Fact]
    public void RejectsNullCollectionAndEntries()
    {
        Assert.Throws<ArgumentNullException>(() => new CharacterGoalSet(null!));
        Assert.Throws<ArgumentException>(() => new CharacterGoalSet(new CharacterGoal[] { null! }));
    }

    [Fact]
    public void ExposesReadOnlyEntriesWithoutGoalMechanics()
    {
        CharacterGoal goal = new("find-safe-passage", "Find safe passage through the woodland.");
        CharacterGoalSet goals = new(new[] { goal });

        Assert.IsAssignableFrom<IReadOnlyList<CharacterGoal>>(goals.Entries);
        Assert.Throws<NotSupportedException>(() => ((IList<CharacterGoal>)goals.Entries).Add(goal));

        string[] publicProperties = typeof(CharacterGoalSet)
            .GetProperties()
            .Select(property => property.Name)
            .OrderBy(name => name, StringComparer.Ordinal)
            .ToArray();
        string[] publicMethods = typeof(CharacterGoalSet)
            .GetMethods()
            .Where(method => method.DeclaringType == typeof(CharacterGoalSet) && !method.IsSpecialName)
            .Select(method => method.Name)
            .OrderBy(name => name, StringComparer.Ordinal)
            .ToArray();

        Assert.Collection(publicProperties, property => Assert.Equal("Entries", property));
        Assert.Empty(publicMethods);
        Assert.Equal(
            typeof(IReadOnlyList<CharacterGoal>),
            typeof(CharacterGoalSet).GetProperty("Entries")!.PropertyType);
    }
}
