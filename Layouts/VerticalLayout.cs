using MNGUI.GUIElements;
using MNGUI.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vintagestory.API.Client;
using Vintagestory.Client.NoObf;
using MNGUITest.MNGUI.GUIElements.Layout;

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
        ElementBounds? prevBound = null;

        foreach (LayoutBase layout in ChildLayouts) {
            GuiElement? elem = null;
            if (layout is SingleLayout sl) {
                elem = sl.Element;

                elem.BeforeCalcBounds();
                // TODO: Replace this with calc for just children only instead of recursive
                elem.Bounds.CalcWorldBounds();
            }
            else {
                var childBounds = ElementBounds.Fixed(0, 0, 100, 100).WithSizing(ElementSizing.FitToChildren);
                if (layout is HorizontalLayout hlayout) {
                    elem = new GuiElementDebugHorizontalLayout(capi, childBounds);

                    hlayout.SetElement(elem);
                }
                else if (layout is VerticalLayout vlayout) {
                    elem = new GuiElementDebugVerticalLayout(capi, childBounds);

                    vlayout.SetElement(elem);
                }
                else {
                    throw new NotImplementedException();
                }

                // Now containers need add all element that returned by GetAllGuiElements, by themselves
                //container.Add(elem);

                // If the child is a layout, this can't determine MinWidth until it determines its one
                layout.Measure();

                // TODO: make chilren do it in their Measure()
                elem.BeforeCalcBounds();
                elem.Bounds.CalcWorldBounds();
            }

            Bounds!.WithChild(elem.Bounds);

            // This layout need layout once to determine MinWidth
            if (prevBound != null) {
                // TODO: abstract how getting MinWidth, instead of relying on ElementBounds
                ConnectBoundsUnderWithInterval(elem.Bounds, prevBound);
                elem.Bounds.CalcWorldBounds();
            }

            prevBound = elem.Bounds;
        }
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
