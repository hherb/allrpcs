using Allrpcs.Domain.Identifiers;
using Allrpcs.Domain.Investigations;

namespace Allrpcs.Domain.Tests.Investigations;

public sealed class InvestigationConclusionSnapshotTests
{
    [Fact]
    public void CopiesConclusionsInStableOrderAndRetainsExactInstances()
    {
        InvestigationConclusion first = Conclusion("first");
        InvestigationConclusion second = Conclusion("second");
        InvestigationConclusion replacement = Conclusion("replacement");
        InvestigationConclusion[] supplied = [first, second];

        InvestigationConclusionSnapshot snapshot = new(supplied);
        supplied[0] = replacement;

        Assert.Collection(
            snapshot.Conclusions,
            conclusion => Assert.Same(first, conclusion),
            conclusion => Assert.Same(second, conclusion));
        Assert.Throws<NotSupportedException>(() =>
            ((IList<InvestigationConclusion>)snapshot.Conclusions).Add(replacement));
    }

    [Fact]
    public void AcceptsEmptyAndTheMaximumDistinctConclusionCount()
    {
        InvestigationConclusion[] maximum = Enumerable
            .Range(0, InvestigationConclusionSnapshot.MaximumConclusionCount)
            .Select(index => Conclusion($"conclusion-{index}"))
            .ToArray();

        Assert.Empty(new InvestigationConclusionSnapshot([]).Conclusions);
        Assert.Equal(
            InvestigationConclusionSnapshot.MaximumConclusionCount,
            new InvestigationConclusionSnapshot(maximum).Conclusions.Count);
    }

    [Fact]
    public void RejectsNullExcessiveDuplicateConclusionAndDuplicateQuestionIdentities()
    {
        InvestigationConclusion first = Conclusion("first");
        InvestigationQuestion duplicateConclusionQuestion = Question("different-question");
        InvestigationConclusion duplicateConclusion = new(
            new InvestigationConclusionId(first.ConclusionId.Value),
            duplicateConclusionQuestion,
            Hypothesis("different-hypothesis", duplicateConclusionQuestion),
            "A different conclusion with the same conclusion identity.");
        InvestigationQuestion sameQuestion = Question(first.Question.QuestionId.Value);
        InvestigationHypothesis alternate = Hypothesis("alternate-hypothesis", sameQuestion);
        InvestigationConclusion duplicateQuestion = new(
            new InvestigationConclusionId("other-conclusion"),
            sameQuestion,
            alternate,
            "An alternate conclusion for the same question identity.");
        InvestigationConclusion[] excessive = Enumerable
            .Range(0, InvestigationConclusionSnapshot.MaximumConclusionCount + 1)
            .Select(index => Conclusion($"conclusion-{index}"))
            .ToArray();

        Assert.Throws<ArgumentNullException>(() => new InvestigationConclusionSnapshot(null!));
        Assert.Throws<ArgumentException>(() => new InvestigationConclusionSnapshot([null!]));
        Assert.Throws<ArgumentException>(() =>
            new InvestigationConclusionSnapshot([first, duplicateConclusion]));
        Assert.Throws<ArgumentException>(() =>
            new InvestigationConclusionSnapshot([first, duplicateQuestion]));
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new InvestigationConclusionSnapshot(excessive));
    }

    [Fact]
    public void ExposesOnlyTheReadOnlySnapshotSurface()
    {
        Type snapshotType = typeof(InvestigationConclusionSnapshot);
        System.Reflection.PropertyInfo property = Assert.Single(snapshotType.GetProperties());
        System.Reflection.ConstructorInfo constructor = Assert.Single(snapshotType.GetConstructors());

        Assert.Equal("Conclusions", property.Name);
        Assert.Equal(typeof(IReadOnlyList<InvestigationConclusion>), property.PropertyType);
        Assert.False(property.CanWrite);
        Assert.Collection(
            constructor.GetParameters(),
            parameter => Assert.Equal(
                typeof(IEnumerable<InvestigationConclusion>),
                parameter.ParameterType));
        Assert.Empty(snapshotType.GetEvents());
        Assert.Empty(snapshotType.GetMethods().Where(
            method => method.DeclaringType == snapshotType && !method.IsSpecialName));
        Assert.True(snapshotType.IsSealed);
    }

    internal static InvestigationConclusion Conclusion(string suffix)
    {
        InvestigationQuestion question = Question($"question-{suffix}");
        InvestigationHypothesis hypothesis = Hypothesis($"hypothesis-{suffix}", question);
        return new InvestigationConclusion(
            new InvestigationConclusionId($"conclusion-{suffix}"),
            question,
            hypothesis,
            "An authored conclusion.");
    }

    private static InvestigationQuestion Question(string id) =>
        new(new InvestigationQuestionId(id), "An authored question?");

    private static InvestigationHypothesis Hypothesis(
        string id,
        InvestigationQuestion question) =>
        new(new InvestigationHypothesisId(id), question, "An authored hypothesis.");
}
