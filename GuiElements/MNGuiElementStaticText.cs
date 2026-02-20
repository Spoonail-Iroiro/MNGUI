using AirThermoMod.MNGui.Util;
using Cairo;
using MNGui.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using Vintagestory.API.Client;
using Vintagestory.API.MathTools;

namespace MNGui.GuiElements;
public class MNGuiElementStaticText : GuiElementTextBase {
    public EnumTextOrientation Orientation { get; set; }
    public Vec4d? BackgroundColor { get; set; }
    public bool AutoWrap { get; set; } = true;
    const double MaxLineLength = 16777216;

    public MNGuiElementStaticText(
            ICoreClientAPI capi,
            string text,
            ElementBounds bounds,
            CairoFont? font = null,
            EnumTextOrientation orientation = EnumTextOrientation.Left,
            Vec4d? backgroundColorRGBA = null,
            bool autoWrap = true
        ) : base(capi, text, font, bounds) {
        if (Font == null) Font = CairoFont.WhiteDetailText();

        this.Orientation = orientation;
        this.BackgroundColor = backgroundColorRGBA;
        this.AutoWrap = autoWrap;
    }


    public override void ComposeTextElements(Context ctx, ImageSurface surface) {
        Bounds.CalcWorldBounds();
        if (BackgroundColor != null) {
            // Background
            ctx.SetSourceRGBA(BackgroundColor.X, BackgroundColor.Y, BackgroundColor.Z, BackgroundColor.W);
            GuiElement.RoundRectangle(ctx, Bounds.bgDrawX, Bounds.bgDrawY, Bounds.OuterWidth, Bounds.OuterHeight, 1.0);
            ctx.Fill();
        }
        // Text
        textUtil.AutobreakAndDrawMultilineTextAt(ctx, Font, text, (int)(Bounds.drawX), (int)(Bounds.drawY), AutoWrap ? Bounds.InnerWidth : MaxLineLength, Orientation);
    }

    public MNGuiElementStaticText WithAutoBoxSize(bool onlyGrow = false) {
        Font.AutoBoxSize(text, Bounds, onlyGrow);
        return this;
    }

    public MNGuiElementStaticText WithAutoFontSize(bool onlyShrink = true) {
        Bounds.CalcWorldBounds();
        Font.AutoFontSizeMN(text, Bounds, onlyShrink);
        return this;
    }

}
