using Allrpcs.Domain.Identifiers;
using Allrpcs.Domain.Investigations;

namespace Allrpcs.Domain.Tests.Investigations;

public sealed class InvestigationConsequenceTests
{
    [Fact]
    public void RetainsExactIdentityConclusionAndAuthoredDescription()
    {
        InvestigationConsequenceId consequenceId = new("patient-can-be-stabilized");
        InvestigationConclusion conclusion = InvestigationConclusionTests.Conclusion();
        const string Description = "The stabilizing remedy becomes available.";

        InvestigationConsequence consequence = new(consequenceId, conclusion, Description);

        Assert.Same(consequenceId, consequence.ConsequenceId);
        Assert.Same(conclusion, consequence.Conclusion);
        Assert.Equal(Description, consequence.Description);
    }

    [Fact]
    public void RejectsBlankNullAndOversizedValues()
    {
        InvestigationConsequenceId consequenceId = new("consequence");
        InvestigationConclusion conclusion = InvestigationConclusionTests.Conclusion();
        string excessive = new('c', InvestigationConsequence.MaximumDescriptionLength + 1);

        Assert.Throws<ArgumentNullException>(() =>
            new InvestigationConsequence(null!, conclusion, "Consequence"));
        Assert.Throws<ArgumentNullException>(() =>
            new InvestigationConsequence(consequenceId, null!, "Consequence"));
        Assert.Throws<ArgumentNullException>(() =>
            new InvestigationConsequence(consequenceId, conclusion, null!));
        Assert.Throws<ArgumentException>(() =>
            new InvestigationConsequence(consequenceId, conclusion, " "));
        Assert.Throws<ArgumentException>(() =>
            new InvestigationConsequence(consequenceId, conclusion, excessive));
    }

    [Fact]
    public void ExposesOnlyTheImmutableNonExecutableSurface()
    {
        Type consequenceType = typeof(InvestigationConsequence);
        System.Reflection.PropertyInfo[] properties = consequenceType.GetProperties();
        System.Reflection.ConstructorInfo constructor = Assert.Single(consequenceType.GetConstructors());

        Assert.Equal(
            ["Conclusion", "ConsequenceId", "Description"],
            properties.Select(property => property.Name).Order());
        Assert.All(properties, property => Assert.False(property.CanWrite));
        Assert.Equal(
            [typeof(InvestigationConsequenceId), typeof(InvestigationConclusion), typeof(string)],
            constructor.GetParameters().Select(parameter => parameter.ParameterType).ToArray());
        Assert.Empty(consequenceType.GetEvents());
        Assert.Empty(consequenceType.GetMethods().Where(
            method => method.DeclaringType == consequenceType && !method.IsSpecialName));
        Assert.True(consequenceType.IsSealed);
    }
}
