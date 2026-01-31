using MNGui.GuiElements;
using MNGui.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vintagestory.API.Client;
using Vintagestory.Client.NoObf;
using MNGui.GuiElements.Layout;
using Vintagestory.API.Config;

namespace MNGui.Layouts;


internal class VerticalLayout : LenearLayoutBase {

    public VerticalLayoutAlignment Alignment { get; private set; }

    // Name only for display (like debugging bounds)
    public override string Name { get; set; } = "layout-vertical";

    public VerticalLayout(ICoreClientAPI capi, int gap = 0, VerticalLayoutAlignment alignment = VerticalLayoutAlignment.Top) : base(capi, gap) {
        Alignment = alignment;
    }

    public VerticalLayout Add(GuiElement element, string name = null) {
        AddInternal(element, name);
        return this;
    }

    public VerticalLayout Add(Func<GuiElement> createElement, string name = null) {
        AddInternal(createElement, name);
        return this;
    }

    public VerticalLayout Add(LayoutBase layout) {
        AddInternal(layout);
        return this;
    }

    public VerticalLayout AddVerticalSpace(double length) {
        return Add(new GuiElementParent(capi, ElementBounds.Fixed(0, 0, 1, length)));
    }

    protected override void MeasureInternal() {
        var thisBounds = ElementBounds.FixedSize(100, 100).WithSizing(ElementSizing.FitToChildren);
        Element = new GuiElementDebugVerticalLayout(capi, thisBounds);

        //ElementBounds? prevBound = null;
        SpaceGreedingPolicy verticalSpaceGreeding = SpaceGreedingPolicy.None;

        foreach (LayoutBase layout in ChildLayouts) {
            ElementBounds? childBounds;
            if (layout is LayoutWithElementBounds lweb) {
                lweb.Measure();
                childBounds = lweb.Bounds;
            }
            else {
                throw new NotImplementedException();
            }

            if (layout.VerticalSpaceGreedingPolicy == SpaceGreedingPolicy.Greeding) {
                verticalSpaceGreeding = SpaceGreedingPolicy.Greeding;
            }

            Bounds!.WithChild(childBounds);
        }

        AlignChildrenTopLeft();

        VerticalSpaceGreedingPolicy = verticalSpaceGreeding;

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
        // Todo: align to bottom
        AlignChildrenTopLeft();
        foreach (LayoutBase layout in ChildLayouts) {
            if (layout is LayoutWithElementBounds lweb) {
                if (lweb.Bounds == null) throw new InvalidOperationException($"Align is called before child ElementBounds set");
                lweb.Arrange(new Vec2(lweb.Bounds.fixedX, lweb.Bounds.fixedY), lweb.MinSize);
            }
            else {
                throw new NotImplementedException();
            }
        }
    }


    protected void AlignChildrenTopLeft() {
        double currentY = 0.0;

        foreach (var layout in ChildLayouts) {
            ElementBounds? childBounds;
            if (layout is LayoutWithElementBounds lweb) {
                if (lweb.Bounds == null) throw new InvalidOperationException($"Align is called before child ElementBounds set");
                childBounds = lweb.Bounds;
            }
            else {
                throw new NotImplementedException();
            }

            childBounds.fixedX = 0.0;
            childBounds.fixedY = currentY;

            childBounds.CalcWorldBounds();

            currentY = childBounds.UnscaledAbsFixedY() + childBounds.UnscaledOuterHeight() + Gap;
        }
    }


    //protected void ConnectBoundsUnder(ElementBounds newBounds, ElementBounds originBounds) {
    //    newBounds.FitToChildrenFixedUnder(originBounds);
    //}

    //protected void ConnectBoundsUnderWithInterval(ElementBounds newBounds, ElementBounds originBounds) {
    //    newBounds.FitToChildrenFixedUnder(originBounds, Gap);
    //}
}
