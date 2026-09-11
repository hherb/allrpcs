using Allrpcs.Domain.Discovery;
using Allrpcs.Domain.Identifiers;
using Allrpcs.Domain.Investigations;

namespace Allrpcs.Domain.Tests.Investigations;

public sealed class InvestigationConclusionConfirmationPolicyTests
{
    [Fact]
    public void AcceptsWhenOneCompleteAuthoredRouteIsVisible()
    {
        InvestigationConclusionEvidencePlan plan = Plan();
        CharacterId characterId = new("healer");
        EvidenceId first = new("witness-account");
        EvidenceId second = new("sample-analysis");
        EvidenceId unrelated = new("unrelated");
        CharacterEvidenceViewSnapshot evidence = new(
            characterId,
            [unrelated, first, second]);

        bool confirmed = InvestigationConclusionConfirmationPolicy.CanConfirm(plan, evidence);

        Assert.True(confirmed);
        Assert.Same(characterId, evidence.CharacterId);
        Assert.Collection(
            evidence.EvidenceIds,
            evidenceId => Assert.Same(unrelated, evidenceId),
            evidenceId => Assert.Same(first, evidenceId),
            evidenceId => Assert.Same(second, evidenceId));
    }

    [Fact]
    public void AcceptsValueEqualVisibleEvidenceWithoutRequiringExactReferences()
    {
        InvestigationConclusionEvidencePlan plan = Plan();
        InvestigationConclusionEvidenceRoute firstRoute = plan.Routes[0];
        CharacterEvidenceViewSnapshot evidence = new(
            new CharacterId("healer"),
            firstRoute.RequiredEvidenceIds.Select(id => new EvidenceId(id.Value)));

        Assert.True(InvestigationConclusionConfirmationPolicy.CanConfirm(plan, evidence));
    }

    [Fact]
    public void RejectsPartialEvidenceEvenWhenPooledAcrossAlternateRoutes()
    {
        InvestigationConclusionEvidencePlan plan = Plan();
        CharacterEvidenceViewSnapshot evidence = new(
            new CharacterId("healer"),
            [new EvidenceId("covered-water-note"), new EvidenceId("witness-account")]);

        Assert.False(InvestigationConclusionConfirmationPolicy.CanConfirm(plan, evidence));
        Assert.False(InvestigationConclusionConfirmationPolicy.CanConfirm(
            plan,
            new CharacterEvidenceViewSnapshot(new CharacterId("healer"), [])));
    }

    [Fact]
    public void RejectsNullsAndExposesOnePureStaticOperation()
    {
        InvestigationConclusionEvidencePlan plan = Plan();
        CharacterEvidenceViewSnapshot evidence = new(new CharacterId("healer"), []);

        Assert.Throws<ArgumentNullException>(() =>
            InvestigationConclusionConfirmationPolicy.CanConfirm(null!, evidence));
        Assert.Throws<ArgumentNullException>(() =>
            InvestigationConclusionConfirmationPolicy.CanConfirm(plan, null!));

        Type policyType = typeof(InvestigationConclusionConfirmationPolicy);
        System.Reflection.MethodInfo method = Assert.Single(policyType.GetMethods().Where(
            candidate => candidate.DeclaringType == policyType));

        Assert.Equal("CanConfirm", method.Name);
        Assert.True(method.IsStatic);
        Assert.Equal(typeof(bool), method.ReturnType);
        Assert.Equal(
            [typeof(InvestigationConclusionEvidencePlan), typeof(CharacterEvidenceViewSnapshot)],
            method.GetParameters().Select(parameter => parameter.ParameterType).ToArray());
        Assert.Empty(policyType.GetProperties());
        Assert.Empty(policyType.GetEvents());
        Assert.True(policyType.IsAbstract && policyType.IsSealed);
    }

    private static InvestigationConclusionEvidencePlan Plan()
    {
        InvestigationConclusion conclusion = InvestigationConclusionTests.Conclusion();
        return new InvestigationConclusionEvidencePlan(
            conclusion,
            [
                InvestigationConclusionEvidencePlanTests.Route(
                    conclusion,
                    "covered-water-note",
                    "cooling-experiment"),
                InvestigationConclusionEvidencePlanTests.Route(
                    conclusion,
                    "witness-account",
                    "sample-analysis"),
            ]);
    }
}
