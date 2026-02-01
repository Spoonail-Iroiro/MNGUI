using MNGui.Extensions;
using MNGui.GuiElements;
using MNGui.GuiElements.Layout;
using MNGui.GuiElements.Layout;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Config;
using Vintagestory.GameContent;

namespace MNGui.Layouts;

public class HorizontalLayout : LenearLayoutBase {

    public HorizontalLayoutAlignment Alignment { get; private set; }

    public override string Name { get; set; } = "layout-horizontal";

    public HorizontalLayout(ICoreClientAPI capi, int gap = 0, HorizontalLayoutAlignment alignment = HorizontalLayoutAlignment.Left) : base(capi, gap) {
        Alignment = alignment;
    }

    public HorizontalLayout Add(GuiElement element, string name = null) {
        AddInternal(element, name);
        return this;
    }

    public HorizontalLayout Add(Func<GuiElement> createElement, string name = null) {
        AddInternal(createElement, name);
        return this;
    }

    public HorizontalLayout Add(LayoutBase layout) {
        AddInternal(layout);
        return this;
    }

    public HorizontalLayout AddHorizontalSpace(double length) {
        return Add(new GuiElementParent(capi, ElementBounds.Fixed(0, 0, length, 1)));
    }

    public override void Init() {
        // Don't init myself twice
        if (Element == null) {
            var thisBounds = CreateDefaultBounds();
            Element = new GuiElementDebugHorizontalLayout(capi, thisBounds);
        }

        foreach (LayoutBase layout in ChildLayouts) {
            layout.Init();
        }
    }

    protected override void MeasureInternal() {
        ResetBounds();

        // Check if any child is space-greeding on long side
        // Note: LinearLayouts are always none space-greeding on its short side!
        SpaceGreedingPolicy horizontalSpaceGreeding = SpaceGreedingPolicy.None;

        foreach (LayoutBase layout in ChildLayouts) {
            ElementBounds? childBounds;
            if (layout is LayoutWithElementBounds lweb) {
                lweb.Measure();
                childBounds = lweb.Bounds;
            }
            else {
                throw new NotImplementedException();
            }

            // Check if any child is space-greeding on long side
            if (layout.HorizontalSpaceGreedingPolicy == SpaceGreedingPolicy.Greeding) {
                horizontalSpaceGreeding = SpaceGreedingPolicy.Greeding;
            }

            Bounds!.WithChildForce(childBounds);

        }

        // This layout needs to layout once to determine MinWidth
        // Because it's still Measure, just top-left aligning is enough to calc MinSize
        AlignChildrenTopLeft();

        HorizontalSpaceGreedingPolicy = horizontalSpaceGreeding;

        // All children set, now calc myself
        // First, just fit to children
        Element.BeforeCalcBounds();
        Bounds!.CalcWorldBounds();

        // TODO: SizePolicy-specific recalc of MinWidth/Height

        // If MinSize is smaller than CustomMinSize, fix for each side
        if (CustomMinWidth != null && MinWidth < CustomMinWidth.Value) {
            Bounds.WithUnscaledOuterWidth(CustomMinWidth.Value);
        }
        if (CustomMinHeight != null && MinHeight < CustomMinHeight.Value) {
            Bounds.WithUnscaledOuterHeight(CustomMinHeight.Value);
        }
    }

    public override void Arrange(Vec2 fixedPos, Size availableSize) {
        // TODO: various aligning (currently only topleft)
        AlignChildrenTopLeft();
        foreach (LayoutBase layout in ChildLayouts) {
            if (layout is LayoutWithElementBounds lweb) {
                if (lweb.Bounds == null) throw new InvalidOperationException($"Call Measure before Arrange!");
                layout.Arrange(new Vec2(lweb.Bounds.fixedX, lweb.Bounds.fixedY), lweb.MinSize);
            }
            else {
                throw new NotImplementedException("We're not prepared for layouts without bounds...");
            }
        }
    }

    protected void AlignChildrenTopLeft() {
        double currentX = 0.0;

        foreach (LayoutBase layout in ChildLayouts) {
            ElementBounds? childBounds;
            if (layout is LayoutWithElementBounds lweb) {
                if (lweb.Bounds == null) throw new InvalidOperationException($"Align is called before child ElementBounds set");
                childBounds = lweb.Bounds;
            }
            else {
                throw new NotImplementedException("We're not prepared for layouts without bounds...");
            }

            childBounds.fixedX = currentX;
            childBounds.fixedY = 0.0;

            childBounds.CalcWorldBounds();

            // Calc fixedX of next element
            currentX = childBounds.UnscaledAbsFixedX() + childBounds.UnscaledOuterWidth() + Gap;
        }
    }

    //public override void Arrange(Size availableSize) {
    //    // Todo: align to right
    //    foreach (LayoutBase layout in ChildLayouts) {
    //        layout.Arrange(layout.MinSize);
    //    }

    //    //if (Alignment == HorizontalLayoutAlignment.Right) {
    //    //    var elem = ThisContainer.Elements.FirstOrDefault();
    //    //    if (elem != null) {
    //    //        // Hacky, only make sense when directly under a vertical layout
    //    //        elem.BeforeCalcBounds();
    //    //        elem.Bounds.CalcWorldBounds();
    //    //        elem.Bounds.fixedOffsetX = (ThisContainer.Bounds.ParentBounds.InnerWidth - elem.Bounds.OuterWidth) / RuntimeEnv.GUIScale;
    //    //        elem.Bounds.CalcWorldBounds();
    //    //    }
    //    //}

    //    //foreach (LayoutBase layout in ChildLayouts) {
    //    //    // Prevent useless right aligned Horizontal layout
    //    //    if (layout is HorizontalLayout hlayout && hlayout.Alignment == HorizontalLayoutAlignment.Right) {
    //    //        throw new InvalidOperationException($"Right aligned HorizontalLayout is currently allowed direct under VerticalLayout");
    //    //    }
    //    //    layout.Arrange();
    //    //}
    //}

    //protected void ConnectBoundsRight(ElementBounds newBounds, ElementBounds originBounds) {
    //    newBounds.FitToChildrenFixedRightOf(originBounds);
    //}

    //protected void ConnectBoundsRightWithInterval(ElementBounds newBounds, ElementBounds originBounds) {
    //    newBounds.fixedX = originBounds.UnscaledAbsFixedX() 
    //    //newBounds.FitToChildrenFixedRightOf(originBounds, Gap);
    //}

}
