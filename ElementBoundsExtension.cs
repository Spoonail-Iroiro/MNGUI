using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Vintagestory.API.Client;
using Vintagestory.API.Config;

namespace RemoteTraderCheckMod.GUI.MNGui {
    internal static class ElementBoundsExtension {
        public static void FitToChildrenFixedRightOf(this ElementBounds elementBounds, ElementBounds refBounds, double leftSpacing = 0.0) {
            refBounds.CalcWorldBounds();
            // Hacky, make it (absFixedX + OuterWidth + leftSpacing / scale) after scaled
            elementBounds.fixedX = (refBounds.absFixedX * 1.0 / (double)RuntimeEnv.GUIScale) + (refBounds.OuterWidth * 1.0 / (double)RuntimeEnv.GUIScale) + leftSpacing;
        }

        public static void FitToChildrenFixedUnder(this ElementBounds elementBounds, ElementBounds refBounds, double upSpacing = 0.0) {
            refBounds.CalcWorldBounds();
            // Hacky, make it (absFixedY + OuterHeight + upSpacing / scale) after scaled
            elementBounds.fixedY = (refBounds.absFixedY * 1.0 / (double)RuntimeEnv.GUIScale) + (refBounds.OuterHeight * 1.0 / (double)RuntimeEnv.GUIScale) + upSpacing;
        }
    }
}
