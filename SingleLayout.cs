using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vintagestory.API.Client;

namespace RemoteTraderCheckMod.GUI.MNGui {
    internal class SingleLayout : LayoutBase {
        public GuiElement GuiElement { get; private set; }

        public string Name { get; private set; }

        public SingleLayout(GuiElement guiElement, string name = null) {
            GuiElement = guiElement;
            Name = name;
        }
    }
}
