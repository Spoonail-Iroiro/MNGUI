using Vintagestory.API.Client;

namespace MNGUI.Layouts;

internal class SingleLayout : LayoutBase {
    public GuiElement GuiElement { get; private set; }

    public string Name { get; private set; }

    public SingleLayout(GuiElement guiElement, string name = null) {
        GuiElement = guiElement;
        Name = name;
    }
}
