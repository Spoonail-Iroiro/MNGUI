using MNGUI.Extensions;
using MNGUI.GUIElements;
using MNGUI.GUIElements.Layout;
using MNGUI.GUIElements.Layout;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Config;
using Vintagestory.GameContent;

namespace MNGUI.Layouts;

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

    protected override void MeasureInternal() {
        var thisBounds = ElementBounds.FixedSize(100, 100).WithSizing(ElementSizing.FitToChildren);
        Element = new GuiElementDebugHorizontalLayout(capi, thisBounds);

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
                ConnectBoundsRightWithInterval(childBounds, prevBound);
                childBounds.CalcWorldBounds();
            }

            prevBound = childBounds;
        }

        // All children set, now calc myself
        Element.BeforeCalcBounds();
        Bounds!.CalcWorldBounds();
    }

    public override void Arrange() {
        // Todo: align to right
        foreach (LayoutBase layout in ChildLayouts) {
            layout.Arrange();
        }

        //if (Alignment == HorizontalLayoutAlignment.Right) {
        //    var elem = ThisContainer.Elements.FirstOrDefault();
        //    if (elem != null) {
        //        // Hacky, only make sense when directly under a vertical layout
        //        elem.BeforeCalcBounds();
        //        elem.Bounds.CalcWorldBounds();
        //        elem.Bounds.fixedOffsetX = (ThisContainer.Bounds.ParentBounds.InnerWidth - elem.Bounds.OuterWidth) / RuntimeEnv.GUIScale;
        //        elem.Bounds.CalcWorldBounds();
        //    }
        //}

        //foreach (LayoutBase layout in ChildLayouts) {
        //    // Prevent useless right aligned Horizontal layout
        //    if (layout is HorizontalLayout hlayout && hlayout.Alignment == HorizontalLayoutAlignment.Right) {
        //        throw new InvalidOperationException($"Right aligned HorizontalLayout is currently allowed direct under VerticalLayout");
        //    }
        //    layout.Arrange();
        //}
    }

    protected void ConnectBoundsRight(ElementBounds newBounds, ElementBounds originBounds) {
        newBounds.FitToChildrenFixedRightOf(originBounds);
    }

    protected void ConnectBoundsRightWithInterval(ElementBounds newBounds, ElementBounds originBounds) {
        newBounds.FitToChildrenFixedRightOf(originBounds, Gap);
    }

}
