using MNGui.Extensions;
using MNGui.GuiElements;
using MNGui.GuiElements.Layout;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Vintagestory.API.Client;
using Vintagestory.API.Config;

namespace MNGui.Layouts;

public abstract class LenearLayoutBase : LayoutWithElementBounds {
    public LenearLayoutBase(ICoreClientAPI capi, int gap) {
        this.capi = capi;
        Gap = gap;
    }

    public double? CustomMinWidth { get; set; } = null;
    public double? CustomMinHeight { get; set; } = null;

    protected ICoreClientAPI capi;

    public int Gap { get; protected set; }

    // Paddings Currently Not Supported
    //public int HorizontalPadding { get; protected set; }
    //public int VerticalPadding { get; protected set; }

    public List<LayoutBase> ChildLayouts { get; protected set; } = new();

    public GuiElement? Element { get; protected set; }

    // Currently not used: for holding ElementBounds without GuiElement for future
    protected ElementBounds? bounds;

    public override ElementBounds? Bounds => (Element?.Bounds ?? bounds);

    protected void AddInternal(GuiElement element, string? name = null) {
        var elementAsLayout = new SingleLayout(element, name);

        AddInternal(elementAsLayout);
    }

    protected void AddInternal(Func<GuiElement> createElement, string? name = null) {
        AddInternal(createElement(), name);
    }

    protected void AddInternal(LayoutBase layout) {
        ChildLayouts.Add(layout);
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
}
