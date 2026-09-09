using Allrpcs.Application.Dialogue;

namespace Allrpcs.Application.Tests.Dialogue;

public sealed class CharacterModelConnectionTestResultTests
{
    [Fact]
    public void CreatesConnectedAndRedactedFailedOutcomes()
    {
        CharacterModelConnectionTestResult connected =
            CharacterModelConnectionTestResult.Connected();
        CharacterModelConnectionTestResult failed =
            CharacterModelConnectionTestResult.Failed(CharacterModelFailureCode.Authentication);

        Assert.True(connected.IsConnected);
        Assert.Null(connected.Failure);
        Assert.False(failed.IsConnected);
        Assert.Equal(CharacterModelFailureCode.Authentication, failed.Failure?.Code);
        Assert.Equal("Connected", connected.ToString());
        Assert.Equal("Failed:Authentication", failed.ToString());
    }
}
