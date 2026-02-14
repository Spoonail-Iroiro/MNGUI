using MNGui.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using Vintagestory.API.Client;
using MNGui.Util;

namespace MNGui.Layouts;

// For wrapper-type GuiElement with single child layout
public class SimpleWrapperLayout : SingleLayout {
    public List<LayoutBase> ChildLayouts { get; set; } = new();

    public SimpleWrapperLayout(GuiElement guiElement, string? name = null) : base(guiElement, name) {
    }

    new public SimpleWrapperLayout WithMaxWidth(double maxWidth) {
        WithMaxWidthInternal(maxWidth);
        return this;
    }

    new public SimpleWrapperLayout WithMaxHeight(double maxHeight) {
        WithMaxHeightInternal(maxHeight);
        return this;
    }

    public SimpleWrapperLayout Add(LayoutBase layout) {
        ChildLayouts.Add(layout);
        return this;
    }

    public override void Init() {
        base.Init();

        foreach (LayoutBase layout in ChildLayouts) {
            layout.Init();
        }
    }

    protected override void MeasureInternal() {
        Bounds.RemoveAllChildBounds();

        var hGreedings = new List<SpaceGreedingPolicy>();
        var vGreedings = new List<SpaceGreedingPolicy>();

        foreach (var childLayout in ChildLayouts) {
            if (childLayout is LayoutWithElementBounds lweb) {
                lweb.Measure();
                Bounds.WithChildForce(lweb.Bounds);

                // SpaceGreedings from child
                hGreedings.Add(lweb.HorizontalSpaceGreedingPolicy);
                vGreedings.Add(lweb.VerticalSpaceGreedingPolicy);
            }
            else {
                throw new NotImplementedException();
            }
        }

        base.MeasureInternal();

        // Determine Horizontal/VerticalSpaceGreeding from child and me
        // TODO: is this right? maybe it's just return this layout is fill (FollowArrange?) or not
        hGreedings.Add(HorizontalSpaceGreedingPolicy);
        vGreedings.Add(VerticalSpaceGreedingPolicy);
        HorizontalSpaceGreedingPolicy = LayoutUtil.AggregateGreeding([.. hGreedings]);
        VerticalSpaceGreedingPolicy = LayoutUtil.AggregateGreeding([.. vGreedings]);
    }

    public override void Arrange(Vec2 fixedPos, Size availableSize) {
        base.Arrange(fixedPos, availableSize);

        var isArrangedFirstChild = false;
        foreach (var childLayout in ChildLayouts) {
            if (childLayout is LayoutWithElementBounds lweb) {
                if (!isArrangedFirstChild) {
                    var target = lweb;
                    var width = 0.0;
                    var height = 0.0;

                    // Arrange with MinSize if the child says, otherwise fill(1.0) always
                    switch (target.HorizontalSizePolicy) {
                        case SizePolicy.MinSize:
                            width = target.MinWidth;
                            break;
                        default:
                            width = Bounds.UnscaledInnerWidth();
                            break;
                    }

                    switch (target.VerticalSizePolicy) {
                        case SizePolicy.MinSize:
                            height = target.MinHeight;
                            break;
                        default:
                            height = Bounds.UnscaledInnerHeight();
                            break;
                    }

                    target.Arrange(new Vec2(0.0, 0.0), new Size(width, height));

                    isArrangedFirstChild = true;
                }
                else {
                    // Won't respect size policy of the second child or later
                    lweb.ArrangeWithMinSize();
                }
            }
            else {
                throw new NotImplementedException();
            }
        }
    }

    public override IEnumerable<GuiElementInfo> GetAllGuiElements() {
        var baseRtn = base.GetAllGuiElements();
        foreach (var layout in baseRtn) {
            yield return layout;
        }

        foreach (var layout in ChildLayouts) {
            foreach (var elem in layout.GetAllGuiElements()) {
                yield return elem;
            }
        }
    }
}
