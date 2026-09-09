using Allrpcs.Application.Dialogue;

namespace Allrpcs.Application.Tests.Dialogue;

public sealed class ICharacterModelGenerationDiagnosticSinkTests
{
    [Fact]
    public void ExposesOneCallerOwnedCancellationAwareOperation()
    {
        Type sinkType = typeof(ICharacterModelGenerationDiagnosticSink);

        Assert.True(sinkType.IsInterface);
        System.Reflection.MethodInfo method = Assert.Single(sinkType.GetMethods());
        Assert.Equal("RecordAsync", method.Name);
        Assert.Equal(typeof(ValueTask), method.ReturnType);
        Assert.Equal(
            [typeof(CharacterModelGenerationDiagnostic), typeof(CancellationToken)],
            method.GetParameters().Select(parameter => parameter.ParameterType));
    }

    [Fact]
    public async Task ImplementationsCanObserveExactDiagnosticAndCancellation()
    {
        RecordingSink sink = new();
        CharacterModelGenerationDiagnostic diagnostic = new(
            CharacterModelProvider.Ollama,
            CharacterModelStreamTerminationStatus.Cancelled,
            null,
            TimeSpan.Zero,
            null,
            null,
            null);
        using CancellationTokenSource cancellation = new();
        cancellation.Cancel();

        await sink.RecordAsync(diagnostic, cancellation.Token);

        Assert.Same(diagnostic, sink.Diagnostic);
        Assert.Equal(cancellation.Token, sink.CancellationToken);
    }

    [Fact]
    public async Task RecorderKeepsSinkFailureAndCancellationObservational()
    {
        CharacterModelGenerationDiagnostic diagnostic = new(
            CharacterModelProvider.OpenAi,
            CharacterModelStreamTerminationStatus.Completed,
            null,
            TimeSpan.Zero,
            null,
            null,
            null);
        using CancellationTokenSource cancellation = new();
        cancellation.Cancel();

        bool synchronousFailure = await CharacterModelGenerationDiagnosticRecorder.TryRecordAsync(
            new ThrowingSink(new InvalidOperationException("private sink detail")),
            diagnostic,
            CancellationToken.None);
        bool cancellationFailure = await CharacterModelGenerationDiagnosticRecorder.TryRecordAsync(
            new ThrowingSink(new OperationCanceledException(cancellation.Token)),
            diagnostic,
            cancellation.Token);

        Assert.False(synchronousFailure);
        Assert.False(cancellationFailure);
        Assert.Equal(CharacterModelStreamTerminationStatus.Completed, diagnostic.Status);
    }

    [Fact]
    public async Task RecorderRejectsNullDependencies()
    {
        CharacterModelGenerationDiagnostic diagnostic = new(
            CharacterModelProvider.OpenAi,
            CharacterModelStreamTerminationStatus.Completed,
            null,
            TimeSpan.Zero,
            null,
            null,
            null);

        await Assert.ThrowsAsync<ArgumentNullException>(async () =>
            await CharacterModelGenerationDiagnosticRecorder.TryRecordAsync(
                null!,
                diagnostic,
                CancellationToken.None));
        await Assert.ThrowsAsync<ArgumentNullException>(async () =>
            await CharacterModelGenerationDiagnosticRecorder.TryRecordAsync(
                new RecordingSink(),
                null!,
                CancellationToken.None));
    }

    private sealed class RecordingSink : ICharacterModelGenerationDiagnosticSink
    {
        public CharacterModelGenerationDiagnostic? Diagnostic { get; private set; }

        public CancellationToken CancellationToken { get; private set; }

        public ValueTask RecordAsync(
            CharacterModelGenerationDiagnostic diagnostic,
            CancellationToken cancellationToken)
        {
            Diagnostic = diagnostic;
            CancellationToken = cancellationToken;
            return ValueTask.CompletedTask;
        }
    }

    private sealed class ThrowingSink(Exception exception) :
        ICharacterModelGenerationDiagnosticSink
    {
        public ValueTask RecordAsync(
            CharacterModelGenerationDiagnostic diagnostic,
            CancellationToken cancellationToken) => throw exception;
    }
}
