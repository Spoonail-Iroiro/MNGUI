using System.Linq;
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
    Fixed,
    Stretch
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
    double clipParentPadding = 5.0;

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
    public InsetContainerLayoutBuilder WithSizeFitToChildrenRange(BoxSide side, double maxLength) {
        if (side == BoxSide.Horizontal) {
            horizontalSizePolicy = InsetContainerSizePolicy.FitToChildrenRange;
            minWidth = 0.0;
            maxWidth = maxLength;
        }
        else {
            verticalSizePolicy = InsetContainerSizePolicy.FitToChildrenRange;
            minHeight = 0.0;
            maxHeight = maxLength;
        }
        return this;
    }

    public InsetContainerLayoutBuilder WithSizeStretch(BoxSide side, double minLength) {
        if (side == BoxSide.Horizontal) {
            horizontalSizePolicy = InsetContainerSizePolicy.Stretch;
            minWidth = minLength;
            maxWidth = double.MaxValue;
        }
        else {
            verticalSizePolicy = InsetContainerSizePolicy.Stretch;
            minHeight = minLength;
            maxHeight = double.MaxValue;
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

    /// <summary>
    /// Set if this layout has inset. Default: true. Also removes padding between inset and clip, if removePaddingIfDisabled == true (defaulet).
    /// </summary>
    /// <param name="enabled"></param>
    /// <returns></returns>
    public InsetContainerLayoutBuilder WithInset(bool enabled, bool removePaddingIfDisabled = true) {
        isInsetEnabled = enabled;

        if (!enabled && removePaddingIfDisabled) {
            clipParentPadding = 0.0;
        }
        return this;
    }

    /// <summary>
    /// Set container's padding. Default: 2.0. Mainly for preventing elements drawing outside of their bounds from being clippled slightly
    /// </summary>
    /// <param name="padding"></param>
    /// <returns></returns>
    public InsetContainerLayoutBuilder WithContainerPadding(double padding) {
        containerPadding = padding;
        return this;
    }

    public readonly static InsetContainerSizePolicy[] policiesBoundsFitToChildren = [
            InsetContainerSizePolicy.FitToChildren,
            InsetContainerSizePolicy.FitToChildrenRange
        ];

    WrapperElementLayout CreateClipStartLayouts() {
        var clipBounds = ElementBounds.FixedSize(minWidth, minHeight);
        var clipStartLayout = new WrapperElementLayout(isClipEnabled ? new MNGuiElementClipStart(capi, clipBounds) : new GuiElementDummy(capi, clipBounds));
        if (policiesBoundsFitToChildren.Contains(horizontalSizePolicy)) {
            clipBounds.horizontalSizing = ElementSizing.FitToChildren;
        }
        if (policiesBoundsFitToChildren.Contains(verticalSizePolicy)) {
            clipBounds.verticalSizing = ElementSizing.FitToChildren;
        }
        if (horizontalSizePolicy == InsetContainerSizePolicy.FitToChildrenRange) {
            clipStartLayout.WithFitToChildrenWithWidthRange(maxWidth);
        }
        if (verticalSizePolicy == InsetContainerSizePolicy.FitToChildrenRange) {
            clipStartLayout.WithFitToChildrenWithHeightRange(maxHeight);
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

        var rowLayout = new HorizontalLayout(capi, 3);
        var insetLayout = isInsetEnabled ?
            new WrapperElementLayout(new MNGuiElementInset(capi, BoundsStd.FitToChildren())) :
            new WrapperElementLayout(new GuiElementDummy(capi, BoundsStd.FitToChildren()));
        var clipParentLayout = new WrapperElementLayout(new GuiElementDummy(capi, BoundsStd.FitToChildren().WithFixedPadding(clipParentPadding)));

        var containerLayout = new ElementLayout(new MNGuiElementInnerLayoutContainer(capi, BoundsStd.FitToChildren().WithFixedPadding(containerPadding), containerInitialLayout), containerName);

        var clipStartLayout = CreateClipStartLayouts();
        var clipEndLayout = isClipEnabled ? new WrapperElementLayout(new MNGuiElementClipEnd(capi)) : new ElementLayout(new GuiElementDummy(capi, ElementBounds.FixedSize(1, 1)));

        LayoutWithElementBounds[] layoutsToBeStretched = [
            insetLayout,
            clipParentLayout,
            containerLayout,
            clipStartLayout
        ];

        if (horizontalSizePolicy == InsetContainerSizePolicy.Stretch) {
            foreach (var layout in layoutsToBeStretched) {
                layout.WithHorizontalSizePolicy(SizePolicy.Stretch);
            }
            //insetLayout.WithHorizontalSizePolicy(SizePolicy.Stretch);
            //clipParentLayout.WithHorizontalSizePolicy(SizePolicy.Stretch);
            //containerLayout.WithHorizontalSizePolicy(SizePolicy.Stretch);
            //clipStartLayout.WithHorizontalSizePolicy(SizePolicy.Stretch);
        }

        if (verticalSizePolicy == InsetContainerSizePolicy.Stretch) {
            foreach (var layout in layoutsToBeStretched) {
                layout.WithVerticalSizePolicy(SizePolicy.Stretch);
            }
        }

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
            var scrollbarHeight = (minHeight > 10.0 ? minHeight : 10) + clipParentPadding * 2;
            var scrollbar = new MNGuiElementVerticalScrollbar(capi, ElementBounds.FixedSize(20, scrollbarHeight));
            var scrollBarLayout = new ElementLayout(scrollbar).WithVerticalSizePolicy(SizePolicy.Stretch);
            scrollbar.SetViewAndContentBounds(clipStartLayout.Bounds, containerLayout.Bounds);
            rowLayout.Add(scrollBarLayout);
        }

        return rowLayout;
    }
}
