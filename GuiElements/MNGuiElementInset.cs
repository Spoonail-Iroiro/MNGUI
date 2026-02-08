using Cairo;
using System.Collections.Generic;
using System.Linq;
using Vintagestory.API.Client;

namespace MNGui.GuiElements;

class MNGuiElementInset : GuiElement {
    int depth;
    float brightness;

    public MNGuiElementInset(ICoreClientAPI capi, ElementBounds bounds, int depth = 4, float brightness = 0.85f) : base(capi, bounds) {
        this.depth = depth;
        this.brightness = brightness;
    }

    public override void ComposeElements(Context ctx, ImageSurface surface) {
        Bounds.CalcWorldBounds();

        if (brightness < 1) {
            ctx.SetSourceRGBA(0, 0, 0, 1 - brightness);
            Rectangle(ctx, Bounds);
            ctx.Fill();
        }

        EmbossRoundRectangleElement(ctx, Bounds, true, depth);
    }
}

