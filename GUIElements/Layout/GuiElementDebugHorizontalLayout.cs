using Cairo;
using System.Collections.Generic;
using System.Linq;
using Vintagestory.API.Client;
using Vintagestory.API.MathTools;

namespace MNGUI.GUIElements.Layout;
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


}
