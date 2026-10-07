using Allrpcs.Application.Audio;

namespace Allrpcs.Application.Tests.Audio;

public sealed class AudioPreferencesTests
{
    [Theory]
    [InlineData(-0.01)]
    [InlineData(1.01)]
    [InlineData(double.NaN)]
    [InlineData(double.PositiveInfinity)]
    public void RejectsInvalidGains(double gain) => Assert.Throws<ArgumentOutOfRangeException>(() =>
        AudioPreferences.Defaults().WithChannel(AudioCategory.Music, gain, false));

    [Fact]
    public void RejectsMissingDuplicateOrUnknownCategories()
    {
        AudioPreferences defaults = AudioPreferences.Defaults();
        Assert.Throws<ArgumentException>(() => new AudioPreferences(defaults.Channels.Skip(1)));
        Assert.Throws<ArgumentException>(() => new AudioPreferences(defaults.Channels.Skip(1).Append(defaults.Channels[1])));
        Assert.Throws<ArgumentOutOfRangeException>(() => defaults.WithChannel((AudioCategory)99, 1, false));
    }

    [Fact]
    public void ReplacesOnlyChosenChannelAndRetainsMutedGain()
    {
        AudioPreferences defaults = AudioPreferences.Defaults();
        AudioPreferences chosen = defaults.WithChannel(AudioCategory.Music, 0.37, true);
        Assert.Equal(1, defaults.Channels[1].Gain);
        Assert.Equal(0.37, chosen.Channels[1].Gain);
        Assert.True(chosen.Channels[1].Muted);
        Assert.Equal(chosen.Channels[1].Gain, chosen.WithChannel(AudioCategory.Music, 0.37, false).Channels[1].Gain);
        foreach (AudioCategory category in Enum.GetValues<AudioCategory>().Where(item => item != AudioCategory.Music))
        {
            Assert.Same(defaults.Channels[(int)category], chosen.Channels[(int)category]);
        }
    }

    [Fact]
    public void CopiesInputAndRestoresCompleteDefaults()
    {
        AudioChannelSetting[] input = AudioPreferences.Defaults().Channels.ToArray();
        AudioPreferences snapshot = new(input, AudioMixPreset.ReducedRange, true, false);
        input[0] = new(AudioCategory.Master, 0, true);
        Assert.Equal(1, snapshot.Channels[0].Gain);
        AudioPreferences defaults = AudioPreferences.Defaults();
        Assert.All(defaults.Channels, item => { Assert.Equal(1, item.Gain); Assert.False(item.Muted); });
        Assert.Equal(AudioMixPreset.Default, defaults.Preset);
        Assert.False(defaults.Mono);
        Assert.True(defaults.Captions);
    }
}
