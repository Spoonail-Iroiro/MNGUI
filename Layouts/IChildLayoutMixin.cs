using System.Collections.Generic;
using System.Linq;

namespace MNGui.Layouts;
public interface IChildLayoutMixin {
    List<LayoutBase> ChildLayouts { get; }

    public void AddChildLayout(LayoutBase layout) {
        ChildLayouts.Add(layout);
    }
}
