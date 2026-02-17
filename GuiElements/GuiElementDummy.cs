using Cairo;
using System.Collections.Generic;
using System.Linq;
using Vintagestory.API.Client;

namespace MNGui.GuiElements;

// "Transparent" element like GuiElementParent, but never consume mouse event
public class GuiElementDummy : MNGuiElementStaticBase {
    public GuiElementDummy(ICoreClientAPI capi, ElementBounds bounds) : base(capi, bounds) {
    }

    public override void ComposeElements(Context ctxStatic, ImageSurface surface) {
        Bounds.CalcWorldBounds();
    }
}
