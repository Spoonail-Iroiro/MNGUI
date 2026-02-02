using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Vintagestory.API.Client;
using Vintagestory.API.Common;

namespace MNGui.Std;
public static class BoundsStd {
    public static ElementBounds FitToChildren() {
        return new ElementBounds().WithSizing(ElementSizing.FitToChildren);
    }
}
