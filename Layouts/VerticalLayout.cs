using MNGUI.GUIElements;
using MNGUI.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vintagestory.API.Client;
using Vintagestory.Client.NoObf;
using MNGUI.GUIElements.Layout;

namespace MNGUI.Layouts;


internal class VerticalLayout : LenearLayoutBase {

    public VerticalLayoutAlignment Alignment { get; private set; }

    // Name only for display (like debugging bounds)
    public override string Name { get; set; } = "layout-vertical";

    public VerticalLayout(ICoreClientAPI capi, int gap = 0, VerticalLayoutAlignment alignment = VerticalLayoutAlignment.Top) : base(capi, gap) {
        if (Alignment == VerticalLayoutAlignment.Bottom) throw new NotImplementedException();
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

        ElementBounds? prevBound = null;

        foreach (LayoutBase layout in ChildLayouts) {
            ElementBounds? childBounds;
            if (layout is SingleLayout sl) {
                var elem = sl.Element;

                elem.BeforeCalcBounds();
                // TODO: Replace this with calc for just children only instead of recursive
                elem.Bounds.CalcWorldBounds();

                childBounds = elem.Bounds;
            }
            else if (layout is LayoutWithElementBounds lweb) {
                // Now containers need add all element that returned by GetAllGuiElements, by themselves
                //container.Add(elem);

                // If the child is a layout, this can't determine MinWidth until it determines its one
                lweb.Measure();

                childBounds = lweb.Bounds;
            }
            else {
                throw new NotImplementedException();
            }

            Bounds!.WithChild(childBounds);

            // This layout needs to layout once to determine MinWidth
            if (prevBound != null) {
                // TODO: abstract how getting MinWidth, instead of relying on ElementBounds
                ConnectBoundsUnderWithInterval(childBounds, prevBound);
                childBounds.CalcWorldBounds();
            }

            prevBound = childBounds;
        }

        // All children set, now calc myself
        Element.BeforeCalcBounds();
        Bounds!.CalcWorldBounds();
    }

    public override void Arrange() {
        // Todo: align to bottom
        foreach (LayoutBase layout in ChildLayouts) {
            layout.Arrange();
        }
    }

    protected void ConnectBoundsUnder(ElementBounds newBounds, ElementBounds originBounds) {
        newBounds.FitToChildrenFixedUnder(originBounds);
    }

    protected void ConnectBoundsUnderWithInterval(ElementBounds newBounds, ElementBounds originBounds) {
        newBounds.FitToChildrenFixedUnder(originBounds, Gap);
    }
}
