using Allrpcs.Domain.Characters;

namespace Allrpcs.Game;

/// <summary>Exposes one action-specific author's exact current request output.</summary>
public interface ICharacterSabotageActionRequestAuthor
{
    /// <summary>Gets the author's exact current request, or null when none was authored.</summary>
    CharacterSabotageActionConsiderationRequest? Request { get; }
}
