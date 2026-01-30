using MNGui.Extensions;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Vintagestory.API.Client;

namespace MNGui.Layouts;

public abstract class LayoutWithElementBounds : LayoutBase {
    public abstract ElementBounds? Bounds { get; }

    public override Size MinSize => new Size(Bounds!.UnscaledOuterWidth(), Bounds!.UnscaledOuterHeight());

    [MemberNotNull(nameof(Bounds))]
    public override Size Measure() {
        MeasureInternal();
        return MinSize;
    }

    /// <summary>
    /// Implementation of Measure. Bounds MUST NOT return null after this and its UnscaledOuterWidth/Height MUST represents MinWidth/Height.
    /// </summary>
    protected abstract void MeasureInternal();
}
