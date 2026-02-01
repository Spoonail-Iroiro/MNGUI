using MNGui.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using Vintagestory.API.Client;

namespace MNGui.GuiElements;
public class MNGuiElementVerticalScrollbar : GuiElementScrollbar {
    public ElementBounds? ContentBounds { get; protected set; }
    public ElementBounds? ViewBounds { get; protected set; }

    public double ContentInitialFixedY { get; protected set; }

    public MNGuiElementVerticalScrollbar(ICoreClientAPI capi, ElementBounds bounds) : base(capi, null, bounds) {
        onNewScrollbarValue = onNewScrollbarValueHandler;
    }

    protected void onNewScrollbarValueHandler(float newValue) {
        if (ContentBounds == null) return;
        ContentBounds.fixedY = ContentInitialFixedY - newValue;
        ContentBounds.CalcWorldBounds();
    }

    public void InitViewAndContentBounds(ElementBounds viewBounds, ElementBounds contentBounds) {
        ViewBounds = viewBounds;
        ContentBounds = contentBounds;

        ContentInitialFixedY = ContentBounds.fixedY;

        SetHeights((float)ViewBounds.UnscaledOuterHeight(), (float)ContentBounds.UnscaledOuterHeight());
    }

    /// <summary>
    /// Call this when ViewBounds or ContentBounds changed its size
    /// </summary>
    public void OnBoundsUpdated() {
        if (ViewBounds != null && ContentBounds != null) {
            SetHeights(ViewBounds.OuterHeightInt, ContentBounds.OuterHeightInt);
        }
    }
}
