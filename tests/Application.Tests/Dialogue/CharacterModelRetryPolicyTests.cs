using Allrpcs.Application.Dialogue;

namespace Allrpcs.Application.Tests.Dialogue;

public sealed class CharacterModelRetryPolicyTests
{
    public static TheoryData<CharacterModelFailureCode> RetryableFailureCodes => new()
    {
        CharacterModelFailureCode.Unavailable,
        CharacterModelFailureCode.RateLimited,
        CharacterModelFailureCode.Transport,
    };

    public static TheoryData<CharacterModelFailureCode> NonRetryableFailureCodes => new()
    {
        CharacterModelFailureCode.Authentication,
        CharacterModelFailureCode.InvalidResponse,
        CharacterModelFailureCode.ProviderRejected,
    };

    [Theory]
    [MemberData(nameof(RetryableFailureCodes))]
    public void AllowsOneTextFreeRetryForTransientFailures(CharacterModelFailureCode code)
    {
        CharacterModelStreamTermination failure = Failed(code);
        CharacterModelStreamSnapshot attempt = new([failure]);

        bool canRetry = CharacterModelRetryPolicy.CanRetry(1, attempt);

        Assert.True(canRetry);
        Assert.Same(failure, Assert.Single(attempt.Updates));
    }

    [Theory]
    [MemberData(nameof(NonRetryableFailureCodes))]
    public void RejectsNonTransientFailures(CharacterModelFailureCode code)
    {
        CharacterModelStreamSnapshot attempt = new([Failed(code)]);

        Assert.False(CharacterModelRetryPolicy.CanRetry(1, attempt));
    }

    [Fact]
    public void RejectsRetryAfterAnyStreamedText()
    {
        CharacterModelTextDelta text = new("Partial response.");
        CharacterModelStreamTermination failure = Failed(CharacterModelFailureCode.Transport);
        CharacterModelStreamSnapshot attempt = new([text, failure]);

        Assert.False(CharacterModelRetryPolicy.CanRetry(1, attempt));
        Assert.Collection(
            attempt.Updates,
            update => Assert.Same(text, update),
            update => Assert.Same(failure, update));
    }

    [Fact]
    public void RejectsSecondRetry()
    {
        CharacterModelStreamSnapshot attempt = new(
            [Failed(CharacterModelFailureCode.Transport)]);

        Assert.False(CharacterModelRetryPolicy.CanRetry(
            CharacterModelRetryPolicy.MaximumAttemptCount,
            attempt));
    }

    [Theory]
    [InlineData(CharacterModelStreamTerminationStatus.Completed)]
    [InlineData(CharacterModelStreamTerminationStatus.Cancelled)]
    [InlineData(CharacterModelStreamTerminationStatus.TimedOut)]
    public void RejectsNonFailureAndNonTerminalStreams(
        CharacterModelStreamTerminationStatus status)
    {
        CharacterModelStreamSnapshot terminal = new(
            [new CharacterModelStreamTermination(status, null)]);
        CharacterModelStreamSnapshot empty = new([]);
        CharacterModelStreamSnapshot progress = new([new CharacterModelTextDelta("Progress")]);

        Assert.False(CharacterModelRetryPolicy.CanRetry(1, terminal));
        Assert.False(CharacterModelRetryPolicy.CanRetry(1, empty));
        Assert.False(CharacterModelRetryPolicy.CanRetry(1, progress));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(3)]
    public void RejectsInvalidCompletedAttemptCounts(int completedAttemptCount)
    {
        CharacterModelStreamSnapshot attempt = new(
            [Failed(CharacterModelFailureCode.Transport)]);

        Assert.Throws<ArgumentOutOfRangeException>(() =>
            CharacterModelRetryPolicy.CanRetry(completedAttemptCount, attempt));
    }

    [Fact]
    public void RejectsNullAttempt()
    {
        Assert.Throws<ArgumentNullException>(() => CharacterModelRetryPolicy.CanRetry(1, null!));
    }

    private static CharacterModelStreamTermination Failed(CharacterModelFailureCode code) =>
        new(
            CharacterModelStreamTerminationStatus.Failed,
            new CharacterModelFailure(code));
}
