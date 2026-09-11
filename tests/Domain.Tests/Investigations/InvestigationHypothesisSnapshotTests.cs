using Allrpcs.Domain.Identifiers;
using Allrpcs.Domain.Investigations;

namespace Allrpcs.Domain.Tests.Investigations;

public sealed class InvestigationHypothesisSnapshotTests
{
    [Fact]
    public void CopiesHypothesesInStableOrderAndRetainsExactInstances()
    {
        InvestigationQuestion question = OpenInvestigationQuestionSnapshotTests.Question("question");
        InvestigationHypothesis first = Hypothesis("first-hypothesis", question);
        InvestigationHypothesis second = Hypothesis("second-hypothesis", question);
        InvestigationHypothesis replacement = Hypothesis("replacement", question);
        InvestigationHypothesis[] supplied = [first, second];

        InvestigationHypothesisSnapshot snapshot = new(supplied);
        supplied[0] = replacement;

        Assert.Collection(
            snapshot.Hypotheses,
            hypothesis => Assert.Same(first, hypothesis),
            hypothesis => Assert.Same(second, hypothesis));
        Assert.Throws<NotSupportedException>(() =>
            ((IList<InvestigationHypothesis>)snapshot.Hypotheses).Add(replacement));
    }

    [Fact]
    public void AcceptsEmptyAndMaximumCountAndRejectsInvalidCollections()
    {
        InvestigationQuestion question = OpenInvestigationQuestionSnapshotTests.Question("question");
        InvestigationHypothesis[] maximum = Enumerable
            .Range(0, InvestigationHypothesisSnapshot.MaximumHypothesisCount)
            .Select(index => Hypothesis($"hypothesis-{index}", question))
            .ToArray();

        Assert.Empty(new InvestigationHypothesisSnapshot([]).Hypotheses);
        Assert.Equal(
            InvestigationHypothesisSnapshot.MaximumHypothesisCount,
            new InvestigationHypothesisSnapshot(maximum).Hypotheses.Count);
        Assert.Throws<ArgumentNullException>(() => new InvestigationHypothesisSnapshot(null!));
        Assert.Throws<ArgumentException>(() => new InvestigationHypothesisSnapshot([null!]));
        Assert.Throws<ArgumentException>(() => new InvestigationHypothesisSnapshot(
            [Hypothesis("duplicate", question), Hypothesis("duplicate", question)]));
        Assert.Throws<ArgumentOutOfRangeException>(() => new InvestigationHypothesisSnapshot(
            maximum.Append(Hypothesis("extra-hypothesis", question))));
    }

    [Fact]
    public void ExposesOnlyTheReadOnlySnapshotSurface()
    {
        Type snapshotType = typeof(InvestigationHypothesisSnapshot);
        System.Reflection.PropertyInfo property = Assert.Single(snapshotType.GetProperties());
        System.Reflection.ConstructorInfo constructor = Assert.Single(snapshotType.GetConstructors());

        Assert.Equal("Hypotheses", property.Name);
        Assert.Equal(typeof(IReadOnlyList<InvestigationHypothesis>), property.PropertyType);
        Assert.False(property.CanWrite);
        Assert.Collection(
            constructor.GetParameters(),
            parameter => Assert.Equal(
                typeof(IEnumerable<InvestigationHypothesis>),
                parameter.ParameterType));
        Assert.Empty(snapshotType.GetEvents());
        Assert.Empty(snapshotType.GetMethods().Where(
            method => method.DeclaringType == snapshotType && !method.IsSpecialName));
        Assert.True(snapshotType.IsSealed);
    }

    internal static InvestigationHypothesis Hypothesis(
        string hypothesisId,
        InvestigationQuestion question) =>
        new(
            new InvestigationHypothesisId(hypothesisId),
            question,
            "The authored evidence may support this hypothesis.");
}
