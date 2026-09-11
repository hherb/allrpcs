using Allrpcs.Domain.Identifiers;
using Allrpcs.Domain.Investigations;

namespace Allrpcs.Domain.Tests.Investigations;

public sealed class OpenInvestigationQuestionSnapshotTests
{
    [Fact]
    public void CopiesQuestionsInStableOrderAndRetainsExactInstances()
    {
        InvestigationQuestion first = Question("first-question");
        InvestigationQuestion second = Question("second-question");
        InvestigationQuestion replacement = Question("replacement");
        InvestigationQuestion[] supplied = [first, second];

        OpenInvestigationQuestionSnapshot snapshot = new(supplied);
        supplied[0] = replacement;

        Assert.Collection(
            snapshot.Questions,
            question => Assert.Same(first, question),
            question => Assert.Same(second, question));
        Assert.Throws<NotSupportedException>(() =>
            ((IList<InvestigationQuestion>)snapshot.Questions).Add(replacement));
    }

    [Fact]
    public void AcceptsEmptyAndTheMaximumDistinctQuestionCount()
    {
        InvestigationQuestion[] maximum = Enumerable
            .Range(0, OpenInvestigationQuestionSnapshot.MaximumQuestionCount)
            .Select(index => Question($"question-{index}"))
            .ToArray();

        Assert.Empty(new OpenInvestigationQuestionSnapshot([]).Questions);
        Assert.Equal(
            OpenInvestigationQuestionSnapshot.MaximumQuestionCount,
            new OpenInvestigationQuestionSnapshot(maximum).Questions.Count);
    }

    [Fact]
    public void RejectsNullsExcessiveQuestionsAndDuplicateIdentities()
    {
        InvestigationQuestion[] excessive = Enumerable
            .Range(0, OpenInvestigationQuestionSnapshot.MaximumQuestionCount + 1)
            .Select(index => Question($"question-{index}"))
            .ToArray();

        Assert.Throws<ArgumentNullException>(() => new OpenInvestigationQuestionSnapshot(null!));
        Assert.Throws<ArgumentException>(() => new OpenInvestigationQuestionSnapshot([null!]));
        Assert.Throws<ArgumentException>(() => new OpenInvestigationQuestionSnapshot(
            [Question("first-question"), Question("first-question")]));
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new OpenInvestigationQuestionSnapshot(excessive));
    }

    [Fact]
    public void ExposesOnlyTheReadOnlySnapshotSurface()
    {
        Type snapshotType = typeof(OpenInvestigationQuestionSnapshot);
        System.Reflection.PropertyInfo property = Assert.Single(snapshotType.GetProperties());
        System.Reflection.ConstructorInfo constructor = Assert.Single(snapshotType.GetConstructors());

        Assert.Equal("Questions", property.Name);
        Assert.Equal(typeof(IReadOnlyList<InvestigationQuestion>), property.PropertyType);
        Assert.False(property.CanWrite);
        Assert.Collection(
            constructor.GetParameters(),
            parameter => Assert.Equal(
                typeof(IEnumerable<InvestigationQuestion>),
                parameter.ParameterType));
        Assert.Empty(snapshotType.GetEvents());
        Assert.Empty(snapshotType.GetMethods().Where(
            method => method.DeclaringType == snapshotType && !method.IsSpecialName));
        Assert.True(snapshotType.IsSealed);
    }

    internal static InvestigationQuestion Question(string questionId) =>
        new(new InvestigationQuestionId(questionId), "What does the evidence support?");
}
