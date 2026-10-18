using Allrpcs.Application.Characters;

namespace Allrpcs.Application.Tests.Characters;

public sealed class StructuredDecisionPipelineTests
{
    [Fact]
    public void PublishesOnlyAfterCompletionAndRetainsTheExactRequestAndDecision()
    {
        object request = new();
        object decision = new();
        int parses = 0;
        StructuredDecisionPipeline<object, object> pipeline = new(request, (actual, text) =>
        {
            Assert.Same(request, actual);
            Assert.Equal("result", text);
            parses++;
            return decision;
        }, 6);

        Assert.Null(pipeline.Apply(StructuredDecisionEvent.Started));
        Assert.Null(pipeline.Apply(StructuredDecisionEvent.InProgress));
        Assert.Null(pipeline.Apply(StructuredDecisionEvent.Metadata));
        Assert.Null(pipeline.Apply(StructuredDecisionEvent.TextDelta, "res"));
        Assert.Null(pipeline.Apply(StructuredDecisionEvent.TextDelta, "ult"));
        Assert.Null(pipeline.Apply(StructuredDecisionEvent.TextDone, "result"));
        Assert.Equal(1, parses);
        Assert.Same(decision, pipeline.Apply(StructuredDecisionEvent.Completed));
        Assert.Throws<InvalidOperationException>(() => pipeline.Apply(StructuredDecisionEvent.Completed));
    }

    [Theory]
    [InlineData(StructuredDecisionEvent.InProgress)]
    [InlineData(StructuredDecisionEvent.Metadata)]
    [InlineData(StructuredDecisionEvent.TextDelta)]
    [InlineData(StructuredDecisionEvent.TextDone)]
    [InlineData(StructuredDecisionEvent.Completed)]
    [InlineData(StructuredDecisionEvent.Failed)]
    [InlineData((StructuredDecisionEvent)999)]
    public void RejectsOutOfOrderOrUnsupportedEventsAndPermanentlyFencesTheStream(
        StructuredDecisionEvent kind)
    {
        StructuredDecisionPipeline<object, object> pipeline = Create();
        Assert.Throws<InvalidDataException>(() => pipeline.Apply(kind, "secret"));
        Assert.Throws<InvalidOperationException>(() => pipeline.Apply(StructuredDecisionEvent.Started));
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void EnforcesBothIncrementalAndFinalDocumentBounds(bool final)
    {
        StructuredDecisionPipeline<object, object> pipeline = Create();
        pipeline.Apply(StructuredDecisionEvent.Started);
        pipeline.Apply(StructuredDecisionEvent.InProgress);
        pipeline.Apply(StructuredDecisionEvent.TextDelta, "1234");
        Assert.Throws<InvalidDataException>(() => pipeline.Apply(
            final ? StructuredDecisionEvent.TextDone : StructuredDecisionEvent.TextDelta,
            final ? "12345" : "5"));
        Assert.Throws<InvalidOperationException>(() => pipeline.Apply(StructuredDecisionEvent.Completed));
    }

    [Fact]
    public void KeepsLegacyPreludeOptionalOnlyWhenExplicitlyConfigured()
    {
        StructuredDecisionPipeline<object, object> pipeline = new(new object(), (_, _) => new object(),
            4, requireLifecycle: false);
        Assert.Null(pipeline.Apply(StructuredDecisionEvent.Metadata));
        Assert.Null(pipeline.Apply(StructuredDecisionEvent.TextDone, "text"));
        Assert.NotNull(pipeline.Apply(StructuredDecisionEvent.Completed));
    }

    [Theory]
    [InlineData(StructuredDecisionEvent.TextDone)]
    [InlineData(StructuredDecisionEvent.TextDelta)]
    public void RejectsOutputAfterTheVerifiedFinalText(StructuredDecisionEvent kind)
    {
        StructuredDecisionPipeline<object, object> pipeline = Create();
        pipeline.Apply(StructuredDecisionEvent.Started);
        pipeline.Apply(StructuredDecisionEvent.InProgress);
        pipeline.Apply(StructuredDecisionEvent.TextDone, "text");
        Assert.Throws<InvalidDataException>(() => pipeline.Apply(kind, "text"));
    }

    [Fact]
    public void RedactsRejectedDecisionDetailsAndFencesFailures()
    {
        StructuredDecisionPipeline<object, object> pipeline = new(new object(),
            (_, _) => throw new ArgumentException("private provider document"), 4, requireLifecycle: false);
        InvalidDataException failure = Assert.Throws<InvalidDataException>(() =>
            pipeline.Apply(StructuredDecisionEvent.TextDone, "text"));
        Assert.DoesNotContain("private", failure.ToString(), StringComparison.Ordinal);
        Assert.Throws<InvalidOperationException>(() => pipeline.Apply(StructuredDecisionEvent.Completed));
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void PreservesTheDecisionSpecificMetadataAfterTextPolicy(bool allowMetadataAfterTextDone)
    {
        StructuredDecisionPipeline<object, object> pipeline = new(new object(), (_, _) => new object(),
            4, allowMetadataAfterTextDone: allowMetadataAfterTextDone);
        pipeline.Apply(StructuredDecisionEvent.Started);
        pipeline.Apply(StructuredDecisionEvent.InProgress);
        pipeline.Apply(StructuredDecisionEvent.TextDone, "text");
        if (allowMetadataAfterTextDone)
        {
            Assert.Null(pipeline.Apply(StructuredDecisionEvent.Metadata));
            Assert.NotNull(pipeline.Apply(StructuredDecisionEvent.Completed));
        }
        else
        {
            Assert.Throws<InvalidDataException>(() => pipeline.Apply(StructuredDecisionEvent.Metadata));
            Assert.Throws<InvalidOperationException>(() => pipeline.Apply(StructuredDecisionEvent.Completed));
        }
    }

    [Fact]
    public void RejectsInvalidConstructionAndMissingOrNullFinalDecisions()
    {
        Assert.Throws<ArgumentNullException>(() =>
            new StructuredDecisionPipeline<object, object>(null!, (_, _) => new object(), 4));
        Assert.Throws<ArgumentNullException>(() =>
            new StructuredDecisionPipeline<object, object>(new object(), null!, 4));
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new StructuredDecisionPipeline<object, object>(new object(), (_, _) => new object(), 0));
        StructuredDecisionPipeline<object, object> pipeline = new(new object(), (_, _) => null!,
            4, requireLifecycle: false);
        Assert.Throws<InvalidDataException>(() => pipeline.Apply(StructuredDecisionEvent.TextDone, "text"));
        Assert.Throws<InvalidOperationException>(() => pipeline.Apply(StructuredDecisionEvent.Completed));
    }

    private static StructuredDecisionPipeline<object, object> Create() =>
        new(new object(), (_, _) => new object(), 4);
}
