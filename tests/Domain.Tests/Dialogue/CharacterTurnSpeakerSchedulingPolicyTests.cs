using Allrpcs.Domain.Dialogue;
using Allrpcs.Domain.Identifiers;

namespace Allrpcs.Domain.Tests.Dialogue;

public sealed class CharacterTurnSpeakerSchedulingPolicyTests
{
    [Fact]
    public void OrdersDirectlyAddressedEligibleSpeakersBeforeOthersInStableEligibleOrder()
    {
        CharacterId hermit = new("woodland-hermit");
        CharacterId sage = new("village-sage");
        CharacterId healer = new("travelling-healer");

        IReadOnlyList<CharacterId> scheduled = CharacterTurnSpeakerSchedulingPolicy.Order(
            [hermit, sage, healer],
            [new CharacterId("travelling-healer"), new CharacterId("village-sage")]);

        Assert.Collection(
            scheduled,
            first => Assert.Same(sage, first),
            second => Assert.Same(healer, second),
            third => Assert.Same(hermit, third));
    }

    [Fact]
    public void IgnoresAbsentAddressesAndRetainsEveryExactEligibleSpeakerOnce()
    {
        CharacterId hermit = new("woodland-hermit");
        CharacterId sage = new("village-sage");
        CharacterId healer = new("travelling-healer");
        CharacterId[] eligible = [hermit, sage, healer];
        CharacterId[] addressed = [new("absent-listener"), new("woodland-hermit")];

        IReadOnlyList<CharacterId> scheduled = CharacterTurnSpeakerSchedulingPolicy.Order(
            eligible,
            addressed);

        Assert.Collection(
            scheduled,
            first => Assert.Same(hermit, first),
            second => Assert.Same(sage, second),
            third => Assert.Same(healer, third));
        Assert.Same(hermit, eligible[0]);
        Assert.Equal("absent-listener", addressed[0].Value);
    }

    [Fact]
    public void ReturnsAnIndependentReadOnlyEmptyOrUnaddressedSchedule()
    {
        CharacterId hermit = new("woodland-hermit");
        CharacterId sage = new("village-sage");
        CharacterId[] eligible = [hermit, sage];

        IReadOnlyList<CharacterId> scheduled = CharacterTurnSpeakerSchedulingPolicy.Order(
            eligible,
            []);

        eligible[0] = new CharacterId("replacement");

        Assert.Collection(
            scheduled,
            first => Assert.Same(hermit, first),
            second => Assert.Same(sage, second));
        Assert.Throws<NotSupportedException>(() =>
            ((IList<CharacterId>)scheduled).Add(hermit));
        Assert.Empty(CharacterTurnSpeakerSchedulingPolicy.Order([], []));
    }

    [Fact]
    public void RejectsNullDuplicateOrOversizedSchedulingInputs()
    {
        CharacterId hermit = new("woodland-hermit");
        CharacterId[] maximum = Enumerable
            .Range(0, DialogueAudienceSnapshot.MaximumListenerCount)
            .Select(index => new CharacterId($"speaker-{index}"))
            .ToArray();

        Assert.Equal(
            maximum.Length,
            CharacterTurnSpeakerSchedulingPolicy.Order(maximum, []).Count);
        Assert.Throws<ArgumentNullException>(() =>
            CharacterTurnSpeakerSchedulingPolicy.Order(null!, []));
        Assert.Throws<ArgumentNullException>(() =>
            CharacterTurnSpeakerSchedulingPolicy.Order([], null!));
        Assert.Throws<ArgumentException>(() =>
            CharacterTurnSpeakerSchedulingPolicy.Order([null!], []));
        Assert.Throws<ArgumentException>(() =>
            CharacterTurnSpeakerSchedulingPolicy.Order([], [null!]));
        Assert.Throws<ArgumentException>(() =>
            CharacterTurnSpeakerSchedulingPolicy.Order([hermit, new("woodland-hermit")], []));
        Assert.Throws<ArgumentException>(() =>
            CharacterTurnSpeakerSchedulingPolicy.Order([], [hermit, new("woodland-hermit")]));
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            CharacterTurnSpeakerSchedulingPolicy.Order(
                maximum.Append(new CharacterId("extra-speaker")),
                []));
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            CharacterTurnSpeakerSchedulingPolicy.Order(
                [],
                maximum.Append(new CharacterId("extra-address"))));
    }

    [Fact]
    public void ExposesOnlyThePureOrderingOperation()
    {
        Type policyType = typeof(CharacterTurnSpeakerSchedulingPolicy);
        System.Reflection.MethodInfo method = Assert.Single(policyType.GetMethods().Where(
            candidate => candidate.DeclaringType == policyType));

        Assert.Equal("Order", method.Name);
        Assert.True(method.IsStatic);
        Assert.Equal(typeof(IReadOnlyList<CharacterId>), method.ReturnType);
        Assert.Equal(
            [typeof(IEnumerable<CharacterId>), typeof(IEnumerable<CharacterId>)],
            method.GetParameters().Select(parameter => parameter.ParameterType));
        Assert.Empty(policyType.GetConstructors());
        Assert.Empty(policyType.GetProperties());
        Assert.Empty(policyType.GetEvents());
        Assert.True(policyType.IsAbstract && policyType.IsSealed);
    }
}
