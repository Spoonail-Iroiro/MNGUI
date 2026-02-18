using MNGui.Extensions;
using MNGui.GuiElements;
using MNGui.GuiElements.Layout;
using MNGui.Layouts.Extensions;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Config;
using Vintagestory.GameContent;

namespace MNGui.Layouts;

public class HorizontalLayout : LinearLayoutBase {
    public override string Name { get; set; } = "layout-horizontal";

    public HorizontalLayout(ICoreClientAPI capi, int gap = 0, AlignmentHorizontal hAlign = AlignmentHorizontal.Left, AlignmentVertical vAlign = AlignmentVertical.Top) : base(capi, gap, hAlign, vAlign) {
    }

    public HorizontalLayout AddHorizontalSpace(double length) {
        return this.Add(new GuiElementParent(capi, ElementBounds.Fixed(0, 0, length, 1)));
    }

    public override void Init() {
        // Don't init myself twice
        if (Element == null) {
            var thisBounds = CreateDefaultBounds();
            Element = new GuiElementDebugHorizontalLayout(capi, thisBounds);
        }

        foreach (LayoutBase layout in ChildLayouts) {
            layout.Init();
        }
    }

    protected override void MeasureInternal() {
        ResetBounds();

        // Check if any child is space-greeding on long side
        // Note: LinearLayouts are always none space-greeding on its short side!
        SpaceGreedingPolicy horizontalSpaceGreeding = SpaceGreedingPolicy.None;

        foreach (LayoutBase layout in ChildLayouts) {
            ElementBounds? childBounds;
            if (layout is LayoutWithElementBounds lweb) {
                lweb.Measure();
                childBounds = lweb.Bounds;
            }
            else {
                throw new NotImplementedException();
            }

            // Check if any child is space-greeding on long side
            if (layout.HorizontalSpaceGreedingPolicy == SpaceGreedingPolicy.Greeding) {
                horizontalSpaceGreeding = SpaceGreedingPolicy.Greeding;
            }

            Bounds!.WithChildForce(childBounds);

        }

        // This layout needs to layout once to determine MinWidth
        // Because it's still Measure, just top-left aligning is enough to calc MinSize
        AlignChildrenTopLeft();

        HorizontalSpaceGreedingPolicy = horizontalSpaceGreeding;

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

        // TODO:  if (ChildLayouts.Count == 0) 

        // Now I have available size (UnscaledInnerWidth/Height), lets assign it to children
        var children = new List<LayoutWithElementBounds>();
        var hasFillElementHorizontal = false;

        foreach (LayoutBase layout in ChildLayouts) {
            if (layout is LayoutWithElementBounds lweb) {
                children.Add(lweb);
                if (IsStretchingSizePolicy(lweb.HorizontalSizePolicy)) {
                    hasFillElementHorizontal = true;
                }
                //layout.Arrange(new Vec2(lweb.Bounds.fixedX, lweb.Bounds.fixedY), lweb.MinSize);
            }
            else {
                throw new NotImplementedException("We're not prepared for layouts without bounds...");
            }
        }

        var actualSizePoliciesHorizontal = children.Select(ch => ch.GetAdjustedHorizontalSizePolicy(hasFillElementHorizontal)).ToList();
        var actualStretchWeightHorizontal = children.Select(ch => ch.HorizontalStretchWeight).ToList(); // MinSize will be ignored
        var minWidthes = children.Select(ch => ch.MinWidth).ToList();
        var actualSizePoliciesVertical = children.Select(ch => ch.VerticalSizePolicy == SizePolicy.UnspecifiedLayout ? SizePolicy.Stretch : ch.VerticalSizePolicy).ToList();
        //var actualStretchWeightVertical = children.Select(ch => 1.0).ToList(); // MinSize will be ignored, full height if fill

        var availableWidth = Bounds.UnscaledInnerWidth();
        var availableHeight = Bounds.UnscaledInnerHeight();
        availableWidth = Math.Max(availableWidth - Gap * (children.Count - 1), 0);

        var distributedWidthes = CalcDistributedLength(availableWidth, minWidthes, actualSizePoliciesHorizontal, actualStretchWeightHorizontal);
        var heights = Enumerable.Zip(actualSizePoliciesVertical, children).Select(pair => pair.First == SizePolicy.MinSize ? pair.Second.MinHeight : availableHeight);

        var needHorizontalAlignment = !actualSizePoliciesHorizontal.Any(IsStretchingSizePolicy); // If some elemnt is fill, remaining space will be consumed, so no need to align
        var startX = 0.0;
        var innerMinWidth = distributedWidthes.Sum() + Gap * (children.Count - 1);
        if (needHorizontalAlignment && innerMinWidth < availableWidth) {
            startX = HorizontalAlignment switch {
                AlignmentHorizontal.Center => (availableWidth - innerMinWidth) / 2.0,
                AlignmentHorizontal.Right => availableWidth - innerMinWidth,
                _ => 0.0
            };
        }

        var fixedXs = CalcAlignedPositions(startX, distributedWidthes, Gap);
        var fixedYs = heights.Select(hei => {
            var y = VerticalAlignment switch {
                AlignmentVertical.Middle => (availableHeight - hei) / 2.0,
                AlignmentVertical.Bottom => availableHeight - hei,
                _ => 0.0
            };
            return y;
        }).ToList();

        foreach (var ((fixedX, fixedY), (width, height), lweb) in Enumerable.Zip(Enumerable.Zip(fixedXs, fixedYs), Enumerable.Zip(distributedWidthes, heights), children)) {
            lweb.Arrange(new Vec2(fixedX, fixedY), new Size(width, height));
        }
    }

    protected void AlignChildrenTopLeft() {
        double currentX = 0.0;

        foreach (LayoutBase layout in ChildLayouts) {
            ElementBounds? childBounds;
            if (layout is LayoutWithElementBounds lweb) {
                if (lweb.Bounds == null) throw new InvalidOperationException($"Align is called before child ElementBounds set");
                childBounds = lweb.Bounds;
            }
            else {
                throw new NotImplementedException("We're not prepared for layouts without bounds...");
            }

            childBounds.fixedX = currentX;
            childBounds.fixedY = 0.0;

            childBounds.CalcWorldBounds();

            // Calc fixedX of next element
            currentX = childBounds.UnscaledAbsFixedX() + childBounds.UnscaledOuterWidth() + Gap;
        }
    }
}
