using MNGUI.GUIElements;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vintagestory.API.Client;

namespace MNGUI.Layouts;
public class LayoutBase {

    public virtual void Layout(MNGuiElementContainer container) {
    }

    public virtual void BeforeComposerCompose() {

    }
}

internal record class ComposeResult(
    GuiElement BottomElement,
    GuiElement RightElement
);
