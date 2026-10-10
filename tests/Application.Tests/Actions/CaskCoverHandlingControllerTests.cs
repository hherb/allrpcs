using Allrpcs.Application.Actions;
using Allrpcs.Domain.Actions;
using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Controllers;
using Allrpcs.Domain.WorldObjects;

namespace Allrpcs.Application.Tests.Actions;

public sealed class CaskCoverHandlingControllerTests
{
    [Fact]
    public void CommitsExactIssuedActorTargetMaterialOnceBeforeIsolatedObservers()
    {
        Character actor = Actor(); var material = Material(); CaskCoverHandlingController owner = new(actor, material);
        var old = owner.State; var request = owner.CreateRequest()!; int observations = 0;
        owner.Committed += _ => throw new InvalidOperationException();
        owner.Committed += commit =>
        {
            observations++; Assert.Same(commit, owner.LastCommit); Assert.NotSame(old, owner.State);
            Assert.Equal(4, owner.State.Cooldown); Assert.Null(owner.CreateRequest());
        };
        Assert.False(owner.TryCommit(new(actor, material, old), actor, material.Target, material, true));
        Assert.False(owner.TryCommit(request, Actor(), material.Target, material, true));
        Assert.False(owner.TryCommit(request, actor, Material().Target, material, true));
        Assert.False(owner.TryCommit(request, actor, material.Target, Material(), true));
        Assert.False(owner.TryCommit(request, actor, material.Target, material, false));
        Assert.True(owner.TryCommit(request, actor, material.Target, material, true));
        Assert.False(owner.TryCommit(request, actor, material.Target, material, true));
        Assert.Equal(1, observations); Assert.Equal(1, owner.ObserverFailureCount);
        for (int i = 0; i < 16; i++) { Assert.True(owner.Advance(0.25, true)); }
        Assert.False(owner.TryCommit(request, actor, material.Target, material, true));
        Assert.True(owner.TryCommit(owner.CreateRequest(), actor, material.Target, material, true));
        Assert.Equal(2, owner.State.Sequence);
    }
    [Theory]
    [InlineData(-1)]
    [InlineData(0.251)]
    [InlineData(double.NaN)]
    [InlineData(double.PositiveInfinity)]
    public void InvalidDeltasAndPausePreserveCooldown(double delta)
    {
        Character actor = Actor(); var material = Material(); CaskCoverHandlingController owner = new(actor, material);
        owner.TryCommit(owner.CreateRequest(), actor, material.Target, material, true);
        Assert.False(owner.Advance(delta, true)); Assert.Equal(4, owner.State.Cooldown);
        owner.Advance(0.25, false); Assert.Equal(4, owner.State.Cooldown);
    }
    [Fact]
    public void ResetInvalidatesAttemptsAndCommitsButRetainsSequence()
    {
        Character actor = Actor(); var material = Material(); CaskCoverHandlingController owner = new(actor, material);
        var request = owner.CreateRequest(); owner.Reset();
        Assert.False(owner.TryCommit(request, actor, material.Target, material, true));
        owner.TryCommit(owner.CreateRequest(), actor, material.Target, material, true); var commit = owner.LastCommit;
        owner.Reset(); Assert.Null(owner.LastCommit); Assert.Equal(0, owner.State.Cooldown); Assert.Equal(1, owner.State.Sequence);
        Assert.False(owner.TryCommit(commit!.Request, actor, material.Target, material, true));
    }
    [Theory]
    [InlineData("llm")]
    [InlineData("dormant")]
    public void NonHumanCannotIssueRequests(string type)
    { Assert.Null(new CaskCoverHandlingController(Actor(type == "llm" ? ControllerType.Llm : ControllerType.Dormant), Material()).CreateRequest()); }
    internal static Character Actor(ControllerType? type = null) => StreamWaterContactControllerTests.Actor(type);
    internal static CaskCoverHandlingMaterial Material() => new(new WorldObject(new(new("village-water-cask"), "Village water cask"), new("A covered cask beside the village well.")));
}
