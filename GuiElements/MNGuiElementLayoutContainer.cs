using Cairo;
using MNGui.Extensions;
using MNGui.GuiElements.Layout;
using MNGui.Layouts;
using System.Diagnostics.CodeAnalysis;
using Vintagestory.API.Client;

namespace MNGui.GuiElements;
public class MNGuiElementLayoutContainer : MNGuiElementContainer, ILayoutableElement {

    protected LayoutWithElementBounds ChildLayout { get; set; }

    protected LayoutWithElementBounds? PendingNewLayout { get; set; }

    public MNGuiElementLayoutContainer(ICoreClientAPI capi, ElementBounds bounds) : base(capi, bounds) {
        // Initial Layout
        var layout = new VerticalLayout(capi).Add(new GuiElementDebugHorizontalLayout(capi, ElementBounds.FixedSize(1, 1)), "temp");
        ApplyNewLayoutImmediately(layout);
    }

    /// <summary>
    /// Discard old elements and set elements and child bound from passed layout. Will be executed deffered to safely update elements in an event handler.
    /// </summary>
    /// <remarks>
    /// Reusing layout is not recommended since this discards all.
    /// </remarks>
    public void SetNewLayout(LayoutWithElementBounds newLayout) {
        // Will be actually applied when ResolvePendingNewLayout is called
        PendingNewLayout = newLayout;
    }

    /// <summary>
    /// Discard old elements and set elements and child bound from passed layout. Warning: Not safe in an event handler. Generally, use SetNewLayout instead.
    /// </summary>
    /// <remarks>
    /// Reusing layout is not recommended since this discards all. Also, Not safe in an event handler.
    /// </remarks>
    [MemberNotNull(nameof(ChildLayout))]
    public void ApplyNewLayoutImmediately(LayoutWithElementBounds layout) {
        DiscardContent();

        ChildLayout = layout;

        ChildLayout.Init();

        var elementInfos = ChildLayout.GetAllGuiElements();

        foreach (var info in elementInfos) {
            Add(info.Element, info.Name);
        }

        SetChildBound(ChildLayout.Bounds!);
    }

    protected void ResolvePendingNewLayout() {
        if (PendingNewLayout != null) {
            ApplyNewLayoutImmediately(PendingNewLayout);
            PendingNewLayout = null;
        }
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

    public override void ComposeElements(Context ctx, ImageSurface surface) {
        ResolvePendingNewLayout();
        base.ComposeElements(ctx, surface);
    }

    public override void OnMouseUp(ICoreClientAPI api, MouseEvent args) {
        base.OnMouseUp(api, args);
        ResolvePendingNewLayout();
    }

    public override void OnMouseDown(ICoreClientAPI api, MouseEvent args) {
        base.OnMouseDown(api, args);
        ResolvePendingNewLayout();
    }

    public override void OnMouseMove(ICoreClientAPI api, MouseEvent args) {
        base.OnMouseMove(api, args);
        ResolvePendingNewLayout();
    }

    public override void OnKeyDown(ICoreClientAPI api, KeyEvent args) {
        base.OnKeyDown(api, args);
        ResolvePendingNewLayout();
    }

    public override void OnKeyUp(ICoreClientAPI api, KeyEvent args) {
        base.OnKeyUp(api, args);
        ResolvePendingNewLayout();
    }

    public override void OnKeyPress(ICoreClientAPI api, KeyEvent args) {
        base.OnKeyPress(api, args);
        ResolvePendingNewLayout();
    }

    public override void OnMouseWheel(ICoreClientAPI api, MouseWheelEventArgs args) {
        base.OnMouseWheel(api, args);
        ResolvePendingNewLayout();
    }

    public override void OnFocusGained() {
        base.OnFocusGained();
        ResolvePendingNewLayout();
    }

    public override void OnFocusLost() {
        base.OnFocusLost();
        ResolvePendingNewLayout();
    }
}
