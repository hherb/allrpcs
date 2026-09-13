using System.Numerics;
using Allrpcs.Application.Dialogue;
using Allrpcs.Domain.Dialogue;
using Allrpcs.Domain.Identifiers;

namespace Allrpcs.Application.Tests.Dialogue;

public sealed class DialogueDeliveryHearingSelectorTests
{
    private const float OrdinaryRangePixels = 100.0f;
    private const float WhisperRangePixels = 40.0f;
    private static readonly Vector2 SpeakerPosition = new(400.0f, 300.0f);

    [Theory]
    [InlineData(false, 480.0f, true)]
    [InlineData(true, 480.0f, false)]
    [InlineData(true, 440.0f, true)]
    public void SelectsListenersUsingOnlyTheExplicitDeliveryModeRange(
        bool whispers,
        float listenerX,
        bool expectedToHear)
    {
        CharacterId speaker = new("seeker");
        CharacterId listener = new("healer");
        DialogueDelivery delivery = new(
            new DialogueUtterance(speaker, "Can you hear this?"),
            whispers ? DialogueDeliveryMode.Whispering : DialogueDeliveryMode.OrdinarySpeech);

        DialogueAudienceSnapshot audience = DialogueDeliveryHearingSelector.Select(
            delivery,
            SpeakerPosition,
            OrdinaryRangePixels,
            WhisperRangePixels,
            [new DialogueHearingCandidate(listener, new Vector2(listenerX, 300.0f))]);

        Assert.Same(speaker, audience.SpeakerCharacterId);
        if (expectedToHear)
        {
            Assert.Same(listener, Assert.Single(audience.ListenerCharacterIds));
        }
        else
        {
            Assert.Empty(audience.ListenerCharacterIds);
        }
    }

    [Fact]
    public void RetainsStableCandidateOrderAndExactIdentifiers()
    {
        CharacterId speaker = new("seeker");
        CharacterId first = new("healer");
        CharacterId second = new("sage");
        DialogueDelivery delivery = new(
            new DialogueUtterance(speaker, "Stay close."),
            DialogueDeliveryMode.Whispering);

        DialogueAudienceSnapshot audience = DialogueDeliveryHearingSelector.Select(
            delivery,
            SpeakerPosition,
            OrdinaryRangePixels,
            WhisperRangePixels,
            [
                new(first, new Vector2(440.0f, 300.0f)),
                new(second, new Vector2(400.0f, 340.0f)),
            ]);

        Assert.Collection(
            audience.ListenerCharacterIds,
            candidate => Assert.Same(first, candidate),
            candidate => Assert.Same(second, candidate));
    }

    [Theory]
    [InlineData(0.0f, 40.0f)]
    [InlineData(100.0f, 0.0f)]
    [InlineData(float.NaN, 40.0f)]
    [InlineData(100.0f, float.PositiveInfinity)]
    public void RejectsInvalidRangeConfigurationBeforeSelecting(
        float ordinaryRangePixels,
        float whisperRangePixels)
    {
        DialogueDelivery delivery = new(
            new DialogueUtterance(new CharacterId("seeker"), "Hello."),
            DialogueDeliveryMode.OrdinarySpeech);

        Assert.Throws<ArgumentOutOfRangeException>(() => DialogueDeliveryHearingSelector.Select(
            delivery,
            SpeakerPosition,
            ordinaryRangePixels,
            whisperRangePixels,
            Array.Empty<DialogueHearingCandidate>()));
    }

    [Fact]
    public void RejectsNullDeliveryBeforeCandidateSelection()
    {
        Assert.Throws<ArgumentNullException>(() => DialogueDeliveryHearingSelector.Select(
            null!,
            SpeakerPosition,
            OrdinaryRangePixels,
            WhisperRangePixels,
            Array.Empty<DialogueHearingCandidate>()));
    }
}
