using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Controllers;
using Allrpcs.Domain.Diseases;
using Allrpcs.Domain.Identifiers;

namespace Allrpcs.Domain.Tests.Diseases;

public sealed class PatientAssessmentEligibilityPolicyTests
{
    [Fact]
    public void AllowsMatchingPerceptionAndAuthoredAssessmentCapability()
    {
        PatientAssessmentRequest request = Request();
        CharacterPatientPerception perception = Perception();
        Character assessor = Assessor(PatientAssessmentEligibilityPolicy.RequiredCapabilityKey);

        Assert.True(PatientAssessmentEligibilityPolicy.CanAssess(
            request,
            perception,
            assessor));
    }

    [Fact]
    public void RejectsSelfAssessmentAndMismatchedOrAbsentPerception()
    {
        Character assessor = Assessor(PatientAssessmentEligibilityPolicy.RequiredCapabilityKey);
        CharacterId assessorId = assessor.Identity.Id;

        Assert.False(PatientAssessmentEligibilityPolicy.CanAssess(
            new PatientAssessmentRequest(assessorId, new CharacterId(assessorId.Value)),
            new CharacterPatientPerception(assessorId, new CharacterId(assessorId.Value)),
            assessor));
        Assert.False(PatientAssessmentEligibilityPolicy.CanAssess(
            Request(),
            new CharacterPatientPerception(new CharacterId("seeker"), new CharacterId("patient")),
            assessor));
        Assert.False(PatientAssessmentEligibilityPolicy.CanAssess(
            Request(),
            new CharacterPatientPerception(
                new CharacterId("travelling-healer"),
                new CharacterId("other-patient")),
            assessor));
        Assert.False(PatientAssessmentEligibilityPolicy.CanAssess(Request(), null, assessor));
    }

    [Fact]
    public void RejectsMismatchedAssessorAndMissingCapability()
    {
        Assert.False(PatientAssessmentEligibilityPolicy.CanAssess(
            Request(),
            Perception(),
            Assessor("herbalism")));
        Assert.False(PatientAssessmentEligibilityPolicy.CanAssess(
            Request(),
            Perception(),
            Character("seeker", PatientAssessmentEligibilityPolicy.RequiredCapabilityKey)));
    }

    [Fact]
    public void RejectsCapabilityInputBeyondInspectionBudget()
    {
        string[] capabilityKeys = Enumerable
            .Range(0, PatientAssessmentEligibilityPolicy.MaximumCapabilityCount + 1)
            .Select(index => $"capability-{index}")
            .Append(PatientAssessmentEligibilityPolicy.RequiredCapabilityKey)
            .ToArray();

        Assert.False(PatientAssessmentEligibilityPolicy.CanAssess(
            Request(),
            Perception(),
            Assessor(capabilityKeys)));
    }

    [Fact]
    public void AcceptsValueEqualIdentityMatchesAndExactCapabilityBudget()
    {
        string[] capabilityKeys = Enumerable
            .Range(0, PatientAssessmentEligibilityPolicy.MaximumCapabilityCount - 1)
            .Select(index => $"capability-{index}")
            .Append(PatientAssessmentEligibilityPolicy.RequiredCapabilityKey)
            .ToArray();

        Assert.True(PatientAssessmentEligibilityPolicy.CanAssess(
            Request(),
            Perception(),
            Assessor(capabilityKeys)));
    }

    [Fact]
    public void FailsClosedForNullInputsAndExposesOnlyOnePureOperation()
    {
        PatientAssessmentRequest request = Request();
        CharacterPatientPerception perception = Perception();
        Character assessor = Assessor(PatientAssessmentEligibilityPolicy.RequiredCapabilityKey);

        Assert.False(PatientAssessmentEligibilityPolicy.CanAssess(null, perception, assessor));
        Assert.False(PatientAssessmentEligibilityPolicy.CanAssess(request, null, assessor));
        Assert.False(PatientAssessmentEligibilityPolicy.CanAssess(request, perception, null));

        Type policyType = typeof(PatientAssessmentEligibilityPolicy);
        System.Reflection.MethodInfo method = Assert.Single(
            policyType.GetMethods().Where(candidate => candidate.DeclaringType == policyType));

        Assert.Equal("CanAssess", method.Name);
        Assert.Equal(typeof(bool), method.ReturnType);
        Assert.True(method.IsStatic);
        Assert.Empty(policyType.GetConstructors());
        Assert.Empty(policyType.GetProperties());
        Assert.Empty(policyType.GetEvents());
        Assert.True(policyType.IsAbstract);
        Assert.True(policyType.IsSealed);
    }

    internal static PatientAssessmentRequest Request() => new(
        new CharacterId("travelling-healer"),
        new CharacterId("patient"));

    internal static CharacterPatientPerception Perception() => new(
        new CharacterId("travelling-healer"),
        new CharacterId("patient"));

    internal static Character Assessor(params string[] capabilityKeys) =>
        Character("travelling-healer", capabilityKeys);

    private static Character Character(string characterId, params string[] capabilityKeys)
    {
        CharacterId id = new(characterId);

        return new Character(
            new CharacterIdentity(id, "Character"),
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
