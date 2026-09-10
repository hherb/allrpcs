using Allrpcs.Application.Dialogue;

namespace Allrpcs.Infrastructure.Dialogue;

/// <summary>Contains one explicit provider-settings storage and materialization outcome.</summary>
public sealed record CharacterModelSettingsDocumentLoadResult
{
    private CharacterModelSettingsDocumentLoadResult(
        CharacterModelSettingsDocumentLoadStatus status,
        CharacterModelSettingsSelectionSnapshot? selection)
    {
        if ((status == CharacterModelSettingsDocumentLoadStatus.Loaded) != (selection is not null))
        {
            throw new ArgumentException(
                "Exactly one loaded result can contain a settings selection.",
                nameof(selection));
        }

        Status = status;
        Selection = selection;
    }

    /// <summary>Gets the explicit outcome of storage and materialization.</summary>
    public CharacterModelSettingsDocumentLoadStatus Status { get; }

    /// <summary>Gets the exact materialized selection, or null when none was returned.</summary>
    public CharacterModelSettingsSelectionSnapshot? Selection { get; }

    /// <summary>Creates a successful result retaining the exact materialized selection.</summary>
    public static CharacterModelSettingsDocumentLoadResult Loaded(
        CharacterModelSettingsSelectionSnapshot selection)
    {
        ArgumentNullException.ThrowIfNull(selection);
        return new(CharacterModelSettingsDocumentLoadStatus.Loaded, selection);
    }

    /// <summary>Creates a result reporting that no stored document exists.</summary>
    public static CharacterModelSettingsDocumentLoadResult Missing() =>
        new(CharacterModelSettingsDocumentLoadStatus.Missing, null);

    /// <summary>Creates a result reporting that storage could not return a document.</summary>
    public static CharacterModelSettingsDocumentLoadResult StorageFailed() =>
        new(CharacterModelSettingsDocumentLoadStatus.StorageFailed, null);

    /// <summary>Creates a result reporting that the stored document is unsupported or malformed.</summary>
    public static CharacterModelSettingsDocumentLoadResult InvalidDocument() =>
        new(CharacterModelSettingsDocumentLoadStatus.InvalidDocument, null);
}
