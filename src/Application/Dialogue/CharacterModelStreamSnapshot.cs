using Allrpcs.Domain.Dialogue;

namespace Allrpcs.Application.Dialogue;

/// <summary>
/// Retains a bounded ordered prefix of one character-model response stream.
/// </summary>
public sealed class CharacterModelStreamSnapshot
{
    /// <summary>
    /// Creates a stable immutable stream prefix from exact provider-neutral updates.
    /// </summary>
    public CharacterModelStreamSnapshot(IEnumerable<CharacterModelStreamUpdate> updates)
    {
        ArgumentNullException.ThrowIfNull(updates);

        List<CharacterModelStreamUpdate> retainedUpdates = new();
        int textLength = 0;
        bool terminated = false;

        foreach (CharacterModelStreamUpdate update in updates)
        {
            if (update is null)
            {
                throw new ArgumentException(
                    "Character-model stream updates cannot contain null values.",
                    nameof(updates));
            }

            if (terminated)
            {
                throw new ArgumentException(
                    "A character-model stream cannot contain updates after termination.",
                    nameof(updates));
            }

            if (update is CharacterModelTextDelta delta)
            {
                textLength += delta.Text.Length;
                if (textLength > DialogueUtterance.MaximumTextLength)
                {
                    throw new ArgumentException(
                        $"Combined model text cannot exceed "
                            + $"{DialogueUtterance.MaximumTextLength} UTF-16 code units.",
                        nameof(updates));
                }
            }
            else if (update is CharacterModelStreamTermination)
            {
                terminated = true;
            }
            else
            {
                throw new ArgumentException(
                    "A character-model stream contains an unknown update type.",
                    nameof(updates));
            }

            retainedUpdates.Add(update);
        }

        Updates = retainedUpdates.AsReadOnly();
    }

    /// <summary>
    /// Gets exact stream updates in stable supplied order.
    /// </summary>
    public IReadOnlyList<CharacterModelStreamUpdate> Updates { get; }
}
