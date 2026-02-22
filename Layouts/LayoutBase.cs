using MNGui.GuiElements;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using Vintagestory.API.Client;

namespace MNGui.Layouts;

/// <summary>
/// Base class for layout node with Measure-Arrange two-path layouting
/// </summary>
public abstract class LayoutBase {

    public virtual string Name { get; set; } = "lauout-other";

    public virtual SizePolicy HorizontalSizePolicy { get; protected set; } = SizePolicy.MinSize;
    public virtual SizePolicy VerticalSizePolicy { get; protected set; } = SizePolicy.MinSize;
    public double HorizontalStretchWeight { get; protected set; } = 0.0;
    public double VerticalStretchWeight { get; protected set; } = 0.0;

    // Policy if this layout needs extra space (if available). Lower priority than any space-greeding SizePolicy
    public virtual SpaceGreedingPolicy HorizontalSpaceGreedingPolicy { get; protected set; } = SpaceGreedingPolicy.None;
    public virtual SpaceGreedingPolicy VerticalSpaceGreedingPolicy { get; protected set; } = SpaceGreedingPolicy.None;

    // Minimum size: available after Measure call
    protected Size minSize = new Size(10, 10.0);
    public virtual Size MinSize => minSize;
    public double MinWidth => MinSize.Width;
    public double MinHeight => MinSize.Height;

    public SizeConstraint MinWidthConstraint { get; protected set; } = new();
    public SizeConstraint MinHeightConstraint { get; protected set; } = new();

    /// <summary>
    /// Init elements, bounds or other data structures. They must be available after calling this. Recover initial state for layout, if necessary.
    /// </summary>
    public virtual void Init() {

    }

    public virtual Size Measure() {
        return MinSize;
    }

    public virtual void Arrange(Vec2 fixedPos, Size availableSize) {

    }

    public virtual IEnumerable<GuiElementInfo> GetAllGuiElements() {

        return Enumerable.Empty<GuiElementInfo>();
    }

    public virtual void SetHorizontalSizePolicy(SizePolicy horizontalSizePolicy, double weight) {
        HorizontalSizePolicy = horizontalSizePolicy;
        HorizontalStretchWeight = weight;
    }

    public virtual void SetVerticalSizePolicy(SizePolicy verticalSizePolicy, double weight) {
        VerticalSizePolicy = verticalSizePolicy;
        VerticalStretchWeight = weight;
    }

    /// <summary>
    /// Sets MinWidthConstraint other than the default
    /// </summary>
    /// <param name="min"></param>
    /// <param name="max"></param>
    public virtual void SetMinWidthConstraint(double? min = null, double? max = null) {
        if (min != null) {
            MinWidthConstraint.Min = min.Value;
        }

        if (max != null) {
            MinWidthConstraint.Max = max.Value;
        }
    }

    /// <summary>
    /// Sets MinWidthConstraint other than the default
    /// </summary>
    /// <param name="min"></param>
    /// <param name="max"></param>
    public virtual void SetMinHeightConstraint(double? min = null, double? max = null) {
        if (min != null) {
            MinHeightConstraint.Min = min.Value;
        }

        if (max != null) {
            MinHeightConstraint.Max = max.Value;
        }
    }

    public SizePolicy GetAdjustedHorizontalSizePolicy(bool hasFillSibling) {
        if (HorizontalSizePolicy == SizePolicy.UnspecifiedLayout) {
            if (hasFillSibling) return SizePolicy.MinSize;
            return HorizontalSpaceGreedingPolicy == SpaceGreedingPolicy.None ? SizePolicy.MinSize : SizePolicy.Stretch;
        }

        return HorizontalSizePolicy;
    }

    public SizePolicy GetAdjustedVerticalSizePolicy(bool hasFillSibling) {
        if (VerticalSizePolicy == SizePolicy.UnspecifiedLayout) {
            if (hasFillSibling) return SizePolicy.MinSize;
            return VerticalSpaceGreedingPolicy == SpaceGreedingPolicy.None ? SizePolicy.MinSize : SizePolicy.Stretch;
        }

        return VerticalSizePolicy;
    }
}

public enum SizePolicy {
    MinSize,
    Stretch,
    EnforceRatio,
    UnspecifiedLayout
}

public enum SpaceGreedingPolicy {
    None,
    Greeding
}

public record class Size(
        double Width,
        double Height
    ) {
    public double Width { get; set; } = Width;
    public double Height { get; set; } = Height;
}

public record class Vec2(
        double X,
        double Y
    ) {
    public double X { get; set; } = X;
    public double Y { get; set; } = Y;
}

public record class GuiElementInfo(
    GuiElement Element,
    string? Name
);

public record class SizeConstraint(
        double Min = 0.0,
        double Max = double.MaxValue
    ) {
    public double Min { get; set; } = Min;
    public double Max { get; set; } = Max;
}
