using MNGUI.Layouts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vintagestory.API.Client;

namespace MNGUI.GUIElements;

internal class MNElementAutoFontSizeStaticText : SingleLayout {

    public MNElementAutoFontSizeStaticText(
        ICoreClientAPI capi,
        string text,
        ElementBounds bounds,
        bool onlyShrink = false,
        EnumTextOrientation orientation = EnumTextOrientation.Left,
        CairoFont font = null
    ) : base(CreateGuiElement(capi, text, bounds, onlyShrink, orientation, font)) {
    }

    public override void BeforeComposerCompose() {
        (GuiElement as GuiElementStaticText).AutoFontSize();
    }

    protected static GuiElement CreateGuiElement(
        ICoreClientAPI capi,
        string text,
        ElementBounds bounds,
        bool onlyShrink,
        EnumTextOrientation orientation,
        CairoFont font
    ) {
        if (font == null) {
            font = CairoFont.WhiteDetailText();
        }

        var element = new GuiElementStaticText(
            capi,
            text,
            orientation,
            bounds,
            font
        );
        //element.AutoFontSize(onlyShrink);

        return element;
    }
}
