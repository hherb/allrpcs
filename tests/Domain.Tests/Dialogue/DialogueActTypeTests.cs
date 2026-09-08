using Allrpcs.Domain.Dialogue;

namespace Allrpcs.Domain.Tests.Dialogue;

public sealed class DialogueActTypeTests
{
    private static readonly string[] ExpectedPropertyNames =
    [
        "All",
        "Command",
        "ConditionalPlan",
        "Promise",
        "Question",
        "ReportedClaim",
        "Request",
        "Speculation",
        "Value",
        "Warning",
    ];

    [Fact]
    public void DefinesTheCompleteCanonicalDialogueActSetInRoadmapOrder()
    {
        Assert.Collection(
            DialogueActType.All,
            type => Assert.Same(DialogueActType.Command, type),
            type => Assert.Same(DialogueActType.Request, type),
            type => Assert.Same(DialogueActType.Question, type),
            type => Assert.Same(DialogueActType.Speculation, type),
            type => Assert.Same(DialogueActType.Warning, type),
            type => Assert.Same(DialogueActType.ReportedClaim, type),
            type => Assert.Same(DialogueActType.Promise, type),
            type => Assert.Same(DialogueActType.ConditionalPlan, type));
    }

    [Fact]
    public void CanonicalTextRoundTripsToTheDefinedInstances()
    {
        string[] expectedValues =
        [
            "command",
            "request",
            "question",
            "speculation",
            "warning",
            "reported-claim",
            "promise",
            "conditional-plan",
        ];

        Assert.Equal(expectedValues, DialogueActType.All.Select(type => type.Value));

        foreach (DialogueActType type in DialogueActType.All)
        {
            Assert.Equal(type.Value, type.ToString());
            Assert.Same(type, DialogueActType.Parse(type.ToString()));
        }
    }

    [Theory]
    [InlineData("")]
    [InlineData("Command")]
    [InlineData("reported_claim")]
    [InlineData("conditional--plan")]
    [InlineData("warning ")]
    [InlineData("statement")]
    public void RejectsUnknownOrNonCanonicalText(string value)
    {
        Assert.Throws<ArgumentException>(() => DialogueActType.Parse(value));
    }

    [Fact]
    public void RejectsNullText()
    {
        Assert.Throws<ArgumentNullException>(() => DialogueActType.Parse(null!));
    }

    [Fact]
    public void ExposesOnlyTheClosedDialogueActTypeSurface()
    {
        string[] publicProperties = typeof(DialogueActType)
            .GetProperties()
            .Select(property => property.Name)
            .OrderBy(name => name, StringComparer.Ordinal)
            .ToArray();

        Assert.Equal(
            ExpectedPropertyNames,
            publicProperties);
        Assert.Empty(typeof(DialogueActType).GetConstructors());
    }
}
