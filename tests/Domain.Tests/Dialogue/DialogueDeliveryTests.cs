using Allrpcs.Domain.Dialogue;

namespace Allrpcs.Domain.Tests.Dialogue;

public sealed class DialogueDeliveryTests
{
    [Theory]
    [MemberData(nameof(Modes))]
    public void RetainsExactUtteranceAndMode(
        DialogueDeliveryMode mode)
    {
        DialogueUtterance utterance = new(new("seeker"), "Keep this between us.");

        DialogueDelivery delivery = new(utterance, mode);

        Assert.Same(utterance, delivery.Utterance);
        Assert.Same(mode, delivery.Mode);
    }

    [Fact]
    public void EqualityUsesUtteranceAndModeValues()
    {
        DialogueDelivery first = new(
            new(new("seeker"), "Keep this between us."),
            DialogueDeliveryMode.Whispering);
        DialogueDelivery same = new(
            new(new("seeker"), "Keep this between us."),
            DialogueDeliveryMode.Whispering);
        DialogueDelivery differentUtterance = new(
            new(new("seeker"), "Tell everyone."),
            DialogueDeliveryMode.Whispering);
        DialogueDelivery differentMode = new(
            new(new("seeker"), "Keep this between us."),
            DialogueDeliveryMode.OrdinarySpeech);

        Assert.Equal(first, same);
        Assert.NotEqual(first, differentUtterance);
        Assert.NotEqual(first, differentMode);
    }

    [Fact]
    public void RejectsNullInputs()
    {
        DialogueUtterance utterance = new(new("seeker"), "Keep this between us.");

        Assert.Throws<ArgumentNullException>(() =>
            new DialogueDelivery(null!, DialogueDeliveryMode.Whispering));
        Assert.Throws<ArgumentNullException>(() => new DialogueDelivery(utterance, null!));
    }

    [Fact]
    public void ExposesOnlyConstructionAndGetOnlyState()
    {
        Type deliveryType = typeof(DialogueDelivery);
        (string Name, Type Type)[] expectedProperties =
        [
            ("Mode", typeof(DialogueDeliveryMode)),
            ("Utterance", typeof(DialogueUtterance)),
        ];
        (string Name, Type Type, bool CanWrite)[] actualProperties = deliveryType
            .GetProperties()
            .OrderBy(property => property.Name, StringComparer.Ordinal)
            .Select(property => (property.Name, property.PropertyType, property.CanWrite))
            .ToArray();
        System.Reflection.ConstructorInfo constructor = Assert.Single(deliveryType.GetConstructors());

        Assert.Equal(
            expectedProperties,
            actualProperties.Select(property => (property.Name, property.Type)).ToArray());
        Assert.All(actualProperties, property => Assert.False(property.CanWrite));
        Assert.Collection(
            constructor.GetParameters(),
            parameter => Assert.Equal(typeof(DialogueUtterance), parameter.ParameterType),
            parameter => Assert.Equal(typeof(DialogueDeliveryMode), parameter.ParameterType));
        Assert.Empty(deliveryType.GetEvents());
        Assert.True(deliveryType.IsSealed);
    }

    public static TheoryData<DialogueDeliveryMode> Modes =>
        new()
        {
            DialogueDeliveryMode.OrdinarySpeech,
            DialogueDeliveryMode.Whispering,
        };
}
