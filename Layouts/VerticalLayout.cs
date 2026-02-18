using MNGui.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using Vintagestory.API.Client;
using MNGui.GuiElements.Layout;
using MNGui.Layouts.Extensions;

namespace MNGui.Layouts;


public class VerticalLayout : LinearLayoutBase {
    // Name only for display (like debugging bounds)
    public override string Name { get; set; } = "layout-vertical";

    public VerticalLayout(ICoreClientAPI capi, int gap = 0, AlignmentHorizontal hAlign = AlignmentHorizontal.Left, AlignmentVertical vAlign = AlignmentVertical.Top) : base(capi, gap, hAlign, vAlign) {
    }

    public VerticalLayout AddVerticalSpace(double length) {
        return this.Add(new GuiElementParent(capi, ElementBounds.Fixed(0, 0, 1, length)));
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
        Element!.BeforeCalcBounds();
        Bounds!.CalcWorldBounds();

        ClampMinWidthToConstraint();
        ClampMinHeightToConstraint();
    }

    public override void Arrange(Vec2 fixedPos, Size availableSize) {
        if (Bounds == null) throw new InvalidOperationException($"Call Measure before Arrange!");
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
        availableHeight = Math.Max(availableHeight - Gap * (children.Count - 1), 0);

        var distributedHeights = CalcDistributedLength(availableHeight, minHeights, actualSizePoliciesVertical, actualStretchWeightVertical);
        var widthes = Enumerable.Zip(actualSizePoliciesHorizontal, children).Select(pair => pair.First == SizePolicy.MinSize ? pair.Second.MinWidth : availableWidth);

        var needVerticalAlignment = !actualSizePoliciesVertical.Any(IsStretchingSizePolicy);
        var startY = 0.0;
        var innerMinHeight = distributedHeights.Sum() + Gap * (children.Count - 1);
        if (needVerticalAlignment && innerMinHeight < availableHeight) {
            startY = VerticalAlignment switch {
                AlignmentVertical.Middle => (availableHeight - innerMinHeight) / 2.0,
                AlignmentVertical.Bottom => (availableHeight - innerMinHeight),
                _ => 0.0
            };
        }

        var fixedXs = widthes.Select(wi => {
            var x = HorizontalAlignment switch {
                AlignmentHorizontal.Center => (availableWidth - wi) / 2.0,
                AlignmentHorizontal.Right => (availableWidth - wi),
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
}
