using Allrpcs.Application.Dialogue;

namespace Allrpcs.Application.Tests.Dialogue;

public sealed class CharacterModelGenerationDiagnosticTests
{
    public static TheoryData<CharacterModelStreamTerminationStatus, CharacterModelFailureCode?>
        TerminalOutcomes => new()
        {
            { CharacterModelStreamTerminationStatus.Completed, null },
            { CharacterModelStreamTerminationStatus.Cancelled, null },
            { CharacterModelStreamTerminationStatus.TimedOut, null },
            {
                CharacterModelStreamTerminationStatus.Failed,
                CharacterModelFailureCode.Authentication
            },
        };

    [Theory]
    [MemberData(nameof(TerminalOutcomes))]
    public void RetainsRedactedTerminalOutcomes(
        CharacterModelStreamTerminationStatus status,
        CharacterModelFailureCode? failureCode)
    {
        CharacterModelGenerationDiagnostic diagnostic = new(
            CharacterModelProvider.OpenAi,
            status,
            failureCode,
            TimeSpan.FromMilliseconds(275),
            11,
            7,
            18);

        Assert.Same(CharacterModelProvider.OpenAi, diagnostic.Provider);
        Assert.Equal(status, diagnostic.Status);
        Assert.Equal(failureCode, diagnostic.FailureCode);
        Assert.Equal(TimeSpan.FromMilliseconds(275), diagnostic.Elapsed);
        Assert.Equal(11, diagnostic.InputTokenCount);
        Assert.Equal(7, diagnostic.OutputTokenCount);
        Assert.Equal(18, diagnostic.TotalTokenCount);
    }

    [Fact]
    public void AcceptsAbsentUsageAndZeroUsage()
    {
        CharacterModelGenerationDiagnostic absent = Diagnostic(null, null, null);
        CharacterModelGenerationDiagnostic zero = Diagnostic(0, 0, 0);

        Assert.Null(absent.InputTokenCount);
        Assert.Null(absent.OutputTokenCount);
        Assert.Null(absent.TotalTokenCount);
        Assert.Equal(0, zero.InputTokenCount);
        Assert.Equal(0, zero.OutputTokenCount);
        Assert.Equal(0, zero.TotalTokenCount);
    }

    [Fact]
    public void AcceptsElapsedAndTokenBoundaries()
    {
        CharacterModelGenerationDiagnostic minimum = Diagnostic(
            0,
            0,
            0,
            TimeSpan.Zero);
        CharacterModelGenerationDiagnostic maximum = Diagnostic(
            CharacterModelGenerationDiagnostic.MaximumTokenCount,
            0,
            CharacterModelGenerationDiagnostic.MaximumTokenCount,
            CharacterModelGenerationDiagnostic.MaximumElapsed);

        Assert.Equal(TimeSpan.Zero, minimum.Elapsed);
        Assert.Equal(
            CharacterModelGenerationDiagnostic.MaximumElapsed,
            maximum.Elapsed);
        Assert.Equal(
            CharacterModelGenerationDiagnostic.MaximumTokenCount,
            maximum.TotalTokenCount);
    }

    [Fact]
    public void RejectsIncompleteInconsistentOrExcessiveUsage()
    {
        Assert.Throws<ArgumentException>(() => Diagnostic(1, null, null));
        Assert.Throws<ArgumentException>(() => Diagnostic(null, 1, null));
        Assert.Throws<ArgumentException>(() => Diagnostic(null, null, 1));
        Assert.Throws<ArgumentException>(() => Diagnostic(1, 2, 4));
        Assert.Throws<ArgumentOutOfRangeException>(() => Diagnostic(-1, 0, -1));
        Assert.Throws<ArgumentOutOfRangeException>(() => Diagnostic(0, -1, -1));
        Assert.Throws<ArgumentOutOfRangeException>(() => Diagnostic(
            CharacterModelGenerationDiagnostic.MaximumTokenCount,
            1,
            CharacterModelGenerationDiagnostic.MaximumTokenCount + 1));
    }

    [Fact]
    public void RejectsInvalidElapsedDuration()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => Diagnostic(
            null,
            null,
            null,
            TimeSpan.FromTicks(-1)));
        Assert.Throws<ArgumentOutOfRangeException>(() => Diagnostic(
            null,
            null,
            null,
            CharacterModelGenerationDiagnostic.MaximumElapsed + TimeSpan.FromTicks(1)));
    }

    [Fact]
    public void RequiresFailureCodeOnlyForFailedOutcome()
    {
        Assert.Throws<ArgumentNullException>(() => new CharacterModelGenerationDiagnostic(
            CharacterModelProvider.OpenAi,
            CharacterModelStreamTerminationStatus.Failed,
            null,
            TimeSpan.Zero,
            null,
            null,
            null));
        Assert.Throws<ArgumentException>(() => new CharacterModelGenerationDiagnostic(
            CharacterModelProvider.OpenAi,
            CharacterModelStreamTerminationStatus.Completed,
            CharacterModelFailureCode.Transport,
            TimeSpan.Zero,
            null,
            null,
            null));
    }

    [Fact]
    public void RejectsNullProviderAndUndefinedEnums()
    {
        Assert.Throws<ArgumentNullException>(() => new CharacterModelGenerationDiagnostic(
            null!,
            CharacterModelStreamTerminationStatus.Completed,
            null,
            TimeSpan.Zero,
            null,
            null,
            null));
        Assert.Throws<ArgumentOutOfRangeException>(() => new CharacterModelGenerationDiagnostic(
            CharacterModelProvider.OpenAi,
            (CharacterModelStreamTerminationStatus)int.MaxValue,
            null,
            TimeSpan.Zero,
            null,
            null,
            null));
        Assert.Throws<ArgumentOutOfRangeException>(() => new CharacterModelGenerationDiagnostic(
            CharacterModelProvider.OpenAi,
            CharacterModelStreamTerminationStatus.Failed,
            (CharacterModelFailureCode)int.MaxValue,
            TimeSpan.Zero,
            null,
            null,
            null));
    }

    [Fact]
    public void UsesAnExplicitRedactedStringRepresentation()
    {
        CharacterModelGenerationDiagnostic completed = Diagnostic(11, 7, 18);
        CharacterModelGenerationDiagnostic failed = new(
            CharacterModelProvider.Anthropic,
            CharacterModelStreamTerminationStatus.Failed,
            CharacterModelFailureCode.ProviderRejected,
            TimeSpan.FromMilliseconds(275),
            null,
            null,
            null);

        Assert.Equal(
            "openai:Completed:275ms:tokens=11/7/18",
            completed.ToString());
        Assert.Equal(
            "anthropic:Failed:ProviderRejected:275ms:tokens=unknown",
            failed.ToString());
    }

    private static CharacterModelGenerationDiagnostic Diagnostic(
        int? inputTokenCount,
        int? outputTokenCount,
        int? totalTokenCount,
        TimeSpan? elapsed = null) => new(
            CharacterModelProvider.OpenAi,
            CharacterModelStreamTerminationStatus.Completed,
            null,
            elapsed ?? TimeSpan.FromMilliseconds(275),
            inputTokenCount,
            outputTokenCount,
            totalTokenCount);
}
