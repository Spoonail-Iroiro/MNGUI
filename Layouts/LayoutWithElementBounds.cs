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

    /// <summary>
    /// Arrange with position (0,0) and current MinSize (= Bounds). Suitable for root layout.
    /// </summary>
    public void ArrangeWithMinSize() {
        Arrange(new(0.0, 0.0), MinSize);
    }

    /// <summary>
    /// Utility method for clamp Bounds to MinSize constraint
    /// </summary>
    protected void ClampMinWidthToConstraint() {
        if (Bounds == null) return;

        if (MinWidth < MinWidthConstraint.Min) {
            Bounds.WithUnscaledOuterWidth(MinWidthConstraint.Min);
        }
        if (MinWidthConstraint.Max < MinWidth) {
            Bounds.WithUnscaledOuterWidth(MinWidthConstraint.Max);
        }
    }

    /// <summary>
    /// Utility method for clamp Bounds to MinSize constraint
    /// </summary>
    protected void ClampMinHeightToConstraint() {
        if (Bounds == null) return;

        if (MinHeight < MinHeightConstraint.Min) {
            Bounds.WithUnscaledOuterHeight(MinHeightConstraint.Min);
        }
        if (MinHeightConstraint.Max < MinHeight) {
            Bounds.WithUnscaledOuterHeight(MinHeightConstraint.Max);
        }
    }
}
