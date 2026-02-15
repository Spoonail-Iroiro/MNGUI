using System.Collections.Generic;
using System.Linq;
using Vintagestory.API.Client;
using Vintagestory.API.MathTools;

namespace MNGui.GuiElements;
public class MNGuiElementStaticBase : GuiElement {
    public MNGuiElementStaticBase(ICoreClientAPI capi, ElementBounds bounds) : base(capi, bounds) {
    }

    // Never respond to mouse event
    public override void OnMouseDown(ICoreClientAPI api, MouseEvent mouse) {
    }

    public override void OnMouseUp(ICoreClientAPI api, MouseEvent args) {
    }
}
