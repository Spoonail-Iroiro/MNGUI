using Cairo;
using MNGui.Extensions;
using MNGui.GuiElements.Layout;
using System;
using System.Collections.Generic;
using System.Linq;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.MathTools;

namespace MNGui.GuiElements;

public class MNGuiElementContainer : GuiElement {
    LoadedTexture contentTexture;

    ElementBounds insideBounds;

    bool renderFocusHighlight;

    public List<GuiElement> Elements { get; protected set; } = new();

    public Dictionary<string, GuiElement> NamedElements { get; protected set; } = new();

    public int unscaledCellSpacing = 10;

    public bool Tabbable = false;

    public override bool Focusable { get { return Tabbable; } }

    protected int currentFocusableElementKey;

    public ActionConsumable<bool>? EventLayoutApplied { get; set; }

    protected Action? EventNotifyLayoutAppliedToParent { get; set; }

    public MNGuiElementContainer(ICoreClientAPI capi, ElementBounds bounds) : base(capi, bounds) {
        contentTexture = new LoadedTexture(capi);
        bounds.IsDrawingSurface = true;
    }

    public override void BeforeCalcBounds() {
        base.BeforeCalcBounds();

        insideBounds = new ElementBounds().WithFixedPadding(unscaledCellSpacing).WithEmptyParent();
        insideBounds.CalcWorldBounds();
        CalcTotalHeight();
    }

    internal void ReloadContent() {
        CalcTotalHeight();
        ComposeList();
    }

    public void CalcTotalHeight() {
        // TODO: CalcTotalHeight
        foreach (GuiElement cell in Elements) {
            cell.BeforeCalcBounds();
        }

        Bounds.CalcWorldBounds();

        //double height = 0;
        //foreach (GuiElement cell in Elements) {
        //    cell.BeforeCalcBounds();
        //    height = Math.Max(height, cell.Bounds.fixedY + cell.Bounds.fixedHeight);
        //}

        //Bounds.fixedHeight = height + unscaledCellSpacing;
    }

    public override void ComposeElements(Context ctx, ImageSurface surface) {
        insideBounds = new ElementBounds().WithFixedPadding(unscaledCellSpacing).WithEmptyParent();
        insideBounds.CalcWorldBounds();

        Bounds.CalcWorldBounds();
        ComposeList();
    }

    void ComposeList() {
        ImageSurface surface = new ImageSurface(Format.Argb32, (int)Bounds.OuterWidth, (int)Bounds.OuterHeight);
        Context ctx = genContext(surface);

        CalcTotalHeight();
        Bounds.CalcWorldBounds();

        foreach (GuiElement elem in Elements) {
            elem.ComposeElements(ctx, surface);
        }

        generateTexture(surface, ref contentTexture);

        ctx.Dispose();
        surface.Dispose();
    }

    public GuiElement? CurrentTabIndexElement {
        get {
            foreach (GuiElement element in Elements) {
                if (element.Focusable && element.HasFocus) {
                    return element;
                }
            }

            return null;
        }
    }

    public GuiElement? FirstTabbableElement {
        get {
            foreach (GuiElement element in Elements) {
                if (element.Focusable) {
                    return element;
                }
            }

            return null;
        }
    }

    public int MaxTabIndex {
        get {
            int tabIndex = -1;
            foreach (GuiElement element in Elements) {
                if (element.Focusable) {
                    tabIndex = Math.Max(tabIndex, element.TabIndex);
                }
            }

            return tabIndex;
        }
    }

    public bool FocusElement(int tabIndex) {
        GuiElement newFocusedElement = null;

        foreach (GuiElement element in Elements) {
            if (element.Focusable && element.TabIndex == tabIndex) {
                newFocusedElement = element;
                break;
            }
        }

        if (newFocusedElement != null) {
            UnfocusOwnElementsExcept(newFocusedElement);
            newFocusedElement.OnFocusGained();
            return true;
        }

        return false;
    }

    public void UnfocusOwnElements() {
        UnfocusOwnElementsExcept(null);
    }

    /// <summary>
    /// Unfocuses all elements except one specific element.
    /// </summary>
    /// <param name="elem">The element to remain in focus.</param>
    public void UnfocusOwnElementsExcept(GuiElement elem) {
        foreach (GuiElement element in Elements) {
            if (element == elem) continue;

            if (element.Focusable && element.HasFocus) {
                element.OnFocusLost();
            }
        }
    }

    /// <summary>
    /// Reset states. Elements are not disposed for re-adding
    /// </summary>
    public virtual void ClearContent() {
        foreach (var elem in Elements) {
            if (elem is MNGuiElementContainer container) {
                container.OnRemovedFromContainer(this);
            }
        }
        Elements.Clear();
        NamedElements.Clear();
        Bounds.RemoveAllChildBounds();
        currentFocusableElementKey = 0;
        Tabbable = false;

        // Events are not content, so handlers are not cleared here
    }

    /// <summary>
    /// Clear + element.Dispose for each element in Elements. Mainly for before constructing and adding new layouts and GuiElements
    /// </summary>
    public virtual void DiscardContent() {
        foreach (var element in Elements) {
            element.Dispose();
        }
        ClearContent();
    }

    public void Add(GuiElement elem, string? name = null) {
        Elements.Add(elem);
        if (name != null) {
            NamedElements[name] = elem;
        }

        if (elem.Focusable) {
            elem.TabIndex = currentFocusableElementKey++;
            Tabbable = true;
        }
        else {
            elem.TabIndex = -1;
        }

        elem.InsideClipBounds = InsideClipBounds;

        if (elem is MNGuiElementContainer container) {
            container.OnAddedToContainer(this);
        }
    }

    public void SetChildBound(ElementBounds bounds) {
        Bounds.RemoveAllChildBounds();
        Bounds.WithChildForce(bounds);
        //Bounds.ChildBounds.Add(bounds);
        //bounds.ParentBounds = Bounds;
    }

    public override void OnMouseUp(ICoreClientAPI api, MouseEvent args) {
        foreach (GuiElement element in Elements) {
            element.OnMouseUp(api, args);
        }

        if (!args.Handled) base.OnMouseUp(api, args);
    }

    public override void OnMouseDown(ICoreClientAPI api, MouseEvent args) {
        bool beforeHandled = false;
        bool nowHandled = false;
        renderFocusHighlight = false;

        foreach (GuiElement element in Elements) {
            if (!beforeHandled) {
                element.OnMouseDown(api, args);
                nowHandled = args.Handled;
            }

            if (!beforeHandled && nowHandled) {
                if (element.Focusable && !element.HasFocus) {
                    element.OnFocusGained();
                }
            }
            else {
                if (element.Focusable && element.HasFocus) {
                    element.OnFocusLost();
                }
            }

            beforeHandled = nowHandled;
        }

        if (!args.Handled) base.OnMouseDown(api, args);
    }


    public override void OnMouseMove(ICoreClientAPI api, MouseEvent args) {
        foreach (GuiElement element in Elements) {
            element.OnMouseMove(api, args);
            if (args.Handled) {
                break;
            }
        }

        if (!args.Handled) base.OnMouseMove(api, args);
    }


    bool tabPressed = false;
    bool shiftTabPressed = false;
    public override void OnKeyDown(ICoreClientAPI api, KeyEvent args) {
        tabPressed = args.KeyCode == (int)GlKeys.Tab;
        shiftTabPressed = tabPressed && args.ShiftPressed;

        if (!HasFocus) return;

        base.OnKeyDown(api, args);

        foreach (GuiElement element in Elements) {
            element.OnKeyDown(api, args);
            if (args.Handled) break;
        }

        if (!args.Handled && args.KeyCode == (int)GlKeys.Tab && Tabbable) {
            renderFocusHighlight = true;
            var elem = CurrentTabIndexElement;
            if (elem != null && MaxTabIndex > 0) {
                int dir = args.ShiftPressed ? -1 : 1;
                int tb = elem.TabIndex + dir;
                if (tb < 0 || tb > MaxTabIndex || args.CtrlPressed) return;
                FocusElement(tb);
                args.Handled = true;
            }
            else if (MaxTabIndex > 0) {
                FocusElement(args.ShiftPressed ? GameMath.Mod(-1, MaxTabIndex + 1) : 0);
                args.Handled = true;
            }
        }

        // Hardcoded element class type :/
        if (!args.Handled && (args.KeyCode == (int)GlKeys.Enter || args.KeyCode == (int)GlKeys.KeypadEnter) && CurrentTabIndexElement is GuiElementEditableTextBase) {
            UnfocusOwnElementsExcept(null);
        }
    }

    public override void OnKeyUp(ICoreClientAPI api, KeyEvent args) {
        tabPressed = false;
        shiftTabPressed = false;

        if (!HasFocus) return;

        base.OnKeyUp(api, args);

        foreach (GuiElement element in Elements) {
            element.OnKeyUp(api, args);
            if (args.Handled) break;
        }
    }

    public override void OnKeyPress(ICoreClientAPI api, KeyEvent args) {
        if (!HasFocus) return;

        base.OnKeyPress(api, args);

        foreach (GuiElement element in Elements) {
            element.OnKeyPress(api, args);
            if (args.Handled) break;
        }
    }

    public override void OnMouseWheel(ICoreClientAPI api, MouseWheelEventArgs args) {
        if (!Bounds.ParentBounds.PointInside(api.Input.MouseX, api.Input.MouseY)) return;

        // Prefer an element that is currently hovered 
        foreach (var element in Elements) {
            if (element.IsPositionInside(api.Input.MouseX, api.Input.MouseY)) {
                element.OnMouseWheel(api, args);
            }

            if (args.IsHandled) return;
        }

        foreach (GuiElement element in Elements) {
            element.OnMouseWheel(api, args);
            if (args.IsHandled) break;
        }
    }

    public override void OnFocusGained() {
        base.OnFocusGained();

        if (CurrentTabIndexElement != null) return;

        renderFocusHighlight = tabPressed;
        if (shiftTabPressed) FocusElement(MaxTabIndex);
        else FocusElement(FirstTabbableElement.TabIndex);
    }

    public override void OnFocusLost() {
        base.OnFocusLost();

        renderFocusHighlight = false;
        UnfocusOwnElements();
    }


    public override void RenderInteractiveElements(float deltaTime) {
        api.Render.Render2DTexturePremultipliedAlpha(contentTexture.TextureId, Bounds);

        MouseOverCursor = null;
        foreach (GuiElement element in Elements) {
            element.RenderInteractiveElements(deltaTime);

            if (element.IsPositionInside(api.Input.MouseX, api.Input.MouseY)) {
                MouseOverCursor = element.MouseOverCursor;
            }
        }

        ElementBounds tempClipBounds;
        foreach (GuiElement element in Elements) {
            // Seperate due to clipping
            if (element.HasFocus && renderFocusHighlight) {
                if (InsideClipBounds != null) {
                    tempClipBounds = element.InsideClipBounds;
                    element.InsideClipBounds = null;
                    element.RenderFocusOverlay(deltaTime);
                    element.InsideClipBounds = tempClipBounds;
                }
                else {
                    element.RenderFocusOverlay(deltaTime);
                }
            }
        }
    }

    public override void Dispose() {
        base.Dispose();
        contentTexture.Dispose();

        foreach (var val in Elements) {
            val.Dispose();
        }
        ClearContent();

        EventLayoutApplied = null;
        EventNotifyLayoutAppliedToParent = null;
    }

    public override void RenderBoundsDebug() {
        base.RenderBoundsDebug();
        foreach (var elem in Elements) {
            // Skip layout elements if Outline mode is 2
            if (GuiComposer.Outlines == 2) {
                if (elem is GuiElementDebugHorizontalLayout || elem is GuiElementDebugVerticalLayout) {
                    continue;
                }
            }
            elem.RenderBoundsDebug();
        }
    }

    public override int OutlineColor() {
        var intVal = ColorUtil.ToRgba(255, 128, 255, 128);
        return intVal;
    }

    /// <summary>
    /// API to parent: should be called when this container is added to the parent
    /// </summary>
    /// <param name="parentContainer"></param>
    protected void OnAddedToContainer(MNGuiElementContainer parentContainer) {
        EventNotifyLayoutAppliedToParent = parentContainer.OnChildContainerNotifyLayoutApplied;

    }

    /// <summary>
    /// API to parent: should be called when this container is removed from the parent
    /// </summary>
    /// <param name="parentContainer"></param>
    protected void OnRemovedFromContainer(MNGuiElementContainer parentContainer) {
        if (EventNotifyLayoutAppliedToParent == parentContainer.OnChildContainerNotifyLayoutApplied) {
            EventNotifyLayoutAppliedToParent = null;
        }
    }

    /// <summary>
    /// Handler when received notification of LayoutApplied from a child container
    /// </summary>
    internal void OnChildContainerNotifyLayoutApplied() {
        NotifyExternalThenPropagate(false);
    }

    /// <summary>
    /// Notify this containe's layout is changed and need to be re-layout (Measure and Arrange), to external handler and (unless it's consumed,) parent
    /// </summary>
    /// <remarks>
    /// MNGuiElementContainer has no timing to notify by itself, so this need to be called from outside when elements are ready
    /// </remarks>
    public void NotifyLayoutApplied() {
        NotifyExternalThenPropagate(true);
    }

    protected void NotifyExternalThenPropagate(bool fromThis) {
        // The arg to handler is true because it's notified from a child
        var consumed = EventLayoutApplied?.Invoke(fromThis);

        // If the event is consumed, don't propagate to parent
        if (consumed == true) return;

        // Propagate to parent
        EventNotifyLayoutAppliedToParent?.Invoke();
    }
}

