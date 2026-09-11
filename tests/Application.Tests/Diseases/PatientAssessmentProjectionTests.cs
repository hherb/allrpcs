using Allrpcs.Application.Diseases;
using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Controllers;
using Allrpcs.Domain.Diseases;
using Allrpcs.Domain.Identifiers;

namespace Allrpcs.Application.Tests.Diseases;

public sealed class PatientAssessmentProjectionTests
{
    [Fact]
    public void ProjectsOnlyObservableConditionsForThePerceivedPatient()
    {
        DiseaseDefinition firstDisease = new(new DiseaseId("grey-sleep"));
        DiseaseDefinition secondDisease = new(new DiseaseId("marsh-fever"));
        DiseaseTruthSnapshot truth = Truth(
            [firstDisease, secondDisease],
            State("patient", firstDisease, DiseaseStage.Exposed),
            State("patient", secondDisease, DiseaseStage.Drowsing),
            State("other-patient", firstDisease, DiseaseStage.FinalDecline));

        CharacterConditionSet? result = PatientAssessmentProjection.Project(
            Request(),
            Perception(),
            Assessor(PatientAssessmentEligibilityPolicy.RequiredCapabilityKey),
            truth);

        CharacterCondition condition = Assert.Single(Assert.IsType<CharacterConditionSet>(result).Entries);
        Assert.Equal("appears-drowsy", condition.Key);
        Assert.Equal("Appears increasingly drowsy.", condition.Description);
        Assert.DoesNotContain("grey-sleep", condition.Description);
        Assert.DoesNotContain("marsh-fever", condition.Description);
    }

    [Fact]
    public void DeduplicatesQualitativeConditionsWithoutExposingDiseaseMultiplicity()
    {
        DiseaseDefinition firstDisease = new(new DiseaseId("grey-sleep"));
        DiseaseDefinition secondDisease = new(new DiseaseId("marsh-fever"));
        DiseaseTruthSnapshot truth = Truth(
            [firstDisease, secondDisease],
            State("patient", firstDisease, DiseaseStage.EarlyIllness),
            State("patient", secondDisease, DiseaseStage.EarlyIllness));

        CharacterConditionSet result = Assert.IsType<CharacterConditionSet>(
            PatientAssessmentProjection.Project(
                Request(),
                Perception(),
                Assessor(PatientAssessmentEligibilityPolicy.RequiredCapabilityKey),
                truth));

        Assert.Equal("appears-unwell", Assert.Single(result.Entries).Key);
    }

    [Fact]
    public void ReturnsAnEmptyObservationForNoInfectionOrExposureOnly()
    {
        DiseaseDefinition disease = new(new DiseaseId("grey-sleep"));
        Character assessor = Assessor(PatientAssessmentEligibilityPolicy.RequiredCapabilityKey);

        CharacterConditionSet uninfected = Assert.IsType<CharacterConditionSet>(
            PatientAssessmentProjection.Project(
                Request(),
                Perception(),
                assessor,
                Truth([disease])));
        CharacterConditionSet exposed = Assert.IsType<CharacterConditionSet>(
            PatientAssessmentProjection.Project(
                Request(),
                Perception(),
                assessor,
                Truth([disease], State("patient", disease, DiseaseStage.Exposed))));

        Assert.Empty(uninfected.Entries);
        Assert.Empty(exposed.Entries);
    }

    [Fact]
    public void FailsClosedWhenAssessmentIsIneligible()
    {
        DiseaseDefinition disease = new(new DiseaseId("grey-sleep"));
        DiseaseTruthSnapshot truth = Truth(
            [disease],
            State("patient", disease, DiseaseStage.Wasting));

        Assert.Null(PatientAssessmentProjection.Project(
            Request(),
            Perception(),
            Assessor("herbalism"),
            truth));
        Assert.Null(PatientAssessmentProjection.Project(
            Request(),
            null,
            Assessor(PatientAssessmentEligibilityPolicy.RequiredCapabilityKey),
            truth));
    }

    [Fact]
    public void RejectsNullTruthAndExposesOnlyOnePureProjection()
    {
        PatientAssessmentRequest request = Request();
        CharacterPatientPerception perception = Perception();
        Character assessor = Assessor(PatientAssessmentEligibilityPolicy.RequiredCapabilityKey);

        Assert.Throws<ArgumentNullException>(() =>
            PatientAssessmentProjection.Project(request, perception, assessor, null!));

        Type projectionType = typeof(PatientAssessmentProjection);
        System.Reflection.MethodInfo method = Assert.Single(
            projectionType.GetMethods().Where(candidate => candidate.DeclaringType == projectionType));

        Assert.Equal("Project", method.Name);
        Assert.Equal(typeof(CharacterConditionSet), method.ReturnType);
        Assert.Empty(projectionType.GetConstructors());
        Assert.Empty(projectionType.GetProperties());
        Assert.Empty(projectionType.GetEvents());
        Assert.True(projectionType.IsAbstract);
        Assert.True(projectionType.IsSealed);
    }

    private static CharacterDiseaseState State(
        string characterId,
        DiseaseDefinition disease,
        DiseaseStage stage) =>
        new(
            new CharacterId(characterId),
            disease,
            stage,
            null,
            DiseaseCareState.Unassisted);

    private static DiseaseTruthSnapshot Truth(
        DiseaseDefinition[] diseases,
        params CharacterDiseaseState[] infections) =>
        new(new DiseaseDefinitionSnapshot(diseases), infections);

    private static PatientAssessmentRequest Request() => new(
        new CharacterId("travelling-healer"),
        new CharacterId("patient"));

    private static CharacterPatientPerception Perception() => new(
        new CharacterId("travelling-healer"),
        new CharacterId("patient"));

    private static Character Assessor(params string[] capabilityKeys)
    {
        CharacterId id = new("travelling-healer");

        return new Character(
            new CharacterIdentity(id, "The travelling healer"),
            new PublicCharacterBackground(string.Empty),
            new PrivateCharacterBackground(string.Empty),
            new CharacterCapabilitySet(capabilityKeys.Select(key =>
                new CharacterCapability(key, "Authored capability."))),
            new CharacterLimitationSet([]),
            new CharacterControllerAssignment(id, ControllerType.Human),
            new CharacterConditionSet([]),
            new CharacterInventory([]),
            new CharacterKnowledge([]),
            new CharacterBeliefSet([]),
            new CharacterRelationshipSet([]),
            new CharacterGoalSet([]));
    }
}
