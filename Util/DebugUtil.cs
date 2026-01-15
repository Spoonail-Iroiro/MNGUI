using System.Collections.Generic;
using System.Linq;
using Vintagestory.API.Client;

namespace MNGUI.Util;
public class DebugUtil {
    public static ElementBounds? SearchBoundsRecursive(ElementBounds rootBounds, string searchingNamePartial) {
        if (rootBounds.Name?.Contains(searchingNamePartial) == true) return rootBounds;
        if (rootBounds.ChildBounds == null) return null;
        foreach (var bounds in rootBounds.ChildBounds) {
            var result = SearchBoundsRecursive(bounds, searchingNamePartial);
            if (result != null) return result;
        }
        return null;

    }
}
