using Cairo;
using System.Collections.Generic;
using System.Linq;
using Vintagestory.API.Client;
using Vintagestory.API.MathTools;

namespace MNGui.GuiElements.Layout;
public class GuiElementDebugHorizontalLayout : GuiElement {
    public GuiElementDebugHorizontalLayout(ICoreClientAPI capi, ElementBounds bounds) : base(capi, bounds) {
    }

    public override int OutlineColor() {
        var intVal = ColorUtil.ToRgba(255, 255, 255, 0);
        return intVal;
    }

    public override void ComposeElements(Context ctxStatic, ImageSurface surface) {
        Bounds.CalcWorldBounds();
    }

    // Never respond to mouse event
    public override void OnMouseDown(ICoreClientAPI api, MouseEvent mouse) {
    }

    public override void OnMouseUp(ICoreClientAPI api, MouseEvent args) {
    }
}
