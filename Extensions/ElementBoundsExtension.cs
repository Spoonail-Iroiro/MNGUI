using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Vintagestory.API.Client;
using Vintagestory.API.Config;

namespace MNGUI.Extensions;

internal static class ElementBoundsExtension {
    public static void FitToChildrenFixedRightOf(this ElementBounds elementBounds, ElementBounds refBounds, double leftSpacing = 0.0) {
        refBounds.CalcWorldBounds();
        // Hacky, make it (absFixedX + OuterWidth + leftSpacing / scale) after scaled
        elementBounds.fixedX = refBounds.absFixedX * 1.0 / RuntimeEnv.GUIScale + refBounds.OuterWidth * 1.0 / RuntimeEnv.GUIScale + leftSpacing;

        // TODO: This SHOULD be the same to one above... test it, and apply it to the under one too
        //elementBounds.fixedX = refBounds.fixedX + (refBounds.fixedWidth + 2.0 * refBounds.fixedPaddingX) + leftSpacing;
    }

    public static void FitToChildrenFixedUnder(this ElementBounds elementBounds, ElementBounds refBounds, double upSpacing = 0.0) {
        refBounds.CalcWorldBounds();
        // Hacky, make it (absFixedY + OuterHeight + upSpacing / scale) after scaled
        elementBounds.fixedY = refBounds.absFixedY * 1.0 / RuntimeEnv.GUIScale + refBounds.OuterHeight * 1.0 / RuntimeEnv.GUIScale + upSpacing;
    }
}
