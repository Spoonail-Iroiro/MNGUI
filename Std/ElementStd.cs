using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Vintagestory.API.Client;
using Vintagestory.API.Common;

namespace MNGui.Std;

internal class ElementStd {
    ICoreClientAPI capi;
    public ElementStd(ICoreClientAPI capi) {
        this.capi = capi;
    }

    public GuiElementStaticText TextAutoBoxSize(
        string text,
        EnumTextOrientation orientation = EnumTextOrientation.Left,
        CairoFont? font = null
    ) {
        if (font == null) {
            font = CairoFont.WhiteDetailText();
        }

        var textBounds = ElementBounds.Fixed(0, 0, 1, 1);
        var element = new GuiElementStaticText(
            capi,
            text,
            orientation,
            textBounds,
            font
        );
        element.AutoBoxSize();

        return element;
    }

    [Obsolete("Use MNElementAutoFontSizeStaticText")]
    public GuiElementStaticText StandardTextAutoFontSize(
        string text,
        ElementBounds bounds,
        bool onlyShrink = false,
        EnumTextOrientation orientation = EnumTextOrientation.Left,
        CairoFont? font = null
    ) {
        throw new NotImplementedException();
    }

    public GuiElementRichtext StandardRichText(
        string vtml,
        double width,
        CairoFont? font = null
    ) {
        if (font == null) font = CairoFont.WhiteDetailText();
        var richText = new GuiElementRichtext(
            capi,
            VtmlUtil.Richtextify(capi, vtml, font),
            ElementBounds.FixedSize(width, 10) // height will be auto-sized
        );

        return richText;
    }


    // Use ElementBounds.FixedSize(width, height)
    //public static ElementBounds ElementBoundsWH(double width, double height) {
    //    return ElementBounds.Fixed(0, 0, width, height);
    //}

}
