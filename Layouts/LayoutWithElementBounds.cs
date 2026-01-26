using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Vintagestory.API.Client;

namespace MNGUI.Layouts;
public abstract class LayoutWithElementBounds : LayoutBase {
    public abstract ElementBounds? Bounds { get; }

    [MemberNotNull(nameof(Bounds))]
    public override sealed void Measure() {
        MeasureInternal();
    }

    /// <summary>
    /// Implementation of Measure. Bounds MUST NOT return null after this.
    /// </summary>
    protected abstract void MeasureInternal();
}
