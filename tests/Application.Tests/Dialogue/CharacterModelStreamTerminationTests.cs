using Allrpcs.Application.Dialogue;
using Allrpcs.Domain.Dialogue;

namespace Allrpcs.Application.Tests.Dialogue;

public sealed class CharacterModelStreamTerminationTests
{
    public static TheoryData<CharacterModelStreamTerminationStatus> NonCompletionStatuses => new()
    {
        CharacterModelStreamTerminationStatus.Cancelled,
        CharacterModelStreamTerminationStatus.TimedOut,
    };

    public static TheoryData<CharacterModelFailureCode> DefinedFailureCodes => new()
    {
        CharacterModelFailureCode.Unavailable,
        CharacterModelFailureCode.Authentication,
        CharacterModelFailureCode.RateLimited,
        CharacterModelFailureCode.Transport,
        CharacterModelFailureCode.InvalidResponse,
        CharacterModelFailureCode.ProviderRejected,
    };

    [Fact]
    public void RetainsTheExactResponseOnlyForCompletedGeneration()
    {
        CharacterTurnResponse response = CreateResponse();

        CharacterModelStreamTermination termination = new(
            CharacterModelStreamTerminationStatus.Completed,
            response,
            null);

        Assert.Equal(CharacterModelStreamTerminationStatus.Completed, termination.Status);
        Assert.Same(response, termination.Response);
        Assert.Null(termination.Failure);
    }

    [Theory]
    [MemberData(nameof(NonCompletionStatuses))]
    public void RetainsANonCompletionStatusWithoutAResponse(
        CharacterModelStreamTerminationStatus status)
    {
        CharacterModelStreamTermination termination = new(status, null, null);

        Assert.Equal(status, termination.Status);
        Assert.Null(termination.Response);
        Assert.Null(termination.Failure);
    }

    [Theory]
    [MemberData(nameof(DefinedFailureCodes))]
    public void RetainsAnExactDefinedFailure(CharacterModelFailureCode code)
    {
        CharacterModelFailure failure = new(code);

        CharacterModelStreamTermination termination = new(
            CharacterModelStreamTerminationStatus.Failed,
            null,
            failure);

        Assert.Equal(CharacterModelStreamTerminationStatus.Failed, termination.Status);
        Assert.Null(termination.Response);
        Assert.Same(failure, termination.Failure);
    }

    [Fact]
    public void RejectsMissingOrMisplacedFailures()
    {
        CharacterModelFailure failure = new(CharacterModelFailureCode.Transport);

        Assert.Throws<ArgumentNullException>(() => new CharacterModelStreamTermination(
            CharacterModelStreamTerminationStatus.Completed,
            null,
            null));
        Assert.Throws<ArgumentException>(() => new CharacterModelStreamTermination(
            CharacterModelStreamTerminationStatus.Failed,
            CreateResponse(),
            failure));
        Assert.Throws<ArgumentNullException>(() => new CharacterModelStreamTermination(
            CharacterModelStreamTerminationStatus.Failed,
            null,
            null));
        Assert.Throws<ArgumentException>(() => new CharacterModelStreamTermination(
            CharacterModelStreamTerminationStatus.Completed,
            CreateResponse(),
            failure));
        Assert.Throws<ArgumentException>(() => new CharacterModelStreamTermination(
            CharacterModelStreamTerminationStatus.Cancelled,
            CreateResponse(),
            null));
        Assert.Throws<ArgumentException>(() => new CharacterModelStreamTermination(
            CharacterModelStreamTerminationStatus.Cancelled,
            null,
            failure));
        Assert.Throws<ArgumentException>(() => new CharacterModelStreamTermination(
            CharacterModelStreamTerminationStatus.TimedOut,
            CreateResponse(),
            null));
        Assert.Throws<ArgumentException>(() => new CharacterModelStreamTermination(
            CharacterModelStreamTerminationStatus.TimedOut,
            null,
            failure));
    }

    [Fact]
    public void RejectsUndefinedStatusAndFailureCode()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new CharacterModelStreamTermination(
            (CharacterModelStreamTerminationStatus)int.MaxValue,
            null,
            null));
        Assert.Throws<ArgumentOutOfRangeException>(() => new CharacterModelFailure(
            (CharacterModelFailureCode)int.MaxValue));
    }

    [Fact]
    public void ExposesClosedImmutableStreamUpdateTypes()
    {
        Assert.True(typeof(CharacterModelStreamUpdate).IsAbstract);
        Assert.Empty(typeof(CharacterModelStreamUpdate).GetConstructors());
        Assert.Empty(typeof(CharacterModelStreamUpdate).GetProperties());

        Assert.True(typeof(CharacterModelFailure).IsSealed);
        Assert.Collection(
            typeof(CharacterModelFailure).GetProperties(),
            property => AssertGetOnlyProperty(property, nameof(CharacterModelFailure.Code)),
            property => AssertGetOnlyProperty(property, nameof(CharacterModelFailure.RetryAfter)));

        Assert.True(typeof(CharacterModelStreamTermination).IsSealed);
        Assert.Equal(typeof(CharacterModelStreamUpdate), typeof(CharacterModelStreamTermination).BaseType);
        Assert.Collection(
            typeof(CharacterModelStreamTermination).GetProperties().OrderBy(property => property.Name),
            property => AssertGetOnlyProperty(property, nameof(CharacterModelStreamTermination.Failure)),
            property => AssertGetOnlyProperty(property, nameof(CharacterModelStreamTermination.Response)),
            property => AssertGetOnlyProperty(property, nameof(CharacterModelStreamTermination.Status)));
    }

    private static CharacterTurnResponse CreateResponse()
    {
        CharacterTurnRequest request = CharacterModelPlayerDialogueContextProjectionTests.CreateRequest();
        return new CharacterTurnResponse(
            request,
            CharacterTurnDisposition.Accept,
            null,
            new CharacterIntention(CharacterIntentionType.Wait, null),
            [],
            []);
    }

    private static void AssertGetOnlyProperty(System.Reflection.PropertyInfo property, string name)
    {
        Assert.Equal(name, property.Name);
        Assert.NotNull(property.GetMethod);
        Assert.Null(property.SetMethod);
    }
}
