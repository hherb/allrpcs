using System.Numerics;
using Allrpcs.Application.Audio;
using Allrpcs.Domain.Dialogue;
using Allrpcs.Domain.Identifiers;

namespace Allrpcs.Application.Tests.Audio;

public sealed class OpeningRegionPositionalAudioProjectionTests
{
    private static readonly Vector2[] Water = [new(1600, 452), new(2000, 452), new(2000, 548), new(1600, 548)];
    [Theory]
    [InlineData(1800, 500, 1800, 500)]
    [InlineData(1500, 220, 1600, 452)]
    [InlineData(2200, 900, 2000, 548)]
    [InlineData(1700, 900, 1700, 548)]
    [InlineData(1600, 452, 1600, 452)]
    public void ExtendedWaterUsesActualGeometryIncludingCornersAndInside(float x, float y, float sx, float sy) =>
        Assert.Equal(new Vector2(sx, sy), OpeningRegionPositionalAudioProjection.NearestWaterPoint(new(x, y), Water));

    [Fact]
    public void InvalidOrNonrectangularGeometryHasNoFallback()
    {
        Assert.Null(OpeningRegionPositionalAudioProjection.NearestWaterPoint(new(float.NaN, 0), Water));
        Assert.Null(OpeningRegionPositionalAudioProjection.NearestWaterPoint(Vector2.Zero, []));
        Assert.Null(OpeningRegionPositionalAudioProjection.NearestWaterPoint(Vector2.Zero,
            [new(0, 0), new(2, 0), new(1, 1), new(0, 1)]));
        Assert.Null(OpeningRegionPositionalAudioProjection.NearestWaterPoint(Vector2.Zero,
            [new(0, 0), new(0, 0), new(0, 0), new(0, 0)]));
    }

    [Theory]
    [InlineData(0, 1)]
    [InlineData(384, 0.5)]
    [InlineData(768, 0)]
    [InlineData(769, 0)]
    public void DistanceFactorIsBoundedAndNotASecondPlayerGain(float distance, double gain) =>
        Assert.Equal(gain, OpeningRegionPositionalAudioProjection.DistanceGain(Vector2.Zero, new(distance, 0), 768), 10);

    [Theory]
    [InlineData(640, -1, "above")]
    [InlineData(1281, -361, "above/right")]
    [InlineData(1281, 360, "right")]
    [InlineData(1281, 1081, "below/right")]
    [InlineData(640, 721, "below")]
    [InlineData(-1, 1081, "below/left")]
    [InlineData(-1, 360, "left")]
    [InlineData(-1, -361, "above/left")]
    public void CaptionUsesScreenBearingWithoutIdentity(float x, float y, string direction) =>
        Assert.Equal("Conversation " + direction, OpeningRegionPositionalAudioProjection.Caption("Conversation", new(x, y), new(1280, 720)));

    [Fact]
    public void CanvasBoundsAreInclusiveAndInvalidCameraDoesNotRevealDirection()
    {
        Assert.Equal("Conversation nearby", OpeningRegionPositionalAudioProjection.Caption("Conversation", new(1280, 720), new(1280, 720)));
        Assert.Equal("Conversation", OpeningRegionPositionalAudioProjection.Caption("Conversation", new(float.NaN, 0), new(1280, 720)));
        Assert.Equal("Running water and rain", OpeningRegionPositionalAudioProjection.Caption("Running water and rain", new(640, 360), new(1280, 720)));
    }

    [Theory]
    [InlineData(false, 192)]
    [InlineData(true, 48)]
    public void CapturedAudienceAndExactRecordedDeliveryPrecedeCurrentDistance(bool whisper, float range)
    {
        CharacterId speaker = new("healer"), seeker = new("seeker");
        DialogueDelivery delivery = new(new(speaker, "A recorded reply."), whisper ? DialogueDeliveryMode.Whispering : DialogueDeliveryMode.OrdinarySpeech);
        DialogueEvent heard = new(delivery, new(speaker, [seeker]));
        CharacterTurnTranscriptSnapshot transcript = new([new(delivery)]);
        Assert.True(OpeningRegionPositionalAudioProjection.Admitted(heard, transcript, seeker, speaker, Vector2.Zero, new(range, 0), range));
        Assert.False(OpeningRegionPositionalAudioProjection.Admitted(heard, transcript, seeker, speaker, Vector2.Zero, new(range + 0.01f, 0), range));
        Assert.False(OpeningRegionPositionalAudioProjection.Admitted(new(delivery, new(speaker, [])), transcript, seeker, speaker, Vector2.Zero, Vector2.Zero, range));
        Assert.False(OpeningRegionPositionalAudioProjection.Admitted(heard, new([]), seeker, speaker, Vector2.Zero, Vector2.Zero, range));
        DialogueDelivery equalLooking = new(new(speaker, "A recorded reply."), delivery.Mode);
        Assert.False(OpeningRegionPositionalAudioProjection.Admitted(heard, new([new(equalLooking)]), seeker, speaker, Vector2.Zero, Vector2.Zero, range));
        Assert.False(OpeningRegionPositionalAudioProjection.Admitted(heard, transcript, seeker, new("unknown"), Vector2.Zero, Vector2.Zero, range));
        Assert.False(OpeningRegionPositionalAudioProjection.Admitted(heard, transcript, seeker, speaker, Vector2.Zero, Vector2.Zero, range + 1));
        Assert.False(OpeningRegionPositionalAudioProjection.Admitted(heard, transcript, seeker, speaker, new(float.NaN, 0), Vector2.Zero, range));
    }

    [Fact]
    public void HistoryIsBoundedDetachedAndRecordsRateLimitedEvents()
    {
        PositionalCueHistory history = new();
        Assert.True(history.Record("Conversation nearby"));
        IReadOnlyList<string> first = history.Entries;
        for (int i = 0; i < 19; i++) { Assert.False(history.Record("Conversation above")); }
        Assert.Single(first);
        Assert.Equal(16, history.Entries.Count);
        Assert.StartsWith("5. ", history.Entries[0]);
        history.Advance(0.999); Assert.False(history.Record("Conversation left"));
        history.Advance(0.001); Assert.True(history.Record("Conversation right"));
        Assert.Throws<ArgumentOutOfRangeException>(() => history.Advance(double.NaN));
        history.Clear(); Assert.Empty(history.Entries); Assert.True(history.Record("Conversation nearby"));
    }
}
