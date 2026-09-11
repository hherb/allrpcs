using Allrpcs.Domain.Diseases;
using Allrpcs.Domain.Identifiers;

namespace Allrpcs.Domain.Tests.Diseases;

public sealed class DiseaseTruthSnapshotTests
{
    [Fact]
    public void CopiesInfectionsInStableOrderAndRetainsExactInstances()
    {
        DiseaseDefinition disease = Definition("grey-sleep");
        DiseaseDefinitionSnapshot definitions = new([disease]);
        CharacterDiseaseState first = State("patient", disease, DiseaseStage.EarlyIllness);
        CharacterDiseaseState second = State("healer", disease, DiseaseStage.Exposed);
        CharacterDiseaseState replacement = State("seeker", disease, DiseaseStage.Exposed);
        CharacterDiseaseState[] source = [first, second];

        DiseaseTruthSnapshot snapshot = new(definitions, source);
        source[0] = replacement;

        Assert.Same(definitions, snapshot.Definitions);
        Assert.Collection(
            snapshot.Infections,
            infection => Assert.Same(first, infection),
            infection => Assert.Same(second, infection));
        Assert.Throws<NotSupportedException>(() =>
            ((IList<CharacterDiseaseState>)snapshot.Infections).Add(replacement));
    }

    [Fact]
    public void AcceptsEmptyAndExactInfectionCountBound()
    {
        DiseaseDefinition disease = Definition("grey-sleep");
        DiseaseDefinitionSnapshot definitions = new([disease]);
        CharacterDiseaseState[] maximum = Enumerable
            .Range(0, DiseaseTruthSnapshot.MaximumInfectionCount)
            .Select(index => State($"patient-{index}", disease, DiseaseStage.Exposed))
            .ToArray();

        Assert.Empty(new DiseaseTruthSnapshot(new DiseaseDefinitionSnapshot([]), []).Infections);
        Assert.Equal(
            DiseaseTruthSnapshot.MaximumInfectionCount,
            new DiseaseTruthSnapshot(definitions, maximum).Infections.Count);
        Assert.Throws<ArgumentOutOfRangeException>(() => new DiseaseTruthSnapshot(
            definitions,
            maximum.Append(State("extra-patient", disease, DiseaseStage.Exposed))));
    }

    [Fact]
    public void RejectsDuplicateCharacterDiseasePairsAndInvalidInput()
    {
        DiseaseDefinition disease = Definition("grey-sleep");
        DiseaseDefinition otherDisease = Definition("marsh-fever");
        DiseaseDefinitionSnapshot definitions = new([disease, otherDisease]);

        Assert.Throws<ArgumentException>(() => new DiseaseTruthSnapshot(
            definitions,
            [
                State("patient", disease, DiseaseStage.Exposed),
                State("patient", disease, DiseaseStage.EarlyIllness),
            ]));
        Assert.Equal(
            2,
            new DiseaseTruthSnapshot(
                definitions,
                [
                    State("patient", disease, DiseaseStage.Exposed),
                    State("patient", otherDisease, DiseaseStage.Exposed),
                ]).Infections.Count);
        Assert.Throws<ArgumentException>(() => new DiseaseTruthSnapshot(definitions, [null!]));
        Assert.Throws<ArgumentNullException>(() => new DiseaseTruthSnapshot(null!, []));
        Assert.Throws<ArgumentNullException>(() => new DiseaseTruthSnapshot(definitions, null!));
    }

    [Fact]
    public void RequiresTheExactRetainedDiseaseDefinitionInstance()
    {
        DiseaseDefinition retained = Definition("grey-sleep");
        DiseaseDefinition valueEqualReplacement = Definition("grey-sleep");
        DiseaseDefinition absent = Definition("marsh-fever");
        DiseaseDefinitionSnapshot definitions = new([retained]);

        Assert.Throws<ArgumentException>(() => new DiseaseTruthSnapshot(
            definitions,
            [State("patient", valueEqualReplacement, DiseaseStage.Exposed)]));
        Assert.Throws<ArgumentException>(() => new DiseaseTruthSnapshot(
            definitions,
            [State("patient", absent, DiseaseStage.Exposed)]));
    }

    [Fact]
    public void ExposesOnlyTheCanonicalTruthSurface()
    {
        Type snapshotType = typeof(DiseaseTruthSnapshot);
        (string Name, Type Type, bool CanWrite)[] properties = snapshotType
            .GetProperties()
            .Select(property => (property.Name, property.PropertyType, property.CanWrite))
            .OrderBy(property => property.Name, StringComparer.Ordinal)
            .ToArray();
        System.Reflection.ConstructorInfo constructor = Assert.Single(snapshotType.GetConstructors());

        Assert.Equal(
            [
                ("Definitions", typeof(DiseaseDefinitionSnapshot), false),
                ("Infections", typeof(IReadOnlyList<CharacterDiseaseState>), false),
            ],
            properties);
        Assert.Collection(
            constructor.GetParameters(),
            parameter => Assert.Equal(typeof(DiseaseDefinitionSnapshot), parameter.ParameterType),
            parameter => Assert.Equal(
                typeof(IEnumerable<CharacterDiseaseState>),
                parameter.ParameterType));
        Assert.Empty(snapshotType.GetEvents());
        Assert.Empty(snapshotType.GetMethods().Where(
            method => method.DeclaringType == snapshotType && !method.IsSpecialName));
        Assert.True(snapshotType.IsSealed);
    }

    private static DiseaseDefinition Definition(string diseaseId) => new(new DiseaseId(diseaseId));

    private static CharacterDiseaseState State(
        string characterId,
        DiseaseDefinition disease,
        DiseaseStage stage) =>
        new(new CharacterId(characterId), disease, stage, null);
}
