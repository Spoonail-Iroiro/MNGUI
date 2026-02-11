using System.Collections.Generic;
using System.Linq;
using MNGui.GuiElements;
using MNGui.Layouts;
using Vintagestory.API.Client;

namespace MNGui.Std;
public class LayoutStd {
    ICoreClientAPI capi;
    public LayoutStd(ICoreClientAPI capi) {
        this.capi = capi;
    }

}
