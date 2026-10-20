using Allrpcs.Application.Movement;

namespace Allrpcs.Application.Tests;

public sealed class HealerModeStatusTextTests
{
    [Theory]
    [InlineData(HealerCommandMode.Follow, false, "Current healer mode: FOLLOW")]
    [InlineData(HealerCommandMode.Wait, false, "Current healer mode: WAIT")]
    [InlineData(HealerCommandMode.MoveToDestination, false, "Current healer mode: MOVE TO PINK RING")]
    [InlineData(HealerCommandMode.MoveToDestination, true, "Current healer mode: EXAMINE HERB PATCH")]
    [InlineData(HealerCommandMode.Follow, true, "Current healer mode: FOLLOW")]
    [InlineData(HealerCommandMode.Wait, true, "Current healer mode: WAIT")]
    [InlineData((HealerCommandMode)99, false, "Current healer mode: UNKNOWN")]
    public void FormatsEachModeAndOnlyMovesReadAsExamination(
        HealerCommandMode mode,
        bool examining,
        string expected) =>
        Assert.Equal(expected, HealerModeStatusText.Format(mode, examining));
}
