using System.Numerics;
using Allrpcs.Application.Actions;
using Allrpcs.Domain.Actions;
using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Controllers;

namespace Allrpcs.Application.Tests.Actions;

public sealed class StreamWaterContactControllerTests
{
    [Fact]
    public void CommitsOnlyExactIssuedRequestAndInstallsStateBeforeIsolatedObservers()
    {
        Character actor = Actor(); StreamWaterContactController owner = new(actor, Site());
        StreamWaterContactState old = owner.State; StreamWaterContactRequest request = owner.CreateRequest()!;
        int observations = 0;
        owner.Committed += _ => throw new InvalidOperationException();
        owner.Committed += commit =>
        {
            observations++; Assert.Same(commit, owner.LastCommit);
            Assert.NotSame(old, owner.State); Assert.Equal(2, owner.State.Cooldown); Assert.Null(owner.CreateRequest());
        };
        Assert.False(owner.TryCommit(new(actor, old.Site, old), actor, true));
        Assert.False(owner.TryCommit(request, Actor(), true)); Assert.False(owner.TryCommit(request, actor, false));
        Assert.True(owner.TryCommit(request, actor, true)); Assert.Equal(1, observations); Assert.Equal(1, owner.ObserverFailureCount);
        Assert.False(owner.TryCommit(request, actor, true)); Assert.Equal(1, owner.State.Sequence);
        for (int i = 0; i < 8; i++) { Assert.True(owner.Advance(0.25, true)); }
        Assert.NotNull(owner.CreateRequest()); Assert.False(owner.TryCommit(request, actor, true));
        Assert.True(owner.TryCommit(owner.CreateRequest(), actor, true)); Assert.Equal(2, owner.State.Sequence);
    }
    [Theory]
    [InlineData(-1)]
    [InlineData(0.251)]
    [InlineData(double.NaN)]
    [InlineData(double.PositiveInfinity)]
    public void InvalidTimeAndPauseCannotReplenishCooldown(double delta)
    {
        Character actor = Actor(); StreamWaterContactController owner = new(actor, Site()); owner.TryCommit(owner.CreateRequest(), actor, true);
        Assert.False(owner.Advance(delta, true)); Assert.Equal(2, owner.State.Cooldown);
        owner.Advance(0.25, false); Assert.Equal(2, owner.State.Cooldown);
    }
    [Fact]
    public void ResetInvalidatesOldAttemptsAndCommitsWithoutReplayingOrLosingSequence()
    {
        Character actor = Actor(); StreamWaterContactController owner = new(actor, Site()); var request = owner.CreateRequest();
        owner.Reset(); Assert.False(owner.TryCommit(request, actor, true));
        owner.TryCommit(owner.CreateRequest(), actor, true); var commit = owner.LastCommit; owner.Reset();
        Assert.Null(owner.LastCommit); Assert.Equal(1, owner.State.Sequence); Assert.Equal(0, owner.State.Cooldown);
        Assert.False(owner.TryCommit(commit!.Request, actor, true));
    }
    [Fact]
    public void ModelAndDormantActorsCannotAcquireContactRequests()
    {
        Assert.Null(new StreamWaterContactController(Actor(ControllerType.Llm), Site()).CreateRequest());
        Assert.Null(new StreamWaterContactController(Actor(ControllerType.Dormant), Site()).CreateRequest());
    }
    internal static Character Actor(ControllerType? type = null)
    {
        Character c = RepresentativeSageCharacterSource.Create();
        return new(c.Identity, c.PublicBackground, c.PrivateBackground, c.Capabilities, c.Limitations,
            new(c.Identity.Id, type ?? ControllerType.Human), c.Conditions, c.Inventory, c.Knowledge,
            c.Beliefs, c.Relationships, c.Goals);
    }
    internal static StreamWaterContactSite Site() => new("stream-west-bridge-contact", new(1734, 484));
}
