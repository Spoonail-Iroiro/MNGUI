using MNGui.Layouts.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Vintagestory.API.Client;

namespace MNGui.Layouts.Extensions;
public static class LayoutFluentExtension {
    public static T WithHorizontalSizePolicy<T>(this T layout, SizePolicy horizontalSizePolicy, double weight = 1.0) where T : LayoutBase {
        layout.SetHorizontalSizePolicy(horizontalSizePolicy, weight);
        return layout;
    }

    public static T WithVerticalSizePolicy<T>(this T layout, SizePolicy verticalSizePolicy, double weight = 1.0) where T : LayoutBase {
        layout.SetVerticalSizePolicy(verticalSizePolicy, weight);
        return layout;
    }

    public static T WithAlignment<T>(this T layout, AlignmentHorizontal? hAlign = null, AlignmentVertical? vAlign = null) where T : LinearLayoutBase {
        layout.SetAlignment(hAlign, vAlign);
        return layout;
    }

    /// <summary>
    /// Sets guaranteed min size of the layout, by setting lower limit of measured MinSize
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="layout"></param>
    /// <param name="width"></param>
    /// <param name="height"></param>
    /// <returns></returns>
    public static T WithGuaranteedMinSize<T>(this T layout, double? width = null, double? height = null) where T : LayoutBase {
        if (width != null) {
            layout.SetMinWidthConstraint(min: width);
        }
        if (height != null) {
            layout.SetMinHeightConstraint(min: height);
        }
        return layout;
    }

    /// <summary>
    /// Sets clamped max size of the content in the layout, by setting upper limit of measured MinSize
    /// Warning: the layout's final size may be larger than specified - this only affects MinSize calc during layout.
    ///   Also setting this might cause elements overlapping, since the layout will notify its parent a MinSize smaller than 
    ///   what the content actually needs.
    ///   This method is useful only for a few layout/element that can be smaller than its content, such as clip element.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="layout"></param>
    /// <param name="width"></param>
    /// <param name="height"></param>
    /// <returns></returns>
    public static T WithContentClampedMaxSize<T>(this T layout, double? width = null, double? height = null) where T : LayoutBase {
        if (width != null) {
            layout.SetMinWidthConstraint(max: width);
        }
        if (height != null) {
            layout.SetMinHeightConstraint(max: height);
        }
        return layout;
    }

    /// <summary>
    /// Forces fixed size on LinearLayouts by clamping measured MinSize and setting SizePolicy to MinSize.
    /// For ElementLayouts, please just pass fixed-sized ElementBounds to the element - by default, it will be used directly.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="layout"></param>
    /// <param name="width"></param>
    /// <param name="height"></param>
    /// <returns></returns>
    public static T WithFixedSize<T>(this T layout, double? width = null, double? height = null) where T : LinearLayoutBase {
        // Sets the value to both Min and Max constraint of MinSize
        layout.WithGuaranteedMinSize(width, height);
        layout.WithContentClampedMaxSize(width, height);
        if (width != null) {
            layout.WithHorizontalSizePolicy(SizePolicy.MinSize);
        }
        if (height != null) {
            layout.WithVerticalSizePolicy(SizePolicy.MinSize);
        }
        return layout;
    }

    public static T WithFitToChildrenWithWidthRange<T>(this T layout, double maxWidth) where T : ElementLayout {
        layout.SetFitToChildrenWithWidthRange(maxWidth);
        return layout;
    }

    public static T WithFitToChildrenWithHeightRange<T>(this T layout, double maxHeight) where T : ElementLayout {
        layout.SetFitToChildrenWithHeightRange(maxHeight);
        return layout;
    }

    public static T Add<T>(
            this T layoutWithChildren,
            LayoutBase childLayout
        ) where T : IChildLayoutMixin {
        layoutWithChildren.AddChildLayout(childLayout);
        return layoutWithChildren;
    }

    public static T Add<T>(
            this T layoutWithChildren,
            GuiElement element,
            string? name = null,
            SizePolicy? hSizePolicy = null,
            double hStretchWeight = 1.0,
            SizePolicy? vSizePolicy = null,
            double vStretchWeight = 1.0
        ) where T : IChildLayoutMixin {
        var elementAsLayout = new ElementLayout(element, name);
        if (hSizePolicy != null) {
            elementAsLayout.SetHorizontalSizePolicy(hSizePolicy.Value, hStretchWeight);
        }

        if (vSizePolicy != null) {
            elementAsLayout.SetVerticalSizePolicy(vSizePolicy.Value, vStretchWeight);
        }
        return layoutWithChildren.Add(elementAsLayout);
    }

    public static T Add<T>(
            this T layoutWithChildren,
            Func<GuiElement> createElement,
            string? name = null,
            SizePolicy? hSizePolicy = null,
            double hStretchWeight = 1.0,
            SizePolicy? vSizePolicy = null,
            double vStretchWeight = 1.0
        ) where T : IChildLayoutMixin {
        var element = createElement();
        return layoutWithChildren.Add(element, name, hSizePolicy, hStretchWeight, vSizePolicy, vStretchWeight);
    }
}
