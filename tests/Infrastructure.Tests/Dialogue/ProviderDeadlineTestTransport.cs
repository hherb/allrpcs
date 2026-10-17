using System.Net;
using System.Net.Http.Headers;
using Allrpcs.Infrastructure.Dialogue;
using Allrpcs.TestSupport;

namespace Allrpcs.Infrastructure.Tests.Dialogue;

internal sealed class ProviderDeadlineTestTransport : HttpMessageHandler
{
    internal ManualTimeProvider Clock { get; } = new();
    internal StalledStream Body { get; } = new();
    internal bool StallHeaders { get; init; }
    internal string MediaType { get; init; } = "text/event-stream";
    internal TaskCompletionSource Started { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);
    internal bool Disposed { get; private set; }
    internal ProviderHttpTimeoutOptions Options => new(timeProvider: Clock);

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request, CancellationToken cancellationToken)
    {
        if (StallHeaders)
        {
            Started.TrySetResult();
            await Task.Delay(Timeout.InfiniteTimeSpan, cancellationToken);
        }

        HttpResponseMessage response = new(HttpStatusCode.OK) { Content = new StreamContent(Body) };
        response.Content.Headers.ContentType = new MediaTypeHeaderValue(MediaType);
        return response;
    }

    protected override void Dispose(bool disposing)
    {
        Disposed = true;
        base.Dispose(disposing);
    }

    internal async Task ExpireAsync(bool callerCancellation, CancellationTokenSource caller)
    {
        await (StallHeaders ? Started.Task : Body.Started.Task).WaitAsync(TimeSpan.FromSeconds(5));
        if (callerCancellation) caller.Cancel();
        else Clock.Advance(TimeSpan.FromSeconds(StallHeaders ? 120 : 30));
    }

    internal sealed class StalledStream : Stream
    {
        internal TaskCompletionSource Started { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);
        internal bool Disposed { get; private set; }
        public override bool CanRead => true;
        public override bool CanSeek => false;
        public override bool CanWrite => false;
        public override long Length => throw new NotSupportedException();
        public override long Position
        {
            get => throw new NotSupportedException();
            set => throw new NotSupportedException();
        }

        public override async ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default)
        {
            Started.TrySetResult();
            await Task.Delay(Timeout.InfiniteTimeSpan, cancellationToken);
            return 0;
        }

        protected override void Dispose(bool disposing)
        {
            Disposed = true;
            base.Dispose(disposing);
        }

        public override int Read(byte[] buffer, int offset, int count) => throw new NotSupportedException();
        public override void Flush() => throw new NotSupportedException();
        public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();
        public override void SetLength(long value) => throw new NotSupportedException();
        public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();
    }
}
