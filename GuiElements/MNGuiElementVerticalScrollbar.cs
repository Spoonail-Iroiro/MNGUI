using Cairo;
using MNGui.Extensions;
using MNGui.Layouts.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using Vintagestory.API.Client;

namespace MNGui.GuiElements;
public class MNGuiElementVerticalScrollbar : GuiElementScrollbar, ILayoutableElement {
    public ElementBounds? ContentBounds { get; protected set; }
    public ElementBounds? ViewBounds { get; protected set; }

    public double ContentInitialFixedY { get; protected set; }

    public MNGuiElementVerticalScrollbar(ICoreClientAPI capi, ElementBounds bounds) : base(capi, null, bounds) {
        onNewScrollbarValue = onNewScrollbarValueHandler;
    }

    protected void onNewScrollbarValueHandler(float newValue) {
        if (ContentBounds == null) return;
        // Scroll by offset not to remember initial fixedY
        ContentBounds.fixedOffsetY = -newValue;
        ContentBounds.CalcWorldBounds();
    }

    public void SetViewAndContentBounds(ElementBounds viewBounds, ElementBounds contentBounds) {
        ViewBounds = viewBounds;
        ContentBounds = contentBounds;
    }

    /// <summary>
    /// Call this when ViewBounds or ContentBounds changed its size
    /// </summary>
    public void OnBoundsUpdated() {
        if (ViewBounds != null && ContentBounds != null) {
            SetHeights((float)ViewBounds.UnscaledOuterHeight(), (float)ContentBounds.UnscaledOuterHeight());
        }
    }

    public override void OnMouseWheel(ICoreClientAPI api, MouseWheelEventArgs args) {
        if (ViewBounds == null || ContentBounds == null) return;
        if (ViewBounds.PointInside(api.Input.MouseX, api.Input.MouseY) || Bounds.PointInside(api.Input.MouseX, api.Input.MouseY)) {
            base.OnMouseWheel(api, args);
        }
    }

    public override void ComposeElements(Context ctxStatic, ImageSurface surface) {
        Bounds.CalcWorldBounds();
        // Set new height from view and content bounds, since this point is usually after all Arrange
        OnBoundsUpdated();
        base.ComposeElements(ctxStatic, surface);
    }

    public void Init() {
        // Set OffsetY to 0 temporary for layouting (will be recovered on ComposeElements)
        if (ContentBounds != null) {
            ContentBounds.fixedOffsetY = 0.0;
        }
    }

    public void BeforeMeasure() {
    }

    public void AfterArrange() {
    }
}
