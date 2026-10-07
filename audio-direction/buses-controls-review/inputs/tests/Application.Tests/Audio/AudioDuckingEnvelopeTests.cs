using Allrpcs.Application.Audio;

namespace Allrpcs.Application.Tests.Audio;

public sealed class AudioDuckingEnvelopeTests
{
    [Fact]
    public void AttacksHoldsAndReleasesAfterOverlappingSpeechCancellation()
    {
        AudioDuckingEnvelope envelope = new();
        long speech = envelope.Begin(AudioForeground.Speech);
        long second = envelope.Begin(AudioForeground.Speech);
        long cue = envelope.Begin(AudioForeground.ConsequentialCue);
        envelope.Advance(0.05);
        Assert.Equal(3, envelope.ReductionDb(AudioCategory.Music), 8);
        envelope.Advance(0.05);
        Assert.Equal(6, envelope.ReductionDb(AudioCategory.Music));
        Assert.Equal(3, envelope.ReductionDb(AudioCategory.Ambience));
        Assert.Equal(0, envelope.ReductionDb(AudioCategory.Speech));
        Assert.True(envelope.End(speech));
        Assert.False(envelope.End(speech));
        Assert.False(envelope.End(999));
        envelope.Advance(1);
        Assert.Equal(6, envelope.ReductionDb(AudioCategory.Music));
        envelope.End(second);
        envelope.End(cue);
        envelope.Advance(0.15);
        Assert.Equal(6, envelope.ReductionDb(AudioCategory.Music));
        envelope.Advance(0.3);
        Assert.Equal(3, envelope.ReductionDb(AudioCategory.Music), 8);
        envelope.Advance(0.3);
        Assert.Equal(1, envelope.Gain(AudioCategory.Music), 8);
    }

    [Fact]
    public void CueUsesShorterEnvelopeAndReadingUsesSpeechTimingWithoutEffectsDuck()
    {
        AudioDuckingEnvelope envelope = new();
        long token = envelope.Begin(AudioForeground.ConsequentialCue);
        envelope.Advance(0.025);
        Assert.Equal(1.5, envelope.ReductionDb(AudioCategory.Music), 8);
        envelope.Advance(0.025);
        envelope.End(token);
        envelope.Advance(0.3);
        Assert.Equal(1.5, envelope.ReductionDb(AudioCategory.Music), 8);
        envelope.Clear();
        token = envelope.Begin(AudioForeground.Reading);
        envelope.Advance(0.05);
        Assert.Equal(1.5, envelope.ReductionDb(AudioCategory.Music), 8);
        Assert.Equal(0, envelope.ReductionDb(AudioCategory.Effects));
        envelope.Advance(0.05);
        envelope.End(token);
        envelope.Advance(0.75);
        Assert.Equal(1, envelope.Gain(AudioCategory.Music), 8);
    }

    [Fact]
    public void NewForegroundDuringReleaseDoesNotJumpAndTeardownClearsAllLeases()
    {
        AudioDuckingEnvelope envelope = new();
        long speech = envelope.Begin(AudioForeground.Speech);
        envelope.Advance(0.1);
        envelope.End(speech);
        envelope.Advance(0.45);
        double before = envelope.Gain(AudioCategory.Music);
        envelope.Begin(AudioForeground.Speech);
        Assert.Equal(before, envelope.Gain(AudioCategory.Music));
        envelope.Clear();
        Assert.Equal(1, envelope.Gain(AudioCategory.Music));
        Assert.False(envelope.End(speech));
        Assert.Throws<ArgumentOutOfRangeException>(() => envelope.Advance(double.NaN));
    }
}
