using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Vintagestory.API.Client;

namespace MNGUI.Extensions;
public static class GuiComposerExtensions {
    public static T? GetElement<T>(this GuiComposer composer, string key) where T : class {
        var elem = composer.GetElement(key);
        return elem as T;
    }
}
