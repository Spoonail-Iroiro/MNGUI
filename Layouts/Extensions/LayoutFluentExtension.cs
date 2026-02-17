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

    public static T WithAlignment<T>(this T layout, HorizontalAlignment? horizontalAlignment = null, VerticalAlignment? verticalAlignment = null) where T : LenearLayoutBase {
        layout.SetAlignment(horizontalAlignment, verticalAlignment);
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
        var elementAsLayout = new SingleLayout(element, name);
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
