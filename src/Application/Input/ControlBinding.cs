namespace Allrpcs.Application.Input;

/// <summary>One engine-independent, validated input event identity.</summary>
public sealed record ControlBinding
{
    /// <summary>Event kind: key, mouseButton, joyButton or joyAxis.</summary>
    public string Kind { get; }
    /// <summary>Physical/logical for keys; empty for other events.</summary>
    public string CodeMode { get; }
    /// <summary>Key code, button index or axis index.</summary>
    public int Code { get; }
    /// <summary>Axis direction; zero for other events.</summary>
    public int Sign { get; }
    /// <summary>Exact keyboard modifiers.</summary>
    public bool Shift { get; }
    /// <summary>Exact keyboard modifiers.</summary>
    public bool Ctrl { get; }
    /// <summary>Exact keyboard modifiers.</summary>
    public bool Alt { get; }
    /// <summary>Exact keyboard modifiers.</summary>
    public bool Meta { get; }
    /// <summary>Canonical device class, independent of connection identifiers.</summary>
    public string DeviceClass => Kind == "key" ? "Keyboard" : Kind == "mouseButton" ? "Mouse" : "Controller";
    /// <summary>Stable ordering identity for documents and duplicate checks.</summary>
    public string Identity => $"{Kind}:{Code:D8}:{Sign}:{CodeMode}:{Shift}:{Ctrl}:{Alt}:{Meta}";
    // Exact non-modifier Key enum codes from the installed Godot 4.6.3 managed API.
    private static readonly HashSet<int> KnownKeys = [32, 33, 34, 35, 36, 37, 38, 39, 40, 41, 42, 43, 44, 45, 46, 47, 48, 49, 50, 51, 52, 53, 54, 55, 56, 57, 58, 59, 60, 61, 62, 63, 64, 65, 66, 67, 68, 69, 70, 71, 72, 73, 74, 75, 76, 77, 78, 79, 80, 81, 82, 83, 84, 85, 86, 87, 88, 89, 90, 91, 92, 93, 94, 95, 96, 123, 124, 125, 126, 165, 167, 4194305, 4194306, 4194307, 4194308, 4194309, 4194310, 4194311, 4194312, 4194313, 4194314, 4194315, 4194316, 4194317, 4194318, 4194319, 4194320, 4194321, 4194322, 4194323, 4194324, 4194329, 4194330, 4194331, 4194332, 4194333, 4194334, 4194335, 4194336, 4194337, 4194338, 4194339, 4194340, 4194341, 4194342, 4194343, 4194344, 4194345, 4194346, 4194347, 4194348, 4194349, 4194350, 4194351, 4194352, 4194353, 4194354, 4194355, 4194356, 4194357, 4194358, 4194359, 4194360, 4194361, 4194362, 4194363, 4194364, 4194365, 4194366, 4194370, 4194371, 4194373, 4194376, 4194377, 4194378, 4194379, 4194380, 4194381, 4194382, 4194388, 4194389, 4194390, 4194391, 4194392, 4194393, 4194394, 4194395, 4194396, 4194397, 4194398, 4194399, 4194400, 4194401, 4194402, 4194403, 4194404, 4194405, 4194406, 4194407, 4194408, 4194409, 4194410, 4194411, 4194412, 4194413, 4194414, 4194415, 4194416, 4194417, 4194418, 4194419, 4194433, 4194434, 4194435, 4194436, 4194437, 4194438, 4194439, 4194440, 4194441, 4194442, 4194443, 4194444, 4194445, 4194446, 4194447];

    /// <summary>Rejects unknown kinds/codes, modifier-only keys and irrelevant event fields.</summary>
    public ControlBinding(string kind, int code, string codeMode = "", int sign = 0,
        bool shift = false, bool ctrl = false, bool alt = false, bool meta = false)
    {
        bool valid = kind switch
        {
            "key" => KnownKeys.Contains(code) && codeMode is "physical" or "logical" && sign == 0,
            "mouseButton" => code is 1 or 2 or 3 or 8 or 9 && codeMode == "" && sign == 0,
            "joyButton" => code >= 0 && code <= 20 && codeMode == "" && sign == 0,
            "joyAxis" => code >= 0 && code <= 5 && codeMode == "" && sign is -1 or 1,
            _ => false,
        };
        if (!valid || (kind != "key" && (shift || ctrl || alt || meta)))
        { throw new ArgumentException("Unsupported control binding."); }
        Kind = kind; Code = code; CodeMode = codeMode; Sign = sign;
        Shift = shift; Ctrl = ctrl; Alt = alt; Meta = meta;
    }

    /// <summary>Compares event identity, conservatively treating equal physical/logical codes as aliases.</summary>
    public bool ConflictsWith(ControlBinding other, bool movement)
    {
        if (Kind != other.Kind || Code != other.Code || Sign != other.Sign) { return false; }
        if (Kind != "key") { return true; }
        if (Shift == other.Shift && Ctrl == other.Ctrl && Alt == other.Alt && Meta == other.Meta) { return true; }
        int mine = (Shift ? 1 : 0) | (Ctrl ? 2 : 0) | (Alt ? 4 : 0) | (Meta ? 8 : 0);
        int theirs = (other.Shift ? 1 : 0) | (other.Ctrl ? 2 : 0) | (other.Alt ? 4 : 0) | (other.Meta ? 8 : 0);
        return movement && ((mine & theirs) == mine || (mine & theirs) == theirs);
    }
}
