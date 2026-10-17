using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Identifiers;

namespace Allrpcs.Domain.Tests.Characters;

public sealed class CharacterBeliefAssessmentTests
{
    [Fact]
    public void RetainsExactBeliefConfidenceAndCopiedEvidence()
    {
        CharacterEpistemicMemory belief = Belief();
        EvidenceId firstSupport = new("moonwort-observation");
        EvidenceId secondSupport = new("healer-notes");
        EvidenceId contradiction = new("patient-reaction");
        EvidenceId[] supporting = [firstSupport, secondSupport];
        EvidenceId[] contradicting = [contradiction];

        CharacterBeliefAssessment assessment = new(
            belief,
            7_250,
            supporting,
            contradicting);
        ListIdentityContractTests.AssertIdentity(assessment);
        supporting[0] = contradiction;
        contradicting[0] = firstSupport;

        Assert.Same(belief, assessment.Belief);
        Assert.Equal(7_250, assessment.ConfidenceBasisPoints);
        Assert.Collection(
            assessment.SupportingEvidenceIds,
            evidenceId => Assert.Same(firstSupport, evidenceId),
            evidenceId => Assert.Same(secondSupport, evidenceId));
        Assert.Collection(
            assessment.ContradictingEvidenceIds,
            evidenceId => Assert.Same(contradiction, evidenceId));
        Assert.Throws<NotSupportedException>(() =>
            ((IList<EvidenceId>)assessment.SupportingEvidenceIds).Add(contradiction));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(10000)]
    public void AcceptsExactConfidenceBounds(int confidenceBasisPoints)
    {
        CharacterBeliefAssessment assessment = new(Belief(), confidenceBasisPoints, [], []);

        Assert.Equal(confidenceBasisPoints, assessment.ConfidenceBasisPoints);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(10001)]
    public void RejectsConfidenceOutsideBounds(int confidenceBasisPoints)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new CharacterBeliefAssessment(Belief(), confidenceBasisPoints, [], []));
    }

    [Fact]
    public void RejectsNonBeliefEpistemicCategories()
    {
        CharacterEpistemicMemory fact = CharacterEpistemicMemoryTests.Memory(
            CharacterEpistemicMemoryCategory.ConfirmedFact,
            null);
        CharacterEpistemicMemory claim = CharacterEpistemicMemoryTests.Memory(
            CharacterEpistemicMemoryCategory.AttributedClaim,
            new CharacterId("seeker"));

        Assert.Throws<ArgumentException>(() => new CharacterBeliefAssessment(fact, 500, [], []));
        Assert.Throws<ArgumentException>(() => new CharacterBeliefAssessment(claim, 500, [], []));
    }

    [Fact]
    public void AcceptsExactEvidenceBounds()
    {
        EvidenceId[] supporting = Enumerable
            .Range(0, CharacterBeliefAssessment.MaximumEvidenceCountPerDisposition)
            .Select(index => new EvidenceId($"support-{index}"))
            .ToArray();
        EvidenceId[] contradicting = Enumerable
            .Range(0, CharacterBeliefAssessment.MaximumEvidenceCountPerDisposition)
            .Select(index => new EvidenceId($"contradiction-{index}"))
            .ToArray();

        CharacterBeliefAssessment assessment = new(Belief(), 500, supporting, contradicting);

        Assert.Equal(
            CharacterBeliefAssessment.MaximumEvidenceCountPerDisposition,
            assessment.SupportingEvidenceIds.Count);
        Assert.Equal(
            CharacterBeliefAssessment.MaximumEvidenceCountPerDisposition,
            assessment.ContradictingEvidenceIds.Count);
    }

    [Fact]
    public void RejectsInvalidEvidenceCollections()
    {
        EvidenceId evidenceId = new("moonwort-observation");
        EvidenceId duplicate = new("moonwort-observation");
        EvidenceId[] tooMany = Enumerable
            .Range(0, CharacterBeliefAssessment.MaximumEvidenceCountPerDisposition + 1)
            .Select(index => new EvidenceId($"evidence-{index}"))
            .ToArray();

        Assert.Throws<ArgumentException>(() =>
            new CharacterBeliefAssessment(Belief(), 500, [evidenceId, duplicate], []));
        Assert.Throws<ArgumentException>(() =>
            new CharacterBeliefAssessment(Belief(), 500, [null!], []));
        Assert.Throws<ArgumentException>(() =>
            new CharacterBeliefAssessment(Belief(), 500, [evidenceId], [duplicate]));
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new CharacterBeliefAssessment(Belief(), 500, tooMany, []));
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new CharacterBeliefAssessment(Belief(), 500, [], tooMany));
    }

    [Fact]
    public void RejectsNullInputs()
    {
        Assert.Throws<ArgumentNullException>(() =>
            new CharacterBeliefAssessment(null!, 500, [], []));
        Assert.Throws<ArgumentNullException>(() =>
            new CharacterBeliefAssessment(Belief(), 500, null!, []));
        Assert.Throws<ArgumentNullException>(() =>
            new CharacterBeliefAssessment(Belief(), 500, [], null!));
    }

    internal static CharacterEpistemicMemory Belief(
        CharacterId? characterId = null,
        string memoryId = "moonwort-cooling") =>
        CharacterEpistemicMemoryTests.Memory(
            CharacterEpistemicMemoryCategory.UncertainBelief,
            null,
            memoryId: memoryId,
            characterId: characterId);
}
