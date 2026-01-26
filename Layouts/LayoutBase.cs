using MNGUI.GUIElements;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vintagestory.API.Client;

namespace MNGUI.Layouts;

public abstract class LayoutBase {

    public virtual string Name { get; set; } = "lauout-other";

    // TODO: abstract how getting MinWidth, instead of relying on ElementBounds
    //public abstract int CalcMinWidth();

    // TODO: abstract how getting MinHeight, instead of relying on ElementBounds
    //public abstract int CalcMinHeight();

    public virtual void Measure() {
    }

    public virtual void Arrange() {

    }

    public virtual IEnumerable<GuiElement> GetAllGuiElements() {

        return Enumerable.Empty<GuiElement>();
    }
}
