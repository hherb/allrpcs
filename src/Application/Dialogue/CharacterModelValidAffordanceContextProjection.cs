namespace Allrpcs.Application.Dialogue;

/// <summary>
/// Projects one exact validation snapshot into bounded provider-neutral affordance context.
/// </summary>
public static class CharacterModelValidAffordanceContextProjection
{
    /// <summary>
    /// Returns fresh entries containing only canonical intention type and target identifier text.
    /// </summary>
    public static IReadOnlyList<CharacterModelContextEntry> Project(
        CharacterIntentionAffordanceSnapshot affordances)
    {
        ArgumentNullException.ThrowIfNull(affordances);

        List<CharacterModelContextEntry> entries = new();
        foreach (CharacterIntentionAffordance affordance in affordances.Entries)
        {
            entries.Add(new CharacterModelContextEntry(
                CharacterModelContextEntryKind.ValidAffordance,
                $"type={affordance.Type.Value};target={affordance.TargetObjectId.Value}"));
        }

        return entries.AsReadOnly();
    }
}
