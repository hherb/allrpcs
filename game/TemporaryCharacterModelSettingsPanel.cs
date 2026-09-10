using Allrpcs.Application.Dialogue;
using Allrpcs.Infrastructure.Dialogue;
using Godot;

namespace Allrpcs.Game;

/// <summary>Applies and explicitly saves secret-free provider and model settings.</summary>
public partial class TemporaryCharacterModelSettingsPanel : Control
{
    /// <summary>Displayed after a valid setting is applied only in memory.</summary>
    public const string AppliedStatusText = "Provider settings: APPLIED · SAVE TO KEEP";

    /// <summary>Displayed after current settings are explicitly saved.</summary>
    public const string SavedStatusText = "Provider settings: SAVED · RESTART TO APPLY";

    /// <summary>Displayed after provider or model input is rejected.</summary>
    public const string RejectedStatusText = "Provider settings: REJECTED";

    /// <summary>Displayed after one unselected provider setting is removed in memory.</summary>
    public const string RemovedStatusText = "Provider settings: REMOVED · SAVE TO KEEP";

    /// <summary>Displayed when no initialized settings can be saved.</summary>
    public const string SaveFailedStatusText = "Provider settings: SAVE FAILED";

    private const string EmptyStatusText = "Provider settings: NONE";
    private const string LoadedStatusText = "Provider settings: LOADED";

    private static readonly string[] ProviderLabels = ["OpenAI", "Anthropic", "Ollama"];

    private CharacterModelSettingsController controller = null!;
    private OptionButton provider = null!;
    private LineEdit modelName = null!;
    private Label status = null!;

    /// <inheritdoc />
    public override void _Ready()
    {
        controller = GetNode<CharacterModelSettingsController>(
            "../../CharacterModelSettingsController");
        provider = GetNode<OptionButton>("Provider");
        modelName = GetNode<LineEdit>("ModelName");
        status = GetNode<Label>("Status");

        PopulateProviderChoices();
        PresentCurrent();

        provider.ItemSelected += OnProviderSelected;
        GetNode<Button>("Apply").Pressed += OnApplyPressed;
        GetNode<Button>("Remove").Pressed += OnRemovePressed;
        GetNode<Button>("Save").Pressed += OnSavePressed;
    }

    private void PopulateProviderChoices()
    {
        if (ProviderLabels.Length != CharacterModelProvider.All.Count)
        {
            throw new InvalidOperationException(
                "Every canonical character-model provider requires one display label.");
        }

        provider.AddItem("Choose provider");
        provider.SetItemDisabled(0, true);
        foreach (string label in ProviderLabels)
        {
            provider.AddItem(label);
        }

        provider.Select(-1);
    }

    private void PresentCurrent()
    {
        CharacterModelSettingsSelectionSnapshot? snapshot = controller.Snapshot;
        if (snapshot is null)
        {
            status.Text = EmptyStatusText;
            return;
        }

        int providerIndex = CharacterModelProvider.All
            .Select((candidate, index) => (candidate, index))
            .Single(pair => ReferenceEquals(pair.candidate, snapshot.SelectedProvider))
            .index;
        CharacterModelSettings selected = snapshot.Settings.Single(
            settings => ReferenceEquals(settings.Provider, snapshot.SelectedProvider));
        provider.Select(providerIndex + 1);
        modelName.Text = selected.ModelName;
        status.Text = LoadedStatusText;
    }

    private void OnProviderSelected(long selectedIndex)
    {
        CharacterModelProvider? selectedProvider = ProviderAt(selectedIndex);
        CharacterModelSettings? settings = controller.Snapshot?.Settings.SingleOrDefault(
            candidate => ReferenceEquals(candidate.Provider, selectedProvider));
        modelName.Text = settings?.ModelName ?? string.Empty;
    }

    private void OnApplyPressed()
    {
        CharacterModelProvider? selectedProvider = ProviderAt(provider.Selected);
        if (selectedProvider is null)
        {
            status.Text = RejectedStatusText;
            return;
        }

        try
        {
            CharacterModelSettings settings = new(selectedProvider, modelName.Text);
            bool applied = controller.Snapshot is null
                ? controller.TryInitialize(
                    new CharacterModelSettingsSelectionSnapshot([settings], selectedProvider))
                : controller.TryApply(settings);
            status.Text = applied ? AppliedStatusText : RejectedStatusText;
        }
        catch (ArgumentException)
        {
            status.Text = RejectedStatusText;
        }
    }

    private void OnSavePressed()
    {
        status.Text = controller.SaveCurrent() == CharacterModelSettingsDocumentWriteResult.Saved
            ? SavedStatusText
            : SaveFailedStatusText;
    }

    private void OnRemovePressed()
    {
        CharacterModelProvider? selectedProvider = ProviderAt(provider.Selected);
        if (selectedProvider is null || !controller.TryRemove(selectedProvider))
        {
            status.Text = RejectedStatusText;
            return;
        }

        modelName.Clear();
        status.Text = RemovedStatusText;
    }

    private static CharacterModelProvider? ProviderAt(long selectedIndex)
    {
        long providerIndex = selectedIndex - 1;
        return providerIndex >= 0 && providerIndex < CharacterModelProvider.All.Count
            ? CharacterModelProvider.All[(int)providerIndex]
            : null;
    }
}
