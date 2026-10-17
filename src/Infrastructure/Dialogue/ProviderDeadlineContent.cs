using System.Net;

namespace Allrpcs.Infrastructure.Dialogue;

/// <summary>Retains the original response content and request timer until response disposal.</summary>
internal sealed class ProviderDeadlineContent : HttpContent
{
    private readonly HttpContent content;
    private readonly ProviderRequestDeadline deadline;

    internal ProviderDeadlineContent(HttpContent content, ProviderRequestDeadline deadline)
    {
        this.content = content;
        this.deadline = deadline;
        foreach (KeyValuePair<string, IEnumerable<string>> header in content.Headers)
        {
            Headers.TryAddWithoutValidation(header.Key, header.Value);
        }
    }

    protected override Task<Stream> CreateContentReadStreamAsync() =>
        CreateContentReadStreamAsync(CancellationToken.None);

    protected override async Task<Stream> CreateContentReadStreamAsync(CancellationToken cancellationToken)
    {
        Stream stream = await deadline.ReadAsync(
            token => new ValueTask<Stream>(content.ReadAsStreamAsync(token)), cancellationToken).ConfigureAwait(false);
        return new ProviderDeadlineStream(stream, deadline);
    }

    protected override async Task SerializeToStreamAsync(Stream stream, TransportContext? context)
    {
        Stream bounded = await ReadAsStreamAsync().ConfigureAwait(false);
        await bounded.CopyToAsync(stream).ConfigureAwait(false);
    }

    protected override bool TryComputeLength(out long length)
    {
        length = 0;
        return false;
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            content.Dispose();
            deadline.Dispose();
        }

        base.Dispose(disposing);
    }
}

/// <summary>Applies an idle deadline to each read without abandoning the underlying operation.</summary>
internal sealed class ProviderDeadlineStream(Stream stream, ProviderRequestDeadline deadline) : Stream
{
    public override bool CanRead => stream.CanRead;
    public override bool CanSeek => false;
    public override bool CanWrite => false;
    public override long Length => throw new NotSupportedException();
    public override long Position
    {
        get => throw new NotSupportedException();
        set => throw new NotSupportedException();
    }

    public override ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default) =>
        deadline.ReadAsync(token => stream.ReadAsync(buffer, token), cancellationToken);

    public override Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken) =>
        ReadAsync(buffer.AsMemory(offset, count), cancellationToken).AsTask();

    public override int Read(byte[] buffer, int offset, int count) => throw new NotSupportedException();
    public override void Flush() => throw new NotSupportedException();
    public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();
    public override void SetLength(long value) => throw new NotSupportedException();
    public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();

    // The original HttpContent owns and disposes its stream. This wrapper never takes a second ownership.
}
