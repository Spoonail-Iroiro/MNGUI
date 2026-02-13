using Cairo;
using System.Collections.Generic;
using System.Linq;
using Vintagestory.API.Client;
using Vintagestory.API.MathTools;

namespace MNGui.GuiElements;

class MNGuiElementClipStart : GuiElement {
    public MNGuiElementClipStart(ICoreClientAPI capi, ElementBounds bounds) : base(capi, bounds) {
    }

    public override void ComposeElements(Context ctxStatic, ImageSurface surface) {
        Bounds.CalcWorldBounds();
    }

    public override void RenderInteractiveElements(float deltaTime) {
        api.Render.PushScissor(Bounds, true);
    }

    public override int OutlineColor() {
        var intVal = ColorUtil.ToRgba(255, 255, 0, 0);
        return intVal;
    }

    // Never respond to mouse event
    public override void OnMouseDown(ICoreClientAPI api, MouseEvent mouse) {
    }

    public override void OnMouseUp(ICoreClientAPI api, MouseEvent args) {
    }
}

class MNGuiElementClipEnd : GuiElement {
    public MNGuiElementClipEnd(ICoreClientAPI capi) : base(capi, ElementBounds.FixedSize(1, 1)) {
    }

    public override void ComposeElements(Context ctxStatic, ImageSurface surface) {
    }

    public override void RenderInteractiveElements(float deltaTime) {
        api.Render.PopScissor();
    }

    // Never respond to mouse event
    public override void OnMouseDown(ICoreClientAPI api, MouseEvent mouse) {
    }

    public override void OnMouseUp(ICoreClientAPI api, MouseEvent args) {
    }
}
