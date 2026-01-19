using MNGUI.GUIElements;
using MNGUI.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vintagestory.API.Client;
using Vintagestory.Client.NoObf;

namespace MNGUI.Layouts;

enum VerticalLayoutAlignment {
    Up,
    Down
}

internal class VerticalLayout : LayoutBase {
    ICoreClientAPI capi;

    public VerticalLayoutAlignment Alignment { get; private set; }

    public int Interval { get; private set; }

    public List<LayoutBase> ChildLayouts { get; private set; } = new();

    public VerticalLayout(ICoreClientAPI capi, int interval = 0, VerticalLayoutAlignment alignment = VerticalLayoutAlignment.Up) {
        if (Alignment == VerticalLayoutAlignment.Down) throw new NotImplementedException();
        this.capi = capi;
        Alignment = alignment;
        Interval = interval;
    }

    public VerticalLayout Add(GuiElement element, string name = null) {
        ChildLayouts.Add(new SingleLayout(element, name));

        return this;
    }

    public VerticalLayout Add(Func<GuiElement> createElement, string name = null) {
        return Add(createElement(), name);
    }

    public VerticalLayout AddVerticalSpace(double length) {
        return Add(new GuiElementParent(capi, ElementBounds.Fixed(0, 0, 1, length)));
    }

    public VerticalLayout Add(LayoutBase layout) {
        ChildLayouts.Add(layout);

        return this;
    }

    public override void Layout(MNGuiElementContainer container) {
        ElementBounds prevBound = null;
        foreach (LayoutBase layout in ChildLayouts) {
            GuiElement elem = null;
            if (layout is SingleLayout sl) {
                elem = sl.GuiElement;

                container.Add(elem);

                // Measure
                elem.BeforeCalcBounds();
                elem.Bounds.CalcWorldBounds();
            }
            else {
                var childContainer = new MNGuiElementContainer(capi, ElementBounds.Fixed(0, 0, 400, 400).WithSizing(ElementSizing.FitToChildren));
                elem = childContainer;

                container.Add(elem);

                layout.Layout(childContainer);
                // Measure
                elem.BeforeCalcBounds();
                elem.Bounds.CalcWorldBounds();
            }


            // Arrange
            if (prevBound != null) {
                // Relatively connect (so, it's okay to arrange children before parents
                ConnectBoundsUnderWithInterval(elem.Bounds, prevBound);
                elem.Bounds.CalcWorldBounds();
            }

            prevBound = elem.Bounds;
        }
    }

    public override void BeforeComposerCompose() {
        foreach (LayoutBase layout in ChildLayouts) {
            layout.BeforeComposerCompose();
        }
    }

    protected void ConnectBoundsUnder(ElementBounds newBounds, ElementBounds originBounds) {
        newBounds.FitToChildrenFixedUnder(originBounds);
    }

    protected void ConnectBoundsUnderWithInterval(ElementBounds newBounds, ElementBounds originBounds) {
        newBounds.FitToChildrenFixedUnder(originBounds, Interval);
    }
}
