using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Experiments;
using Allrpcs.Domain.Identifiers;
using Allrpcs.Domain.Ingredients;

namespace Allrpcs.Domain.Tests.Ingredients;

public sealed class IngredientAdministrationExperimentExecutionTransitionTests
{
    [Fact]
    public void AtomicallyConsumesTheExactBatchAndRecordsBothJournalForms()
    {
        IngredientAdministrationExperimentRecordingPolicyTests.RecordingInput input =
            IngredientAdministrationExperimentRecordingPolicyTests.Input();
        PreparedIngredientBatch retained = new(
            IngredientDosageTests.Outcome(2, IngredientQuantityUnit.Millilitre));
        PreparedIngredientBatch administered = new(input.Administration.Dosage.Preparation);
        PreparedIngredientBatchSnapshot batches = new([retained, administered]);
        IngredientExperimentJournalSnapshot administrationExperiments = new([]);
        ExperimentJournalSnapshot experiments = new([]);
        ExperimentId experimentId = new("patient-cooling-dose");
        CharacterConditionSet conditions = new(
            [new CharacterCondition("appearance", "Breathing is quieter.")]);

        IngredientAdministrationExperimentExecutionResult result =
            IngredientAdministrationExperimentExecutionTransition.Execute(
                experimentId,
                input.Administration,
                conditions,
                "Administer one cooled prepared dose.",
                "The patient's breathing became quieter.",
                input.WorldEvents,
                input.EventLog,
                batches,
                administrationExperiments,
                experiments);

        Assert.Collection(
            result.PreparedBatches.Batches,
            batch => Assert.Same(retained, batch),
            batch =>
            {
                Assert.Same(administered.Preparation, batch.Preparation);
                Assert.Equal(
                    administered.RemainingQuantity.Amount
                        - input.Administration.Dosage.Quantity.Amount,
                    batch.RemainingQuantity.Amount);
            });
        IngredientAdministrationExperimentRecord administrationRecord = Assert.Single(
            result.AdministrationExperiments.Records);
        Assert.Same(experimentId, administrationRecord.ExperimentId);
        Assert.Same(input.Administration, administrationRecord.Administration);
        Assert.Same(conditions, administrationRecord.ObservedConditions);
        ExperimentJournalRecord universalRecord = Assert.Single(result.Experiments.Records);
        Assert.Same(experimentId, universalRecord.ExperimentId);
        Assert.Same(input.Administration.WorldEvent, universalRecord.WorldEvent);
        Assert.Equal("Administer one cooled prepared dose.", universalRecord.Procedure);
        Assert.Equal(
            "The patient's breathing became quieter.",
            universalRecord.ObservedOutcome);
        Assert.Equal(2, batches.Batches.Count);
        Assert.Empty(administrationExperiments.Records);
        Assert.Empty(experiments.Records);
    }

    [Fact]
    public void RejectsAnyUnavailableOrDuplicateOutputWithoutChangingInputs()
    {
        IngredientAdministrationExperimentRecordingPolicyTests.RecordingInput input =
            IngredientAdministrationExperimentRecordingPolicyTests.Input();
        PreparedIngredientBatch administered = new(input.Administration.Dosage.Preparation);
        PreparedIngredientBatchSnapshot batches = new([administered]);
        PreparedIngredientBatchSnapshot noBatches = new([]);
        IngredientExperimentJournalSnapshot administrationExperiments = new([]);
        ExperimentJournalSnapshot experiments = new(
            [new ExperimentJournalRecord(
                new ExperimentId("patient-cooling-dose"),
                input.Administration.WorldEvent,
                "Earlier procedure.",
                "Earlier outcome.")]);
        CharacterConditionSet conditions = new([]);

        Assert.Throws<InvalidOperationException>(() =>
            IngredientAdministrationExperimentExecutionTransition.Execute(
                new ExperimentId("patient-cooling-dose"),
                input.Administration,
                conditions,
                "Procedure",
                "Outcome",
                input.WorldEvents,
                input.EventLog,
                batches,
                administrationExperiments,
                experiments));
        Assert.Throws<InvalidOperationException>(() =>
            IngredientAdministrationExperimentExecutionTransition.Execute(
                new ExperimentId("different-experiment"),
                input.Administration,
                conditions,
                "Procedure",
                "Outcome",
                input.WorldEvents,
                input.EventLog,
                noBatches,
                administrationExperiments,
                new ExperimentJournalSnapshot([])));

        Assert.Same(administered, Assert.Single(batches.Batches));
        Assert.Empty(noBatches.Batches);
        Assert.Empty(administrationExperiments.Records);
        Assert.Single(experiments.Records);
    }

    [Fact]
    public void ResultRetainsOnlyTheThreeExactResultingSnapshots()
    {
        PreparedIngredientBatchSnapshot batches = new([]);
        IngredientExperimentJournalSnapshot administrationExperiments = new([]);
        ExperimentJournalSnapshot experiments = new([]);

        IngredientAdministrationExperimentExecutionResult result = new(
            batches,
            administrationExperiments,
            experiments);

        Assert.Same(batches, result.PreparedBatches);
        Assert.Same(administrationExperiments, result.AdministrationExperiments);
        Assert.Same(experiments, result.Experiments);
        Assert.Throws<ArgumentNullException>(() =>
            new IngredientAdministrationExperimentExecutionResult(
                null!, administrationExperiments, experiments));
        Assert.Throws<ArgumentNullException>(() =>
            new IngredientAdministrationExperimentExecutionResult(
                batches, null!, experiments));
        Assert.Throws<ArgumentNullException>(() =>
            new IngredientAdministrationExperimentExecutionResult(
                batches, administrationExperiments, null!));

        Type resultType = typeof(IngredientAdministrationExperimentExecutionResult);
        Assert.Equal(
            ["AdministrationExperiments", "Experiments", "PreparedBatches"],
            resultType.GetProperties().Select(property => property.Name).Order());
        Assert.All(resultType.GetProperties(), property => Assert.False(property.CanWrite));
        Assert.Empty(resultType.GetEvents());
        Assert.Empty(resultType.GetMethods().Where(
            method => method.DeclaringType == resultType && !method.IsSpecialName));
        Assert.True(resultType.IsSealed);
    }
}
