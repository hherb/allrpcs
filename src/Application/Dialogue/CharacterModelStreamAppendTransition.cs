namespace Allrpcs.Application.Dialogue;

/// <summary>
/// Appends one exact provider-neutral update to a character-model stream prefix.
/// </summary>
public static class CharacterModelStreamAppendTransition
{
    /// <summary>
    /// Returns a fresh validated stream prefix retaining the supplied update last.
    /// </summary>
    public static CharacterModelStreamSnapshot Append(
        CharacterModelStreamUpdate update,
        CharacterModelStreamSnapshot snapshot)
    {
        ArgumentNullException.ThrowIfNull(update);
        ArgumentNullException.ThrowIfNull(snapshot);

        return new CharacterModelStreamSnapshot(snapshot.Updates.Append(update));
    }
}
