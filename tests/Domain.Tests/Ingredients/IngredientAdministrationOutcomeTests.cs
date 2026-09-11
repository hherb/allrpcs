using Allrpcs.Domain.Identifiers;
using Allrpcs.Domain.Ingredients;
using Allrpcs.Domain.Tests.Diseases;
using Allrpcs.Domain.WorldEvents;

namespace Allrpcs.Domain.Tests.Ingredients;

public sealed class IngredientAdministrationOutcomeTests
{
    [Fact]
    public void RetainsExactAdministratorPatientDosageAndAuthoredEvent()
    {
        CharacterId administrator = new("healer");
        CharacterId patient = new("patient");
        IngredientDosage dosage = new(
            IngredientDosageTests.Outcome(4, IngredientQuantityUnit.Millilitre),
            new IngredientQuantity(1, IngredientQuantityUnit.Millilitre));
        WorldEvent worldEvent = DiseaseProgressionPolicyTests.Event(
            "patient-dosed",
            WorldEventTimeEffect.NoAdvance);

        IngredientAdministrationOutcome outcome = new(
            administrator,
            patient,
            dosage,
            worldEvent);

        Assert.Same(administrator, outcome.AdministratorId);
        Assert.Same(patient, outcome.PatientId);
        Assert.Same(dosage, outcome.Dosage);
        Assert.Same(worldEvent, outcome.WorldEvent);
    }

    [Fact]
    public void PermitsAuthoredSelfAdministrationWithoutInferringPolicy()
    {
        CharacterId characterId = new("patient");
        IngredientDosage dosage = new(
            IngredientDosageTests.Outcome(1, IngredientQuantityUnit.Count),
            new IngredientQuantity(1, IngredientQuantityUnit.Count));
        WorldEvent worldEvent = DiseaseProgressionPolicyTests.Event(
            "patient-self-dosed",
            WorldEventTimeEffect.NoAdvance);

        IngredientAdministrationOutcome outcome = new(
            characterId,
            characterId,
            dosage,
            worldEvent);

        Assert.Same(characterId, outcome.AdministratorId);
        Assert.Same(characterId, outcome.PatientId);
    }

    [Fact]
    public void RejectsNullsAndExposesOnlyTheImmutableOutcomeSurface()
    {
        CharacterId administrator = new("healer");
        CharacterId patient = new("patient");
        IngredientDosage dosage = new(
            IngredientDosageTests.Outcome(1, IngredientQuantityUnit.Count),
            new IngredientQuantity(1, IngredientQuantityUnit.Count));
        WorldEvent worldEvent = DiseaseProgressionPolicyTests.Event(
            "patient-dosed",
            WorldEventTimeEffect.NoAdvance);

        Assert.Throws<ArgumentNullException>(() =>
            new IngredientAdministrationOutcome(null!, patient, dosage, worldEvent));
        Assert.Throws<ArgumentNullException>(() =>
            new IngredientAdministrationOutcome(administrator, null!, dosage, worldEvent));
        Assert.Throws<ArgumentNullException>(() =>
            new IngredientAdministrationOutcome(administrator, patient, null!, worldEvent));
        Assert.Throws<ArgumentNullException>(() =>
            new IngredientAdministrationOutcome(administrator, patient, dosage, null!));

        Type outcomeType = typeof(IngredientAdministrationOutcome);
        System.Reflection.PropertyInfo[] properties = outcomeType.GetProperties();
        System.Reflection.ConstructorInfo constructor = Assert.Single(outcomeType.GetConstructors());

        Assert.Equal(
            ["AdministratorId", "Dosage", "PatientId", "WorldEvent"],
            properties.Select(property => property.Name).Order());
        Assert.All(properties, property => Assert.False(property.CanWrite));
        Assert.Equal(
            [typeof(CharacterId), typeof(CharacterId), typeof(IngredientDosage), typeof(WorldEvent)],
            constructor.GetParameters().Select(parameter => parameter.ParameterType).ToArray());
        Assert.Empty(outcomeType.GetEvents());
        Assert.Empty(outcomeType.GetMethods().Where(
            method => method.DeclaringType == outcomeType && !method.IsSpecialName));
        Assert.True(outcomeType.IsSealed);
    }
}
