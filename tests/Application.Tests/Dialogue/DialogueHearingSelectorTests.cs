using System.Numerics;
using Allrpcs.Application.Dialogue;
using Allrpcs.Domain.Dialogue;
using Allrpcs.Domain.Identifiers;

namespace Allrpcs.Application.Tests.Dialogue;

public sealed class DialogueHearingSelectorTests
{
    private const float HearingRangePixels = 100.0f;
    private static readonly Vector2 SpeakerPosition = new(400.0f, 300.0f);

    [Fact]
    public void SelectsEveryNonSpeakerCandidateInsideTheInclusiveRangeInStableOrder()
    {
        CharacterId speakerCharacterId = new("seeker");
        CharacterId edgeListener = new("healer");
        CharacterId nearListener = new("hermit");
        DialogueHearingCandidate[] candidates =
        [
            new(speakerCharacterId, SpeakerPosition),
            CandidateAt(edgeListener, 500.0f, 300.0f),
            CandidateAt(new CharacterId("sage"), 500.01f, 300.0f),
            CandidateAt(nearListener, 430.0f, 340.0f),
        ];

        DialogueAudienceSnapshot audience = DialogueHearingSelector.Select(
            speakerCharacterId,
            SpeakerPosition,
            HearingRangePixels,
            candidates);

        Assert.Same(speakerCharacterId, audience.SpeakerCharacterId);
        Assert.Collection(
            audience.ListenerCharacterIds,
            listener => Assert.Same(edgeListener, listener),
            listener => Assert.Same(nearListener, listener));
    }

    [Fact]
    public void ReturnsEmptyAudienceWhenNoCandidateCanHear()
    {
        CharacterId speakerCharacterId = new("seeker");

        DialogueAudienceSnapshot audience = DialogueHearingSelector.Select(
            speakerCharacterId,
            SpeakerPosition,
            HearingRangePixels,
            [new DialogueHearingCandidate(speakerCharacterId, SpeakerPosition),
                CandidateAt(new CharacterId("healer"), 501.0f, 300.0f)]);

        Assert.Empty(audience.ListenerCharacterIds);
    }

    [Fact]
    public void AcceptsMaximumCandidateCount()
    {
        CharacterId speakerCharacterId = new("seeker");
        DialogueHearingCandidate[] candidates = Enumerable
            .Range(0, DialogueHearingSelector.MaximumCandidateCount)
            .Select(index => CandidateAt(
                new CharacterId($"listener-{index}"),
                SpeakerPosition.X,
                SpeakerPosition.Y))
            .ToArray();

        DialogueAudienceSnapshot audience = DialogueHearingSelector.Select(
            speakerCharacterId,
            SpeakerPosition,
            HearingRangePixels,
            candidates);

        Assert.Equal(
            DialogueAudienceSnapshot.MaximumListenerCount,
            audience.ListenerCharacterIds.Count);
    }

    [Fact]
    public void RejectsCandidateSetAboveTheBound()
    {
        DialogueHearingCandidate[] candidates = Enumerable
            .Range(0, DialogueHearingSelector.MaximumCandidateCount + 1)
            .Select(index => CandidateAt(
                new CharacterId($"candidate-{index}"),
                SpeakerPosition.X,
                SpeakerPosition.Y))
            .ToArray();

        Assert.Throws<ArgumentOutOfRangeException>(() => DialogueHearingSelector.Select(
            new CharacterId("seeker"),
            SpeakerPosition,
            HearingRangePixels,
            candidates));
    }

    [Fact]
    public void RejectsDuplicateCandidateIdentifiersEvenWhenOneIsOutOfRange()
    {
        DialogueHearingCandidate[] candidates =
        [
            CandidateAt(new CharacterId("healer"), 420.0f, 300.0f),
            CandidateAt(new CharacterId("healer"), 900.0f, 300.0f),
        ];

        Assert.Throws<ArgumentException>(() => DialogueHearingSelector.Select(
            new CharacterId("seeker"),
            SpeakerPosition,
            HearingRangePixels,
            candidates));
    }

    [Theory]
    [InlineData(float.NaN, 300.0f)]
    [InlineData(float.PositiveInfinity, 300.0f)]
    [InlineData(400.0f, float.NegativeInfinity)]
    public void RejectsNonFiniteSpeakerPosition(float x, float y)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => DialogueHearingSelector.Select(
            new CharacterId("seeker"),
            new Vector2(x, y),
            HearingRangePixels,
            Array.Empty<DialogueHearingCandidate>()));
    }

    [Theory]
    [InlineData(float.NaN, 300.0f)]
    [InlineData(float.PositiveInfinity, 300.0f)]
    [InlineData(400.0f, float.NegativeInfinity)]
    public void RejectsNonFiniteCandidatePositionEvenWhenOutOfRange(float x, float y)
    {
        DialogueHearingCandidate[] candidates =
        [
            new(new CharacterId("healer"), new Vector2(x, y)),
        ];

        Assert.Throws<ArgumentOutOfRangeException>(() => DialogueHearingSelector.Select(
            new CharacterId("seeker"),
            SpeakerPosition,
            HearingRangePixels,
            candidates));
    }

    [Theory]
    [InlineData(0.0f)]
    [InlineData(-0.01f)]
    [InlineData(float.NaN)]
    [InlineData(float.PositiveInfinity)]
    public void RejectsInvalidHearingRange(float hearingRangePixels)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => DialogueHearingSelector.Select(
            new CharacterId("seeker"),
            SpeakerPosition,
            hearingRangePixels,
            Array.Empty<DialogueHearingCandidate>()));
    }

    [Fact]
    public void RejectsNullInputsAndCandidateIdentifier()
    {
        Assert.Throws<ArgumentNullException>(() => DialogueHearingSelector.Select(
            null!,
            SpeakerPosition,
            HearingRangePixels,
            Array.Empty<DialogueHearingCandidate>()));
        Assert.Throws<ArgumentNullException>(() => DialogueHearingSelector.Select(
            new CharacterId("seeker"),
            SpeakerPosition,
            HearingRangePixels,
            null!));
        Assert.Throws<ArgumentException>(() => DialogueHearingSelector.Select(
            new CharacterId("seeker"),
            SpeakerPosition,
            HearingRangePixels,
            [new DialogueHearingCandidate(null!, SpeakerPosition)]));
    }

    private static DialogueHearingCandidate CandidateAt(
        CharacterId characterId,
        float x,
        float y)
    {
        return new DialogueHearingCandidate(characterId, new Vector2(x, y));
    }
}
