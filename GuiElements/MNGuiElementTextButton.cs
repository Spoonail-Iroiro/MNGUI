using Cairo;
using System;
using System.Collections.Generic;
using System.Linq;
using Vintagestory.API.Client;
using Vintagestory.API.Common;

namespace MNGui.GuiElements;
/// <summary>
/// Text button with proper default args and pressed handler
/// </summary>
/// <remarks>
/// Wraps GuiElementTextButton BECAUSE the handler is private and can be set in the constructor
/// </remarks>
public class MNGuiElementTextButton : GuiElementControl {
    public override bool Enabled {
        get => InnerButton.Enabled;
        set => InnerButton.Enabled = value;
    }

    public GuiElementTextButton InnerButton { get; private set; }

    public ActionConsumable? EventClicked { get; set; }

    public MNGuiElementTextButton(
            ICoreClientAPI capi,
            string text,
            ElementBounds bounds,
            CairoFont? font = null,
            CairoFont? hoverFont = null,
            EnumButtonStyle style = EnumButtonStyle.Normal,
            ActionConsumable? onClick = null
        )
        : base(capi, bounds) {

        if (font == null) {
            font = CairoFont.ButtonText();
        }

        if (hoverFont == null) {
            hoverFont = font.Clone().WithColor(GuiStyle.ActiveButtonTextColor);
        }

        InnerButton = new GuiElementTextButton(capi, text, font, hoverFont, InternalOnClick, bounds, style);

        EventClicked = onClick;
    }

    private bool InternalOnClick() {
        return EventClicked?.Invoke() ?? false;
    }

    #region events (handled by InnerButton) 

    public override void BeforeCalcBounds() {
        InnerButton.BeforeCalcBounds();
    }

    public override void ComposeElements(Context ctx, ImageSurface surface) {
        InnerButton.ComposeElements(ctx, surface);
    }

    public override void RenderInteractiveElements(float deltaTime) {
        InnerButton.RenderInteractiveElements(deltaTime);
    }

    public override void OnKeyDown(ICoreClientAPI api, KeyEvent args) {
        InnerButton.OnKeyDown(api, args);
    }

    public override void OnMouseMove(ICoreClientAPI api, MouseEvent args) {
        InnerButton.OnMouseMove(api, args);
    }

    public override void OnMouseDownOnElement(ICoreClientAPI api, MouseEvent args) {
        InnerButton.OnMouseDownOnElement(api, args);
    }

    public override void OnMouseUp(ICoreClientAPI api, MouseEvent args) {
        InnerButton.OnMouseUp(api, args);
    }

    public override void OnMouseUpOnElement(ICoreClientAPI api, MouseEvent args) {
        InnerButton.OnMouseUpOnElement(api, args);
    }

    public override void Dispose() {
        InnerButton.Dispose();
        base.Dispose();
    }

    #endregion
}
