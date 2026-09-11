using Allrpcs.Domain.Diseases;
using Allrpcs.Domain.Identifiers;

namespace Allrpcs.Domain.Tests.Diseases;

public sealed class DiseaseDefinitionSnapshotTests
{
    [Fact]
    public void CopiesDefinitionsInStableOrderAndRetainsExactInstances()
    {
        DiseaseDefinition first = Definition("grey-sleep");
        DiseaseDefinition second = Definition("marsh-fever");
        DiseaseDefinition replacement = Definition("replacement");
        DiseaseDefinition[] source = [first, second];

        DiseaseDefinitionSnapshot snapshot = new(source);
        source[0] = replacement;

        Assert.Collection(
            snapshot.Definitions,
            definition => Assert.Same(first, definition),
            definition => Assert.Same(second, definition));
        Assert.Throws<NotSupportedException>(() =>
            ((IList<DiseaseDefinition>)snapshot.Definitions).Add(replacement));
    }

    [Fact]
    public void AcceptsEmptyAndExactCountBound()
    {
        DiseaseDefinition[] maximum = Enumerable
            .Range(0, DiseaseDefinitionSnapshot.MaximumDefinitionCount)
            .Select(index => Definition($"disease-{index}"))
            .ToArray();

        Assert.Empty(new DiseaseDefinitionSnapshot([]).Definitions);
        Assert.Equal(
            DiseaseDefinitionSnapshot.MaximumDefinitionCount,
            new DiseaseDefinitionSnapshot(maximum).Definitions.Count);
        Assert.Throws<ArgumentOutOfRangeException>(() => new DiseaseDefinitionSnapshot(
            maximum.Append(Definition("extra-disease"))));
    }

    [Fact]
    public void RejectsNullsAndValueEqualDuplicateIdentities()
    {
        Assert.Throws<ArgumentNullException>(() => new DiseaseDefinitionSnapshot(null!));
        Assert.Throws<ArgumentException>(() => new DiseaseDefinitionSnapshot([null!]));
        Assert.Throws<ArgumentException>(() => new DiseaseDefinitionSnapshot(
            [Definition("grey-sleep"), Definition("grey-sleep")]));
    }

    [Fact]
    public void ExposesOnlyTheReadOnlySnapshotSurface()
    {
        Type snapshotType = typeof(DiseaseDefinitionSnapshot);
        System.Reflection.PropertyInfo property = Assert.Single(snapshotType.GetProperties());
        System.Reflection.ConstructorInfo constructor = Assert.Single(snapshotType.GetConstructors());

        Assert.Equal("Definitions", property.Name);
        Assert.Equal(typeof(IReadOnlyList<DiseaseDefinition>), property.PropertyType);
        Assert.False(property.CanWrite);
        Assert.Collection(
            constructor.GetParameters(),
            parameter => Assert.Equal(
                typeof(IEnumerable<DiseaseDefinition>),
                parameter.ParameterType));
        Assert.Empty(snapshotType.GetEvents());
        Assert.Empty(snapshotType.GetMethods().Where(
            method => method.DeclaringType == snapshotType && !method.IsSpecialName));
        Assert.True(snapshotType.IsSealed);
    }

    private static DiseaseDefinition Definition(string diseaseId) =>
        new(new DiseaseId(diseaseId));
}
