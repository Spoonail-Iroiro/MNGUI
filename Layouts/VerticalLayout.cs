using MNGui.GuiElements;
using MNGui.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vintagestory.API.Client;
using Vintagestory.Client.NoObf;
using MNGui.GuiElements.Layout;
using Vintagestory.API.Config;

namespace MNGui.Layouts;


public class VerticalLayout : LenearLayoutBase {
    // Name only for display (like debugging bounds)
    public override string Name { get; set; } = "layout-vertical";

    public VerticalLayout(ICoreClientAPI capi, int gap = 0, HorizontalAlignment hAlign = HorizontalAlignment.Left, VerticalAlignment vAlign = VerticalAlignment.Top) : base(capi, gap, hAlign, vAlign) {
    }

    public VerticalLayout WithSizePolicy(SizePolicy? horizontalSizePolicy = null, SizePolicy? verticalSizePolicy = null) {
        WithSizePolicyInternal(horizontalSizePolicy, verticalSizePolicy);
        return this;
    }

    public VerticalLayout WithAlignment(HorizontalAlignment? horizontalAlignment = null, VerticalAlignment? verticalAlignment = null) {
        WithAlignmentInternal(horizontalAlignment, verticalAlignment);
        return this;
    }

    public VerticalLayout Add(
            GuiElement element,
            string? name = null,
            SizePolicy? hSizePolicy = null,
            double hStretchWeight = 1.0,
            SizePolicy? vSizePolicy = null,
            double vStretchWeight = 1.0
        ) {
        AddInternal(element, name, hSizePolicy, hStretchWeight, vSizePolicy, vStretchWeight);
        return this;
    }

    public VerticalLayout Add(
            Func<GuiElement> createElement,
            string? name = null,
            SizePolicy? hSizePolicy = null,
            double hStretchWeight = 1.0,
            SizePolicy? vSizePolicy = null,
            double vStretchWeight = 1.0
        ) {
        AddInternal(createElement, name, hSizePolicy, hStretchWeight, vSizePolicy, vStretchWeight);
        return this;
    }

    public VerticalLayout Add(LayoutBase layout) {
        AddInternal(layout);
        return this;
    }

    public VerticalLayout AddVerticalSpace(double length) {
        return Add(new GuiElementParent(capi, ElementBounds.Fixed(0, 0, 1, length)));
    }

    public override void Init() {
        // Don't init myself twice
        if (Element == null) {
            var thisBounds = CreateDefaultBounds();
            Element = new GuiElementDebugVerticalLayout(capi, thisBounds);
        }

        foreach (LayoutBase layout in ChildLayouts) {
            layout.Init();
        }
    }

    protected override void MeasureInternal() {
        ResetBounds();

        //ElementBounds? prevBound = null;
        SpaceGreedingPolicy verticalSpaceGreeding = SpaceGreedingPolicy.None;

        foreach (LayoutBase layout in ChildLayouts) {
            ElementBounds? childBounds;
            if (layout is LayoutWithElementBounds lweb) {
                lweb.Measure();
                childBounds = lweb.Bounds;
            }
            else {
                throw new NotImplementedException();
            }

            if (layout.VerticalSpaceGreedingPolicy == SpaceGreedingPolicy.Greeding) {
                verticalSpaceGreeding = SpaceGreedingPolicy.Greeding;
            }

            Bounds!.WithChildForce(childBounds);
        }

        AlignChildrenTopLeft();

        VerticalSpaceGreedingPolicy = verticalSpaceGreeding;

        // All children set, now calc myself
        // First, just fit to children
        Element.BeforeCalcBounds();
        Bounds!.CalcWorldBounds();

        // TODO: SizePolicy-specific recalc of MinWidth/Height

        // If MinSize is smaller than CustomMinSize, fix for each side
        if (CustomMinWidth != null && MinWidth < CustomMinWidth.Value) {
            Bounds.WithUnscaledOuterWidth(CustomMinWidth.Value);
            Bounds.CalcWorldBounds();
        }
        if (CustomMinHeight != null && MinHeight < CustomMinHeight.Value) {
            Bounds.WithUnscaledOuterHeight(CustomMinHeight.Value);
            Bounds.CalcWorldBounds();
        }
    }

    public override void Arrange(Vec2 fixedPos, Size availableSize) {
        if (Bounds == null) throw new InvalidOperationException($"Call Measure before Arrange!");
        var innerMinSize = new Size(Bounds.UnscaledInnerWidth(), Bounds.UnscaledInnerHeight()); // Needs when align
        // Apply arrange myself
        Bounds.WithFixedPosition(fixedPos.X, fixedPos.Y);
        Bounds.WithUnscaledOuterWidth(availableSize.Width);
        Bounds.WithUnscaledOuterHeight(availableSize.Height);
        Bounds.CalcWorldBounds();

        // Now I have available size (UnscaledInnerWidth/Height), lets assign it to children
        var children = new List<LayoutWithElementBounds>();
        var hasFillElementVertical = false;

        foreach (LayoutBase layout in ChildLayouts) {
            if (layout is LayoutWithElementBounds lweb) {
                children.Add(lweb);
                if (IsStretchingSizePolicy(lweb.VerticalSizePolicy)) {
                    hasFillElementVertical = true;
                }
            }
            else {
                throw new NotImplementedException();
            }
        }

        var actualSizePoliciesVertical = children.Select(ch => ch.GetAdjustedVerticalSizePolicy(hasFillElementVertical)).ToList();
        var actualStretchWeightVertical = children.Select(ch => ch.VerticalStretchWeight).ToList();
        var minHeights = children.Select(ch => ch.MinHeight).ToList();
        var actualSizePoliciesHorizontal = children.Select(ch => ch.HorizontalSizePolicy == SizePolicy.UnspecifiedLayout ? SizePolicy.Stretch : ch.HorizontalSizePolicy);

        var availableWidth = Bounds.UnscaledInnerWidth();
        var availableHeight = Bounds.UnscaledInnerHeight();
        availableHeight -= Gap * (children.Count - 1);

        var distributedHeights = CalcDistributedLength(availableHeight, minHeights, actualSizePoliciesVertical, actualStretchWeightVertical);
        var widthes = Enumerable.Zip(actualSizePoliciesHorizontal, children).Select(pair => pair.First == SizePolicy.MinSize ? pair.Second.MinWidth : availableWidth);

        var needVerticalAlignment = !actualSizePoliciesVertical.Any(IsStretchingSizePolicy);
        var startY = 0.0;
        if (needVerticalAlignment) {
            startY = VerticalAlignment switch {
                VerticalAlignment.Middle => (availableHeight - innerMinSize.Height) / 2.0,
                VerticalAlignment.Bottom => (availableHeight - innerMinSize.Height),
                _ => 0.0
            };
        }

        var fixedXs = widthes.Select(wi => {
            var x = HorizontalAlignment switch {
                HorizontalAlignment.Center => (availableWidth - wi) / 2.0,
                HorizontalAlignment.Right => (availableWidth - wi),
                _ => 0.0
            };
            return x;
        }).ToList();
        var fixedYs = CalcAlignedPositions(startY, distributedHeights, Gap);

        foreach (var ((fixedX, fixedY), (width, height), lweb) in Enumerable.Zip(Enumerable.Zip(fixedXs, fixedYs), Enumerable.Zip(widthes, distributedHeights), children)) {
            lweb.Arrange(new Vec2(fixedX, fixedY), new Size(width, height));
        }
    }

    protected void AlignChildrenTopLeft() {
        double currentY = 0.0;

        foreach (var layout in ChildLayouts) {
            ElementBounds? childBounds;
            if (layout is LayoutWithElementBounds lweb) {
                if (lweb.Bounds == null) throw new InvalidOperationException($"Align is called before child ElementBounds set");
                childBounds = lweb.Bounds;
            }
            else {
                throw new NotImplementedException();
            }

            childBounds.fixedX = 0.0;
            childBounds.fixedY = currentY;

            childBounds.CalcWorldBounds();

            currentY = childBounds.UnscaledAbsFixedY() + childBounds.UnscaledOuterHeight() + Gap;
        }
    }


    //protected void ConnectBoundsUnder(ElementBounds newBounds, ElementBounds originBounds) {
    //    newBounds.FitToChildrenFixedUnder(originBounds);
    //}

    //protected void ConnectBoundsUnderWithInterval(ElementBounds newBounds, ElementBounds originBounds) {
    //    newBounds.FitToChildrenFixedUnder(originBounds, Gap);
    //}
}
