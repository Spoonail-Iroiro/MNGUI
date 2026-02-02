using MNGui.Extensions;
using MNGui.GuiElements.Layout;
using MNGui.Layouts;
using System.Diagnostics.CodeAnalysis;
using Vintagestory.API.Client;

namespace MNGui.GuiElements;
public class MNGuiElementLayoutContainer : MNGuiElementContainer, ILayoutableElement {

    protected LayoutWithElementBounds ChildLayout { get; set; }

    public MNGuiElementLayoutContainer(ICoreClientAPI capi, ElementBounds bounds) : base(capi, bounds) {
        // Initial Layout
        var layout = new VerticalLayout(capi).Add(new GuiElementDebugHorizontalLayout(capi, ElementBounds.FixedSize(1, 1)), "temp");
        SetNewLayout(layout);
    }

    /// <summary>
    /// Set layout for this container. Warning: old element will be DISPOSED on Init! it's not recommended to reuse layout 
    /// </summary>
    [MemberNotNull(nameof(ChildLayout))]
    public void SetNewLayout(LayoutWithElementBounds layout) {
        DiscardContent();

        ChildLayout = layout;

        ChildLayout.Init();

        var elementInfos = ChildLayout.GetAllGuiElements();

        foreach (var info in elementInfos) {
            Add(info.Element, info.Name);
        }

        SetChildBound(ChildLayout.Bounds!);
    }

    public void Init() {
    }

    public void BeforeMeasure() {
        ChildLayout.Measure();

        Bounds.CalcWorldBounds();

        return;
    }

    public void AfterArrange() {
        // After parent layout's arrange, this element has proper spaceing in its bounds
        ChildLayout.Arrange(new(0.0, 0.0), new(Bounds.UnscaledInnerWidth(), Bounds.UnscaledInnerWidth()));
    }
}
