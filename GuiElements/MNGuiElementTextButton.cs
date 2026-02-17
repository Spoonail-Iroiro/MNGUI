using System.Collections.Generic;
using System.Linq;
using Vintagestory.API.Client;
using Vintagestory.API.Common;

namespace MNGui.GuiElements;

/// <summary>
/// A button GuiElement that allows the click handler to be assigned after construction
/// </summary>
public class MNGuiElementTextButton : GuiElementTextButton {
    /// <summary>
    /// Click event handler invoked instead of the vanilla onClick handler.
    /// </summary>
    public ActionConsumable? EventClicked { get; set; }

    public MNGuiElementTextButton(
            ICoreClientAPI capi,
            string text,
            ElementBounds bounds,
            CairoFont? font = null,
            CairoFont? hoverFont = null,
            EnumButtonStyle style = EnumButtonStyle.Normal,
            ActionConsumable? onClick = null
        ) : base(capi, text, font ?? CairoFont.ButtonText(), GetHoverFontAuto(hoverFont, font), () => true, bounds, style) {
        /*
         * GuiElementTextButton stores its click handler in a private field
         * that can only be assigned via the constructor.
         * 
         * To allow late binding of the handler, this class:
         * - passes a dummy handler to the base constructor
         * - intercepts input handling and invokes EventClicked instead
         */
        EventClicked = onClick;
    }

    protected static CairoFont GetHoverFontAuto(CairoFont? hoverFontSpecified, CairoFont? mainFont) {
        if (hoverFontSpecified != null) return hoverFontSpecified;

        if (mainFont == null) {
            mainFont = CairoFont.ButtonText();
        }

        return mainFont.Clone().WithColor(GuiStyle.ActiveButtonTextColor);
    }

    public override void OnKeyDown(ICoreClientAPI api, KeyEvent args) {
        // Overrides vanilla behavior to invoke EventClicked instead of the base handler

        if (!Visible) return;
        if (!HasFocus) return;

        if (args.KeyCode == (int)GlKeys.Enter) {
            args.Handled = true;
            if (enabled) {
                if (PlaySound) {
                    api.Gui.PlaySound("menubutton_press");
                }
                args.Handled = EventClicked?.Invoke() ?? false;
            }
        }
    }

    public override void OnMouseUpOnElement(ICoreClientAPI api, MouseEvent args) {
        var prevHandled = args.Handled;
        base.OnMouseUpOnElement(api, args);
        // If Handled changed to true, that means the dummy onClick was called, therefore EventClicked should be invoked
        if (!prevHandled && args.Handled) {
            args.Handled = EventClicked?.Invoke() ?? false;
        }
    }
}
