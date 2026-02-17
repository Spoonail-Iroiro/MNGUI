using System.Collections.Generic;
using System.Linq;
using System.Threading;
using MNGui.GuiElements;
using MNGui.Layouts;
using MNGui.Layouts.Extensions;
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

    bool isClipEnabled = true;

    bool isInsetEnabled = true;

    double containerPadding = 2.0;

    LayoutWithElementBounds? containerInitialLayout;

    public InsetContainerLayoutBuilder(ICoreClientAPI capi, string containerName) {
        this.capi = capi;
        this.containerName = containerName;
    }

    public InsetContainerLayoutBuilder WithSizeFixed(BoxSide side, double fixedLength) {
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

    public InsetContainerLayoutBuilder WithSizeFitToChildren(BoxSide side) {
        if (side == BoxSide.Horizontal) {
            horizontalSizePolicy = InsetContainerSizePolicy.FitToChildren;
            minWidth = 0.0;
        }
        else {
            verticalSizePolicy = InsetContainerSizePolicy.FitToChildren;
            minHeight = 0.0;
        }
        return this;
    }

    // Currently only max is supported
    public InsetContainerLayoutBuilder WithSizeFitToChildrenRange(BoxSide side, double MaxLength) {
        if (side == BoxSide.Horizontal) {
            horizontalSizePolicy = InsetContainerSizePolicy.FitToChildrenRange;
            minWidth = 0.0;
            maxWidth = MaxLength;
        }
        else {
            verticalSizePolicy = InsetContainerSizePolicy.FitToChildrenRange;
            minHeight = 0.0;
            maxHeight = MaxLength;
        }
        return this;
    }

    /// <summary>
    /// Set if this layout has scrollbar. Default: true
    /// </summary>
    /// <param name="exist"></param>
    /// <returns></returns>
    public InsetContainerLayoutBuilder WithScrollbar(bool exist) {
        hasScrollbar = exist;
        return this;
    }

    /// <summary>
    /// Set if this layout has clip. Default: true
    /// </summary>
    /// <param name="enabled"></param>
    /// <returns></returns>
    public InsetContainerLayoutBuilder WithClip(bool enabled) {
        isClipEnabled = enabled;
        return this;
    }

    public InsetContainerLayoutBuilder WithInitialLayout(LayoutWithElementBounds layout) {
        containerInitialLayout = layout;
        return this;
    }

    public InsetContainerLayoutBuilder WithInset(bool enabled) {
        isInsetEnabled = enabled;
        return this;
    }

    /// <summary>
    /// Set containe's padding. Default: 2.0. Mainly for preventing elements drawing outside of their bounds from being clippled slightly
    /// </summary>
    /// <param name="padding"></param>
    /// <returns></returns>
    public InsetContainerLayoutBuilder WithContainerPadding(double padding) {
        containerPadding = padding;
        return this;
    }

    WrapperElementLayout CreateClipStartLayouts() {
        var clipBounds = ElementBounds.FixedSize(minWidth, minHeight);
        var clipStartLayout = new WrapperElementLayout(isClipEnabled ? new MNGuiElementClipStart(capi, clipBounds) : new GuiElementDummy(capi, clipBounds));
        if (horizontalSizePolicy != InsetContainerSizePolicy.Fixed) {
            clipBounds.horizontalSizing = ElementSizing.FitToChildren;
        }
        if (verticalSizePolicy != InsetContainerSizePolicy.Fixed) {
            clipBounds.verticalSizing = ElementSizing.FitToChildren;
        }
        if (horizontalSizePolicy == InsetContainerSizePolicy.FitToChildrenRange) {
            clipStartLayout.WithMaxWidth(maxWidth);
        }
        if (verticalSizePolicy == InsetContainerSizePolicy.FitToChildrenRange) {
            clipStartLayout.WithMaxHeight(maxHeight);
        }

        return clipStartLayout;

        //if (!isClipEnabled) {
        //    yield break;
        //}

        //var clipEndLayout = new SimpleWrapperLayout(new MNGuiElementClipEnd(capi));

        //yield return clipEndLayout;
    }

    public LayoutWithElementBounds Build() {
        var elementStd = new ElementStd(capi);
        var padding = 5.0;

        var rowLayout = new HorizontalLayout(capi, 3);
        var insetLayout = isInsetEnabled ?
            new WrapperElementLayout(new MNGuiElementInset(capi, BoundsStd.FitToChildren())) :
            new WrapperElementLayout(new GuiElementDummy(capi, BoundsStd.FitToChildren()));
        var clipParentLayout = new WrapperElementLayout(new GuiElementDummy(capi, BoundsStd.FitToChildren().WithFixedPadding(padding)));

        var containerLayout = new ElementLayout(new MNGuiElementInnerLayoutContainer(capi, BoundsStd.FitToChildren().WithFixedPadding(containerPadding), containerInitialLayout), containerName);

        var clipStartLayout = CreateClipStartLayouts();
        var clipEndLayout = isClipEnabled ? new WrapperElementLayout(new MNGuiElementClipEnd(capi)) : new ElementLayout(new GuiElementDummy(capi, ElementBounds.FixedSize(1, 1)));

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
            var scrollbarHeight = (minHeight > 10.0 ? minHeight : 10) + padding * 2;
            var scrollbar = new MNGuiElementVerticalScrollbar(capi, ElementBounds.FixedSize(20, scrollbarHeight));
            var scrollBarLayout = new ElementLayout(scrollbar).WithVerticalSizePolicy(SizePolicy.Stretch);
            scrollbar.SetViewAndContentBounds(clipStartLayout.Bounds, containerLayout.Bounds);
            rowLayout.Add(scrollBarLayout);
        }

        return rowLayout;
    }
}
