using Godot;

namespace Allrpcs.Game;

/// <summary>Fits the existing settings controls together without changing their owners or focus cycle.</summary>
public partial class GameplaySettingsLayout : CanvasLayer
{
    private const float Margin = 16;
    private const float Padding = 24;
    private const float SidebarWidth = 256;
    private const float ColumnGap = 16;
    private const float ModelHeight = 428;
    private const float HeaderHeight = 52;
    private ColorRect frame = null!;
    private Control model = null!;
    private Label heading = null!;
    private Control hud = null!;
    private Control saves = null!;
    private Button review = null!;
    private Vector2 fittedViewport;

    /// <inheritdoc />
    public override void _Ready()
    {
        model = GetNode<Control>("Panel");
        hud = GetNode<Control>("../CommandOverlay/Panel");
        saves = GetNode<Control>("../CommandOverlay/SaveLoadPanel");
        review = GetNode<Button>("../TemporaryCharacterDialogueMisquotationEvidenceReviewPanel/Review");
        frame = new ColorRect
        {
            Name = "SettingsFrame",
            Color = new Color(0.045f, 0.055f, 0.07f, 0.98f),
            MouseFilter = Control.MouseFilterEnum.Stop,
        };
        AddChild(frame);
        MoveChild(frame, 0);
        heading = new Label { Text = "Settings", MouseFilter = Control.MouseFilterEnum.Ignore };
        heading.AddThemeFontSizeOverride("font_size", 26);
        frame.AddChild(heading);
        GetNode<Button>("ControlsEntry").ClipText = true;
        GetNode<Label>("Panel/Title").Text = "Character model";
        foreach (string name in new[] { "Status", "RestartHint", "CredentialHint", "ConnectionStatus" })
        {
            GetNode<Label>("Panel/" + name).AutowrapMode = TextServer.AutowrapMode.WordSmart;
        }
        Fit();
    }

    /// <inheritdoc />
    public override void _Process(double delta)
    {
        _ = delta;
        if (Visible || fittedViewport != GetViewport().GetVisibleRect().Size) { Fit(); }
    }

    private void Fit()
    {
        Vector2 viewport = GetViewport().GetVisibleRect().Size;
        fittedViewport = viewport;
        float width = Math.Min(1120, viewport.X - 2 * Margin);
        float height = HeaderHeight + ModelHeight + Padding;
        float hudBottom = Math.Max(hud.GetGlobalRect().End.Y, saves.GetGlobalRect().End.Y);
        Vector2 reviewPosition = new(saves.GetGlobalRect().End.X + 12, saves.Position.Y);
        if (reviewPosition.X + review.Size.X > viewport.X - Margin)
        {
            reviewPosition = new(viewport.X - review.Size.X - Margin, hudBottom + Margin);
        }
        review.Position = reviewPosition;
        Vector2 origin = new((viewport.X - width) / 2,
            Math.Max(Margin, Math.Min(Math.Max((viewport.Y - height) / 2, hudBottom + Margin),
                viewport.Y - height - Margin)));
        frame.Position = origin;
        frame.Size = new(width, height);
        heading.Position = new(Padding, 10);
        heading.Size = new(width - 2 * Padding, 34);
        string[] entries = ["ControlsEntry", "AudioEntry", "DialogueEntry", "SettingsClose"];
        for (int i = 0; i < entries.Length; i++)
        {
            Place(GetNode<Control>(entries[i]), origin + new Vector2(Padding, HeaderHeight + i * 56),
                new Vector2(SidebarWidth, 44));
        }
        float modelWidth = width - 2 * Padding - SidebarWidth - ColumnGap;
        Place(model, origin + new Vector2(Padding + SidebarWidth + ColumnGap, HeaderHeight),
            new Vector2(modelWidth, ModelHeight));
        float inner = modelWidth - 24;
        Field("Title", 12, 8, inner, 24);
        Field("Provider", 12, 40, 184, 36);
        Field("ModelName", 204, 40, inner - 192, 36);
        Field("Apply", 12, 84, 96, 36);
        Field("Remove", 116, 84, 100, 36);
        Field("Save", 224, 84, 88, 36);
        Field("Activate", 320, 84, inner - 308, 36);
        Field("Status", 12, 128, inner, 24);
        Field("RestartHint", 12, 160, inner, 40);
        Field("OpenAiApiKey", 12, 208, inner, 36);
        Field("SaveOpenAiApiKey", 12, 252, 144, 36);
        Field("DeleteOpenAiApiKey", 164, 252, 156, 36);
        Field("CredentialStatus", 12, 296, inner, 24);
        Field("CredentialHint", 12, 328, inner, 40);
        Field("TestConnection", 12, 376, 184, 36);
        Field("ConnectionStatus", 204, 376, inner - 192, 36);
    }

    private void Field(string name, float x, float y, float width, float height) =>
        Place(model.GetNode<Control>(name), new Vector2(x, y), new Vector2(width, height));

    private static void Place(Control control, Vector2 position, Vector2 size)
    {
        control.Position = position;
        control.Size = size;
    }
}
