using Vintagestory.API.Client;
using Vintagestory.API.Config;

namespace MNGui.Util;

public class BoundsUtil {
    public static double UnScaled(double length) {
        return length / RuntimeEnv.GUIScale;
    }
}
