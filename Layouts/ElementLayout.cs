using MNGui.Util;
using MNGui.Extensions;
using System;
using System.Collections.Generic;
using Vintagestory.API.Client;
using Vintagestory.GameContent;
using MNGui.Layouts.Interfaces;

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
public class ElementLayout : LayoutWithElementBounds {
    // Bounds to calc MinSize
    protected ElementBounds? InitialBounds { get; set; } = null;

    public GuiElement Element { get; private set; }

    public ElementSizeConstraint HorizontalElementSizeConstraint { get; protected set; } = ElementSizeConstraint.RawBounds;
    public ElementSizeConstraint VerticalElementSizeConstraint { get; protected set; } = ElementSizeConstraint.RawBounds;

    public override ElementBounds Bounds => Element.Bounds;

    public override string Name { get; set; } = "layout-single";

    protected bool IsCustomNameSet { get; set; } = false;

    /// <summary>
    /// Creates a ElementLayout, which usually holds a GuiElement.
    /// </summary>
    /// <param name="guiElement"></param>
    /// <param name="name">Name for this layout, and also a part of the name of the element's bounds if it doesn't have one yet</param>
    /// <remarks>
    /// If name is specified and the element's bounds has no name, "bounds-{name}" will be set
    /// </remarks>
    public ElementLayout(GuiElement guiElement, string? name = null) {
        Element = guiElement;
        if (name != null) {
            Name = name;
            IsCustomNameSet = true;

            if (guiElement.Bounds.Name == null) {
                Element.Bounds.Name = $"bounds-{name}";
            }
        }
    }

    /// <summary>
    /// Creates a ElementLayout, which usually holds a GuiElement.
    /// </summary>
    /// <param name="guiElement"></param>
    /// <param name="name">Name for this layout, and also a part of the name of the element's bounds if it doesn't have one yet</param>
    /// <remarks>
    /// If name is specified and the element's bounds has no name, "bounds-{name}" will be set
    /// </remarks>
    public ElementLayout(Func<GuiElement> createGuiElement, string? name = null) : this(createGuiElement(), name) {
    }

    public override void SetHorizontalSizePolicy(SizePolicy horizontalSizePolicy, double weight) {
        base.SetHorizontalSizePolicy(horizontalSizePolicy, weight);

        // Adjust size constraint not to be inconsistent with the size policy
        switch (HorizontalSizePolicy) {
            // Allow MinSize with FollowArrange - measure MinSize will be clamped by the constraint while RawBounds ignore it
            case SizePolicy.Stretch:
                HorizontalElementSizeConstraint = ElementSizeConstraint.FollowArrange;
                break;
            default:
                break;
        }
    }

    public override void SetVerticalSizePolicy(SizePolicy verticalSizePolicy, double weight) {
        base.SetVerticalSizePolicy(verticalSizePolicy, weight);

        // Adjust size constraint not to be inconsistent with the size policy
        switch (VerticalSizePolicy) {
            // Allow MinSize with FollowArrange - measure MinSize will be clamped by the constraint while RawBounds ignore it
            case SizePolicy.Stretch:
                VerticalElementSizeConstraint = ElementSizeConstraint.FollowArrange;
                break;
            default:
                break;
        }
    }

    public void SetFitToChildrenWithWidthRange(double maxWidth) {
        SetMinWidthConstraint(max: maxWidth);
        HorizontalSizePolicy = SizePolicy.MinSize;
    }


    public void SetFitToChildrenWithHeightRange(double maxHeight) {
        SetMinHeightConstraint(max: maxHeight);
        VerticalSizePolicy = SizePolicy.MinSize;
    }

    public override void SetMinWidthConstraint(double? min = null, double? max = null) {
        base.SetMinWidthConstraint(min, max);
        if (min != null || max != null) {
            HorizontalElementSizeConstraint = ElementSizeConstraint.FollowArrange;
        }
    }

    public override void SetMinHeightConstraint(double? min = null, double? max = null) {
        base.SetMinHeightConstraint(min, max);
        if (min != null || max != null) {
            VerticalElementSizeConstraint = ElementSizeConstraint.FollowArrange;
        }
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

        // Ignores MinSize constraint as whell when RawBounds is specified
        if (HorizontalElementSizeConstraint != ElementSizeConstraint.RawBounds) {
            ClampMinWidthToConstraint();
        }

        if (VerticalElementSizeConstraint != ElementSizeConstraint.RawBounds) {
            ClampMinHeightToConstraint();
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
