namespace Allrpcs.Application.Speech;

/// <summary>Appends recognition to an initial draft and protects any subsequent manual correction.</summary>
public sealed class EditableSpeechDraft
{
    private readonly string initial;
    private readonly List<string> order = [];
    private readonly Dictionary<string, string> lines = [];
    private readonly HashSet<string> final = [];
    /// <summary>Retains existing typed text exactly, with bounded recognition appended afterward.</summary>
    public EditableSpeechDraft(string initial)
    { ArgumentNullException.ThrowIfNull(initial); this.initial = initial; Text = initial; }
    /// <summary>Gets the last recognition-owned draft.</summary>
    public string Text { get; private set; }
    /// <summary>Gets whether a manual edit has permanently fenced this session's automatic writes.</summary>
    public bool ManuallyEdited { get; private set; }
    /// <summary>Fences recognition as soon as the player changes the displayed draft.</summary>
    public void Edited(string text) { if (text != Text) { ManuallyEdited = true; } }
    /// <summary>Updates a draft only; finals remain stable and callbacks never submit anything.</summary>
    public bool Apply(SpeechRecognitionEvent value)
    {
        ArgumentNullException.ThrowIfNull(value);
        if (ManuallyEdited || value.Kind is not (SpeechRecognitionKind.Partial or SpeechRecognitionKind.Final)
            || final.Contains(value.UtteranceId)) { return false; }
        if (!lines.ContainsKey(value.UtteranceId)) { order.Add(value.UtteranceId); }
        lines[value.UtteranceId] = value.Text.Trim();
        if (value.Kind == SpeechRecognitionKind.Final) { final.Add(value.UtteranceId); }
        string spoken = string.Join(" ", order.Select(id => lines[id]).Where(text => text.Length > 0));
        string candidate = initial + (initial.Length > 0 && spoken.Length > 0 && !char.IsWhiteSpace(initial[^1]) ? " " : "") + spoken;
        if (candidate.Length > 2000) { throw new InvalidOperationException("Recognition draft exceeded its bound."); }
        bool changed = candidate != Text; Text = candidate; return changed;
    }
}
