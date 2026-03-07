using System;
using System.Diagnostics.CodeAnalysis;
using Cairo;
using MNGui.Extensions;
using MNGui.Layouts;
using MNGui.Layouts.Extensions;
using MNGui.Layouts.Interfaces;
using Vintagestory.API.Client;

namespace MNGui.GuiElements;

/// <summary>
/// A container GuiElement for the Measure/Arrange layout system.
/// Only works under a Measure/Arrange layout.
/// Layout lifecycle (Init/Measure/ Arrange) is delegated to a parent;
///   a parent container, usually the root, must listen to EventRelayoutRequired and trigger relayout when needed.
/// </summary>
public class MNGuiElementInnerLayoutContainer : MNGuiElementContainer, ILayoutableElement {

    protected LayoutWithElementBounds ChildLayout { get; set; }

    protected LayoutWithElementBounds? PendingNewLayout { get; set; }
    protected Action<MNGuiElementInnerLayoutContainer>? PendingCallbackRelayouted { get; set; }

    /// Default layout Must not need to be disposed, since it might remain after container's Dispose
    protected LayoutWithElementBounds DefaultLayout => new VerticalLayout(api).Add(new GuiElementDummy(api, ElementBounds.FixedSize(1, 1)), "temp");

    public MNGuiElementInnerLayoutContainer(ICoreClientAPI capi, ElementBounds bounds, LayoutWithElementBounds? initialLayout = null) : base(capi, bounds) {
        // Initial Layout
        var layout = initialLayout ?? DefaultLayout;
        ApplyNewLayoutImmediately(layout);
    }

    /// <summary>
    /// Discard old elements and set elements and child bound from passed layout. Will be executed deffered to safely update elements in an event handler.
    /// </summary>
    /// <remarks>
    /// Reusing layout is not recommended since this discards all.
    /// LayoutApplied will be notified on actual apply.
    /// </remarks>
    public void SetNewLayout(LayoutWithElementBounds newLayout, Action<MNGuiElementInnerLayoutContainer>? callbackRelayouted = null) {
        // Will be actually applied when ResolvePendingNewLayout is called
        PendingNewLayout = newLayout;
        PendingCallbackRelayouted = callbackRelayouted;
        // Hacky, ensuring resolution of layout outside of event handler
        api.Event.RegisterCallback(dt => ResolvePendingNewLayout(), 0);
    }

    /// <summary>
    /// Discard old elements and set elements and child bound from passed layout. Warning: Not safe in an event handler. Generally, use SetNewLayout instead.
    /// </summary>
    /// <remarks>
    /// Reusing layout is not recommended since this discards all. Also, it's not safe called in an event handler.
    /// LayoutApplied will be notified.
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

        // Notify layout actually applied, to outside and parent
        NotifyRelayoutRequired();
    }

    public void ResolvePendingNewLayout() {
        if (PendingNewLayout != null) {
            // To prevent discarded, saving here
            var callback = PendingCallbackRelayouted;
            ApplyNewLayoutImmediately(PendingNewLayout);
            callback?.Invoke(this);
            PendingNewLayout = null;
            PendingCallbackRelayouted = null;
        }
    }

    public override void ClearContent() {
        base.ClearContent();
        ChildLayout = DefaultLayout;
        PendingNewLayout = null;
        PendingCallbackRelayouted = null;
        // Tied actions are not content, so not cleared here
    }

    public void Init() {
    }

    public void BeforeMeasure() {
        ChildLayout.Measure();

        BeforeCalcBounds();
        Bounds.CalcWorldBounds();

        return;
    }

    public void AfterArrange() {
        // After parent layout's arrange, this element has proper spaceing in its bounds
        ChildLayout.Arrange(new(0.0, 0.0), new(Bounds.UnscaledInnerWidth(), Bounds.UnscaledInnerHeight()));
    }

    public override void ComposeElements(Context ctx, ImageSurface surface) {
        // Shouldn't do that, since it might cause recursive ReCompose 
        //ResolvePendingNewLayout();
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
