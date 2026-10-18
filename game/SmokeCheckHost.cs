using Godot;

namespace Allrpcs.Game;

/// <summary>
/// Attaches only the engine checks a launch requests, so the shipping Main scene holds none. An
/// entry is requested when any of its gates is set, and the check still evaluates its own gate
/// value. Entries keep their former Main.tscn order and index (after the named sibling), so process
/// and input order are unchanged. They attach before Main's children are ready; Godot readies
/// children in insertion order, so a hosted check readies after Main's scene children and before
/// Main itself. A fixture that other checks or proofs reach also lists their gates.
/// </summary>
internal static class SmokeCheckHost
{
    private static readonly Entry[] Entries =
    [
        Check<AdaptiveDisplaySmokeCheck>("AdaptiveDisplay", "ALLRPCS_ADAPTIVE_DISPLAY_CHECK"),
        Check<OptionalOverlayVisibilitySmokeCheck>(
            "OptionalOverlayVisibilityController", "ALLRPCS_OPTIONAL_OVERLAY_VISIBILITY_CHECK"),
        Check<NavigationSmokeCheck>("GreyboxNavigation", "ALLRPCS_NAVIGATION_CHECK"),
        Check<SeekerCustomizationMenuSmokeCheck>(
            "SeekerBackgroundRouteAccessSceneSource", "ALLRPCS_MODAL_INPUT_CHECK",
            "ALLRPCS_SEEKER_CUSTOMIZATION_MENU_CHECK"),
        Check<SaveDocumentStorageControllerSmokeCheck>(
            "CharacterModelGenerationFlowController", SaveDocumentStorageController.SmokeCheckEnvironmentVariable),
        Check<LiveCharacterModelOrdinaryFlowSmokeCheck>(
            "TemporaryCharacterTurnCompletionController", "ALLRPCS_LIVE_OLLAMA_ORDINARY_ACTION",
            "ALLRPCS_LIVE_OLLAMA_ORDINARY_WAIT", "ALLRPCS_LIVE_OPENAI_ORDINARY_ACTION",
            "ALLRPCS_LIVE_OPENAI_ORDINARY_WAIT"),
        Check<RuntimeWoodlandClearingDressingSmokeCheck>(
            "WoodlandClearingDressingPresentation", "ALLRPCS_RUNTIME_WOODLAND_CLEARING_DRESSING_CHECK"),
        Scene("ControlSettingsMainSmokeCheck", "ControlSettingsPanel", "ALLRPCS_CONTROLS_MAIN_CHECK"),
    ];

    /// <summary>Called from <see cref="Main._EnterTree"/>, while Main's children are not yet ready.</summary>
    internal static void Attach(Main main)
    {
        // Exports exclude every harness script and scene.
        if (OS.HasFeature("template"))
        {
            return;
        }

        // Reverse order keeps entries that follow the same sibling in their former order.
        for (int index = Entries.Length - 1; index >= 0; index--)
        {
            Entry entry = Entries[index];
            if (main.HasNode(entry.Name) || !entry.Gates.Any(gate => OS.GetEnvironment(gate).Length > 0))
            {
                continue;
            }

            Node check = entry.Create();
            check.Name = entry.Name;
            main.AddChild(check);
            main.MoveChild(check, entry.After is null ? 0 : main.GetNode(entry.After).GetIndex() + 1);
        }
    }

    private static Entry Check<T>(string? after, params string[] gates)
        where T : Node, new()
    {
        return new(typeof(T).Name, () => new T(), after, gates);
    }

    private static Entry Scene(string name, string? after, params string[] gates)
    {
        return new(name, () => GD.Load<PackedScene>($"res://{name}.tscn").Instantiate(), after, gates);
    }

    private sealed record Entry(string Name, Func<Node> Create, string? After, string[] Gates);
}
