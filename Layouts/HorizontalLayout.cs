using MNGUI.Extensions;
using MNGUI.GUIElements;
using MNGUITest.MNGUI.GUIElements.Layout;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Vintagestory.API.Client;
using Vintagestory.API.Config;

namespace MNGUI.Layouts;


internal class HorizontalLayout : LayoutWithElementBounds {
    ICoreClientAPI capi;

    public HorizontalLayoutAlignment Alignment { get; private set; }

    public int Gap { get; private set; }

    // Paddings Currently Not Supported
    //public int HorizontalPadding { get; private set; }
    //public int VerticalPadding { get; private set; }

    public List<LayoutBase> ChildLayouts { get; private set; } = new();

    public GuiElement? Element { get; private set; }

    // Currently not used: for holding ElementBounds without GuiElement for future
    protected ElementBounds? bounds;

    public override ElementBounds? Bounds => (Element?.Bounds ?? bounds);

    // Name only for display (like debugging bounds)
    public override string Name { get; set; } = "layout-horizontal";

    public HorizontalLayout(ICoreClientAPI capi, int gap = 0, HorizontalLayoutAlignment alignment = HorizontalLayoutAlignment.Left) {
        this.capi = capi;
        Alignment = alignment;
        Gap = gap;
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

    // TODO: remove, by making children setup themselves in Measure()
    public void SetElement(GuiElement element) {
        Element = element;
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
                // TODO: make children do this themselves in their Measure
                var childBounds = ElementBounds.FixedSize(100, 100).WithSizing(ElementSizing.FitToChildren);
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

            // This layout needs to layout once to determine MinWidth
            if (prevBound != null) {
                // TODO: abstract how getting MinWidth, instead of relying on ElementBounds
                ConnectBoundsRightWithInterval(elem.Bounds, prevBound);
                elem.Bounds.CalcWorldBounds();
            }

            prevBound = elem.Bounds;
        }
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

    public override IEnumerable<GuiElement> GetAllGuiElements() {
        if (Element != null) {
            yield return Element;
        }

        foreach (LayoutBase layout in ChildLayouts) {
            foreach (var elem in layout.GetAllGuiElements()) {
                yield return elem;
            }
        }
    }

    protected void ConnectBoundsRight(ElementBounds newBounds, ElementBounds originBounds) {
        newBounds.FitToChildrenFixedRightOf(originBounds);
    }

    protected void ConnectBoundsRightWithInterval(ElementBounds newBounds, ElementBounds originBounds) {
        newBounds.FitToChildrenFixedRightOf(originBounds, Gap);
    }

}
