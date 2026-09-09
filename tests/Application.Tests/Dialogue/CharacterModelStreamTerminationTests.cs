using Allrpcs.Application.Dialogue;

namespace Allrpcs.Application.Tests.Dialogue;

public sealed class CharacterModelStreamTerminationTests
{
    public static TheoryData<CharacterModelStreamTerminationStatus> NonFailureStatuses => new()
    {
        CharacterModelStreamTerminationStatus.Completed,
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

    [Theory]
    [MemberData(nameof(NonFailureStatuses))]
    public void RetainsANonFailureTerminalStatus(CharacterModelStreamTerminationStatus status)
    {
        CharacterModelStreamTermination termination = new(status, null);

        Assert.Equal(status, termination.Status);
        Assert.Null(termination.Failure);
    }

    [Theory]
    [MemberData(nameof(DefinedFailureCodes))]
    public void RetainsAnExactDefinedFailure(CharacterModelFailureCode code)
    {
        CharacterModelFailure failure = new(code);

        CharacterModelStreamTermination termination = new(
            CharacterModelStreamTerminationStatus.Failed,
            failure);

        Assert.Equal(CharacterModelStreamTerminationStatus.Failed, termination.Status);
        Assert.Same(failure, termination.Failure);
    }

    [Fact]
    public void RejectsMissingOrMisplacedFailures()
    {
        CharacterModelFailure failure = new(CharacterModelFailureCode.Transport);

        Assert.Throws<ArgumentNullException>(() => new CharacterModelStreamTermination(
            CharacterModelStreamTerminationStatus.Failed,
            null));
        Assert.Throws<ArgumentException>(() => new CharacterModelStreamTermination(
            CharacterModelStreamTerminationStatus.Completed,
            failure));
        Assert.Throws<ArgumentException>(() => new CharacterModelStreamTermination(
            CharacterModelStreamTerminationStatus.Cancelled,
            failure));
        Assert.Throws<ArgumentException>(() => new CharacterModelStreamTermination(
            CharacterModelStreamTerminationStatus.TimedOut,
            failure));
    }

    [Fact]
    public void RejectsUndefinedStatusAndFailureCode()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new CharacterModelStreamTermination(
            (CharacterModelStreamTerminationStatus)int.MaxValue,
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
            property => AssertGetOnlyProperty(property, nameof(CharacterModelFailure.Code)));

        Assert.True(typeof(CharacterModelStreamTermination).IsSealed);
        Assert.Equal(typeof(CharacterModelStreamUpdate), typeof(CharacterModelStreamTermination).BaseType);
        Assert.Collection(
            typeof(CharacterModelStreamTermination).GetProperties().OrderBy(property => property.Name),
            property => AssertGetOnlyProperty(property, nameof(CharacterModelStreamTermination.Failure)),
            property => AssertGetOnlyProperty(property, nameof(CharacterModelStreamTermination.Status)));
    }

    private static void AssertGetOnlyProperty(System.Reflection.PropertyInfo property, string name)
    {
        Assert.Equal(name, property.Name);
        Assert.NotNull(property.GetMethod);
        Assert.Null(property.SetMethod);
    }
}
