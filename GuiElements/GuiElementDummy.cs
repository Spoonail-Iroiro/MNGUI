using Cairo;
using System.Collections.Generic;
using System.Linq;
using Vintagestory.API.Client;

namespace MNGui.GuiElements;

// "Transparent" element like GuiElementParent, but never consume mouse event
public class GuiElementDummy : GuiElement {
    public GuiElementDummy(ICoreClientAPI capi, ElementBounds bounds) : base(capi, bounds) {
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
