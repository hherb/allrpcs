using Allrpcs.Application.Speech;

namespace Allrpcs.Application.Tests.Speech;

public sealed class SpeechContractTests
{
    [Fact]
    public void RequestPreservesExactTextAndKeepsItOutOfDiagnostics()
    {
        const string text = "  Nimm den Farn noch nicht.\nBleib hier!  ";
        SpeechSynthesisRequest request = new(text, SpeechLanguage.German, "healer-default");
        Assert.Equal(text, request.Text);
        Assert.DoesNotContain(text, request.ToString(), StringComparison.Ordinal);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("a\0b")]
    public void RejectsInvalidText(string text) => Assert.Throws<ArgumentException>(
        () => new SpeechSynthesisRequest(text, SpeechLanguage.English, "default"));

    [Fact]
    public void RejectsUnboundedTextLanguageAndProviderPaths()
    {
        Assert.Throws<ArgumentException>(() => new SpeechSynthesisRequest(
            new string('a', SpeechSynthesisRequest.MaximumTextLength + 1), SpeechLanguage.English, "default"));
        Assert.Throws<ArgumentOutOfRangeException>(() => new SpeechSynthesisRequest("Hello", (SpeechLanguage)99, "default"));
        Assert.Throws<ArgumentException>(() => new SpeechSynthesisRequest("Hello", SpeechLanguage.English, "../voice"));
    }

    [Fact]
    public void ChunkCopiesSamplesAndRejectsInvalidData()
    {
        float[] samples = [0.1f, -0.2f];
        SpeechAudioChunk chunk = new(24000, samples);
        samples[0] = 1;
        Assert.Equal(0.1f, chunk.Samples[0]);
        Assert.Throws<ArgumentException>(() => new SpeechAudioChunk(24000, [float.NaN]));
        Assert.Throws<ArgumentException>(() => new SpeechAudioChunk(24000, [2f]));
        Assert.Throws<ArgumentException>(() => new SpeechAudioChunk(24000, []));
        Assert.Throws<ArgumentOutOfRangeException>(() => new SpeechAudioChunk(1, [0f]));
    }
}
