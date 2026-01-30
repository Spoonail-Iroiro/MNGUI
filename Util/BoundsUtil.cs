using Vintagestory.API.Client;
using Vintagestory.API.Config;

namespace MNGui.Util;

public class BoundsUtil {
    public static void CopyFixedProperties(ElementBounds src, ElementBounds dst) {
        dst.Alignment = src.Alignment;
        dst.verticalSizing = src.verticalSizing;
        dst.horizontalSizing = src.horizontalSizing;
        dst.fixedOffsetX = src.fixedOffsetX;
        dst.fixedOffsetY = src.fixedOffsetY;
        dst.fixedX = src.fixedX;
        dst.fixedY = src.fixedY;
        dst.fixedWidth = src.fixedWidth;
        dst.fixedHeight = src.fixedHeight;
        dst.fixedPaddingX = src.fixedPaddingX;
        dst.fixedPaddingY = src.fixedPaddingY;
        dst.fixedMarginX = src.fixedMarginX;
        dst.fixedMarginY = src.fixedMarginY;
    }

    public static double UnScaled(double length) {
        return length / RuntimeEnv.GUIScale;
    }
}
