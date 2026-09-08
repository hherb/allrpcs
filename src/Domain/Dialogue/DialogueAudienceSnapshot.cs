using Allrpcs.Domain.Identifiers;

namespace Allrpcs.Domain.Dialogue;

/// <summary>
/// Associates one exact speaker with the stable, bounded set of characters who heard them.
/// </summary>
public sealed class DialogueAudienceSnapshot
{
    /// <summary>
    /// Gets the maximum number of listeners retained for one dialogue utterance.
    /// </summary>
    public const int MaximumListenerCount = 32;

    /// <summary>
    /// Creates an immutable audience snapshot in the supplied stable listener order.
    /// </summary>
    public DialogueAudienceSnapshot(
        CharacterId speakerCharacterId,
        IEnumerable<CharacterId> listenerCharacterIds)
    {
        ArgumentNullException.ThrowIfNull(speakerCharacterId);
        ArgumentNullException.ThrowIfNull(listenerCharacterIds);

        List<CharacterId> retainedListeners = new();
        HashSet<CharacterId> identifiers = new() { speakerCharacterId };

        foreach (CharacterId listenerCharacterId in listenerCharacterIds)
        {
            if (listenerCharacterId is null)
            {
                throw new ArgumentException(
                    "Listener identifiers cannot contain null values.",
                    nameof(listenerCharacterIds));
            }

            if (!identifiers.Add(listenerCharacterId))
            {
                throw new ArgumentException(
                    "Listener identifiers cannot duplicate the speaker or another listener.",
                    nameof(listenerCharacterIds));
            }

            retainedListeners.Add(listenerCharacterId);

            if (retainedListeners.Count > MaximumListenerCount)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(listenerCharacterIds),
                    retainedListeners.Count,
                    $"Dialogue audiences accept at most {MaximumListenerCount} listeners.");
            }
        }

        SpeakerCharacterId = speakerCharacterId;
        ListenerCharacterIds = retainedListeners.AsReadOnly();
    }

    /// <summary>
    /// Gets the exact stable identifier of the speaking character.
    /// </summary>
    public CharacterId SpeakerCharacterId { get; }

    /// <summary>
    /// Gets listener identifiers in stable supplied order.
    /// </summary>
    public IReadOnlyList<CharacterId> ListenerCharacterIds { get; }
}
