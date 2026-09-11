using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Identifiers;
using Allrpcs.Domain.Ingredients;
using Allrpcs.Domain.Tests.Diseases;
using Allrpcs.Domain.WorldEvents;

namespace Allrpcs.Domain.Tests.Ingredients;

public sealed class IngredientAdministrationExperimentRecordTests
{
    [Fact]
    public void RetainsExactIdentityAdministrationAndObservedQualitativeConditions()
    {
        ExperimentId experimentId = new("first-cooling-tonic-dose");
        IngredientAdministrationOutcome administration = Administration();
        CharacterConditionSet observedConditions = new(
            [new CharacterCondition("appearance", "Breathing is quieter.")]);

        IngredientAdministrationExperimentRecord record = new(
            experimentId,
            administration,
            observedConditions);

        Assert.Same(experimentId, record.ExperimentId);
        Assert.Same(administration, record.Administration);
        Assert.Same(observedConditions, record.ObservedConditions);
    }

    [Fact]
    public void PermitsAnEmptyObservationWithoutInferringNoEffect()
    {
        CharacterConditionSet observedConditions = new([]);

        IngredientAdministrationExperimentRecord record = new(
            new ExperimentId("first-cooling-tonic-dose"),
            Administration(),
            observedConditions);

        Assert.Same(observedConditions, record.ObservedConditions);
        Assert.Empty(record.ObservedConditions.Entries);
    }

    [Fact]
    public void RejectsNullsAndExposesOnlyTheImmutableRecordSurface()
    {
        ExperimentId experimentId = new("first-cooling-tonic-dose");
        IngredientAdministrationOutcome administration = Administration();
        CharacterConditionSet observedConditions = new([]);

        Assert.Throws<ArgumentNullException>(() =>
            new IngredientAdministrationExperimentRecord(
                null!,
                administration,
                observedConditions));
        Assert.Throws<ArgumentNullException>(() =>
            new IngredientAdministrationExperimentRecord(
                experimentId,
                null!,
                observedConditions));
        Assert.Throws<ArgumentNullException>(() =>
            new IngredientAdministrationExperimentRecord(
                experimentId,
                administration,
                null!));

        Type recordType = typeof(IngredientAdministrationExperimentRecord);
        System.Reflection.PropertyInfo[] properties = recordType.GetProperties();
        System.Reflection.ConstructorInfo constructor = Assert.Single(recordType.GetConstructors());

        Assert.Equal(
            ["Administration", "ExperimentId", "ObservedConditions"],
            properties.Select(property => property.Name).Order());
        Assert.All(properties, property => Assert.False(property.CanWrite));
        Assert.Equal(
            [typeof(ExperimentId), typeof(IngredientAdministrationOutcome), typeof(CharacterConditionSet)],
            constructor.GetParameters().Select(parameter => parameter.ParameterType).ToArray());
        Assert.Empty(recordType.GetEvents());
        Assert.Empty(recordType.GetMethods().Where(
            method => method.DeclaringType == recordType && !method.IsSpecialName));
        Assert.True(recordType.IsSealed);
    }

    internal static IngredientAdministrationOutcome Administration()
    {
        IngredientDosage dosage = new(
            IngredientDosageTests.Outcome(4, IngredientQuantityUnit.Millilitre),
            new IngredientQuantity(1, IngredientQuantityUnit.Millilitre));
        WorldEvent worldEvent = DiseaseProgressionPolicyTests.Event(
            "patient-dosed",
            WorldEventTimeEffect.NoAdvance);
        return new IngredientAdministrationOutcome(
            new CharacterId("healer"),
            new CharacterId("patient"),
            dosage,
            worldEvent);
    }
}
