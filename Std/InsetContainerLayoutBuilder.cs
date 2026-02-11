using System.Collections.Generic;
using System.Linq;
using System.Threading;
using MNGui.GuiElements;
using MNGui.Layouts;
using Vintagestory.API.Client;

namespace MNGui.Std;

public enum BoxSide {
    Horizontal,
    Vertical
}

public enum InsetContainerSizePolicy {
    FitToChildren,
    FitToChildrenRange,
    Fixed
}

public class InsetContainerLayoutBuilder {
    ICoreClientAPI capi;

    InsetContainerSizePolicy horizontalSizePolicy = InsetContainerSizePolicy.FitToChildren;
    InsetContainerSizePolicy verticalSizePolicy = InsetContainerSizePolicy.Fixed;

    string containerName;

    double minWidth = 0.0;
    double minHeight = 200.0;

    double maxWidth = double.MaxValue;
    double maxHeight = double.MaxValue;

    bool hasScrollbar = true;

    LayoutWithElementBounds? containerInitialLayout;

    public InsetContainerLayoutBuilder(ICoreClientAPI capi, string containerName) {
        this.capi = capi;
        this.containerName = containerName;
    }

    public InsetContainerLayoutBuilder WithFixed(BoxSide side, double fixedLength) {
        if (side == BoxSide.Horizontal) {
            horizontalSizePolicy = InsetContainerSizePolicy.Fixed;
            minWidth = fixedLength;
        }
        else {
            verticalSizePolicy = InsetContainerSizePolicy.Fixed;
            minHeight = fixedLength;
        }
        return this;
    }

    public InsetContainerLayoutBuilder WithFitToChildren(BoxSide side) {
        if (side == BoxSide.Horizontal) {
            horizontalSizePolicy = InsetContainerSizePolicy.FitToChildren;
        }
        else {
            horizontalSizePolicy = InsetContainerSizePolicy.FitToChildren;
        }
        return this;
    }

    // Currently only max is supported
    public InsetContainerLayoutBuilder WithFitToChildrenRange(BoxSide side, double MaxLength) {
        if (side == BoxSide.Horizontal) {
            horizontalSizePolicy = InsetContainerSizePolicy.FitToChildrenRange;
            minWidth = 0.0;
            maxWidth = MaxLength;
        }
        else {
            horizontalSizePolicy = InsetContainerSizePolicy.FitToChildrenRange;
            minHeight = 0.0;
            maxHeight = MaxLength;
        }
        return this;
    }

    public InsetContainerLayoutBuilder WithScrollbar(bool exist) {
        hasScrollbar = exist;
        return this;
    }

    public InsetContainerLayoutBuilder WithInitialLayout(LayoutWithElementBounds layout) {
        containerInitialLayout = layout;
        return this;
    }

    public LayoutBase Build() {
        var elementStd = new ElementStd(capi);
        var padding = 5.0;

        var rowLayout = new HorizontalLayout(capi, 3);
        var insetLayout = new SimpleWrapperLayout(new MNGuiElementInset(capi, BoundsStd.FitToChildren()));
        var clipParentLayout = new SimpleWrapperLayout(new GuiElementDummy(capi, BoundsStd.FitToChildren().WithFixedPadding(padding)));

        var clipBounds = ElementBounds.FixedSize(minWidth, minHeight);
        if (horizontalSizePolicy != InsetContainerSizePolicy.Fixed) {
            clipBounds.horizontalSizing = ElementSizing.FitToChildren;
        }
        if (verticalSizePolicy != InsetContainerSizePolicy.Fixed) {
            clipBounds.verticalSizing = ElementSizing.FitToChildren;
        }
        var clipStartLayout = new SimpleWrapperLayout(new MNGuiElementClipStart(capi, clipBounds));
        if (horizontalSizePolicy == InsetContainerSizePolicy.FitToChildrenRange) {
            clipStartLayout.WithMaxWidth(maxWidth);
        }
        if (verticalSizePolicy == InsetContainerSizePolicy.FitToChildrenRange) {
            clipStartLayout.WithMaxHeight(maxHeight);
        }

        var clipEndLayout = new SimpleWrapperLayout(new MNGuiElementClipEnd(capi));
        var containerLayout = new SingleLayout(new MNGuiElementLayoutContainer(capi, BoundsStd.FitToChildren(), containerInitialLayout), containerName);

        rowLayout
            .Add(
                insetLayout
                    .Add(
                        clipParentLayout
                            .Add(
                                clipStartLayout
                                    .Add(
                                        containerLayout
                                    )
                            )
                            .Add(
                                clipEndLayout
                            )
                    )
            );

        if (hasScrollbar) {
            // TODO: proper scrollbar layouting
            var scrollbarHeight = (minHeight > 10.0 ? minHeight : 100) + padding * 2;
            var scrollbar = new MNGuiElementVerticalScrollbar(capi, ElementBounds.FixedSize(20, scrollbarHeight));
            var scrollBarLayout = new SingleLayout(scrollbar);
            scrollbar.SetViewAndContentBounds(clipStartLayout.Bounds, containerLayout.Bounds);
            rowLayout.Add(scrollBarLayout);
        }

        return rowLayout;
    }
}
