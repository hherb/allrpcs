using Allrpcs.Domain.EnvironmentalStates;

namespace Allrpcs.Domain.Tests.EnvironmentalStates;

public sealed class EnvironmentalStateSetTests
{
    [Fact]
    public void EmptySetIsExplicitlyValid()
    {
        EnvironmentalStateSet states = new(Array.Empty<EnvironmentalState>());

        Assert.Empty(states.Entries);
    }

    [Fact]
    public void PreservesAuthoredEntryOrder()
    {
        EnvironmentalState first = new("heavy-rain", "Rain falls steadily.");
        EnvironmentalState second = new("muddy-ground", "The ground is rain-softened.");

        EnvironmentalStateSet states = new(new[] { first, second });

        Assert.Collection(
            states.Entries,
            entry => Assert.Same(first, entry),
            entry => Assert.Same(second, entry));
    }

    [Fact]
    public void CopiesCallerCollection()
    {
        EnvironmentalState original = new("heavy-rain", "Rain falls steadily.");
        EnvironmentalState replacement = new("clear-skies", "The rain has stopped.");
        EnvironmentalState[] source = [original];

        EnvironmentalStateSet states = new(source);
        source[0] = replacement;

        Assert.Equal(new[] { original }, states.Entries);
    }

    [Fact]
    public void RejectsDuplicateKeysEvenWhenDescriptionsDiffer()
    {
        EnvironmentalState first = new("heavy-rain", "Rain falls steadily.");
        EnvironmentalState duplicate = new("heavy-rain", "Rain obscures the path.");

        Assert.Throws<ArgumentException>(
            () => new EnvironmentalStateSet(new[] { first, duplicate }));
    }

    [Fact]
    public void RejectsNullCollectionAndEntries()
    {
        Assert.Throws<ArgumentNullException>(() => new EnvironmentalStateSet(null!));
        Assert.Throws<ArgumentException>(
            () => new EnvironmentalStateSet(new EnvironmentalState[] { null! }));
    }

    [Fact]
    public void ExposesReadOnlyEntriesWithoutAssignmentOrSimulationMechanics()
    {
        EnvironmentalState state = new("heavy-rain", "Rain falls steadily.");
        EnvironmentalStateSet states = new(new[] { state });

        Assert.IsAssignableFrom<IReadOnlyList<EnvironmentalState>>(states.Entries);
        Assert.Throws<NotSupportedException>(
            () => ((IList<EnvironmentalState>)states.Entries).Add(state));

        string[] publicProperties = typeof(EnvironmentalStateSet)
            .GetProperties()
            .Select(property => property.Name)
            .OrderBy(name => name, StringComparer.Ordinal)
            .ToArray();
        string[] publicMethods = typeof(EnvironmentalStateSet)
            .GetMethods()
            .Where(
                method => method.DeclaringType == typeof(EnvironmentalStateSet)
                    && !method.IsSpecialName)
            .Select(method => method.Name)
            .OrderBy(name => name, StringComparer.Ordinal)
            .ToArray();
        System.Reflection.ConstructorInfo constructor = Assert.Single(
            typeof(EnvironmentalStateSet).GetConstructors());

        Assert.Collection(publicProperties, property => Assert.Equal("Entries", property));
        Assert.Empty(publicMethods);
        Assert.Collection(
            constructor.GetParameters(),
            parameter => Assert.Equal(
                typeof(IEnumerable<EnvironmentalState>),
                parameter.ParameterType));
        Assert.Equal(
            typeof(IReadOnlyList<EnvironmentalState>),
            typeof(EnvironmentalStateSet).GetProperty("Entries")!.PropertyType);
    }
}
