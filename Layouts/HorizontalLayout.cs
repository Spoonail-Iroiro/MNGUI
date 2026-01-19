using MNGUI.Extensions;
using MNGUI.GUIElements;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Vintagestory.API.Client;
using Vintagestory.API.Config;

namespace MNGUI.Layouts;

enum HorizontalLayoutAlignment {
    Left,
    Right
}

internal class HorizontalLayout : LayoutBase {
    ICoreClientAPI capi;
    public HorizontalLayoutAlignment Alignment { get; private set; }

    public int Interval { get; private set; }

    public List<LayoutBase> ChildLayouts { get; private set; } = new();

    public MNGuiElementContainer ThisContainer { get; private set; }

    public HorizontalLayout(ICoreClientAPI capi, int interval = 0, HorizontalLayoutAlignment alignment = HorizontalLayoutAlignment.Left) {
        this.capi = capi;
        Alignment = alignment;
        Interval = interval;
    }

    public HorizontalLayout Add(GuiElement element, string name = null) {
        var elementAsLayout = new SingleLayout(element, name);

        return Add(elementAsLayout);
    }

    public HorizontalLayout Add(Func<GuiElement> createElement, string name = null) {
        return Add(createElement(), name);
    }

    public HorizontalLayout Add(LayoutBase layout) {
        if (Alignment == HorizontalLayoutAlignment.Right && ChildLayouts.Count >= 1) throw new InvalidOperationException($"HorizontalLayout now supports one element when Alignment == Right");
        ChildLayouts.Add(layout);

        return this;
    }

    public HorizontalLayout AddHorizontalSpace(double length) {
        return Add(new GuiElementParent(capi, ElementBounds.Fixed(0, 0, length, 1)));
    }

    public override void Layout(MNGuiElementContainer container) {
        ThisContainer = container;
        ElementBounds prevBound = null;

        foreach (LayoutBase layout in ChildLayouts) {
            GuiElement elem = null;
            if (layout is SingleLayout sl) {
                elem = sl.GuiElement;

                container.Add(elem);
                elem.BeforeCalcBounds();
                elem.Bounds.CalcWorldBounds();
            }
            else {
                var childContainer = new MNGuiElementContainer(capi, ElementBounds.Fixed(0, 0, 100, 100).WithSizing(ElementSizing.FitToChildren));
                elem = childContainer;

                container.Add(elem);

                layout.Layout(childContainer);

                elem.BeforeCalcBounds();
                elem.Bounds.CalcWorldBounds();
            }

            if (prevBound != null) {
                ConnectBoundsRightWithInterval(elem.Bounds, prevBound);
                elem.Bounds.CalcWorldBounds();
            }

            prevBound = elem.Bounds;
        }
    }

    public override void BeforeComposerCompose() {
        if (Alignment == HorizontalLayoutAlignment.Right) {
            var elem = ThisContainer.Elements.FirstOrDefault();
            if (elem != null) {
                // Hacky, only make sense when directly under a vertical layout
                elem.BeforeCalcBounds();
                elem.Bounds.CalcWorldBounds();
                elem.Bounds.fixedOffsetX = (ThisContainer.Bounds.ParentBounds.InnerWidth - elem.Bounds.OuterWidth) / RuntimeEnv.GUIScale;
                elem.Bounds.CalcWorldBounds();
            }
        }

        foreach (LayoutBase layout in ChildLayouts) {
            // Prevent useless right aligned Horizontal layout
            if (layout is HorizontalLayout hlayout && hlayout.Alignment == HorizontalLayoutAlignment.Right) {
                throw new InvalidOperationException($"Right aligned HorizontalLayout is currently allowed direct under VerticalLayout");
            }
            layout.BeforeComposerCompose();
        }
    }


    protected void ConnectBoundsRight(ElementBounds newBounds, ElementBounds originBounds) {
        newBounds.FitToChildrenFixedRightOf(originBounds);
    }

    protected void ConnectBoundsRightWithInterval(ElementBounds newBounds, ElementBounds originBounds) {
        newBounds.FitToChildrenFixedRightOf(originBounds, Interval);
    }
}
