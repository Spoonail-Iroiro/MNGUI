using MNGui.Util;
using Vintagestory.API.Client;
using Vintagestory.API.Config;

namespace MNGui.Extensions;

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

    /// <summary>
    /// ElementBounds.OuterWidth is GUI-scaled width (should be called absOuterWidth?). This returns unscaled value.
    /// </summary>
    /// <param name="elementBounds"></param>
    /// <returns></returns>
    public static double UnscaledOuterWidth(this ElementBounds elementBounds) {
        return BoundsUtil.UnScaled(elementBounds.OuterWidth);
    }

    /// <summary>
    /// ElementBounds.OuterHeight is GUI-scaled height (should be called absOuterHeight?). This returns unscaled value.
    /// </summary>
    /// <param name="elementBounds"></param>
    /// <returns></returns>
    public static double UnscaledOuterHeight(this ElementBounds elementBounds) {
        return BoundsUtil.UnScaled(elementBounds.OuterHeight);
    }

    /// <summary>
    /// ElementBounds.InnerWidth is GUI-scaled width. This returns unscaled value.
    /// </summary>
    /// <param name="elementBounds"></param>
    /// <returns></returns>
    public static double UnscaledInnerWidth(this ElementBounds elementBounds) {
        return BoundsUtil.UnScaled(elementBounds.absInnerWidth);
    }

    /// <summary>
    /// ElementBounds.InnerHeight is GUI-scaled height. This returns unscaled value.
    /// </summary>
    /// <param name="elementBounds"></param>
    /// <returns></returns>
    public static double UnscaledInnerHeight(this ElementBounds elementBounds) {
        return BoundsUtil.UnScaled(elementBounds.absInnerHeight);
    }

    public static double UnscaledAbsFixedX(this ElementBounds elementBounds) {
        return BoundsUtil.UnScaled(elementBounds.absFixedX);
    }

    public static double UnscaledAbsFixedY(this ElementBounds elementBounds) {
        return BoundsUtil.UnScaled(elementBounds.absFixedY);
    }

    public static void WithUnscaledOuterWidth(this ElementBounds elementBounds, double unscaledWidth) {
        elementBounds.horizontalSizing = ElementSizing.Fixed;
        elementBounds.WithFixedWidth(unscaledWidth - 2.0 * elementBounds.fixedPaddingX);
    }

    public static void WithUnscaledOuterHeight(this ElementBounds elementBounds, double unscaledHeight) {
        elementBounds.verticalSizing = ElementSizing.Fixed;
        elementBounds.WithFixedHeight(unscaledHeight - 2.0 * elementBounds.fixedPaddingY);
    }

    /// <summary>
    /// vanilla ElementBounds.WithChild EASILY CREATE INCOSISTENT BOUNDS TREE. This prevents that.
    /// </summary>
    /// <param name="elementBounds"></param>
    /// <param name="childBounds"></param>
    /// <returns></returns>
    public static ElementBounds WithChildForce(this ElementBounds elementBounds, ElementBounds childBounds) {
        if (!elementBounds.ChildBounds.Contains(childBounds)) {
            elementBounds.ChildBounds.Add(childBounds);
        }
        childBounds.ParentBounds = elementBounds;

        return elementBounds;
    }

}
