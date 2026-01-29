using System.Collections.Generic;
using Vintagestory.API.Client;

namespace MNGui.Layouts;

internal class SingleLayout : LayoutWithElementBounds {
    public GuiElement Element { get; private set; }

    public override ElementBounds? Bounds => Element.Bounds;

    public override string Name { get; set; } = "layout-single";

    /// <summary>
    /// Creates a SingleLayout, which usually holds a GuiElement. If name is specified and the element's bounds has no name, it will be set.
    /// </summary>
    /// <param name="guiElement"></param>
    /// <param name="name">Name for this layout, and also a part of the name of the element's bounds if it doesn't have one yet</param>
    public SingleLayout(GuiElement guiElement, string name = null) {
        Element = guiElement;
        if (name != null) {
            Name = name;

            if (guiElement.Bounds.Name == null) {
                Element.Bounds.Name = $"bounds-{name}";
            }
        }
    }

    // TODO: make this own Measure (like, calling Element.BeforeCalcBounds. Currently parent layout do that instead)
    protected override void MeasureInternal() {
        // Bounds already returns non-null (GuiElement.Bounds), so nothing here
    }

    public override void Arrange() {
    }

    public override IEnumerable<GuiElement> GetAllGuiElements() {
        yield return Element;
    }

}
