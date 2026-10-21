using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Identifiers;
using Allrpcs.Domain.Persistence;

namespace Allrpcs.Application.Characters;

/// <summary>Decides how a saved Seeker profile restores into the current session's selection.</summary>
public static class SeekerCustomizationRestorationPolicy
{
    /// <summary>
    /// Returns false when the save belongs to a different, already customized Seeker, names an
    /// unknown option, or names a Seeker other than <paramref name="seekerId"/>. Otherwise <paramref name="selectionToApply"/> is the selection to install,
    /// or null to keep the session as it is (nothing saved, or the same profile). The applied
    /// identity reuses the live <paramref name="seekerId"/> instance the scene owners compare.
    /// </summary>
    public static bool TryDecide(
        SeekerCustomizationSelectionState current,
        SeekerCustomizationSaveState? saved,
        CharacterId seekerId,
        out SeekerCustomizationSelectionState? selectionToApply)
    {
        ArgumentNullException.ThrowIfNull(current);
        ArgumentNullException.ThrowIfNull(seekerId);

        selectionToApply = null;
        if (saved is null)
        {
            return true;
        }

        if (current.SelectedProfile is { } selected)
        {
            return saved.Matches(selected);
        }

        SeekerAppearanceChoice? appearance = current.Options.AppearanceChoices
            .SingleOrDefault(choice => choice.Key == saved.AppearanceKey);
        SeekerBackgroundChoice? background = current.Options.BackgroundChoices
            .SingleOrDefault(choice => choice.Key == saved.BackgroundKey);
        if (appearance is null || background is null || saved.Identity.Id != seekerId)
        {
            return false;
        }

        selectionToApply = SeekerCustomizationSelectionStateTransition.Select(
            current,
            new CharacterIdentity(seekerId, saved.Identity.DisplayName),
            saved.Pronouns,
            appearance,
            background);
        return true;
    }
}
