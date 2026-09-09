using Allrpcs.Domain.Dialogue;

namespace Allrpcs.Application.Dialogue;

/// <summary>
/// Retains one exact non-empty text fragment streamed by a character model.
/// </summary>
public sealed record CharacterModelTextDelta : CharacterModelStreamUpdate
{
    /// <summary>
    /// Creates an immutable streamed text fragment without interpreting or normalizing it.
    /// </summary>
    public CharacterModelTextDelta(string text)
    {
        ArgumentNullException.ThrowIfNull(text);

        if (text.Length == 0 || text.Length > DialogueUtterance.MaximumTextLength)
        {
            throw new ArgumentException(
                $"A model text delta must contain between 1 and "
                    + $"{DialogueUtterance.MaximumTextLength} UTF-16 code units.",
                nameof(text));
        }

        Text = text;
    }

    /// <summary>
    /// Gets the exact streamed text fragment as supplied.
    /// </summary>
    public string Text { get; }
}
