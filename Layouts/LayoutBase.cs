using MNGui.GuiElements;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using Vintagestory.API.Client;

namespace MNGui.Layouts;

public abstract class LayoutBase {

    public virtual string Name { get; set; } = "lauout-other";

    public virtual SizePolicy HorizontalSizePolicy { get; protected set; } = SizePolicy.FitToChildren;
    public virtual SizePolicy VerticalSizePolicy { get; protected set; } = SizePolicy.FitToChildren;
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

    // TODO: abstract how getting MinWidth, instead of relying on ElementBounds
    //public abstract int CalcMinWidth();

    // TODO: abstract how getting MinHeight, instead of relying on ElementBounds
    //public abstract int CalcMinHeight();

    public virtual Size Measure() {
        return MinSize;
    }

    public virtual void Arrange(Vec2 fixedPos, Size availableSize) {

    }

    public virtual IEnumerable<GuiElementInfo> GetAllGuiElements() {

        return Enumerable.Empty<GuiElementInfo>();
    }
}

public enum SizePolicy {
    FitToChildren,
    Stretch,
    EnforceRatio,
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

