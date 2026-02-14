using MNGui.Layouts;
using System.Collections.Generic;
using System.Linq;

namespace MNGui.Util;
public class LayoutUtil {
    public static readonly double EPSILON_LENGTH = 0.001;

    public static SpaceGreedingPolicy AggregateGreeding(params SpaceGreedingPolicy[] policies) {
        foreach (var policy in policies) {
            if (policy == SpaceGreedingPolicy.Greeding) return SpaceGreedingPolicy.Greeding;
        }

        return SpaceGreedingPolicy.None;
    }
}
