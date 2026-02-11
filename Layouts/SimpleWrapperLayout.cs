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
        hGreedings.Add(HorizontalSpaceGreedingPolicy);
        vGreedings.Add(VerticalSpaceGreedingPolicy);
        HorizontalSpaceGreedingPolicy = LayoutUtil.AggregateGreeding([.. hGreedings]);
        VerticalSpaceGreedingPolicy = LayoutUtil.AggregateGreeding([.. vGreedings]);
    }

    public override void Arrange(Vec2 fixedPos, Size availableSize) {
        base.Arrange(fixedPos, availableSize);

        foreach (var childLayout in ChildLayouts) {
            if (childLayout is LayoutWithElementBounds lweb) {
                if (lweb.Bounds == null) throw new InvalidOperationException($"Call Measure before Arrange!");
                lweb.Arrange(new Vec2(lweb.Bounds.fixedX, lweb.Bounds.fixedY), lweb.MinSize);
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
