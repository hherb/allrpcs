namespace Allrpcs.Application.Dialogue;

/// <summary>
/// Provides one bounded player-visible text projection of exact character-model stream deltas.
/// </summary>
public sealed class CharacterModelStreamTextPresentation
{
    private CharacterModelStreamTextPresentation(string text)
    {
        Text = text;
    }

    /// <summary>Gets the canonical presentation for a stream with no display text.</summary>
    public static CharacterModelStreamTextPresentation Empty { get; } = new(string.Empty);

    /// <summary>Gets the exact concatenated stream-delta text.</summary>
    public string Text { get; }

    /// <summary>Gets whether any stream-delta text is available for display.</summary>
    public bool HasText => Text.Length > 0;

    /// <summary>
    /// Projects only exact text deltas without retaining the stream or inspecting terminal data.
    /// </summary>
    public static CharacterModelStreamTextPresentation Project(
        CharacterModelStreamSnapshot snapshot)
    {
        ArgumentNullException.ThrowIfNull(snapshot);

        string text = string.Concat(
            snapshot.Updates
                .OfType<CharacterModelTextDelta>()
                .Select(delta => delta.Text));
        return text.Length == 0 ? Empty : new CharacterModelStreamTextPresentation(text);
    }

    /// <inheritdoc />
    public override string ToString() => Text;
}
