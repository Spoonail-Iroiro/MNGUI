using MNGui.Layouts;
using System.Collections.Generic;
using System.Linq;

namespace MNGui.Layouts.Interfaces;
public interface IChildLayoutMixin {
    List<LayoutBase> ChildLayouts { get; }

    public void AddChildLayout(LayoutBase layout) {
        ChildLayouts.Add(layout);
    }
}
