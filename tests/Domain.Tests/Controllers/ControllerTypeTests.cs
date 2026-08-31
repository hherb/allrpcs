using Allrpcs.Domain.Controllers;

namespace Allrpcs.Domain.Tests.Controllers;

public sealed class ControllerTypeTests
{
    [Fact]
    public void DefinesTheCompleteCanonicalControllerTypeSet()
    {
        Assert.Collection(
            ControllerType.All,
            type => Assert.Equal("human", type.Value),
            type => Assert.Equal("llm", type.Value),
            type => Assert.Equal("deterministic-test", type.Value),
            type => Assert.Equal("dormant", type.Value));
    }

    [Fact]
    public void CanonicalTextRoundTripsToTheDefinedInstances()
    {
        foreach (ControllerType type in ControllerType.All)
        {
            Assert.Same(type, ControllerType.Parse(type.ToString()));
        }
    }

    [Fact]
    public void EqualityDistinguishesControllerTypes()
    {
        Assert.Equal(ControllerType.Human, ControllerType.Parse("human"));
        Assert.True(ControllerType.Human == ControllerType.Parse("human"));
        Assert.NotEqual(ControllerType.Human, ControllerType.Llm);
        Assert.True(ControllerType.Human != ControllerType.Llm);
    }

    [Theory]
    [InlineData("")]
    [InlineData("Human")]
    [InlineData("deterministic")]
    [InlineData("deterministic_test")]
    [InlineData("provider-openai")]
    [InlineData("dormant ")]
    public void RejectsUnknownOrNonCanonicalText(string value)
    {
        Assert.Throws<ArgumentException>(() => ControllerType.Parse(value));
    }

    [Fact]
    public void RejectsNullText()
    {
        Assert.Throws<ArgumentNullException>(() => ControllerType.Parse(null!));
    }
}
