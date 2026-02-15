using Cairo;
using System;
using System.Collections.Generic;
using System.Linq;
using Vintagestory.API.Client;

namespace MNGui.GuiElements;

using DrawHandler = Action<Context, ImageSurface, ElementBounds>;

// If dynamic CustomDraw is required, use GuiElementCustomDraw
public class MNGuiElementStaticCustomDraw : MNGuiElementStaticBase {
    DrawHandler? EventDraw { get; set; }

    public MNGuiElementStaticCustomDraw(ICoreClientAPI capi, ElementBounds bounds, DrawHandler handler = null) : base(capi, bounds) {
        EventDraw = handler;
    }

    public override void ComposeElements(Context ctxStatic, ImageSurface surfaceStatic) {
        this.Bounds.CalcWorldBounds();
        this.EventDraw?.Invoke(ctxStatic, surfaceStatic, this.Bounds);
        return;
    }
}
