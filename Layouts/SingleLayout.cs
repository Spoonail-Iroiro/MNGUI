using MNGui.Util;
using MNGui.Extensions;
using System;
using System.Collections.Generic;
using Vintagestory.API.Client;

namespace MNGui.Layouts;

// Layout with single element.
// If SizePolicy is FitToChildren, this layout uses Element.Bounds as is; otherwise, change Element.Bounds to fixed size to change it to availableSize passed from parent.
internal class SingleLayout : LayoutWithElementBounds {
    // Bounds to calc MinSize
    protected ElementBounds? InitialBounds { get; set; } = null;

    public GuiElement Element { get; private set; }

    public override ElementBounds Bounds => Element.Bounds;

    public override string Name { get; set; } = "layout-single";

    /// <summary>
    /// Creates a SingleLayout, which usually holds a GuiElement. If name is specified and the element's bounds has no name, it will be set.
    /// </summary>
    /// <param name="guiElement"></param>
    /// <param name="name">Name for this layout, and also a part of the name of the element's bounds if it doesn't have one yet</param>
    public SingleLayout(GuiElement guiElement, string name = null) {
        Element = guiElement;
        if (name != null) {
            Name = name;

            if (guiElement.Bounds.Name == null) {
                Element.Bounds.Name = $"bounds-{name}";
            }
        }
    }

    public SingleLayout WithHorizontalSizePolicy(SizePolicy horizontalSizePolicy, double weight = 1.0) {
        HorizontalSizePolicy = horizontalSizePolicy;
        HorizontalStretchWeight = weight;
        return this;
    }

    public SingleLayout WithVerticalSizePolicy(SizePolicy verticalSizePolicy, double weight = 1.0) {
        VerticalSizePolicy = verticalSizePolicy;
        VerticalStretchWeight = weight;
        return this;
    }

    // TODO: make this own Measure (like, calling Element.BeforeCalcBounds. Currently parent layout do that instead)
    protected override void MeasureInternal() {
        if (InitialBounds == null) {
            InitialBounds = Element.Bounds.FlatCopy();
        }

        // "Recover" initial state
        BoundsUtil.CopyFixedProperties(InitialBounds, Element.Bounds);

        // TODO: Element-specific auto calc such as AutoHeight()
        Element.BeforeCalcBounds();
        Element.Bounds.CalcWorldBounds();

        if (HorizontalSizePolicy == SizePolicy.FitToChildren) {

        }
        else {
            HorizontalSpaceGreedingPolicy = SpaceGreedingPolicy.Greeding;
            throw new NotImplementedException("SizePolicy should be FitToChildren");
        }

        if (VerticalSizePolicy == SizePolicy.FitToChildren) {

        }
        else {
            VerticalSpaceGreedingPolicy = SpaceGreedingPolicy.Greeding;
            throw new NotImplementedException("SizePolicy should be FitToChildren");
        }
    }

    public override void Arrange(Vec2 fixedPos, Size availableSize) {
        if (HorizontalSizePolicy == SizePolicy.FitToChildren) {
            Element.Bounds.fixedX = fixedPos.X;
        }
        else {
            // Set sizing Fixed and set fixedWidth to make UnscaledOuterWidth ==     availableSize
            throw new NotImplementedException("SizePolicy should be FitToChildren");
        }

        if (VerticalSizePolicy == SizePolicy.FitToChildren) {
            Element.Bounds.fixedY = fixedPos.Y;
        }
        else {
            throw new NotImplementedException("SizePolicy should be FitToChildren");
        }
    }

    public override IEnumerable<GuiElement> GetAllGuiElements() {
        yield return Element;
    }

}
