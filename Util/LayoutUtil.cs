using MNGui.Layouts;
using System.Collections.Generic;
using System.Linq;

namespace MNGui.Util;
public class LayoutUtil {
    public static SpaceGreedingPolicy AggregateGreeding(params SpaceGreedingPolicy[] policies) {
        foreach (var policy in policies) {
            if (policy == SpaceGreedingPolicy.Greeding) return SpaceGreedingPolicy.Greeding;
        }

        return SpaceGreedingPolicy.None;
    }
}
