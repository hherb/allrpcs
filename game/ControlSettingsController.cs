using System.Text.RegularExpressions;
using Allrpcs.Application.Input;
using Allrpcs.Infrastructure.Input;
using Godot;

namespace Allrpcs.Game;

/// <summary>Complete controls preference transactions and binding-aware presentation guidance.</summary>
public partial class ControlSettingsController : Node
{
    internal static ControlSettingsController? Current { get; private set; }
    private static readonly Dictionary<Label, string> Guidance = [];
    private ControlSettingsStorageController storage = null!;
    private ControlInputMapAdapter adapter = null!;
    /// <summary>Current active complete controls.</summary>
    public ControlPreferences Preferences { get; private set; } = ControlPreferences.Defaults();
    /// <summary>Path-free load/apply/save outcome.</summary>
    public string Status { get; private set; } = "Controls: defaults";
    /// <summary>Whether applied changes differ from the persisted snapshot.</summary>
    public bool Unsaved { get; private set; }
    /// <summary>Raised after a complete installation or status change.</summary>
    public event Action? Changed;
    /// <inheritdoc />
    public override void _Ready()
    {
        Current = this; storage = GetNode<ControlSettingsStorageController>("../ControlSettingsStorageController");
        adapter = GetNode<ControlInputMapAdapter>("../ControlInputMapAdapter");
        ControlPreferencesLoadResult loaded = storage.Load();
        if (adapter.Apply(loaded.Preferences, out string result))
        {
            Preferences = loaded.Preferences; Status = loaded.Status switch
            { ControlPreferencesLoadStatus.Loaded => "Controls loaded", ControlPreferencesLoadStatus.Invalid => "Unreadable controls · Complete defaults restored", _ => "Controls: defaults" };
        }
        else
        {
            ControlPreferences completeDefaults = ControlPreferences.Defaults();
            if (adapter.Apply(completeDefaults, out _))
            { Preferences = completeDefaults; Status = "Controls conflict with this keyboard · Complete defaults restored"; }
            else { Status = result; }
        }
        CallDeferred(MethodName.RefreshPresentation);
    }
    /// <summary>Installs a fully validated immutable draft or retains the previous active snapshot.</summary>
    public bool Apply(ControlPreferences draft)
    {
        if (!adapter.Apply(draft, out string status)) { Status = status; Changed?.Invoke(); return false; }
        Preferences = draft; Unsaved = true; Status = status; RefreshPresentation(); Changed?.Invoke(); return true;
    }
    /// <summary>Applies explicitly, then persists; failure retains active values and previous disk bytes.</summary>
    public bool Save(ControlPreferences draft)
    {
        if (!Apply(draft)) { return false; }
        bool saved = storage.Save(Preferences); Unsaved = !saved;
        Status = saved ? "Controls saved" : "Controls save failed · Active controls retained";
        Changed?.Invoke(); return saved;
    }
    /// <summary>Canonical complete-value equality for draft/active comparisons.</summary>
    internal static bool Same(ControlPreferences left, ControlPreferences right) => ControlPreferencesJson.Serialize(left) == ControlPreferencesJson.Serialize(right);
    /// <summary>Registers source guidance so an existing status updates immediately after remapping.</summary>
    internal static void SetGuidance(Label label, string source)
    { Guidance[label] = source; label.Text = Current?.FormatGuidance(source) ?? source; }
    /// <summary>Describes every assignment without assuming a last-used device.</summary>
    internal string Assignments(string id) => string.Join(" / ", Preferences.Get(id).Bindings.Select(ControlInputMapAdapter.Describe));
    internal string ShortBinding(string id)
    {
        ControlBinding[] bindings = Preferences.Get(id).Bindings.ToArray();
        return string.Join(" / ", bindings.Select(b => b.Kind == "key" ? ((InputEventKey)ControlInputMapAdapter.ToEvent(b)).AsTextKeycode() is string t && b.CodeMode == "logical" ? t
            : ((InputEventKey)ControlInputMapAdapter.ToEvent(b)).AsTextPhysicalKeycode() : ControlInputMapAdapter.Describe(b)));
    }
    private string FormatGuidance(string source) => Regex.Replace(source, @"(?i)(\bpress\s+)([a-z])\b", match =>
    {
        ControlActionDefinition? action = ControlActionCatalog.Actions.FirstOrDefault(a => a.Context == "gameplay"
            && a.Defaults.Any(b => b.Kind == "key" && b.Code == char.ToUpperInvariant(match.Groups[2].Value[0])));
        return action is null ? match.Value : match.Groups[1].Value + ShortBinding(action.Id);
    });
    /// <summary>Refreshes current guidance while preserving original transition/status source text.</summary>
    private void RefreshPresentation()
    {
        foreach (var pair in Guidance.ToArray())
        { if (!GodotObject.IsInstanceValid(pair.Key)) { Guidance.Remove(pair.Key); } else { pair.Key.Text = FormatGuidance(pair.Value); } }
        Label? instructions = GetNodeOrNull<Label>("../CommandOverlay/Panel/Instructions");
        if (instructions is not null)
        {
            instructions.Text = "Talk: " + ShortBinding("dialogue_open") + " · Journal: " + ShortBinding("journal_toggle")
                + " · Map: " + ShortBinding("map_toggle") + " · Settings and all commands: " + ShortBinding("optional_overlays_toggle");
        }
        Button? save = GetNodeOrNull<Button>("../CommandOverlay/SaveLoadPanel/Save");
        Button? load = GetNodeOrNull<Button>("../CommandOverlay/SaveLoadPanel/Load");
        if (save is not null) { save.Text = ShortBinding("save_current") + "  SAVE"; }
        if (load is not null) { load.Text = ShortBinding("load_current") + "  LOAD"; }
    }
    /// <inheritdoc />
    public override void _ExitTree() { if (ReferenceEquals(Current, this)) { Current = null; Guidance.Clear(); } }
}
