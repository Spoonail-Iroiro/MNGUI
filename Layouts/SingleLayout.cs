using MNGui.Util;
using MNGui.Extensions;
using System;
using System.Collections.Generic;
using Vintagestory.API.Client;
using Vintagestory.GameContent;

namespace MNGui.Layouts;

// How SingleLayout adjust actual Bounds of the element: not the same as SizePolicy, which tells the parent sizing preference
public enum ElementSizeConstraint {
    RawBounds,
    // Everytime Measure, it recovers initial bounds and calc MinSize, then limit it to MaxSize
    // Usually used with FitToChildren, for clip element - FitToChildren if under MaxSize, otherwise limit
    LimitToMax,
    // Everytime Measure, it recovers initial bounds and calc MinSize, report it to parent
    // Then resize with Arrange-d size - usually used with stretching SizePolicy
    FollowArrange
}

// Layout with single element.
public class SingleLayout : LayoutWithElementBounds {
    // Bounds to calc MinSize
    protected ElementBounds? InitialBounds { get; set; } = null;

    public GuiElement Element { get; private set; }

    public ElementSizeConstraint HorizontalElementSizeConstraint { get; set; } = ElementSizeConstraint.RawBounds;
    public ElementSizeConstraint VerticalElementSizeConstraint { get; set; } = ElementSizeConstraint.RawBounds;
    public double MaxWidth { get; set; } = double.MaxValue;
    public double MaxHeight { get; set; } = double.MaxValue;

    public override ElementBounds Bounds => Element.Bounds;

    public override string Name { get; set; } = "layout-single";

    protected bool IsCustomNameSet { get; set; } = false;

    /// <summary>
    /// Creates a SingleLayout, which usually holds a GuiElement. If name is specified and the element's bounds has no name, "bounds-{name}" will be set.
    /// </summary>
    /// <param name="guiElement"></param>
    /// <param name="name">Name for this layout, and also a part of the name of the element's bounds if it doesn't have one yet</param>
    public SingleLayout(GuiElement guiElement, string? name = null) {
        Element = guiElement;
        if (name != null) {
            Name = name;
            IsCustomNameSet = true;

            if (guiElement.Bounds.Name == null) {
                Element.Bounds.Name = $"bounds-{name}";
            }
        }
    }

    public SingleLayout WithHorizontalSizePolicy(SizePolicy horizontalSizePolicy, double weight = 1.0) {
        HorizontalSizePolicy = horizontalSizePolicy;
        HorizontalStretchWeight = weight;
        // Adjust size constraint not to be inconsistent with the size policy
        switch (HorizontalSizePolicy) {
            case SizePolicy.MinSize:
                HorizontalElementSizeConstraint = ElementSizeConstraint.RawBounds;
                break;
            case SizePolicy.Stretch:
                HorizontalElementSizeConstraint = ElementSizeConstraint.FollowArrange;
                break;
            default:
                throw new NotImplementedException();
        }
        return this;
    }

    public SingleLayout WithVerticalSizePolicy(SizePolicy verticalSizePolicy, double weight = 1.0) {
        VerticalSizePolicy = verticalSizePolicy;
        VerticalStretchWeight = weight;
        // Adjust size constraint not to be inconsistent with the size policy
        switch (VerticalSizePolicy) {
            case SizePolicy.MinSize:
                VerticalElementSizeConstraint = ElementSizeConstraint.RawBounds;
                break;
            case SizePolicy.Stretch:
                VerticalElementSizeConstraint = ElementSizeConstraint.FollowArrange;
                break;
            default:
                throw new NotImplementedException();
        }
        return this;
    }

    protected void WithMaxWidthInternal(double maxWidth) {
        HorizontalElementSizeConstraint = ElementSizeConstraint.LimitToMax;
        MaxWidth = maxWidth;
        // Adjust size policy not to be inconsistent with the size constraint
        HorizontalSizePolicy = SizePolicy.MinSize;
    }

    public SingleLayout WithMaxWidth(double maxWidth) {
        WithMaxWidthInternal(maxWidth);
        return this;
    }

    protected void WithMaxHeightInternal(double maxHeight) {
        VerticalElementSizeConstraint = ElementSizeConstraint.LimitToMax;
        MaxHeight = maxHeight;
        // Adjust size policy not to be inconsistent with the size constraint
        VerticalSizePolicy = SizePolicy.MinSize;
    }

    public SingleLayout WithMaxHeight(double maxHeight) {
        WithMaxHeightInternal(maxHeight);
        return this;
    }

    public override void Init() {
        if (InitialBounds == null) {
            InitialBounds = Element.Bounds.FlatCopy();
        }

        if (Element is ILayoutableElement lelement) {
            lelement.Init();
        }
    }

    protected override void MeasureInternal() {
        // "Recover" initial state unless when constraint is RawBounds
        if (HorizontalElementSizeConstraint != ElementSizeConstraint.RawBounds) {
            Element.Bounds.CopyHorizontalFixedPropertiesFrom(InitialBounds!);
        }
        if (VerticalElementSizeConstraint != ElementSizeConstraint.RawBounds) {
            Element.Bounds.CopyVerticalFixedPropertiesFrom(InitialBounds!);
        }

        // TODO: Element-specific auto calc such as AutoHeight()
        if (Element is ILayoutableElement lelement) {
            lelement.BeforeMeasure();
        }
        Element.BeforeCalcBounds();
        Element.Bounds.CalcWorldBounds();

        if (HorizontalElementSizeConstraint == ElementSizeConstraint.LimitToMax) {
            if (Element.Bounds.UnscaledOuterWidth() > MaxWidth) {
                Element.Bounds.WithUnscaledOuterWidth(MaxWidth);
                Element.Bounds.CalcWorldBounds();
            }
        }

        if (VerticalElementSizeConstraint == ElementSizeConstraint.LimitToMax) {
            if (Element.Bounds.UnscaledOuterHeight() > MaxHeight) {
                Element.Bounds.WithUnscaledOuterHeight(MaxHeight);
                Element.Bounds.CalcWorldBounds();
            }
        }

        switch (HorizontalSizePolicy) {
            case SizePolicy.MinSize:
                break;
            case SizePolicy.Stretch:
            case SizePolicy.EnforceRatio:
                HorizontalSpaceGreedingPolicy = SpaceGreedingPolicy.Greeding;
                break;
            default:
                throw new NotImplementedException();
        }

        switch (VerticalSizePolicy) {
            case SizePolicy.MinSize:
                break;
            case SizePolicy.Stretch:
            case SizePolicy.EnforceRatio:
                VerticalSpaceGreedingPolicy = SpaceGreedingPolicy.Greeding;
                break;
            default:
                throw new NotImplementedException();

        }
    }

    public override void Arrange(Vec2 fixedPos, Size availableSize) {
        Element.Bounds.WithFixedPosition(fixedPos.X, fixedPos.Y);

        switch (HorizontalElementSizeConstraint) {
            case ElementSizeConstraint.RawBounds:
            case ElementSizeConstraint.LimitToMax:
                break;
            case ElementSizeConstraint.FollowArrange:
                Element.Bounds.WithUnscaledOuterWidth(availableSize.Width);
                Element.Bounds.CalcWorldBounds();
                break;
            default:
                throw new NotImplementedException();
        }

        switch (VerticalElementSizeConstraint) {
            case ElementSizeConstraint.RawBounds:
            case ElementSizeConstraint.LimitToMax:
                break;
            case ElementSizeConstraint.FollowArrange:
                Element.Bounds.WithUnscaledOuterHeight(availableSize.Height);
                Element.Bounds.CalcWorldBounds();
                break;
            default:
                throw new NotImplementedException();
        }

        if (Element is ILayoutableElement lelement) {
            lelement.AfterArrange();
        }
    }

    public override IEnumerable<GuiElementInfo> GetAllGuiElements() {
        yield return new(Element, IsCustomNameSet ? Name : null);
    }

}
