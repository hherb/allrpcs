namespace Allrpcs.Application.Input;

/// <summary>One immutable, validated action event set.</summary>
public sealed class ControlActionSetting
{
    /// <summary>Exact catalog ID.</summary>
    public string Id { get; }
    /// <summary>Finite input deadzone.</summary>
    public double Deadzone { get; }
    /// <summary>Immutable event values in stable order.</summary>
    public IReadOnlyList<ControlBinding> Bindings { get; }
    /// <summary>Copies and validates events before exposing them.</summary>
    public ControlActionSetting(string id, double deadzone, IEnumerable<ControlBinding> bindings)
    {
        ControlActionDefinition action = ControlActionCatalog.Get(id);
        ArgumentNullException.ThrowIfNull(bindings);
        ControlBinding[] copy = bindings.OrderBy(b => b?.Identity, StringComparer.Ordinal).ToArray();
        if (!double.IsFinite(deadzone) || deadzone < 0.1 || deadzone > 0.9 || (!action.Direction && deadzone != 0.5))
        { throw new ArgumentException("Invalid control deadzone."); }
        if (copy.Length is < 1 or > ControlActionCatalog.MaximumBindings || copy.Any(b => b is null)
            || copy.GroupBy(b => b.DeviceClass).Any(g => g.Count() > ControlActionCatalog.MaximumDeviceBindings)
            || (!action.Direction && copy.Any(b => b.Kind == "joyAxis")))
        { throw new ArgumentException("Invalid event set for " + action.Label + "."); }
        for (int i = 0; i < copy.Length; i++)
            for (int j = i + 1; j < copy.Length; j++)
            {
                if (copy[i].ConflictsWith(copy[j], action.Direction))
                { throw new ArgumentException("Duplicate/overlapping binding on " + action.Label + "."); }
            }
        Id = id; Deadzone = deadzone; Bindings = Array.AsReadOnly(copy);
    }
}

/// <summary>A complete immutable 29-action snapshot; never a partial live update.</summary>
public sealed class ControlPreferences
{
    /// <summary>Complete stable catalog order.</summary>
    public IReadOnlyList<ControlActionSetting> Actions { get; }
    /// <summary>Validates exact completeness and conflicts before materialization.</summary>
    public ControlPreferences(IEnumerable<ControlActionSetting> actions)
    {
        ArgumentNullException.ThrowIfNull(actions);
        ControlActionSetting[] copy = actions.ToArray();
        if (copy.Length != ControlActionCatalog.Actions.Count || copy.Any(a => a is null)
            || copy.Select(a => a.Id).Distinct(StringComparer.Ordinal).Count() != copy.Length)
        { throw new ArgumentException("A complete unique controls document is required."); }
        copy = ControlActionCatalog.Actions.Select(d => copy.Single(a => a.Id == d.Id)).ToArray();
        for (int i = 0; i < copy.Length; i++)
            for (int j = i + 1; j < copy.Length; j++)
            {
                ControlActionDefinition left = ControlActionCatalog.Get(copy[i].Id), right = ControlActionCatalog.Get(copy[j].Id);
                if (left.Context != right.Context) { continue; }
                if (copy[i].Bindings.Any(a => copy[j].Bindings.Any(b => a.ConflictsWith(b, left.Direction || right.Direction))))
                { throw new ArgumentException("Binding conflicts: " + left.Label + " / " + right.Label + "."); }
            }
        Actions = Array.AsReadOnly(copy);
    }
    /// <summary>Returns one exact catalog row.</summary>
    public ControlActionSetting Get(string id) => Actions.Single(a => a.Id == id);
    /// <summary>Creates complete authored defaults.</summary>
    public static ControlPreferences Defaults() => new(ControlActionCatalog.Actions.Select(d => new ControlActionSetting(d.Id, 0.5, d.Defaults)));
}
