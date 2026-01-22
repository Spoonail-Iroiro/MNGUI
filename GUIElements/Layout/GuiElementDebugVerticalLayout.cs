using Cairo;
using System.Collections.Generic;
using System.Linq;
using Vintagestory.API.Client;
using Vintagestory.API.MathTools;

namespace MNGUITest.MNGUI.GUIElements.Layout;
public class GuiElementDebugVerticalLayout : GuiElement {
    public GuiElementDebugVerticalLayout(ICoreClientAPI capi, ElementBounds bounds) : base(capi, bounds) {
    }

    public override int OutlineColor() {
        var intVal = ColorUtil.ToRgba(255, 0, 255, 255);
        //var intVal = ColorUtil.ColorFromRgba(128, 255, 255, 255);
        return intVal;
    }

    public override void ComposeElements(Context ctxStatic, ImageSurface surface) {
        Bounds.CalcWorldBounds();
    }


}
