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


internal class VerticalLayout : LayoutWithElementBounds {
    ICoreClientAPI capi;

    public VerticalLayoutAlignment Alignment { get; private set; }

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
    public override string Name { get; set; } = "layout-vertical";

    public VerticalLayout(ICoreClientAPI capi, int gap = 0, VerticalLayoutAlignment alignment = VerticalLayoutAlignment.Top) {
        if (Alignment == VerticalLayoutAlignment.Bottom) throw new NotImplementedException();
        this.capi = capi;
        Alignment = alignment;
        Gap = gap;
    }

    public VerticalLayout Add(GuiElement element, string name = null) {
        var elementAsLayout = new SingleLayout(element, name);

        return Add(elementAsLayout);
    }

    public VerticalLayout Add(Func<GuiElement> createElement, string name = null) {
        return Add(createElement(), name);
    }

    public VerticalLayout Add(LayoutBase layout) {
        ChildLayouts.Add(layout);

        return this;
    }

    public VerticalLayout AddVerticalSpace(double length) {
        return Add(new GuiElementParent(capi, ElementBounds.Fixed(0, 0, 1, length)));
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

    protected void ConnectBoundsUnder(ElementBounds newBounds, ElementBounds originBounds) {
        newBounds.FitToChildrenFixedUnder(originBounds);
    }

    protected void ConnectBoundsUnderWithInterval(ElementBounds newBounds, ElementBounds originBounds) {
        newBounds.FitToChildrenFixedUnder(originBounds, Gap);
    }
}
