using System.Collections.Generic;
using System.Linq;
using Vintagestory.API.Client;
using Vintagestory.API.Common;

namespace MNGui.GuiElements;

public class MNGuiElementTextButton : GuiElementTextButton {
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
        // Overwrites base implemantation
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
        // If Handled changed to true, the handler to the original called
        if (!prevHandled && args.Handled) {
            args.Handled = EventClicked?.Invoke() ?? false;
        }
    }
}
