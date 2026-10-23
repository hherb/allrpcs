using Allrpcs.Application.Input;

namespace Allrpcs.Application.Tests.Input;

public sealed class ControlBindingEditTests
{
    [Fact]
    public void EditsAreImmutableAndDoNotDropOtherRows()
    {
        ControlPreferences before = ControlPreferences.Defaults();
        ControlPreferences after = ControlBindingEdit.Set(before, "save_current", new("mouseButton", 8));
        Assert.Single(before.Get("save_current").Bindings); Assert.Equal(2, after.Get("save_current").Bindings.Count);
        Assert.Same(before.Get("load_current"), after.Get("load_current"));
        Assert.Equal(1, ControlBindingEdit.Remove(after, "save_current", after.Get("save_current").Bindings.ToList().FindIndex(b => b.Kind == "mouseButton")).Get("save_current").Bindings.Count);
        Assert.Throws<ArgumentException>(() => ControlBindingEdit.Remove(before, "save_current", 0));
    }
    [Fact]
    public void ReplaceAndDefaultsAreExplicitCompleteTransactions()
    {
        ControlPreferences before = ControlPreferences.Defaults();
        ControlPreferences after = ControlBindingEdit.Set(before, "save_current", new("key", 4194332, "physical"), 0);
        Assert.Equal(4194332, after.Get("save_current").Bindings[0].Code);
        Assert.Equal(81, before.Get("save_current").Bindings[0].Code);
        Assert.Equal(81, ControlBindingEdit.Reset(after, "save_current").Get("save_current").Bindings[0].Code);
    }
    [Fact]
    public void ConflictsNameBothActionsAndPreserveSource()
    {
        ControlPreferences before = ControlPreferences.Defaults();
        ArgumentException failure = Assert.Throws<ArgumentException>(() => ControlBindingEdit.Set(before, "save_current", new("key", 76, "physical"), 0));
        Assert.Contains("Save", failure.Message); Assert.Contains("Load", failure.Message);
        Assert.Equal(81, before.Get("save_current").Bindings[0].Code);
        Assert.Throws<ArgumentException>(() => ControlBindingEdit.Set(before, "move_left", new("key", 65, "logical")));
        Assert.Throws<ArgumentException>(() => ControlBindingEdit.Set(before, "save_current", new("key", 65, "physical", shift: true)));
    }
    [Fact]
    public void ContextReuseAndUiModifierPairAreDeliberate()
    {
        ControlPreferences before = ControlPreferences.Defaults();
        // Keypad Enter is UI Accept only (Enter itself is now gameplay Talk), so gameplay may reuse it.
        Assert.NotNull(ControlBindingEdit.Set(before, "save_current", new("key", 4194310, "logical"), 0));
        Assert.NotNull(ControlBindingEdit.Set(before, "save_current", new("key", 70, "physical", ctrl: true), 0));
        Assert.Contains(before.Get("ui_focus_prev").Bindings, b => b.Shift);
        Assert.Throws<ArgumentException>(() => ControlBindingEdit.Set(before, "move_left", new("joyAxis", 0, sign: 1)));
        Assert.Contains(before.Get("move_left").Bindings, b => b.Kind == "joyAxis" && b.Sign == -1);
    }
    [Fact]
    public void RowResetRejectsConflictingCustomizedAction()
    {
        ControlPreferences defaults = ControlPreferences.Defaults();
        ControlPreferences first = ControlBindingEdit.Set(defaults, "save_current", new("key", 4194332, "physical"), 0);
        ControlPreferences second = ControlBindingEdit.Set(first, "load_current", new("key", 81, "physical"), 0);
        Assert.Throws<ArgumentException>(() => ControlBindingEdit.Reset(second, "save_current"));
    }
}
