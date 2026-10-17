using Allrpcs.Application.Audio;

namespace Allrpcs.Application.Tests.Audio;

public sealed class AudioDuckingEnvelopeTests
{
    [Theory]
    [InlineData(AudioForeground.Reading)]
    [InlineData(AudioForeground.ConsequentialCue)]
    public void PartialReleaseIsMonotonicAndNewSpeechAttacksWithoutResurrectingPeak(AudioForeground weaker)
    {
        AudioDuckingEnvelope envelope = new();
        long speech = envelope.Begin(AudioForeground.Speech);
        long weak = envelope.Begin(weaker);
        envelope.Advance(0.1);
        envelope.End(speech);
        double previous = 6;
        for (int step = 0; step < 10; step++)
        {
            envelope.Advance(0.05);
            double current = envelope.ReductionDb(AudioCategory.Music);
            Assert.InRange(current, 3, previous);
            previous = current;
        }
        Assert.Equal(3, previous, 8);
        long secondSpeech = envelope.Begin(AudioForeground.Speech);
        Assert.Equal(3, envelope.ReductionDb(AudioCategory.Music), 8);
        envelope.Advance(0.025);
        Assert.Equal(4.5, envelope.ReductionDb(AudioCategory.Music), 8);
        envelope.End(secondSpeech);
        envelope.Advance(1);
        Assert.Equal(3, envelope.ReductionDb(AudioCategory.Music), 8);
        Assert.Equal(0, envelope.ReductionDb(AudioCategory.Effects), 8);
        envelope.End(weak);
        Assert.Equal(3, envelope.ReductionDb(AudioCategory.Music), 8);
        envelope.Advance(0.15);
        Assert.Equal(3, envelope.ReductionDb(AudioCategory.Music), 8);
        envelope.Advance(0.6);
        Assert.Equal(0, envelope.ReductionDb(AudioCategory.Music), 8);
    }

    [Fact]
    public void WeakerEndingFirstAndDuplicateStrongLeasePreserveActiveTarget()
    {
        AudioDuckingEnvelope envelope = new();
        long first = envelope.Begin(AudioForeground.Speech);
        long second = envelope.Begin(AudioForeground.Speech);
        long weaker = envelope.Begin(AudioForeground.Reading);
        envelope.Advance(0.025);
        envelope.End(weaker);
        envelope.End(first);
        envelope.Advance(0.075);
        Assert.Equal(6, envelope.ReductionDb(AudioCategory.Music), 8);
        envelope.End(second);
        envelope.Advance(0.75);
        Assert.Equal(0, envelope.ReductionDb(AudioCategory.Music), 8);
    }

    [Fact]
    public void CueStartedDuringReleaseUsesItsOwnActiveAttackTiming()
    {
        AudioDuckingEnvelope envelope = new();
        long speech = envelope.Begin(AudioForeground.Speech);
        envelope.Advance(0.1);
        envelope.End(speech);
        envelope.Advance(0.7);
        Assert.Equal(0.5, envelope.ReductionDb(AudioCategory.Music), 8);
        envelope.Begin(AudioForeground.ConsequentialCue);
        Assert.Equal(0.5, envelope.ReductionDb(AudioCategory.Music), 8);
        envelope.Advance(0.025);
        Assert.Equal(2, envelope.ReductionDb(AudioCategory.Music), 8);
    }

    [Fact]
    public void StrongestEndingReleasesToWeakerPlateauThenBaseline()
    {
        AudioDuckingEnvelope envelope = new();
        long speech = envelope.Begin(AudioForeground.Speech);
        long reading = envelope.Begin(AudioForeground.Reading);
        envelope.Advance(0.1);
        envelope.End(speech);
        envelope.Advance(0.3);
        Assert.Equal(3, envelope.ReductionDb(AudioCategory.Music), 8);
        envelope.Advance(1);
        Assert.Equal(3, envelope.ReductionDb(AudioCategory.Music), 8);
        Assert.Equal(0, envelope.ReductionDb(AudioCategory.Effects), 8);
        envelope.End(reading);
        envelope.Advance(0.15);
        Assert.Equal(3, envelope.ReductionDb(AudioCategory.Music), 8);
        envelope.Advance(0.6);
        Assert.Equal(1, envelope.Gain(AudioCategory.Music), 8);
    }

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
