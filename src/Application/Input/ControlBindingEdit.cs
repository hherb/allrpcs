namespace Allrpcs.Application.Input;

/// <summary>Pure transactional edits; rejection leaves both active and draft values unchanged.</summary>
public static class ControlBindingEdit
{
    /// <summary>Adds a binding or replaces one index, then validates the complete document.</summary>
    public static ControlPreferences Set(ControlPreferences source, string id, ControlBinding binding, int? index = null)
    {
        ArgumentNullException.ThrowIfNull(source); ArgumentNullException.ThrowIfNull(binding);
        ControlActionSetting row = source.Get(id);
        List<ControlBinding> events = row.Bindings.ToList();
        if (index is int slot) { events[slot] = binding; } else { events.Add(binding); }
        return Replace(source, new ControlActionSetting(id, row.Deadzone, events));
    }
    /// <summary>Removes one index; last-binding removal is rejected.</summary>
    public static ControlPreferences Remove(ControlPreferences source, string id, int index)
    {
        ControlActionSetting row = source.Get(id); List<ControlBinding> events = row.Bindings.ToList();
        events.RemoveAt(index); return Replace(source, new ControlActionSetting(id, row.Deadzone, events));
    }
    /// <summary>Resets one row while still rejecting conflicts with other customized rows.</summary>
    public static ControlPreferences Reset(ControlPreferences source, string id) => Replace(source,
        new ControlActionSetting(id, 0.5, ControlActionCatalog.Get(id).Defaults));
    /// <summary>Changes a direction deadzone, preserving all event values.</summary>
    public static ControlPreferences Deadzone(ControlPreferences source, string id, double value) => Replace(source,
        new ControlActionSetting(id, value, source.Get(id).Bindings));
    private static ControlPreferences Replace(ControlPreferences source, ControlActionSetting row) =>
        new(source.Actions.Select(a => a.Id == row.Id ? row : a));
}
