using Allrpcs.Domain.Diseases;
using Allrpcs.Domain.Identifiers;

namespace Allrpcs.Domain.Tests.Diseases;

public sealed class DiseaseProtectionSnapshotTests
{
    [Fact]
    public void CopiesDefinitionsAndActiveProtectionsInStableOrder()
    {
        DiseaseDefinition disease = DiseaseProgressionPolicyTests.Definition();
        DiseaseProtectionDefinition first = Protection(disease, "covered-water");
        DiseaseProtectionDefinition second = Protection(disease, "protective-herb");
        DiseaseProtectionDefinition[] definitions = [first, second];
        CharacterDiseaseProtection active = new(new CharacterId("patient"), second);
        CharacterDiseaseProtection[] activeProtections = [active];

        DiseaseProtectionSnapshot snapshot = new(
            new DiseaseProtectionDefinitionSnapshot(
                new DiseaseDefinitionSnapshot([disease]),
                definitions),
            activeProtections);

        definitions[0] = second;
        activeProtections[0] = new(new CharacterId("healer"), first);

        Assert.Collection(
            snapshot.Definitions.Definitions,
            definition => Assert.Same(first, definition),
            definition => Assert.Same(second, definition));
        Assert.Same(active, Assert.Single(snapshot.ActiveProtections));
    }

    [Fact]
    public void DefinitionSnapshotRequiresExactDiseasesAndUniqueEvidencePerDisease()
    {
        DiseaseDefinition disease = DiseaseProgressionPolicyTests.Definition();
        DiseaseDefinition replacement = DiseaseProgressionPolicyTests.Definition();
        DiseaseDefinitionSnapshot diseases = new([disease]);
        DiseaseProtectionDefinition retained = Protection(disease, "covered-water");

        Assert.Throws<ArgumentException>(() => new DiseaseProtectionDefinitionSnapshot(
            diseases,
            [Protection(replacement, "different-evidence")]));
        Assert.Throws<ArgumentException>(() => new DiseaseProtectionDefinitionSnapshot(
            diseases,
            [retained, Protection(disease, retained.EvidenceId.Value)]));
    }

    [Fact]
    public void ActiveProtectionsRequireExactDefinitionsAndUniqueCharacterMeasurePairs()
    {
        DiseaseDefinition disease = DiseaseProgressionPolicyTests.Definition();
        DiseaseProtectionDefinition retained = Protection(disease, "covered-water");
        DiseaseProtectionDefinition replacement = Protection(disease, "covered-water");
        DiseaseProtectionDefinitionSnapshot definitions = new(
            new DiseaseDefinitionSnapshot([disease]),
            [retained]);
        CharacterId characterId = new("patient");

        Assert.Throws<ArgumentException>(() => new DiseaseProtectionSnapshot(
            definitions,
            [new CharacterDiseaseProtection(characterId, replacement)]));
        Assert.Throws<ArgumentException>(() => new DiseaseProtectionSnapshot(
            definitions,
            [
                new CharacterDiseaseProtection(characterId, retained),
                new CharacterDiseaseProtection(new CharacterId("patient"), retained),
            ]));
    }

    [Fact]
    public void RejectsNullsAndEnforcesBounds()
    {
        DiseaseDefinition disease = DiseaseProgressionPolicyTests.Definition();
        DiseaseDefinitionSnapshot diseases = new([disease]);
        DiseaseProtectionDefinition[] maximumDefinitions = Enumerable
            .Range(0, DiseaseProtectionDefinitionSnapshot.MaximumDefinitionCount)
            .Select(index => Protection(disease, $"measure-{index}"))
            .ToArray();
        DiseaseProtectionDefinitionSnapshot definitions = new(diseases, maximumDefinitions);
        CharacterDiseaseProtection[] maximumActive = Enumerable
            .Range(0, DiseaseProtectionSnapshot.MaximumActiveProtectionCount)
            .Select(index => new CharacterDiseaseProtection(
                new CharacterId($"patient-{index}"),
                maximumDefinitions[0]))
            .ToArray();

        Assert.Equal(maximumDefinitions.Length, definitions.Definitions.Count);
        Assert.Equal(
            maximumActive.Length,
            new DiseaseProtectionSnapshot(definitions, maximumActive).ActiveProtections.Count);
        Assert.Throws<ArgumentOutOfRangeException>(() => new DiseaseProtectionDefinitionSnapshot(
            diseases,
            maximumDefinitions.Append(Protection(disease, "extra-measure"))));
        Assert.Throws<ArgumentOutOfRangeException>(() => new DiseaseProtectionSnapshot(
            definitions,
            maximumActive.Append(new CharacterDiseaseProtection(
                new CharacterId("extra-patient"),
                maximumDefinitions[0]))));
        Assert.Throws<ArgumentException>(() => new DiseaseProtectionDefinitionSnapshot(diseases, [null!]));
        Assert.Throws<ArgumentException>(() => new DiseaseProtectionSnapshot(definitions, [null!]));
        Assert.Throws<ArgumentNullException>(() => new DiseaseProtectionDefinitionSnapshot(null!, []));
        Assert.Throws<ArgumentNullException>(() => new DiseaseProtectionDefinitionSnapshot(diseases, null!));
        Assert.Throws<ArgumentNullException>(() => new DiseaseProtectionSnapshot(null!, []));
        Assert.Throws<ArgumentNullException>(() => new DiseaseProtectionSnapshot(definitions, null!));
    }

    private static DiseaseProtectionDefinition Protection(
        DiseaseDefinition disease,
        string evidenceId) =>
        new(disease, new EvidenceId(evidenceId));
}
