using Allrpcs.Application.Input;

namespace Allrpcs.Application.Tests.Input;

public sealed class ControlPreferencesTests
{
    private static readonly int[] MouseButtons = [1, 2, 3, 8];
    [Fact]
    public void DefaultsAreCompleteAndDeviceAssignmentsStayBounded()
    {
        ControlPreferences value = ControlPreferences.Defaults();
        Assert.Equal(29, value.Actions.Count); Assert.Equal(21, ControlActionCatalog.Actions.Count(a => a.Context == "gameplay"));
        Assert.Equal(16, value.Actions.Count(a => ControlActionCatalog.Get(a.Id).Context == "gameplay" && a.Bindings.All(b => b.DeviceClass != "Controller")));
        Assert.Equal(65, value.Get("move_left").Bindings.Single(b => b.Kind == "key").Code);
        Assert.Equal(72, value.Get("healer_move").Bindings.Single(b => b.Kind == "key").Code);
        Assert.Contains(value.Get("ui_accept").Bindings, b => b.Kind == "joyButton" && b.Code == 0);
        Assert.Contains(value.Get("ui_cancel").Bindings, b => b.Kind == "joyButton" && b.Code == 1);
        Assert.All(value.Actions, a => Assert.Equal(0.5, a.Deadzone));
    }
    [Fact]
    public void MissingDuplicateAndUnknownActionsAreRejected()
    {
        ControlPreferences value = ControlPreferences.Defaults();
        Assert.Throws<ArgumentException>(() => new ControlPreferences(value.Actions.Skip(1)));
        Assert.Throws<ArgumentException>(() => new ControlPreferences(value.Actions.Skip(1).Append(value.Actions[1])));
        Assert.Throws<ArgumentException>(() => new ControlActionSetting("MOVE_LEFT", 0.5, value.Actions[0].Bindings));
    }
    [Theory]
    [InlineData(-0.1)]
    [InlineData(0)]
    [InlineData(0.099)]
    [InlineData(0.901)]
    [InlineData(double.NaN)]
    [InlineData(double.PositiveInfinity)]
    public void RejectsBadDeadzones(double value) => Assert.Throws<ArgumentException>(() =>
        ControlBindingEdit.Deadzone(ControlPreferences.Defaults(), "move_left", value));
    [Fact]
    public void OnlyDirectionRowsPermitAxesAndDeadzoneChanges()
    {
        ControlPreferences value = ControlPreferences.Defaults();
        Assert.Throws<ArgumentException>(() => ControlBindingEdit.Set(value, "save_current", new("joyAxis", 2, sign: 1)));
        Assert.Throws<ArgumentException>(() => ControlBindingEdit.Deadzone(value, "save_current", 0.2));
        Assert.Equal(0.1, ControlBindingEdit.Deadzone(value, "ui_left", 0.1).Get("ui_left").Deadzone);
        Assert.Equal(0.9, ControlBindingEdit.Deadzone(value, "move_left", 0.9).Get("move_left").Deadzone);
    }
    [Theory]
    [InlineData("key", 0, "physical", 0)]
    [InlineData("key", 4194325, "physical", 0)]
    [InlineData("key", 97, "physical", 0)]
    [InlineData("key", 999999, "physical", 0)]
    [InlineData("key", 65, "guess", 0)]
    [InlineData("key", 65, "logical", 1)]
    [InlineData("mouseButton", 4, "", 0)]
    [InlineData("mouseButton", 7, "", 0)]
    [InlineData("joyButton", -1, "", 0)]
    [InlineData("joyButton", 21, "", 0)]
    [InlineData("joyAxis", 6, "", 1)]
    [InlineData("joyAxis", 0, "", 0)]
    [InlineData("resource", 1, "", 0)]
    public void RejectsUnsupportedEventValues(string kind, int code, string mode, int sign) =>
        Assert.Throws<ArgumentException>(() => new ControlBinding(kind, code, mode, sign));
    [Fact]
    public void CopiesCallerListsAndExposesNoMutableArrays()
    {
        ControlBinding[] bindings = [new("key", 65, "physical")];
        ControlActionSetting row = new("move_left", 0.5, bindings); bindings[0] = new("key", 66, "physical");
        Assert.Equal(65, row.Bindings[0].Code); Assert.False(row.Bindings is ControlBinding[]);
        ControlActionSetting[] rows = ControlPreferences.Defaults().Actions.ToArray();
        ControlPreferences prefs = new(rows); rows[0] = row;
        Assert.Equal(3, prefs.Actions[0].Bindings.Count); Assert.False(prefs.Actions is ControlActionSetting[]);
    }
    [Fact]
    public void EnforcesEightTotalAndFourPerDevice()
    {
        ControlBinding[] keys = Enumerable.Range(65, 5).Select(c => new ControlBinding("key", c, "physical")).ToArray();
        Assert.Throws<ArgumentException>(() => new ControlActionSetting("save_current", 0.5, keys));
        ControlBinding[] nine = Enumerable.Range(65, 4).Select(c => new ControlBinding("key", c, "physical"))
            .Concat(MouseButtons.Select(c => new ControlBinding("mouseButton", c)))
            .Append(new("joyButton", 20)).ToArray();
        Assert.Throws<ArgumentException>(() => new ControlActionSetting("save_current", 0.5, nine));
        Assert.Equal(8, new ControlActionSetting("save_current", 0.5, nine.Take(8)).Bindings.Count);
    }
}
